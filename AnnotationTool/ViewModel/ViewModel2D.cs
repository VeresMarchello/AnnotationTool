using SharpDX;
using System.Windows.Input;
using System.IO;
using AnnotationTool.Commands;
using System.Windows.Media.Imaging;
using HelixToolkit.Wpf.SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using AnnotationTool.Model;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Threading;
using System.Windows.Forms;
using AnnotationTool.Utils;
using System.Windows.Threading;

namespace AnnotationTool.ViewModel
{
    public class ViewModel2D : ViewModelBase, IDisposable
    {
        private string _selectedLeftImage = null;
        private string[] _images;

        private _2DLine _selected2dLine;
        private List<_2DLine> _2dLeftLineList;
        private List<_2DLine> _2dRightLineList;

        private CancellationTokenSource _source;
        private ObservableCollection<string> _errorMessages;

        private bool disposedValue;
        private readonly ReaderWriterLockSlim xmlLock = new ReaderWriterLockSlim();

        private int _selectedTabIndex;
        private bool _isAnnotationEnabled;

        private static string[] LeftDirectoryFiles = new string[] { };
        private static string[] RightDirectoryFiles = new string[] { };
        private static string[] LeftPrunedDirectoryFiles = new string[] { };

        private static System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
        private static System.Configuration.KeyValueConfigurationElement configPath = config.AppSettings.Settings["ImagesPath"];

        public ViewModel2D()
        {
            //config.AppSettings.Settings.Remove("ImagesPath");
            //config.Save(System.Configuration.ConfigurationSaveMode.Modified);
            //configPath = config.AppSettings.Settings["ImagesPath"];
            ImageGroupSize = 30;
            ImageGroupStartingIndex = 0;

            _images = new string[] { };

            _2dLeftLineList = new List<_2DLine>();
            _2dRightLineList = new List<_2DLine>();
            _selected2dLine = new _2DLine(new Vector3(0), new Vector3(0), MarkingType.GeneralPruning);
            _source = new CancellationTokenSource();
            _errorMessages = new ObservableCollection<string>() { };
            _isAnnotationEnabled = true;

            SelectImageCommand = new RelayCommand<object>(ChangeSelectedImage);
            DeleteErrorMessageCommand = new RelayCommand<object>(DeleteErrorMessage);
            ShowFilesCommand = new RelayCommand(ShowFiles);
            View2DLoaded = new RelayCommand(SetImagesPath);

            NextImageGroupCommand = new RelayCommand(() => ChangeImageGroup(true), (x) => ImageGroupStartingIndex + ImageGroupSize < Images.Length - 1);
            PreviousImageGroupCommand = new RelayCommand(() => ChangeImageGroup(false), (x) => 0 <= ImageGroupStartingIndex - ImageGroupSize);

            IncreaseDeltaCommand = new RelayCommand(() => Delta++, (x) => Index + Delta < LeftPrunedDirectoryFiles.Length - 1);
            DecreaseDeltaCommand = new RelayCommand(() => Delta--, (x) => 0 < Index + Delta);
            //IncreaseIndexCommand = new RelayCommand(() => Index++, (x) => Index < LeftDirectoryFiles.Length - 1);
            //DecreaseIndexCommand = new RelayCommand(() => Index--, (x) => 0 < Index);

            //IncreaseDeltaCommand = new RelayCommand(() => Delta++, (x) => Index + Delta < ImageGroupStartingIndex + ImageGroupSize - 1);
            //DecreaseDeltaCommand = new RelayCommand(() => Delta--, (x) => ImageGroupStartingIndex < Index + Delta);
            IncreaseIndexCommand = new RelayCommand(() => Index++, (x) => Index < ImageGroupStartingIndex + ImageGroupSize - 1 && Index < LeftDirectoryFiles.Length - 1);
            DecreaseIndexCommand = new RelayCommand(() => Index--, (x) => ImageGroupStartingIndex < Index);
        }

        /// <summary>
        /// eltérő index delta command felteleke kellenek attol fuggoen hogy a pruned vagy az unpruned a hosszabb
        /// </summary>

