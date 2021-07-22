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

        public ViewPort()
        {
            InitializeComponent();
            DataContext = this;
            var box = new MeshBuilder();
            box.AddBox(new Vector3(0, 0, 0), 10, 10, 0, BoxFaces.PositiveZ);
            _plane = box.ToMeshGeometry3D();
            _unprunedPlaneMaterial = PhongMaterials.Blue;
            _planeTransform = new TranslateTransform3D(0, 0, 0);

            EffectsManager = new DefaultEffectsManager();

            DirectionalLightDirection = new Vector3D(-0, -0, -10);
            DirectionalLightColor = Colors.White;
            AmbientLightColor = Colors.Black;

            ResetNewLine();

            LeftClickCommand = new RelayCommand<object>(AddLine);
            CTRLLeftClickCommand = new RelayCommand<object>(SelectLine);
            //CTRLRigtClickCommand = new RelayCommand<object>(DeleteLine);
            //ESCCommand = new RelayCommand<object>(CancelLine);
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

                var material = value.CloneMaterial();
                var prunedImage = new BitmapImage(new Uri(SelectedUnprunedImage.Replace("Unpruned", "Pruned"), UriKind.RelativeOrAbsolute));
                material.DiffuseMap = new MemoryStream(prunedImage.ToByteArray());
                PrunedPlaneMaterial = material;

                IsFirstPoint = true;
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

        private LineGeometry3D _newLine;

        //private _2DLine _selected2dLine;
        //public _2DLine Selected2dLine
        //{
        //    get { return _selected2dLine; }
        //    set
        //    {
        //        _selected2dLine = value;
        //        NotifyPropertyChanged();

        //        if (value != null)
        //        {
        //            var target = GetVectorFromPixel(value.CenterPoint);
        //            SetCameraTarget(target);
        //        }
        //    }
        //}
        public LineGeometry3D NewLine
        {
            get { return _newLine; }
            set
            {
                _newLine = value; 
                NotifyPropertyChanged();
            }
        }

        //private LineGeometry3D _lines;

        //public LineGeometry3D Lines
        //{
        //    get { return _lines; }
        //    set 
        //    { 
        //        _lines = value;
        //        NotifyPropertyChanged();
        //    }
        //}


        private string _selectedPrunedImage;

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

        public ICommand LeftClickCommand { get; set; }
        public ICommand CTRLLeftClickCommand { get; set; }

        private void SetImage(BitmapSource image)
        {
            var ratio = image.PixelWidth / (double)image.PixelHeight;
            var transform = Transform3D.Identity;
            transform = transform.AppendTransform(new ScaleTransform3D(ratio, 1.0, 1.0));

            PlaneTransform = transform;
            var material = new PhongMaterial()
            {
                DiffuseColor = Color4.White,
                AmbientColor = Color4.Black,
                ReflectiveColor = Color4.Black,
                EmissiveColor = Color4.Black,
                SpecularColor = Color4.Black,
                DiffuseMap = new MemoryStream(image.ToByteArray()),
            };

            UnprunedPlaneMaterial = material;
        }


        public Vector3 GetVector(object parameter)
        {
            var viewPort = (Viewport3DX)parameter;
            var position = Mouse.GetPosition(viewPort);
            var hits = viewPort.FindHits(position);
            
            foreach (var hit in hits)
            {
                if (hit.ModelHit is MeshGeometryModel3D)
                {
                    return hit.PointHit;
                }
            }

            return new Vector3(1000);
        }
        private Vector3 GetPixelFromVector(Vector3 vector)
        {
            var image = new BitmapImage(new Uri(SelectedUnprunedImage, UriKind.RelativeOrAbsolute));
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
        private Vector3 GetVectorFromPixel(Vector3 vector)
        {
            var image = new BitmapImage(new Uri(SelectedUnprunedImage, UriKind.RelativeOrAbsolute));
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
        private bool LineIntersect(Vector3 p01, Vector3 p11, Vector3 p02, Vector3 p12)
        {
            var dx = p11.X - p01.X;
            var dy = p11.Y - p01.Y;
            var da = p12.X - p02.X;
            var db = p12.Y - p02.Y;

            if (da * dy - db * dx == 0)
            {
                // The segments are parallel.
                return false;
            }

            var s = (dx * (p02.Y - p01.Y) + dy * (p01.X - p02.X)) / (da * dy - db * dx);
            var t = (da * (p01.Y - p02.Y) + db * (p02.X - p01.X)) / (db * dx - da * dy);

            if ((s >= 0) & (s <= 1) & (t >= 0) & (t <= 1))
                return true;
            else
                return false;
        }
        private bool IsNewLineValid(_2DLine line)
        {
            bool newLineIsValid = true;

            var mirroredPoint = GetVectorFromPixel(line.MirroredPoint);
            var selectedPoint = GetVectorFromPixel(line.FirstPoint);
            for (int i = 0; i < Lines.Positions.Count - 1; i += 2)
            {
                newLineIsValid = !LineIntersect(mirroredPoint, selectedPoint, Lines.Positions[i], Lines.Positions[i + 1]);

                if (!newLineIsValid)
                {
                    break;
                }
            }

            return newLineIsValid;
        }


        private void ResetLines()
        {
            Lines = new LineGeometry3D()
            {
                Positions = new Vector3Collection(),
                Indices = new IntCollection(),
                Colors = new Color4Collection()
            };
        }

        private void ResetNewLine()
        {
            NewLine = new LineGeometry3D()
            {
                Positions = new Vector3Collection(),
                Indices = new IntCollection(),
                Colors = new Color4Collection()
            };
        }
        private void AddLine(object parameter)
        {
            var vector = GetVector(parameter);
            if (vector == new Vector3(1000))
            {
                return;
            }
            vector.Z = 0;
            if (IsFirstPoint)
            {
                FirstPoint = vector;
                SetCameraTarget(vector);
            }
            else
            {
                var newLine = new _2DLine(GetPixelFromVector(FirstPoint), GetPixelFromVector(vector), MarkingType);

                if (IsNewLineValid(newLine))
                {
                    //_2DLineList = _2DLineList.Append(newLine).ToList();

                    Lines.Positions.Add(GetVectorFromPixel(newLine.MirroredPoint));
                    Lines.Positions.Add(GetVectorFromPixel(newLine.FirstPoint));
                    Lines.Indices.Add(Lines.Indices.Count);
                    Lines.Indices.Add(Lines.Indices.Count);
                    Lines.Colors.Add(ViewModelBase.GetColor(MarkingType).ToColor4());
                    Lines.Colors.Add(ViewModelBase.GetColor(MarkingType).ToColor4());

                    Lines = new LineGeometry3D()
                    {
                        Positions = Lines.Positions,
                        Indices = Lines.Indices,
                        Colors = Lines.Colors
                    };
                    //SaveLineToXML(newLine);

                    ResetNewLine();
                }
                else
                {
                    return;
                }
            }

            IsFirstPoint = !IsFirstPoint;
        }
        private void SelectLine(object parameter)
        {
            var vector = GetVector(parameter);
            if (vector == new Vector3(1000))
            {
                return;
            }

            var lines = Lines.Lines.ToList();
            var index = lines.IndexOf(GetNearestLine(vector));

            if (index > -1)
            {
                var target = new Vector3((lines[index].P0.X + lines[index].P1.X)/2, (lines[index].P0.Y + lines[index].P1.Y) / 2, 0);
                SetCameraTarget(target);
            }
        }
        private MeshGeometry3D.Line GetNearestLine(Vector3 vector)
        {
            Dictionary<MeshGeometry3D.Line, float> lineDistancePairs = new Dictionary<MeshGeometry3D.Line, float>();
            foreach (var line in Lines.Lines)
            {
                var dxc = vector.X - line.P0.X;
                var dyc = vector.Y - line.P0.Y;
                var dxl = line.P1.X - line.P0.X;
                var dyl = line.P1.Y - line.P0.Y;
                var cross = dxc * dyl - dyc * dxl;

                lineDistancePairs.Add(line, Math.Abs(cross));
            }

            if (lineDistancePairs.Count < 1)
            {
                return new MeshGeometry3D.Line();
            }

            return lineDistancePairs.OrderBy(x => x.Value).First().Key;
        }

        protected void SetCameraTarget(Vector3 target, double offset = 0)
        {
            if (offset == 0)
            {
                Camera.Position = new Point3D(target.X, target.Y, Camera.Position.Z);
                Camera.LookDirection = new Vector3D(0, 0, -Camera.Position.Z);
            }
            else
            {
                Camera.Position = new Point3D(target.X + offset, target.Y + offset, target.Z + offset);
                Camera.LookDirection = new Vector3D(-offset, -offset, -offset);
            }

            Camera = Camera;
        }

        public void MouseMove3DHandler(object sender, MouseMove3DEventArgs e)
        {
            if (IsFirstPoint)
            {
                return;
            }

            var vector = GetVector(sender);

            if (vector == new Vector3(1000))
            {
                return;
            }
            vector.Z = 0;

            var newLine = new _2DLine(GetPixelFromVector(FirstPoint), GetPixelFromVector(vector), MarkingType);

            if (IsNewLineValid(newLine))
            {
                var lineBuilder = new LineBuilder();
                lineBuilder.AddLine(GetVectorFromPixel(newLine.MirroredPoint), GetVectorFromPixel(newLine.FirstPoint));
                lineBuilder.AddCircle(GetVectorFromPixel(newLine.CenterPoint), new Vector3(0, 0, 1), 0.04f, 360);
                var lineGeometry = lineBuilder.ToLineGeometry3D();
                lineGeometry.Colors = new Color4Collection();

                for (int i = 0; i < lineGeometry.Positions.Count; i++)
                {
                    lineGeometry.Colors.Add(ViewModelBase.GetColor(MarkingType).ToColor4());
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

        public static readonly DependencyProperty CameraProperty =
            DependencyProperty.Register("Camera", typeof(Camera), typeof(ViewPort));

        public static readonly DependencyProperty SelectedUnprunedImageProperty =
            DependencyProperty.Register("SelectedUnprunedImage", typeof(string), typeof(ViewPort), new PropertyMetadata(SelectedUnprunedImagePropertyChanged));

        public static readonly DependencyProperty LinesProperty =
            DependencyProperty.Register("Lines", typeof(LineGeometry3D), typeof(ViewPort));

        public static readonly DependencyProperty MarkingTypeProperty =
            DependencyProperty.Register("MarkingType", typeof(MarkingType), typeof(ViewPort));


        private static void SelectedUnprunedImagePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = (ViewPort)obj;
            var image = new BitmapImage(new Uri(e.NewValue.ToString(), UriKind.RelativeOrAbsolute));

            control.SetImage(image);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string info = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
