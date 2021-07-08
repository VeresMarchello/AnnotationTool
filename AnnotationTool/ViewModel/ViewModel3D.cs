﻿using Media3D = System.Windows.Media.Media3D;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using HelixToolkit.Wpf.SharpDX.Assimp;
using System.Threading.Tasks;
using System.Windows.Media;
using HelixToolkit.Wpf.SharpDX.Model.Scene;
using HelixToolkit.Wpf.SharpDX.Model;
using AnnotationTool.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AnnotationTool.Model;
using System.Windows.Input;
using System.Windows.Controls;

namespace AnnotationTool.ViewModel
{
    class ViewModel3D : ViewModelBase
    {
        private MeshGeometry3D _planes;

        public SceneNodeGroupModel3D GroupModel { get; set; }
        public Geometry3D PLY { get; set; }
        public MeshGeometry3D Planes
        {
            get { return _planes; }
            set
            {
                _planes = value;
                NotifyPropertyChanged();
            }
        }
        public LineGeometry3D CoordinateSystem { get; private set; }

        private ObservableCollection<_3DPlane> _positions;

        public ObservableCollection<_3DPlane> Positions
        {
            get { return _positions; }
            set
            {
                _positions = value;
                NotifyPropertyChanged();
            }
        }

        private _3DPlane _selectedPlane;

        public _3DPlane SelectedPlane
        {
            get { return _selectedPlane; }
            set
            {
                _selectedPlane = value;
                NotifyPropertyChanged();

                SetCameraTarget(new Vector3(value.X, value.Y, value.Z));
            }
        }


        public ViewModel3D()
        {
            ResetCamera(null);
            ResetAxixModel();

            GroupModel = new SceneNodeGroupModel3D();
            _planes = new MeshGeometry3D()
            {
                Positions = new Vector3Collection(),
                TriangleIndices = new IntCollection(),
                Normals = new Vector3Collection(),
                Indices = new IntCollection(),
                Colors = new Color4Collection()
            };
            _positions = new ObservableCollection<_3DPlane>();

            IsLoading = true;
            //ImportSTL("Madi_Cloud_mesh_center");
            ImportSTL("toke_scan_photogrammetry");

            LeftClickCommand = new RelayCommand<object>(AddPlane);
        }

        private void AddPlane(object parameter)
        {
            //float size = 0.02f;
            float size = 5f;
            var viewPort = (Viewport3DX)parameter;
            Vector3 vector;
            object model;
            viewPort.FindNearest(Mouse.GetPosition(viewPort).ToVector2(), out vector, out _, out model);

            if (model is MeshNode)
            {
                var meshBuilder = new MeshBuilder();
                meshBuilder.AddBox(vector, size, size, 0, BoxFaces.PositiveZ);

                for (int i = 0; i < Planes.Positions.Count; i += 4)
                {
                    var position1 = Planes.Positions[i];
                    var position2 = Planes.Positions[i + 2];
                    var center = new Vector3((position1.X + position2.X) / 2, (position1.Y + position2.Y) / 2, (position1.Z + position2.Z) / 2);
                    meshBuilder.AddBox(center, size, size, 0, BoxFaces.PositiveZ);
                }

                Planes = meshBuilder.ToMeshGeometry3D();

                ObservableCollection<_3DPlane> points = Positions;
                points.Add(new _3DPlane(vector.X, vector.Y, vector.Z));
                Positions = points;
            }

            return;
        }

        private void ResetAxixModel()
        {
            var lineBuilder = new LineBuilder();
            lineBuilder.AddLine(new Vector3(0, 0, 0), new Vector3(10, 0, 0));
            lineBuilder.AddLine(new Vector3(0, 0, 0), new Vector3(0, 10, 0));
            lineBuilder.AddLine(new Vector3(0, 0, 0), new Vector3(0, 0, 10));

            CoordinateSystem = lineBuilder.ToLineGeometry3D();
            CoordinateSystem.Colors = new Color4Collection(CoordinateSystem.Positions.Count)
            {
                Colors.Red.ToColor4(),
                Colors.Red.ToColor4(),
                Colors.Green.ToColor4(),
                Colors.Green.ToColor4(),
                Colors.Blue.ToColor4(),
                Colors.Blue.ToColor4()
            };
        }

        public void ImportSTL(string fileName)
        {
            Task.Run(() =>
            {
                var loader = new Importer();
                return loader.Load($"../../PLYs/{ fileName }.ply");
            }).ContinueWith((result) =>
            {
                IsLoading = false;
                if (result.IsCompleted)
                {
                    var scene = result.Result;
                    GroupModel.Clear();

                    if (scene != null)
                    {
                        if (scene.Root != null)
                        {
                            foreach (var node in scene.Root.Traverse())
                            {
                                if (node is MaterialGeometryNode m)
                                {
                                    if (m.Material is PBRMaterialCore pbr)
                                    {
                                        pbr.RenderEnvironmentMap = true;
                                    }
                                    else if (m.Material is PhongMaterialCore phong)
                                    {
                                        phong.RenderEnvironmentMap = true;
                                    }
                                }
                            }
                        }
                        GroupModel.AddNode(scene.Root);
                        NotifyPropertyChanged("GroupModel");
                    }
                }

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void MouseDown3DHandler(object sender, MouseDown3DEventArgs e)
        {
            if (e.HitTestResult == null)
            {
                return;
            }

            //var originalEvent = e.OriginalInputEventArgs as MouseButtonEventArgs;
            //var pointHit = e.HitTestResult.PointHit;
            //var nearestPoint = e.Viewport.FindNearestPoint(new System.Windows.Point(pointHit.X, pointHit.Y));

            //var vector = nearestPoint.Value.ToVector3();
            var vector = e.HitTestResult.PointHit;
            var meshBuilder = new MeshBuilder();
            //foreach (var position in PointsLayer.Positions)
            //{
            //    meshBuilder.AddBox(position, 0.02, 0.02, 0, BoxFaces.PositiveZ);
            //}
            meshBuilder.AddBox(vector, 0.02, 0.02, 0, BoxFaces.PositiveZ);

            Planes = meshBuilder.ToMeshGeometry3D();
            NotifyPropertyChanged("Vectors");
        }

        public void SelectionChangedHandler(object sender, SelectionChangedEventArgs e)
        {
            if (e != null && e.AddedItems != null && e.AddedItems.Count >= 1 && e.AddedItems[0] is _3DPlane)
                SelectedPlane = (_3DPlane)e.AddedItems[0];
        }

        private void SetCameraTarget(Vector3 target)
        {
            //var offset = 0.3;
            var offset = 75;
            Camera.Position = new Media3D.Point3D(target.X + offset, target.Y + offset, target.Z + offset);
            Camera.LookDirection = new Media3D.Vector3D(-offset, -offset, -offset);
            NotifyPropertyChanged("Camera");
        }
    }
}
