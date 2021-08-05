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

namespace AnnotationTool.ViewModel
{
    public class ViewModel2D : ViewModelBase, IDisposable
    {
        private string _selectedLeftImage;
        private string _selectedRightImage;
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


        public ViewModel2D()
        {
            _images = GetFolderFiles();
            if (_images.Count() > 0)
            {
                ChangeSelectedImage(_images[0]);
            }
            _2dLeftLineList = new List<_2DLine>();
            _2dRightLineList = new List<_2DLine>();
            _selected2dLine = new _2DLine(new Vector3(0), new Vector3(0), MarkingType.GeneralPruning);
            _source = new CancellationTokenSource();
            _errorMessages = new ObservableCollection<string>() { };
            _isAnnotationEnabled = true;
            SelectImageCommand = new RelayCommand<object>(ChangeSelectedImage);
            DeleteErrorMessageCommand = new RelayCommand<object>(DeleteErrorMessage);
        }


        public string SelectedLeftImage
        {
            get { return _selectedLeftImage; }
            set
            {
                _selectedLeftImage = value;
                NotifyPropertyChanged();

                SelectedRightImage = value.Replace("Left", "Right");
            }
        }
        public string SelectedRightImage
        {
            get { return _selectedRightImage; }
            set
            {
                _selectedRightImage = value;
                NotifyPropertyChanged();
            }
        }
        public string[] Images
        {
            get { return _images; }
            set
            {
                _images = value;
                NotifyPropertyChanged();
            }
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
                    var target = GetVectorFromPixel(value.CenterPoint);
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


        private LineGeometry3D GetLineGeometry(List<_2DLine> _2DLines)
        {
            var lineBuilder = new LineBuilder();

            foreach (var line in _2DLines)
            {
                var v1 = GetVectorFromPixel(line.FirstPoint);
                var v2 = GetVectorFromPixel(line.MirroredPoint);

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
                var newLine = new _2DLine(GetPixelFromVector(center), GetPixelFromVector(lineGeometry.Positions[i]), GetMarkingType(lineGeometry.Colors[i]));
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
                P0 = GetVectorFromPixel(line.FirstPoint),
                P1 = GetVectorFromPixel(line.MirroredPoint),
            };
        }
        private _2DLine Get_2DLine(Geometry3D.Line line)
        {
            var index = _2DLeftLineList.IndexOf(_2DLeftLineList.Where(x => ((x.MirroredPoint == GetPixelFromVector(line.P0)) && (x.FirstPoint == GetPixelFromVector(line.P1))) || ((x.FirstPoint == GetPixelFromVector(line.P0)) && (x.MirroredPoint == GetPixelFromVector(line.P1)))).FirstOrDefault());
            if (index < 0)
            {
                index = _2DRightLineList.IndexOf(_2dRightLineList.Where(x => ((x.MirroredPoint == GetPixelFromVector(line.P0)) && (x.FirstPoint == GetPixelFromVector(line.P1))) || ((x.FirstPoint == GetPixelFromVector(line.P0)) && (x.MirroredPoint == GetPixelFromVector(line.P1)))).FirstOrDefault());

                if (index < 0)
                {
                    return new _2DLine(new Vector3(0), new Vector3(0), MarkingType.GeneralPruning);
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

                _2DLeftLineList = results[0];
                _2DRightLineList = results[1];

                _source.Dispose();
                _source = null;

                ResetCamera();
            }
        }
        private string[] GetFolderFiles()
        {
            try
            {
                return Directory.GetFiles($@"{AppDomain.CurrentDomain.BaseDirectory}Images\Left\Unpruned", "*.JPG");
            }
            catch
            {
                ErrorMessages.Add("Képek betöltése sikertelen. Ellenőrizze a fájlokat. Újraindítás szükséges.");

                return new string[] { };
            }
        }
        private void DeleteErrorMessage(object parameter)
        {
            if (parameter is string message)
            {
                ErrorMessages.Remove(message);
            }
        }

        private Vector3 GetVectorFromPixel(Vector3 vector)
        {
            var image = new BitmapImage(new Uri(SelectedLeftImage, UriKind.RelativeOrAbsolute));
            int imageWidth = image.PixelWidth;
            int imageHeight = image.PixelHeight;

            double vertical = 5.0;
            double horizontal = imageWidth / (imageHeight / vertical);
            Vector2 center = new Vector2(imageWidth / 2, imageHeight / 2);
            double computedX = Math.Abs(vector.X - center.X);
            double computedY = Math.Abs(vector.Y - center.Y);

            double computedPointX;
            if (vector.X >= center.X)
                computedPointX = computedX / (center.X / vertical);
            else
                computedPointX = -computedX / (center.X / vertical);

            double computedPointY;
            if (vector.Y >= center.Y)
                computedPointY = -computedY / (center.Y / horizontal);
            else
                computedPointY = computedY / (center.Y / horizontal);

            return new Vector3((float)computedPointX, (float)computedPointY, 0);
        }
        private Vector3 GetPixelFromVector(Vector3 vector)
        {
            var image = new BitmapImage(new Uri(SelectedLeftImage, UriKind.RelativeOrAbsolute));
            int imageWidth = image.PixelWidth;
            int imageHeight = image.PixelHeight;

            double vertical = 5.0;
            double horizontal = imageWidth / (imageHeight / vertical);
            Vector2 center = new Vector2(imageWidth / 2, imageHeight / 2);
            Vector3 computedPoint = new Vector3(0);

            double computedX = Math.Abs(center.X / vertical * vector.X);
            if (vector.X >= 0)
                computedPoint.X = Convert.ToInt32(center.X + computedX);
            else
                computedPoint.X = Convert.ToInt32(center.X - computedX);

            double computedY = Math.Abs(center.Y / horizontal * vector.Y);
            if (vector.Y >= 0)
                computedPoint.Y = Convert.ToInt32(center.Y - computedY);
            else
                computedPoint.Y = Convert.ToInt32(center.Y + computedY);

            return computedPoint;
        }
        private void SaveLineToXML(List<_2DLine> newLines, string fullFileName)
        {
            _ = Task.Run(() =>
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
