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
using System.Xml;
using System.Collections.Generic;
using System.Linq;

namespace AnnotationTool.ViewModel
{
    class ViewModelBase : INotifyPropertyChanged
    {
        private EffectsManager _effectsManager;
        private Camera _camera;

        private Media3D.Vector3D _directionalLightDirection;
        private Color4 _directionalLightColor;
        private Color4 _ambientLightColor;

        private MeshGeometry3D _plane;
        private PhongMaterial _planeMaterial;
        private Media3D.Transform3D _planeTransform;

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

            var box = new MeshBuilder();
            box.AddBox(new Vector3(0, 0, 0), 10, 10, 0, BoxFaces.PositiveZ);
            _plane = box.ToMeshGeometry3D();
            _planeMaterial = PhongMaterials.Blue;
            _planeTransform = new Media3D.TranslateTransform3D(0, 0, 0);

            _directionalLightDirection = new Media3D.Vector3D(-0, -0, -10);
            _directionalLightColor = Color.White;
            _ambientLightColor = new Color4(0f, 0f, 0f, 0f);
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

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                NotifyPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string info = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
