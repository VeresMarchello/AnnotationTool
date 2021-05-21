using Media3D = System.Windows.Media.Media3D;
using SharpDX;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Windows.Input;
using System.IO;
using AnnotationTool.Commands;
using System.Windows.Media.Imaging;
using HelixToolkit.Wpf.SharpDX;
using System;
using System.Windows.Controls;

namespace AnnotationTool.ViewModel
{
    class ViewModel2D : INotifyPropertyChanged
    {
        private EffectsManager _effectsManager;
        private Camera _camera;

        private MeshGeometry3D _plane;
        private PhongMaterial _planeMaterial;
        private Media3D.Transform3D _planeTransform;

        //private LineGeometry3D _grid;
        //private Color _gridColor;
        //private Media3D.Transform3D _gridTransform;

        private Media3D.Vector3D _directionalLightDirection;
        private Color4 _directionalLightColor;
        private Color4 _ambientLightColor;

        private string[] _images;
        private bool _isLoading;

        private TextureModel _picture;

        public TextureModel Picture
        {
            get { return _picture; }
            set
            {
                _picture = value;
                NotifyPropertyChanged();
            }
        }

        public Color LineColor { get; set; } = new Color(new Vector3(255, 0, 255));

        private Point _mouse;

        public Point Mouse
        {
            get { return _mouse; }
            set
            {
                _mouse = value;
                NotifyPropertyChanged();
            }
        }

        public Vector3 FirstPoint { get; set; }


        public ViewModel2D()
        {
            _effectsManager = new DefaultEffectsManager();
            _camera = new PerspectiveCamera
            {
                Position = new Media3D.Point3D(0, 0, 5),
                LookDirection = new Media3D.Vector3D(0, 0, -5),
                UpDirection = new Media3D.Vector3D(0, 1, 0),
                NearPlaneDistance = 0.5,
                FarPlaneDistance = 150
            };

            var box = new MeshBuilder();
            box.AddBox(new Vector3(0, 0, 0), 10, 10, 0, BoxFaces.PositiveZ);
            _plane = box.ToMeshGeometry3D();
            _planeMaterial = PhongMaterials.Blue;
            _planeTransform = new Media3D.TranslateTransform3D(0, 0, 0);

            _lines = new LineGeometry3D()
            {
                Positions = new Vector3Collection(),
                Indices = new IntCollection(),
                Colors = new Color4Collection()
            };
            //var lineBuilder = new LineBuilder();
            //lineBuilder.AddLine(new Vector3(0, 0, 0), new Vector3(1, 0, 0));
            //Lines = lineBuilder.ToLineGeometry3D();
            //_grid = LineBuilder.GenerateGrid(Vector3.UnitZ, -5, 5, -5, 5);
            //_gridColor = Color.Black;
            //_gridTransform = new Media3D.TranslateTransform3D(0, 0, 0);

            _directionalLightDirection = new Media3D.Vector3D(-0, -0, -10);
            _directionalLightColor = Color.White;
            _ambientLightColor = new Color4(0f, 0f, 0f, 0f);

            IsLoading = true;
            _images = GetFolderFiles();
            IsLoading = false;

            SelectImageCommand = new RelayCommand<object>(ChangeSelectedImage);
        }


        public EffectsManager EffectsManager
        {
            get { return _effectsManager; }
            set
            {
                _effectsManager = value;
                NotifyPropertyChanged();
            }
        }
        public Camera Camera
        {
            get { return _camera; }
            set
            {
                _camera = value;
                NotifyPropertyChanged();
            }
        }

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
        public Media3D.Transform3D PlaneTransform
        {
            get { return _planeTransform; }
            set
            {
                _planeTransform = value;
                NotifyPropertyChanged();
            }
        }

        //public LineGeometry3D Grid
        //{
        //    get { return _grid; }
        //    set
        //    {
        //        _grid = value;
        //        NotifyPropertyChanged();
        //    }
        //}
        //public Color GridColor
        //{
        //    get { return _gridColor; }
        //    set
        //    {
        //        _gridColor = value;
        //        NotifyPropertyChanged();
        //    }
        //}
        //public Media3D.Transform3D GridTransform
        //{
        //    get { return _gridTransform; }
        //    set
        //    {
        //        _gridTransform = value;
        //        NotifyPropertyChanged();
        //    }
        //}

