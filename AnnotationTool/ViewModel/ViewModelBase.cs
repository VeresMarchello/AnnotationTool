using Media3D = System.Windows.Media.Media3D;
using Media = System.Windows.Media;
using SharpDX;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using HelixToolkit.Wpf.SharpDX;
using System.Windows.Input;

namespace AnnotationTool.ViewModel
{
    class ViewModelBase : INotifyPropertyChanged
    {
        private EffectsManager _effectsManager;
        private Camera _camera;

        private Media3D.Vector3D _directionalLightDirection;
        private Media.Color _directionalLightColor;
        private Media.Color _ambientLightColor;

        private bool _isLoading;


        public ViewModelBase()
        {
            _effectsManager = new DefaultEffectsManager();
            _camera = new PerspectiveCamera
            {
                Position = new Media3D.Point3D(0, 0, 10),
                LookDirection = new Media3D.Vector3D(0, 0, -5),
                UpDirection = new Media3D.Vector3D(0, 1, 0),
                NearPlaneDistance = 0.5,
                FarPlaneDistance = 150,
            };

            _directionalLightDirection = new Media3D.Vector3D(-0, -0, -10);
            _directionalLightColor = Media.Colors.White;
            _ambientLightColor = Media.Colors.Black;

            PurpleColor = Media.Color.FromArgb(255, 255, 0, 255);
            PurpleBrush = new Media.SolidColorBrush(PurpleColor);
            var material = PhongMaterials.Red;
            material.DiffuseColor = PurpleColor.ToColor4();
            PurpleMaterial = material;
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

        public Media3D.Vector3D DirectionalLightDirection
        {
            get { return _directionalLightDirection; }
            set
            {
                _directionalLightDirection = value;
                NotifyPropertyChanged();
            }
        }
        public Media.Color DirectionalLightColor
        {
            get { return _directionalLightColor; }
            set
            {
                _directionalLightColor = value;
                NotifyPropertyChanged();
            }
        }
        public Media.Color AmbientLightColor
        {
            get { return _ambientLightColor; }
            set
            {
                _ambientLightColor = value;
                NotifyPropertyChanged();
            }
        }

        public PhongMaterial PurpleMaterial { get; private set; }
        public Media.Color PurpleColor { get; private set; }
        public Media.Brush PurpleBrush { get; private set; }
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand LeftClickCommand { get; protected set; }


        public Vector3 GetVector(object parameter)
        {
            var viewPort = (Viewport3DX)parameter;
            var point = viewPort.FindNearestPoint(Mouse.GetPosition(viewPort));
            var vector = new Vector3();

            if (point.HasValue)
            {
                vector = point.Value.ToVector3();
            }

            return vector;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string info = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
