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

namespace AnnotationTool.View
{
    /// <summary>
    /// Interaction logic for ViewPort.xaml
    /// </summary>
    public partial class ViewPort : UserControl, INotifyPropertyChanged
    {
        private MeshGeometry3D _plane;
        private PhongMaterial _planeMaterial;
        private Transform3D _planeTransform;

        //private LineGeometry3D _lines;
        //private _2DLine _selected2dLine;
        //private LineGeometry3D _newLine;


        public ViewPort()
        {
            InitializeComponent();
            DataContext = this;

            var box = new MeshBuilder();
            box.AddBox(new Vector3(0, 0, 0), 10, 10, 0, BoxFaces.PositiveZ);
            _plane = box.ToMeshGeometry3D();
            _planeMaterial = PhongMaterials.Blue;
            _planeTransform = new TranslateTransform3D(0, 0, 0);

            EffectsManager = new DefaultEffectsManager();

            DirectionalLightDirection = new Vector3D(-0, -0, -10);
            DirectionalLightColor = Colors.White;
            AmbientLightColor = Colors.Black;
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
        public PhongMaterial PlaneMaterial
        {
            get { return _planeMaterial; }
            set
            {
                _planeMaterial = value;
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

            PlaneMaterial = material;
        }

        //public Vector3 GetVector(object parameter)
        //{
        //    var viewPort = (Viewport3DX)parameter;
        //    var position = Mouse.GetPosition(viewPort);
        //    var asd = viewPort.FindHits(position);
        //    foreach (var item in asd)
        //    {
        //        if (item.ModelHit is MeshGeometryModel3D)
        //        {
        //            return item.PointHit;
        //        }
        //    }

        //    return new Vector3(1000);
        //}
        //public static Color GetColor(MarkingType markingType)
        //{
        //    var color = new Color();

        //    switch (markingType)
        //    {
        //        case MarkingType.GeneralPruning:
        //            color = Color.FromArgb(255, 255, 0, 255);
        //            break;
        //        case MarkingType.UncertainPruning:
        //            color = Color.FromArgb(255, 0, 255, 0);
        //            break;
        //        case MarkingType.PruningFromStems:
        //            color = Color.FromArgb(255, 0, 0, 255);
        //            break;
        //    }

        //    return color;
        //}
        //protected void SetCameraTarget(Vector3 target, double offset = 0)
        //{
        //    if (offset == 0)
        //    {
        //        Camera.Position = new Point3D(target.X, target.Y, Camera.Position.Z);
        //        Camera.LookDirection = new Vector3D(0, 0, -Camera.Position.Z);
        //    }
        //    else
        //    {
        //        Camera.Position = new Point3D(target.X + offset, target.Y + offset, target.Z + offset);
        //        Camera.LookDirection = new Vector3D(-offset, -offset, -offset);
        //    }

        //    NotifyPropertyChanged("Camera");
        //}
        //protected void ResetCamera(object parameter = null)
        //{
        //    Camera = new HelixToolkit.Wpf.SharpDX.PerspectiveCamera
        //    {
        //        Position = new Point3D(0, 0, 10),
        //        LookDirection = new Vector3D(0, 0, -10),
        //        UpDirection = new Vector3D(0, 1, 0),
        //        NearPlaneDistance = 0,
        //        FarPlaneDistance = 1500,
        //    };
        //}

        public Camera Camera
        {
            get { return (Camera)GetValue(CameraProperty); }
            set { SetValue(CameraProperty, value); }
        }
        public string SelectedImage
        {
            get { return (string)GetValue(SelectedImageProperty); }
            set { SetValue(SelectedImageProperty, value); }
        }

        
        public static readonly DependencyProperty CameraProperty =
            DependencyProperty.Register("Camera", typeof(Camera), typeof(ViewPort));

        public static readonly DependencyProperty SelectedImageProperty =
            DependencyProperty.Register("SelectedImage", typeof(string), typeof(ViewPort), new PropertyMetadata(SelectedImagePropertyChanged));



        public LineGeometry3D Lines
        {
            get { return (LineGeometry3D)GetValue(LinesProperty); }
            set { SetValue(LinesProperty, value); }
        }

        public static readonly DependencyProperty LinesProperty =
            DependencyProperty.Register("Lines", typeof(LineGeometry3D), typeof(ViewPort));




        private static void SelectedImagePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
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
