using HelixToolkit.Wpf.SharpDX;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Media3D;
using System.Windows;
using SharpDX;
using Camera = HelixToolkit.Wpf.SharpDX.Camera;
using Color = System.Windows.Media.Color;
using AnnotationTool.Model;
using System.Windows.Input;
using System.Windows.Controls;
using MeshGeometry3D = HelixToolkit.Wpf.SharpDX.MeshGeometry3D;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.IO;
using System;
using AnnotationTool.Commands;
using AnnotationTool.ViewModel;
using System.Linq;
using Geometry3D = HelixToolkit.Wpf.SharpDX.Geometry3D;
using AnnotationTool.Utils;
using System.Threading.Tasks;

namespace AnnotationTool.View
{
    /// <summary>
    /// Interaction logic for ViewPort.xaml
    /// </summary>
    public partial class ViewPort : UserControl, INotifyPropertyChanged
    {
        private MeshGeometry3D _plane;
        private PhongMaterial _unprunedPlaneMaterial;
        private PhongMaterial _prunedPlaneMaterial;
        private Transform3D _planeTransform;
        private LineGeometry3D _newLine;
        private string _selectedPrunedImage = null;

        private ScaleTransform _scaleTransform;
        private TranslateTransform _translateTransform;
        private TransformGroup _transformGroup;

        public ViewPort()
        {
            InitializeComponent();
            DataContext = this;

            var box = new MeshBuilder();
            box.AddBox(new Vector3(0, 0, 0), 10, 10, 0, BoxFaces.PositiveZ);
            _plane = box.ToMeshGeometry3D();
            _unprunedPlaneMaterial = PhongMaterials.White;
            _planeTransform = new TranslateTransform3D(0, 0, 0);

            EffectsManager = new DefaultEffectsManager();

            DirectionalLightDirection = new Vector3D(-0, -0, -10);
            DirectionalLightColor = Colors.White;
            AmbientLightColor = Colors.Black;

            ResetNewLine();

            LeftClickCommand = new RelayCommand<object>(AddLine);
            CTRLLeftClickCommand = new RelayCommand<object>(SelectLine);
            CTRLRigtClickCommand = new RelayCommand<object>(DeleteLine);
            ESCCommand = new RelayCommand<object>(CancelLine);


            _translateTransform = new TranslateTransform();
            _scaleTransform = new ScaleTransform();
            _transformGroup = new TransformGroup();

            _transformGroup.Children.Add(_scaleTransform);
            _transformGroup.Children.Add(_translateTransform);
        }


        public TransformGroup TransformGroup
        {
            get { return _transformGroup; }
            set { _transformGroup = value; }
        }


        public EffectsManager EffectsManager { get; private set; }
        public Vector3D DirectionalLightDirection { get; private set; }
        public Color DirectionalLightColor { get; private set; }
        public Color AmbientLightColor { get; private set; }

