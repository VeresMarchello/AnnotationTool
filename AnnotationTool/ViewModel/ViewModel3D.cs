using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using System.Windows.Input;
using System.Windows.Media;

namespace AnnotationTool.ViewModel
{
    class ViewModel3D : ViewModelBase
    {
        private MeshGeometry3D _pointsLayer;

        public SceneNodeGroupModel3D GroupModel { get; set; }
        public MeshGeometry3D PointsLayer
        {
            get { return _pointsLayer; }
            set
            {
                _pointsLayer = value;
                NotifyPropertyChanged();
            }
        }
        public BillboardText3D PointsLabel { private set; get; }
        public LineGeometry3D AxisModel { get; private set; }
        public BillboardText3D AxisLabel { get; private set; }

        public Vector3Collection Vectors { get; private set; }

        public ViewModel3D()
        {
            Camera = new OrthographicCamera()
            {
                LookDirection = new System.Windows.Media.Media3D.Vector3D(0, 0, -10),
                Position = new System.Windows.Media.Media3D.Point3D(0, 0, 10),
                UpDirection = new System.Windows.Media.Media3D.Vector3D(0, 1, 0),
                FarPlaneDistance = 100000,
                NearPlaneDistance = 0.01f
            };

            GroupModel = new SceneNodeGroupModel3D();

            var lineBuilder = new LineBuilder();
            lineBuilder.AddLine(new Vector3(0, 0, 0), new Vector3(1000, 0, 0));
            lineBuilder.AddLine(new Vector3(0, 0, 0), new Vector3(0, 1000, 0));
            lineBuilder.AddLine(new Vector3(0, 0, 0), new Vector3(0, 0, 1000));

            AxisModel = lineBuilder.ToLineGeometry3D();
            AxisModel.Colors = new Color4Collection(AxisModel.Positions.Count)
            {
                Colors.Red.ToColor4(),
                Colors.Red.ToColor4(),
                Colors.Green.ToColor4(),
                Colors.Green.ToColor4(),
                Colors.Blue.ToColor4(),
                Colors.Blue.ToColor4()
            };

            AxisLabel = new BillboardText3D();
            AxisLabel.TextInfo.Add(new TextInfo() { Origin = new Vector3(1100, 0, 0), Text = "X", Foreground = Colors.Red.ToColor4() });
            AxisLabel.TextInfo.Add(new TextInfo() { Origin = new Vector3(0, 1100, 0), Text = "Y", Foreground = Colors.Green.ToColor4() });
            AxisLabel.TextInfo.Add(new TextInfo() { Origin = new Vector3(0, 0, 1100), Text = "Z", Foreground = Colors.Blue.ToColor4() });

            PointsLabel = new BillboardText3D();
            _pointsLayer = new MeshGeometry3D() { Positions = new Vector3Collection()};

            Vectors = new Vector3Collection();
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

            PointsLayer = meshBuilder.ToMeshGeometry3D();
            Vectors.Add(vector);
            NotifyPropertyChanged("Vectors");
        }
    }
}
