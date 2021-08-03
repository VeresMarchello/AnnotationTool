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

        private static readonly Dictionary<Color4, MarkingType> colorMarkingTypePairs =
            new Dictionary<Color4, MarkingType> {
                { Media.Color.FromArgb(255, 255, 0, 255).ToColor4(), MarkingType.GeneralPruning },
                { Media.Color.FromArgb(255, 0, 255, 0).ToColor4(), MarkingType.UncertainPruning },
                { Media.Color.FromArgb(255, 0, 0, 255).ToColor4(), MarkingType.PruningFromStems }
            };
        //private static readonly Dictionary<MarkingType, string> markingTypeNamePairs = new Dictionary<MarkingType, string>();


        public ViewModelBase()
        {
            _camera = new PerspectiveCamera
            {
                Position = new Media3D.Point3D(0, 0, 10),
                LookDirection = new Media3D.Vector3D(0, 0, -10),
                UpDirection = new Media3D.Vector3D(0, 1, 0),
                NearPlaneDistance = 0,
                FarPlaneDistance = 1500,
            };

            _markingType = MarkingType.GeneralPruning;
            //var types = Enum.GetValues(typeof(MarkingType)).Cast<MarkingType>().Where(x => (int)x > 0);
            //foreach (var type in types)
            //{
            //    markingTypeNamePairs.Add(type, GetMarkingTypeName(type));
            //}
            MarkingTypes = Enum.GetValues(typeof(MarkingType)).Cast<MarkingType>().Where(x => x != MarkingType.None);

            SelectTypeCommand = new RelayCommand<object>(SetMarkingType);
            KeyCommand = new RelayCommand<object>(SetMarkingType);
            CTRLRCommand = new RelayCommand<object>(ResetCamera);
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

        public ICommand SelectTypeCommand { get; set; }
        public ICommand KeyCommand { get; set; }
        public ICommand CTRLRCommand { get; set; }


        public static Color4 GetColor(MarkingType markingType)
        {
            return colorMarkingTypePairs.FirstOrDefault(x => x.Value == markingType).Key;
        }
        public static MarkingType GetMarkingType(Color4 color)
        {
            return colorMarkingTypePairs[color];
        }
        //public static MarkingType GetMarkingType(string typeName)
        //{
        //    return markingTypeNamePairs.First(x => x.Value == typeName).Key;
        //}
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
            //if (parameter is string)
            //{
            //    MarkingType = GetMarkingType((string)parameter);
            //}
            //else if (parameter is int)
            //{
            //    MarkingType = markingTypeNamePairs.First(x => (int)x.Key == (int)parameter).Key;
            //}
            MarkingType = (MarkingType)Enum.Parse(typeof(MarkingType), parameter.ToString());
        }

        public static string GetMarkingTypeName(MarkingType type)
        {
            var name = "";

            switch (type)
            {
                case MarkingType.GeneralPruning:
                    name = "Általános metszés";
                    break;
                case MarkingType.UncertainPruning:
                    name = "Bizonytalan metszés";
                    break;
                case MarkingType.PruningFromStems:
                    name = "Tőből metszés";
                    break;
            }

            return name;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string info = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
