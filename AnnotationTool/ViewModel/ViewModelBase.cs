﻿using Media3D = System.Windows.Media.Media3D;
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
using System.Windows.Forms;
using System.IO;

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

        //public string AppPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        public ViewModelBase()
        {
            ResetCamera();

            _markingType = MarkingType.GeneralPruning;
            MarkingTypes = Enum.GetValues(typeof(MarkingType)).Cast<MarkingType>().Where(x => x != MarkingType.None);

            SelectTypeCommand = new RelayCommand<object>(SetMarkingType);
            KeyCommand = new RelayCommand<object>(SetMarkingType);
            CTRLRCommand = new RelayCommand<object>(ResetCamera);
            //ChangePathCommand = new RelayCommand<object>(SetPath);
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
        public ICommand ChangePathCommand { get; set; }


        public static Color4 GetColor(MarkingType markingType)
        {
            return colorMarkingTypePairs.FirstOrDefault(x => x.Value == markingType).Key;
        }
        public static MarkingType GetMarkingType(Color4 color)
        {
            return colorMarkingTypePairs[color];
        }
        protected void SetCameraTarget(Vector3 target)
        {
            Camera.Position = new Media3D.Point3D(target.X, target.Y, Camera.Position.Z);
            Camera.LookDirection = new Media3D.Vector3D(0, 0, -Camera.Position.Z);

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
        //private void SetPath(object parameter) 
        //{
        //    using (var fbd = new FolderBrowserDialog())
        //    {
        //        fbd.RootFolder = Environment.SpecialFolder.Desktop;
        //        DialogResult result = fbd.ShowDialog();

        //        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
        //        {
        //            AppPath = fbd.SelectedPath;
        //        }
        //    }
        //}

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