        public MeshGeometry3D Plane
        {
            get { return _plane; }
            set
            {
                _plane = value;
                NotifyPropertyChanged();
            }
        }
        public PhongMaterial UnprunedPlaneMaterial
        {
            get { return _unprunedPlaneMaterial; }
            set
            {
                _unprunedPlaneMaterial = value;
                NotifyPropertyChanged();

                IsFirstPoint = true;
                if (string.IsNullOrEmpty(SelectedUnprunedImage) || !File.Exists(SelectedUnprunedImage))
                {
                    return;
                }

                //var fileInfo = new FileInfo(viewPort.SelectedUnprunedImage);
                //var prunedDirectoryInfo = new FileInfo(viewPort.SelectedUnprunedImage.Replace("Unpruned", "Pruned")).Directory;
                //var list = fileInfo.Directory.EnumerateFiles("*.JPG").Select(x => x.FullName).ToList();
                //var index = 0;
                //if ((int)e.NewValue <= 0)
                //{
                //    index = Math.Max(0, list.IndexOf(fileInfo.FullName) + viewPort.Delta);
                //}
                //else
                //{
                //    index = Math.Min(prunedDirectoryInfo.GetFiles("*.JPG").Length - 1, list.IndexOf(fileInfo.FullName) + viewPort.Delta);
                //}

                var fileInfo = new FileInfo(SelectedUnprunedImage);
                var prunedDirectoryInfo = new FileInfo(SelectedUnprunedImage.Replace("Unpruned", "Pruned")).Directory;
                var list = fileInfo.Directory.EnumerateFiles("*.JPG").Select(x => x.FullName).ToList();
                var index = list.IndexOf(fileInfo.FullName);
                var prunedImages = prunedDirectoryInfo.GetFiles("*.JPG");
                if (index + Delta < 0)
                {
                    Delta = -index;
                }
                else if (index + Delta > prunedImages.Length - 1)
                {
                    Delta = prunedImages.Length - index - 1;
                }

                SelectedPrunedImage = prunedImages[index + Delta].FullName;
                PrunedPlaneMaterial = SetPlane(CreateImage(SelectedPrunedImage));
            }
        }
        public PhongMaterial PrunedPlaneMaterial
        {
            get { return _prunedPlaneMaterial; }
            set
            {
                _prunedPlaneMaterial = value;
                NotifyPropertyChanged();
            }
        }
        public Transform3D PlaneTransform
        {
            get { return _planeTransform; }
            set
            {
                _planeTransform = value;
                NotifyPropertyChanged();
            }
        }
        public LineGeometry3D NewLine
        {
            get { return _newLine; }
            set
            {
                _newLine = value;
                NotifyPropertyChanged();
            }
        }
        public string SelectedPrunedImage
        {
            get { return _selectedPrunedImage; }
            set
            {
                _selectedPrunedImage = value;
                NotifyPropertyChanged();
            }
        }

        public Vector3 FirstPoint { get; set; }
        public bool IsFirstPoint { get; set; }

        public Vector2 MouseInfo { get; set; }

        public ICommand LeftClickCommand { get; set; }
        public ICommand CTRLLeftClickCommand { get; set; }
        public ICommand CTRLRigtClickCommand { get; set; }
        public ICommand ESCCommand { get; set; }


        private PhongMaterial SetPlane(BitmapSource image)
        {
            if (image == null)
            {
                return new PhongMaterial();
            }

            var ratio = image.PixelWidth / (double)image.PixelHeight;
            var transform = Transform3D.Identity;
            transform = transform.AppendTransform(new ScaleTransform3D(ratio, 1.0, 1.0));

            PlaneTransform = transform;
            return new PhongMaterial()
            {
                DiffuseColor = Color4.White,
                AmbientColor = Color4.Black,
                ReflectiveColor = Color4.Black,
                EmissiveColor = Color4.Black,
                SpecularColor = Color4.Black,
                DiffuseMap = new MemoryStream(image.ToByteArray()),
            };
        }
        public async Task<Vector3> GetVector(object parameter)
        {
            return await Task.Run(() =>
            {
                var viewPort = (Viewport3DX)parameter;
                System.Windows.Point position = new System.Windows.Point();
                this.Dispatcher.Invoke(() => position = Mouse.GetPosition(viewPort));
                var hits = viewPort.FindHits(position);

                foreach (var hit in hits)
                {
                    if (hit.ModelHit is MeshGeometryModel3D)
                    {
                        return hit.PointHit;
                    }
                }

                return new Vector3(1000);
            });
        }
        private Geometry3D.Line GetNearestLine(Vector3 vector)
        {
            Dictionary<Geometry3D.Line, double> lineDistancePairs = new Dictionary<Geometry3D.Line, double>();
            foreach (var line in Lines.Lines.Distinct())
            {
                var lineLength = (float)(Math.Pow(line.P0.X - line.P1.X, 2) + Math.Pow(line.P0.Y - line.P1.Y, 2));
                var t = ((vector.X - line.P0.X) * (line.P1.X - line.P0.X) + (vector.Y - line.P0.Y) * (line.P1.Y - line.P0.Y)) / lineLength;
                t = Math.Max(0, Math.Min(1, t));
                var vector2 = new Vector3(line.P0.X + t * (line.P1.X - line.P0.X), line.P0.Y + t * (line.P1.Y - line.P0.Y), 0);
                lineDistancePairs.Add(line, Math.Sqrt((float)(Math.Pow(vector.X - vector2.X, 2) + Math.Pow(vector.Y - vector2.Y, 2))));
            }

            if (lineDistancePairs.Count < 1)
            {
                return new Geometry3D.Line();
            }

            return lineDistancePairs.OrderBy(x => x.Value).First().Key;
        }
        private bool IsNewLineValid(Vector3 p0, Vector3 p1)
        {
            bool newLineIsValid = true;

            for (int i = 0; i < Lines.Positions.Count - 1; i += 2)
            {
                newLineIsValid = !LineIntersect(p0, p1, Lines.Positions[i], Lines.Positions[i + 1]);

                if (!newLineIsValid)
                {
                    break;
                }
            }

            return newLineIsValid;
        }
        private bool LineIntersect(Vector3 p01, Vector3 p11, Vector3 p02, Vector3 p12)
        {
            var dx = p11.X - p01.X;
            var dy = p11.Y - p01.Y;
            var da = p12.X - p02.X;
            var db = p12.Y - p02.Y;

            if (da * dy - db * dx == 0)
            {
                return false;
            }

            var s = (dx * (p02.Y - p01.Y) + dy * (p01.X - p02.X)) / (da * dy - db * dx);
            var t = (da * (p01.Y - p02.Y) + db * (p02.X - p01.X)) / (db * dx - da * dy);

            if ((s >= 0) & (s <= 1) & (t >= 0) & (t <= 1))
                return true;
            else
                return false;
        }

