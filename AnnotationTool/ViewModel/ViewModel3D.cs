using Media3D = System.Windows.Media.Media3D;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using HelixToolkit.Wpf.SharpDX.Assimp;
using System.Threading.Tasks;
using System.Windows.Media;
using HelixToolkit.Wpf.SharpDX.Model.Scene;
using HelixToolkit.Wpf.SharpDX.Model;
using AnnotationTool.Commands;
using System.Collections.Generic;

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

        public ViewModel3D()
        {
            ResetCamera();
            ResetAxixModel();

            GroupModel = new SceneNodeGroupModel3D();
            _planes = new MeshGeometry3D() { Positions = new Vector3Collection() };

            IsLoading = true;
            ImportSTL("Madi_Cloud_mesh_center");

            LeftClickCommand = new RelayCommand<object>(AddPlane);
        }

        private void AddPlane(object parameter)
        {
            var vector = GetVector(parameter);

            var meshBuilder = new MeshBuilder();
            for (int i = 0; i < Planes.Positions.Count; i+=4)
            {
                meshBuilder.AddBox(Planes.Positions[i], 0.02, 0.02, 0, BoxFaces.PositiveZ);
            }
            meshBuilder.AddBox(vector, 0.02, 0.02, 0, BoxFaces.PositiveZ);

            Planes = meshBuilder.ToMeshGeometry3D();
            NotifyPropertyChanged("Vectors");
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
                        //if (scene.Root != null)
                        //{
                        //    foreach (var node in scene.Root.Traverse())
                        //    {
                        //        if (node is MaterialGeometryNode m)
                        //        {
                        //            if (m.Material is PBRMaterialCore pbr)
                        //            {
                        //                pbr.RenderEnvironmentMap = true;
                        //            }
                        //            else if (m.Material is PhongMaterialCore phong)
                        //            {
                        //                phong.RenderEnvironmentMap = true;
                        //            }
                        //        }
                        //    }
                        //}
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

        private void ResetCamera()
        {
            Camera = new OrthographicCamera()
            {
                Position = new Media3D.Point3D(10, 10, 10),
                LookDirection = new Media3D.Vector3D(-10, -10, -10),
                UpDirection = new Media3D.Vector3D(0, 1, 0),
                FarPlaneDistance = 100000,
                NearPlaneDistance = 0.01f
            };
        }
    }
}