        private int _delta;
        public int Delta
        {
            get { return _delta; }
            set
            {
                _delta = value;
                NotifyPropertyChanged();

                ResetCamera();
            }
        }
        public int Index
        {
            get { return Array.IndexOf(Images, SelectedLeftImage); }
            set
            {
                ChangeSelectedImage(LeftDirectoryFiles[value]);
            }
        }

        private void SetDelta()
        {
            if (Index + Delta < 0)
            {
                Delta = -Index;
            }
            else if (Index + Delta > LeftPrunedDirectoryFiles.Length - 1)
            {
                Delta = LeftPrunedDirectoryFiles.Length - Index - 1;
            }
        }

        public string SelectedLeftImage
        {
            get { return _selectedLeftImage; }
            set
            {
                _selectedLeftImage = value;
                NotifyPropertyChanged("Index");
                SetDelta();
                ResetCamera();
                NotifyPropertyChanged();
                NotifyPropertyChanged("SelectedRightImage");
            }
        }
        public string SelectedRightImage => Index > -1 ? RightDirectoryFiles[Index] : "";

        public string[] Images
        {
            get { return _images; }
            set
            {
                _images = value;
                NotifyPropertyChanged();

                ImageGroupStartingIndex = 0;
                Delta = 0;
                if (Images != null && Images.Length > 0)
                {
                    ImageGroup = Images.Skip(ImageGroupStartingIndex).Take(ImageGroupSize);
                    ChangeSelectedImage(ImageGroup.First());
                }
            }
        }


        private async void SetImagesPath()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                int count = 0;
                int maxCount = 3;
                fbd.ShowNewFolderButton = false;
                if (configPath == null)
                {
                    fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                }
                else
                {
                    fbd.SelectedPath = configPath.Value;
                }

                DialogResult result = DialogResult.None;
                do
                {
                    fbd.Description = $"Válassza ki a képeket tartalmazó mappát!\nA mappának tartalmaznia kell a Left, Right, azokon belül is az Unpruned, Pruned mappákat!\nPróbákozások száma {maxCount - count}";
                    result = fbd.ShowDialog();
                    count++;
                }
                while ((result != DialogResult.OK || !CheckForFiles(fbd.SelectedPath)) && count < maxCount);

                if (result != DialogResult.OK || !CheckForFiles(fbd.SelectedPath))
                {
                    if (Images.Length > 0)
                    {
                        return;
                    }
                    else
                    {
                        System.Windows.Application.Current.Shutdown();
                    }
                }

                config.AppSettings.Settings.Remove("ImagesPath");
                config.AppSettings.Settings.Add("ImagesPath", fbd.SelectedPath);
                config.Save(System.Configuration.ConfigurationSaveMode.Modified);
                configPath = config.AppSettings.Settings["ImagesPath"];

                await SetDirectories();
                Images = await GetFolderFiles();

                if (Images.Length > 0)
                {
                    SelectedLeftImage = Images[0];
                }