        private async void AddLine(object parameter)
        {
            var vector = await GetVector(parameter);
            if (vector == new Vector3(1000))
            {
                return;
            }
            vector.Z = 0;

            if (IsFirstPoint)
            {
                FirstPoint = vector;

                var lineBuilder = new LineBuilder();
                lineBuilder.AddCircle(FirstPoint, new Vector3(0, 0, 1), 0.04f, 360);
                var lineGeometry = lineBuilder.ToLineGeometry3D();
                lineGeometry.Colors = new Color4Collection();

                for (int i = 0; i < lineGeometry.Positions.Count; i++)
                {
                    lineGeometry.Colors.Add(ViewModelBase.GetColor(MarkingType));
                }

                NewLine = lineGeometry;
            }
            else
            {
                if (!IsNewLineValid(FirstPoint + (FirstPoint - vector), vector))
                {
                    return;
                }

                Lines.Positions.Add(FirstPoint + (FirstPoint - vector));
                Lines.Positions.Add(vector);
                Lines.Indices.Add(Lines.Indices.Count);
                Lines.Indices.Add(Lines.Indices.Count);
                Lines.Colors.Add(ViewModelBase.GetColor(MarkingType));
                Lines.Colors.Add(ViewModelBase.GetColor(MarkingType));

                Lines = new LineGeometry3D()
                {
                    Positions = Lines.Positions,
                    Indices = Lines.Indices,
                    Colors = Lines.Colors
                };

                ResetNewLine();
            }

            IsFirstPoint = !IsFirstPoint;
        }
        private async void SelectLine(object parameter)
        {
            var vector = await GetVector(parameter);
            if (vector == new Vector3(1000))
            {
                return;
            }
            ResetNewLine();
            SelectedLine = GetNearestLine(vector);
        }
        private void CancelLine(object parameter)
        {
            if (!IsFirstPoint)
            {
                ResetNewLine();
            }
        }
        private async void DeleteLine(object parameter)
        {
            var vector = await GetVector(parameter);
            if (vector == new Vector3(1000))
            {
                return;
            }

            var nearest = GetNearestLine(vector);
            var remainingPositions = Lines.Positions;
            var index = remainingPositions.IndexOf(nearest.P0);

            if (index > -1)
            {
                var remainingIndices = Lines.Indices;
                var remainingColors = Lines.Colors;

                remainingPositions.RemoveRange(index, 2);
                remainingIndices.RemoveRange(remainingIndices.Count - 2, 2);
                remainingColors.RemoveRange(index, 2);

                Lines = new LineGeometry3D()
                {
                    Positions = remainingPositions,
                    Indices = remainingIndices,
                    Colors = remainingColors
                };

                ResetNewLine();
            }
        }
        private void ResetNewLine()
        {
            NewLine = new LineGeometry3D()
            {
                Positions = new Vector3Collection(),
                Indices = new IntCollection(),
                Colors = new Color4Collection()
            };

            IsFirstPoint = true;
        }
        public async void MouseMove3DHandler(object sender, MouseMove3DEventArgs e)
        {
            //var image = (Image)sender;
            //ImageSource imageSource = image.Source;
            //BitmapSource bitmapImage = (BitmapSource)imageSource;
            //var x = Convert.ToInt32(e.GetPosition(image).X * bitmapImage.PixelWidth / image.ActualWidth);
            //var y = Convert.ToInt32(e.GetPosition(image).Y * bitmapImage.PixelHeight / image.ActualHeight);
            //MouseInfo = new System.Windows.Point(x, y);
            //NotifyPropertyChanged("MouseInfo");
            var vector = await GetVector(sender);

            if (vector == new Vector3(1000))
            {
                return;
            }

            var pixel = VectorPixelConverter.GetPixelFromVector(vector, SelectedUnprunedImage);
            MouseInfo = new Vector2(pixel.X, pixel.Y);
            NotifyPropertyChanged("MouseInfo");

            if (IsFirstPoint)
            {
                return;
            }

            vector.Z = 0;

            if (IsNewLineValid(FirstPoint + (FirstPoint - vector), vector))
            {
                var lineBuilder = new LineBuilder();
                lineBuilder.AddLine(FirstPoint + (FirstPoint - vector), vector);
                lineBuilder.AddCircle(FirstPoint, new Vector3(0, 0, 1), 0.04f, 360);
                var lineGeometry = lineBuilder.ToLineGeometry3D();
                lineGeometry.Colors = new Color4Collection();

                for (int i = 0; i < lineGeometry.Positions.Count; i++)
                {
                    lineGeometry.Colors.Add(ViewModelBase.GetColor(MarkingType));
                }

                NewLine = lineGeometry;
            }
        }


