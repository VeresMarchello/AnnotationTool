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
        private string _selectedPrunedImage;

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
            CTRLRigtClickCommand = new RelayCommand<object>(DeleteLine);
            ESCCommand = new RelayCommand<object>(CancelLine);
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

        public ICommand LeftClickCommand { get; set; }
        public ICommand CTRLLeftClickCommand { get; set; }
        public ICommand CTRLRigtClickCommand { get; set; }
        public ICommand ESCCommand { get; set; }


        private void SetPlane(BitmapSource image)
        {
            if (image == null)
            {
                return;
            }

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
        private void SelectLine(object parameter)
        {
            var vector = GetVector(parameter);
            if (vector == new Vector3(1000))
            {
                return;
            }

            SelectedLine = GetNearestLine(vector);
        }
        private void CancelLine(object parameter)
        {
            if (!IsFirstPoint)
            {
                IsFirstPoint = true;
                ResetNewLine();
            }
        }
        private void DeleteLine(object parameter)
        {
            var vector = GetVector(parameter);
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
            DependencyProperty.Register("MarkingType", typeof(MarkingType), typeof(ViewPort));

        public static readonly DependencyProperty SelectedLineProperty =
            DependencyProperty.Register("SelectedLine", typeof(Geometry3D.Line), typeof(ViewPort),
                new FrameworkPropertyMetadata(new Geometry3D.Line(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        private static void SelectedUnprunedImagePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var viewPort = (ViewPort)obj;
            var image = new BitmapImage(new Uri(e.NewValue.ToString(), UriKind.RelativeOrAbsolute));
            viewPort.SetPlane(image);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string info = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