                bool CheckForFiles(string path)
                {
                    DirectoryInfo rootDirectoryInfo = new DirectoryInfo(path);
                    var directionsDirectoryInfo = rootDirectoryInfo.GetDirectories();

                    if (directionsDirectoryInfo.Count(x => x.Name == "Left" || x.Name == "Right") != 2)
                    {
                        return false;
                    }

                    bool filesExists = true;
                    foreach (var item in directionsDirectoryInfo)
                    {
                        var correct = item.GetDirectories().Where(x => x.Name == "Pruned" || x.Name == "Unpruned");
                        if (correct.Count() == 2)
                        {
                            filesExists &= correct.All(x => x.GetFiles("*.jpg").Any());
                        }
                    }

                    return filesExists;
                }
            }
        }

        private async Task SetDirectories()
        {
            List<Task<string[]>> tasks = new List<Task<string[]>>();
            tasks.Add(Task.Run(() => Directory.GetFiles($@"{configPath.Value}\Left\Unpruned", "*.jpg")));
            tasks.Add(Task.Run(() => Directory.GetFiles($@"{configPath.Value}\Right\Unpruned", "*.jpg")));
            tasks.Add(Task.Run(() => Directory.GetFiles($@"{configPath.Value}\Left\Pruned", "*.jpg")));
            var result = await Task.WhenAll(tasks);

            LeftDirectoryFiles = result[0];
            RightDirectoryFiles = result[1];
            LeftPrunedDirectoryFiles = result[2];
        }

        public _2DLine Selected2dLine
        {
            get { return _selected2dLine; }
            set
            {
                _selected2dLine = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("SelectedLine");

                if (value != null)
                {
                    var target = VectorPixelConverter.GetVectorFromPixel(value.CenterPoint, SelectedLeftImage);

                    SetCameraTarget(target);
                }
            }
        }
        public List<_2DLine> _2DLeftLineList
        {
            get { return _2dLeftLineList; }
            set
            {
                _2dLeftLineList = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("LeftLines");
                SelectedTabIndex = 0;
            }
        }
        public List<_2DLine> _2DRightLineList
        {
            get { return _2dRightLineList; }
            set
            {
                _2dRightLineList = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("RightLines");
                SelectedTabIndex = 1;
            }
        }

        public Geometry3D.Line SelectedLine
        {
            get { return GetLine(Selected2dLine); }
            set { Selected2dLine = Get_2DLine(value); }
        }

        public LineGeometry3D LeftLines
        {
            get { return GetLineGeometry(_2DLeftLineList); }
            set
            {
                _2DLeftLineList = Get_2DLineList(value);
                SaveLineToXML(_2DLeftLineList, SelectedLeftImage);
            }
        }
        public LineGeometry3D RightLines
        {
            get { return GetLineGeometry(_2DRightLineList); }
            set
            {
                _2DRightLineList = Get_2DLineList(value);
                SaveLineToXML(_2DRightLineList, SelectedRightImage);
            }
        }

        public ObservableCollection<string> ErrorMessages
        {
            get { return _errorMessages; }
            set
            {
                _errorMessages = value;
                NotifyPropertyChanged();
            }
        }

        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                _selectedTabIndex = value;
                NotifyPropertyChanged();
            }
        }
        public bool IsAnnotationEnabled
        {
            get { return _isAnnotationEnabled; }
            set
            {
                _isAnnotationEnabled = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand SelectImageCommand { get; private set; }
        public ICommand DeleteErrorMessageCommand { get; private set; }
        public ICommand ShowFilesCommand { get; private set; }

        public ICommand IncreaseDeltaCommand { get; private set; }
        public ICommand DecreaseDeltaCommand { get; private set; }
        public ICommand IncreaseIndexCommand { get; private set; }
        public ICommand DecreaseIndexCommand { get; private set; }
        public ICommand View2DLoaded { get; private set; }

        public ICommand NextImageGroupCommand { get; private set; }
        public ICommand PreviousImageGroupCommand { get; private set; }


        private LineGeometry3D GetLineGeometry(List<_2DLine> _2DLines)
        {
            var lineBuilder = new LineBuilder();

            foreach (var line in _2DLines)
            {
                var v1 = VectorPixelConverter.GetVectorFromPixel(line.FirstPoint, SelectedLeftImage);
                var v2 = VectorPixelConverter.GetVectorFromPixel(line.MirroredPoint, SelectedLeftImage);

                lineBuilder.AddLine(v1, v2);
            }

            var lineGeometry = lineBuilder.ToLineGeometry3D();
            lineGeometry.Colors = new Color4Collection();
            for (int i = 0; i < lineGeometry.Indices.Count / 2; i++)
            {
                lineGeometry.Colors.Add(GetColor(_2DLines[i].Type));
                lineGeometry.Colors.Add(GetColor(_2DLines[i].Type));
            }

            return lineGeometry;
        }
        private List<_2DLine> Get_2DLineList(LineGeometry3D lineGeometry)
        {
            var lineList = new List<_2DLine>();
            var count = lineGeometry.Positions.Count;

            for (int i = 0; i < count; i += 2)
            {
                var center = (lineGeometry.Positions[i] + lineGeometry.Positions[i + 1]) / 2;
                var pixelCenter = VectorPixelConverter.GetPixelFromVector(center, SelectedLeftImage);
                var pixelFiirst = VectorPixelConverter.GetPixelFromVector(lineGeometry.Positions[i], SelectedLeftImage);
                var type = GetMarkingType(lineGeometry.Colors[i]);
                var newLine = new _2DLine(pixelCenter, pixelFiirst, type);
                lineList.Add(newLine);
            }

            return lineList;
        }


        private Geometry3D.Line GetLine(_2DLine line)
        {
            if (line == null)
            {
                return new Geometry3D.Line();
            }

            return new Geometry3D.Line()
            {
                P0 = VectorPixelConverter.GetVectorFromPixel(line.FirstPoint, SelectedLeftImage),
                P1 = VectorPixelConverter.GetVectorFromPixel(line.MirroredPoint, SelectedLeftImage),
            };
        }
        private _2DLine Get_2DLine(Geometry3D.Line line)
        {
            var p0 = VectorPixelConverter.GetPixelFromVector(line.P0, SelectedLeftImage);
            var p1 = VectorPixelConverter.GetPixelFromVector(line.P1, SelectedLeftImage);
            var index = _2DLeftLineList.IndexOf(_2DLeftLineList.Where(x => ((x.MirroredPoint == p0) && (x.FirstPoint == p1)) || ((x.FirstPoint == p0) && (x.MirroredPoint == p1))).FirstOrDefault());
            if (index < 0)
            {
                index = _2DRightLineList.IndexOf(_2dRightLineList.Where(x => ((x.MirroredPoint == p0) && (x.FirstPoint == p1)) || ((x.FirstPoint == p0) && (x.MirroredPoint == p1))).FirstOrDefault());

                if (index < 0)
                {
                    var vector = VectorPixelConverter.GetPixelFromVector(new Vector3(0), SelectedLeftImage);
                    return new _2DLine(vector, vector, MarkingType.GeneralPruning);
                }

                SelectedTabIndex = 1;
                return _2dRightLineList[index];
            }

            SelectedTabIndex = 0;
            return _2dLeftLineList[index];
        }

        private async void ChangeSelectedImage(object newPath)
        {
            if (newPath is string imagePath && SelectedLeftImage != imagePath)
            {
                IsAnnotationEnabled = false;
                if (_source != null && !_source.IsCancellationRequested)
                {
                    _source.Cancel();
                    await Task.Delay(500);
                    _source.Dispose();
                    _source = null;
                }

                _source = new CancellationTokenSource();

                SelectedLeftImage = imagePath;

                var results = await Task.WhenAll(
                    LoadLinesFromXML(SelectedLeftImage, _source.Token),
                    LoadLinesFromXML(SelectedRightImage, _source.Token));

                IsAnnotationEnabled = true;

                _2DRightLineList = results[1];
                _2DLeftLineList = results[0];

                _source.Dispose();
                _source = null;
            }
        }
        private async Task<string[]> GetFolderFiles()
        {
            return await Task.Run(() =>
            {
                try
                {
                    return Directory.GetFiles($@"{configPath.Value}\Left\Unpruned", "*.JPG").OrderBy(x => x).ToArray();
                }
                catch
                {
                    return new string[] { };
                }
            });
        }

        private IEnumerable<string> _imageGroup;

        public IEnumerable<string> ImageGroup
        {
            get { return _imageGroup; }
            set
            {
                _imageGroup = value;
                NotifyPropertyChanged();
            }
        }

        public int ImageGroupSize { get; set; }
        public int ImageGroupStartingIndex { get; set; }
        private void ChangeImageGroup(bool forward)
        {
            if (forward)
            {
                //ImageGroupStartingIndex = Math.Min(Images.Length - ImageGroupSize, ImageGroupStartingIndex + ImageGroupSize);
                ImageGroupStartingIndex = ImageGroupStartingIndex + ImageGroupSize;
            }
            else
            {
                ImageGroupStartingIndex = Math.Max(0, ImageGroupStartingIndex - ImageGroupSize);
            }

            if (Images != null && Images.Length > 0)
            {
                ImageGroup = Images.Skip(ImageGroupStartingIndex).Take(Math.Min(ImageGroupSize, Images.Length - ImageGroupStartingIndex));

                if (forward)
                {
                    ChangeSelectedImage(ImageGroup.First());
                }
                else
                {
                    ChangeSelectedImage(ImageGroup.Last());
                }
            }
        }

        private void DeleteErrorMessage(object parameter)
        {
            if (parameter is string message)
            {
                ErrorMessages.Remove(message);
            }
        }


        private Task SaveLineToXML(List<_2DLine> newLines, string fullFileName)
        {
            return Task.Run(() =>
              {
                  fullFileName = fullFileName.Replace("JPG", "XML");

                  Lines lines = new Lines()
                  {
                      Line = new List<Line>(newLines.Count)
                  };

                  foreach (_2DLine item in newLines)
                  {
                      lines.Line.Add(new Line
                      {
                          Points = new Points
                          {
                              Point = new List<Model.Point>(3)
                          {
                            new Model.Point(item.CenterPoint.X, item.CenterPoint.Y),
                            new Model.Point(item.FirstPoint.X, item.FirstPoint.Y),
                            new Model.Point(item.MirroredPoint.X, item.MirroredPoint.Y)
                          },
                          },
                          Type = item.Type
                      });
                  }

                  if (!xmlLock.TryEnterWriteLock(500))
                  {
                      return;
                  }
                  FileStream stream = File.Open(fullFileName, FileMode.OpenOrCreate);
                  try
                  {
                      stream.SetLength(0);
                      XmlSerializer serializer = new XmlSerializer(typeof(Lines));
                      serializer.Serialize(stream, lines);
                  }
                  finally
                  {
                      stream.Flush(true);
                      stream.Close();
                      stream.Dispose();
                      xmlLock.ExitWriteLock();
                  }
              });
        }
        private Task<List<_2DLine>> LoadLinesFromXML(string fullFileName, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var list = new List<_2DLine>();
                fullFileName = fullFileName.Replace("JPG", "XML");
                if (!File.Exists(fullFileName))
                {
                    return list;
                }

                FileStream stream = File.OpenRead(fullFileName);
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Lines));

                    if (serializer.Deserialize(stream) is Lines lines)
                    {
                        list = lines.Line.ConvertAll(line =>
                        {
                            var firstPoint = line.Points.Point[0];
                            var secondPoint = line.Points.Point[1];
                            return new _2DLine(new Vector3(firstPoint.X, firstPoint.Y, 0), new Vector3(secondPoint.X, secondPoint.Y, 0), line.Type);
                        });
                    }
                }
                catch (InvalidOperationException)
                {
                    using (File.Open(fullFileName, FileMode.Truncate)) { }
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        ErrorMessages.Add($"Hibás xml formátum. {new FileInfo(fullFileName).Name} felülírása...");
                    });
                    return new List<_2DLine>();
                }
                finally
                {
                    stream.Flush(true);
                    stream.Close();
                    stream.Dispose();
                }

                return list;
            }, cancellationToken);
        }

        private void ShowFiles()
        {
            var directoryInfo = new DirectoryInfo(configPath.Value);
            if (!directoryInfo.Exists || !directoryInfo.Exists)
            {
                ErrorMessages.Add("Fájlok nem találhatók. Újraindítás szükséges");
                return;
            }

            using (System.Diagnostics.Process process = new System.Diagnostics.Process())
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = directoryInfo.FullName
                };
                process.StartInfo = startInfo;
                process.Start();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    xmlLock.Dispose();
                }

                disposedValue = true;
            }
        }
        ~ViewModel2D()
        {
            Dispose(disposing: false);
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