        public Camera Camera
        {
            get { return (Camera)GetValue(CameraProperty); }
            set { SetValue(CameraProperty, value); }
        }
        public string SelectedUnprunedImage
        {
            get { return (string)GetValue(SelectedUnprunedImageProperty); }
            set { SetValue(SelectedUnprunedImageProperty, value); }
        }
        public LineGeometry3D Lines
        {
            get { return (LineGeometry3D)GetValue(LinesProperty); }
            set { SetValue(LinesProperty, value); }
        }
        public MarkingType MarkingType
        {
            get { return (MarkingType)GetValue(MarkingTypeProperty); }
            set { SetValue(MarkingTypeProperty, value); }
        }
        public Geometry3D.Line SelectedLine
        {
            get { return (Geometry3D.Line)GetValue(SelectedLineProperty); }
            set { SetValue(SelectedLineProperty, value); }
        }
        public int Delta
        {
            get { return (int)GetValue(DeltaProperty); }
            set { SetValue(DeltaProperty, value); }
        }


        public static readonly DependencyProperty CameraProperty =
            DependencyProperty.Register("Camera", typeof(Camera), typeof(ViewPort),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty SelectedUnprunedImageProperty =
            DependencyProperty.Register("SelectedUnprunedImage", typeof(string), typeof(ViewPort),
        new PropertyMetadata(SelectedUnprunedImagePropertyChanged));

        public static readonly DependencyProperty LinesProperty =
            DependencyProperty.Register("Lines", typeof(LineGeometry3D), typeof(ViewPort),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty MarkingTypeProperty =
            DependencyProperty.Register("MarkingType", typeof(MarkingType), typeof(ViewPort),
                new PropertyMetadata(MarkingTypePropertyChanged));

        public static readonly DependencyProperty SelectedLineProperty =
            DependencyProperty.Register("SelectedLine", typeof(Geometry3D.Line), typeof(ViewPort),
                new FrameworkPropertyMetadata(new Geometry3D.Line(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty DeltaProperty =
            DependencyProperty.Register("Delta", typeof(int), typeof(ViewPort),
                new PropertyMetadata(DeltaPropertyChanged));



        private static void SelectedUnprunedImagePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                var viewPort = (ViewPort)obj;
                viewPort.ResetNewLine();
                viewPort.UnprunedPlaneMaterial = viewPort.SetPlane(CreateImage(e.NewValue.ToString()));
            }
        }

        private static BitmapImage CreateImage(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            image.CreateOptions = BitmapCreateOptions.None;
            image.CacheOption = BitmapCacheOption.Default;
            image.DecodePixelWidth = 2000;
            image.EndInit();

            return image;
        }
        private static void MarkingTypePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var viewPort = (ViewPort)obj;
            if (viewPort.NewLine.Positions.Count < 1)
            {
                return;
            }

            var newMarkingType = (MarkingType)e.NewValue;
            var colors = new Color4Collection();

            for (int i = 0; i < viewPort.NewLine.Positions.Count; i++)
            {
                colors.Add(ViewModelBase.GetColor(newMarkingType));
            }

            viewPort.NewLine = new LineGeometry3D()
            {
                Positions = viewPort.NewLine.Positions,
                Indices = viewPort.NewLine.Indices,
                Colors = colors
            };
        }
        private static void DeltaPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var viewPort = (ViewPort)obj;
            if (string.IsNullOrEmpty(viewPort.SelectedUnprunedImage))
            {
                return;
            }
            var fileInfo = new FileInfo(viewPort.SelectedUnprunedImage);
            var prunedDirectoryInfo = new FileInfo(viewPort.SelectedUnprunedImage.Replace("Unpruned", "Pruned")).Directory;
            var list = fileInfo.Directory.EnumerateFiles("*.JPG").Select(x => x.FullName).ToList();
            var index = 0;
            if ((int)e.NewValue <= 0)
            {
                index = Math.Max(0, list.IndexOf(fileInfo.FullName) + viewPort.Delta);
            }
            else
            {
                index = Math.Min(prunedDirectoryInfo.GetFiles("*.JPG").Length - 1, list.IndexOf(fileInfo.FullName) + viewPort.Delta);
            }
            try
            {
                viewPort.SelectedPrunedImage = prunedDirectoryInfo.GetFiles("*.JPG")[index].FullName;
                viewPort.PrunedPlaneMaterial = viewPort.SetPlane(CreateImage(viewPort.SelectedPrunedImage));
            }
            catch
            {
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string info = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {

            var st = _scaleTransform;
            double zoom = e.Delta > 0 ? .2 : -.2;
            st.ScaleX += zoom;
            st.ScaleY += zoom;
            var tg = new TransformGroup();
            tg.Children.Add(st);
            tg.Children.Add(_translateTransform);

            TransformGroup = tg;

            //double zoomNow = Math.Round(TransformGroup.Value.M11, 1);
            //double zoomScale = 0.1;
            //double valZoom = e.Delta > 0 ? zoomScale : -zoomScale;
            ////RenderTransformOrigin = new System.Windows.Point(mousePosition.X / MainCanvas.ActualWidth, mousePosition.Y / MainCanvas.ActualHeight);
            //Zoom(new System.Windows.Point(mousePosition.X, mousePosition.Y), zoomNow + valZoom);

            //void Zoom(System.Windows.Point point, double scale)
            //{
            //    double centerX = (point.X - _translateTransform.X) / _scaleTransform.ScaleX;
            //    double centerY = (point.Y - _translateTransform.Y) / _scaleTransform.ScaleY;

            //    _scaleTransform.ScaleX = scale;
            //    _scaleTransform.ScaleY = scale;

            //    _translateTransform.X = point.X - centerX * _scaleTransform.ScaleX;
            //    _translateTransform.Y = point.Y - centerY * _scaleTransform.ScaleY;


            //    var tg = new TransformGroup();
            //    tg.Children.Add(_scaleTransform);
            //    tg.Children.Add(_translateTransform);

            //    TransformGroup = tg;
            //    NotifyPropertyChanged("TransformGroup");
            //}
        }
    }
}
