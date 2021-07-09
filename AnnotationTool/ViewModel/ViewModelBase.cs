using Media3D = System.Windows.Media.Media3D;
using Media = System.Windows.Media;
using SharpDX;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using HelixToolkit.Wpf.SharpDX;
using System.Windows.Input;
using AnnotationTool.Model;
using System;

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
            ResetCamera(null);

            _directionalLightDirection = new Media3D.Vector3D(-0, -0, -10);
            _directionalLightColor = Media.Colors.White;
            _ambientLightColor = Media.Colors.Black;

            var material = PhongMaterials.Red;
            material.DiffuseColor = GetColor(MarkingType.GeneralPruning).ToColor4();
            LineMaterial = material;
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

        public PhongMaterial LineMaterial { get; private set; }

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
            var position = Mouse.GetPosition(viewPort);
            var asd = viewPort.FindHits(position);
            foreach (var item in asd)
            {
                if (item.ModelHit is MeshGeometryModel3D)
                {
                    return item.PointHit;
                }
            }

            return new Vector3(1000);
        }

        public static Media.Color GetColor(MarkingType markingType)
        {
            var color = new Media.Color();

            switch (markingType)
            {
                case MarkingType.GeneralPruning:
                    color = Media.Color.FromArgb(255, 255, 0, 255);
                    break;
                case MarkingType.UncertainPruning:
                    color = Media.Color.FromArgb(255, 0, 255, 0);
                    break;
                case MarkingType.PruningFromStems:
                    color = Media.Color.FromArgb(255, 0, 0, 255);
                    break;
            }

            return color;
        }
        protected void ResetCamera(object parameter)
        {
            Camera = new PerspectiveCamera
            {
                Position = new Media3D.Point3D(0, 0, 10),
                LookDirection = new Media3D.Vector3D(0, 0, -10),
                UpDirection = new Media3D.Vector3D(0, 1, 0),
                NearPlaneDistance = 0,
                FarPlaneDistance = 1500,
            };
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string info = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