        public double LineThickness => 3;
        private LineGeometry3D _lines;
        public LineGeometry3D Lines
        {
            get { return _lines; }
            set
            {
                _lines = value;
                NotifyPropertyChanged();
            }
        }

        public Media3D.Vector3D DirectionalLightDirection
        {
            get { return _directionalLightDirection; }
            set
            {
                _directionalLightDirection = value;
                NotifyPropertyChanged();
            }
        }
        public Color4 DirectionalLightColor
        {
            get { return _directionalLightColor; }
            set
            {
                _directionalLightColor = value;
                NotifyPropertyChanged();
            }
        }
        public Color4 AmbientLightColor
        {
            get { return _ambientLightColor; }
            set
            {
                _ambientLightColor = value;
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

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsFirstPoint { get; set; } = true;
        public ICommand SelectImageCommand { get; private set; }

        //private Element3D _target;
        //public Element3D Target
        //{
        //    get { return _target; }
        //    set
        //    {
        //        _target = value;
        //        NotifyPropertyChanged();
        //    }
        //}

        //private Vector3 _centerOffset;
        //public Vector3 CenterOffset
        //{
        //    get { return _centerOffset; }
        //    set
        //    {
        //        _centerOffset = value;
        //        NotifyPropertyChanged();
        //    }
        //}

        private void ChangeSelectedImage(object newPath)
        {
            var image = new BitmapImage(new Uri((string)newPath, UriKind.RelativeOrAbsolute));
            SetImage(image);
        }
        private void SetImage(BitmapSource image)
        {
            var ratio = image.PixelWidth / (double)image.PixelHeight;
            var transform = Media3D.Transform3D.Identity;
            transform = transform.AppendTransform(new Media3D.ScaleTransform3D(ratio, 1.0, 1.0));

            PlaneTransform = transform;
            //GridTransform = transform;

            Picture = new MemoryStream(image.ToByteArray());
            //var white = new PhongMaterial()
            //{
            //    DiffuseColor = Color.White,
            //    AmbientColor = Color.Black,
            //    ReflectiveColor = Color.Black,
            //    EmissiveColor = Color.Black,
            //    SpecularColor = Color.Black,
            //    DiffuseMap = new MemoryStream(image.ToByteArray()),
            //};

            //PlaneMaterial = white;
        }

        public void OnMouseDown3DHandler(object sender, MouseDown3DEventArgs e)
        {
            var pressedMouseButton = (e.OriginalInputEventArgs as MouseButtonEventArgs).ChangedButton;

            if (e.HitTestResult == null || pressedMouseButton != MouseButton.Left)
            {
                return;
            }
            var vector = e.HitTestResult.PointHit;

            if (IsFirstPoint)
            {
                FirstPoint = vector;
            }
            else
            {
                if (!(vector.X == FirstPoint.X && vector.Y == FirstPoint.Y))
                {
                    var lineBuilder = new LineBuilder();
                    foreach (var item in Lines.Lines)
                    {
                        lineBuilder.AddLine(item.P0, item.P1);
                    }

                    lineBuilder.AddLine(FirstPoint, vector);
                    //Lines.Positions.Add(FirstPoint);
                    //Lines.Positions.Add(vector);
                    //Lines.Indices.Add(Lines.Indices.Count);
                    //Lines.Indices.Add(Lines.Indices.Count);
                    //NotifyPropertyChanged("Lines");
                    Lines = lineBuilder.ToLineGeometry3D();
                }
            }
            IsFirstPoint = !IsFirstPoint;

            //if (e.HitTestResult.ModelHit is MeshGeometryModel3D m)
            //{
            //    Target = null;
            //    CenterOffset = m.Geometry.Bound.Center; // Must update this before updating target
            //    Target = e.HitTestResult.ModelHit as Element3D;

            //}
        }


        private string[] GetFolderFiles()
        {
            string[] files = null;
            //using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            //{
            //    System.Windows.Forms.DialogResult result = fbd.ShowDialog();

            //    if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            //    {
            //        files = Directory.GetFiles(fbd.SelectedPath);
            files = Directory.GetFiles("../../Images");
            //    }
            //}

            return files;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string info = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
