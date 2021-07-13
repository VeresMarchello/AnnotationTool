using Media3D = System.Windows.Media.Media3D;
using Media = System.Windows.Media;
using SharpDX;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using HelixToolkit.Wpf.SharpDX;
using System.Windows.Input;
using AnnotationTool.Model;
using System;
using AnnotationTool.Commands;
using System.Linq;
using System.Collections.Generic;

namespace AnnotationTool.ViewModel
{
    class ViewModelBase : INotifyPropertyChanged
    {
        private Camera _camera;

        private MarkingType _markingType;


        public ViewModelBase()
        {
            EffectsManager = new DefaultEffectsManager();
            ResetCamera();

            DirectionalLightDirection = new Media3D.Vector3D(-0, -0, -10);
            DirectionalLightColor = Media.Colors.White;
            AmbientLightColor = Media.Colors.Black;

            _markingType = MarkingType.GeneralPruning;
            MarkingTypes = Enum.GetValues(typeof(MarkingType)).Cast<MarkingType>();

            _selectedTabIndex = 1;

            var material = PhongMaterials.Red;
            material.DiffuseColor = GetColor(MarkingType.GeneralPruning).ToColor4();
            LineMaterial = material;

            SelectTypeCommand = new RelayCommand<object>(SetMarkingType);
            CTRLRCommand = new RelayCommand<object>(ResetCamera);
            KeyCommand = new RelayCommand<object>(SetMarkingType);
            ChangeTabindexCommand = new RelayCommand<object>(ChangeTabIndex);
        }


        private int _selectedTabIndex;

        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set 
            { 
                _selectedTabIndex = value;
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
        public EffectsManager EffectsManager { get; private set; }

        public Media3D.Vector3D DirectionalLightDirection { get; private set; }
        public Media.Color DirectionalLightColor { get; private set; }
        public Media.Color AmbientLightColor { get; private set; }

        public PhongMaterial LineMaterial { get; protected set; }

        public MarkingType MarkingType
        {
            get { return _markingType; }
            set
            {
                _markingType = value;
                NotifyPropertyChanged();
            }
        }
        public IEnumerable<MarkingType> MarkingTypes { get; set; }

        public ICommand LeftClickCommand { get; set; }
        public ICommand CTRLLeftClickCommand { get; set; }
        public ICommand CTRLRigtClickCommand { get; set; }
        public ICommand SelectTypeCommand { get; set; }
        public ICommand ESCCommand { get; set; }
        public ICommand KeyCommand { get; set; }
        public ICommand CTRLRCommand { get; set; }
        public ICommand ChangeTabindexCommand { get; set; }

        private void ChangeTabIndex(object parameter) 
        {
            int index;
            if (int.TryParse(parameter.ToString(), out index))
            {
                SelectedTabIndex = index;
            }
        }
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
        protected void SetCameraTarget(Vector3 target, double offset = 0)
        {
            if (offset == 0)
            {
                Camera.Position = new Media3D.Point3D(target.X, target.Y, Camera.Position.Z);
                Camera.LookDirection = new Media3D.Vector3D(0, 0, -Camera.Position.Z);
            }
            else
            {
                Camera.Position = new Media3D.Point3D(target.X + offset, target.Y + offset, target.Z + offset);
                Camera.LookDirection = new Media3D.Vector3D(-offset, -offset, -offset);
            }

            NotifyPropertyChanged("Camera");
        }
        protected void ResetCamera(object parameter = null)
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
        private void SetMarkingType(object parameter)
        {
            MarkingType = (MarkingType)Enum.Parse(typeof(MarkingType), parameter.ToString());
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string info = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
