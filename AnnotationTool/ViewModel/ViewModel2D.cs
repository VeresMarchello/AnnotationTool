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
    class ViewModel2D : ViewModelBase
    {
        private LineGeometry3D _lines;

        private string _selectedImage;
        private string[] _images;

        //remove later
        private TextureModel _picture;

        //remove later
        public TextureModel Picture
        {
            get { return _picture; }
            set
            {
                _picture = value;
                NotifyPropertyChanged();
            }
        }


        public ViewModel2D()
        {
            _lines = new LineGeometry3D()
            {
                Positions = new Vector3Collection(),
                Indices = new IntCollection(),
                Colors = new Color4Collection()
            };

            IsFirstPoint = true;

            IsLoading = true;
            _images = GetFolderFiles();
            IsLoading = false;
            if (Images.Length >= 1)
                _selectedImage = Images[0];

            var image = new BitmapImage(new Uri(SelectedImage, UriKind.RelativeOrAbsolute));
            SetImage(image);

            SelectImageCommand = new RelayCommand<object>(ChangeSelectedImage);
        }

        public double LineThickness => 3;
        public LineGeometry3D Lines
        {
            get { return _lines; }
            set
            {
                _lines = value;
                NotifyPropertyChanged();
            }
        }
        public Color LineColor { get; set; } = new Color(new Vector3(255, 0, 255));
        public Vector3 FirstPoint { get; set; }

        public string SelectedImage
        {
            get { return _selectedImage; }
            set
            {
                _selectedImage = value;
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
        
        public bool IsFirstPoint { get; set; }

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
            SelectedImage = (string)newPath;

            var image = new BitmapImage(new Uri(SelectedImage, UriKind.RelativeOrAbsolute));
            SetImage(image);

            LoadPoints(SelectedImage);
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

        public void MouseDown3DHandler(object sender, MouseDown3DEventArgs e)
        {
            if (e.HitTestResult == null)
            {
                return;
            }
            var originalEvent = e.OriginalInputEventArgs as MouseButtonEventArgs;
            var pressedMouseButton = originalEvent.ChangedButton;

            var vector = e.HitTestResult.PointHit;
            vector.Z = 0;

            if (pressedMouseButton == MouseButton.Left)
            {
                if (IsFirstPoint)
                {
                    FirstPoint = vector;
                }
                else
                {
                    //if (!(vector.X == FirstPoint.X && vector.Y == FirstPoint.Y))
                    //{
                    var lineBuilder = new LineBuilder();
                    foreach (var item in Lines.Lines)
                    {
                        lineBuilder.AddLine(item.P0, item.P1);
                    }

                    lineBuilder.AddLine(FirstPoint, vector);
                    Console.WriteLine();

                    //Lines.Positions.Add(FirstPoint);
                    //Lines.Positions.Add(vector);
                    //Lines.Indices.Add(Lines.Indices.Count);
                    //Lines.Indices.Add(Lines.Indices.Count);
                    //NotifyPropertyChanged("Lines");
                    Lines = lineBuilder.ToLineGeometry3D();
                    SavePoints(Lines.Lines.Last());
                    //}
                }
                IsFirstPoint = !IsFirstPoint;
            }
            else if (pressedMouseButton == MouseButton.Right)
            {
                var hitTests = e.Viewport.FindHits(originalEvent.GetPosition(e.Viewport));
                Vector3 hitPt;
                if (hitTests != null && hitTests.Count > 0)
                {
                    foreach (var hit in hitTests)
                    {
                        var line = hit.ModelHit as LineGeometryModel3D;

                        if (line != null)
                        {
                            hitPt = hit.PointHit;
                            break;
                        }
                    }
                }
            }
            else if (pressedMouseButton == MouseButton.Middle)
            {
                if (e.HitTestResult.ModelHit is LineGeometryModel3D)
                {
                    Console.WriteLine(vector.X + "\t" + vector.Y);
                    Dictionary<Geometry3D.Line, float> keyValues = new Dictionary<Geometry3D.Line, float>();
                    foreach (var line in Lines.Lines)
                    {
                        var dxc = vector.X - line.P0.X;
                        var dyc = vector.Y - line.P0.Y;
                        var dxl = line.P1.X - line.P0.X;
                        var dyl = line.P1.Y - line.P0.Y;
                        var cross = dxc * dyl - dyc * dxl;

                        keyValues.Add(line, Math.Abs(cross));
                    }

                    var clickedLine = keyValues.OrderBy(x => x.Value).First().Key;
                    var target = new Vector3((clickedLine.P1.X + clickedLine.P0.X) / 2, (clickedLine.P1.Y + clickedLine.P0.Y) / 2, 0);
                    Camera.Position = new Media3D.Point3D(target.X, target.Y, 5);
                    Camera.LookDirection = new Media3D.Vector3D(0, 0, -5);
                    NotifyPropertyChanged("Camera");
                }
            }
            //if (e.HitTestResult.ModelHit is MeshGeometryModel3D m)
            //{
            //    Target = null;
            //    CenterOffset = m.Geometry.Bound.Center; // Must update this before updating target
            //    Target = e.HitTestResult.ModelHit as Element3D;
            //}
        }

        public void SavePoints(Geometry3D.Line newLine)
        {
            XmlDocument document = new XmlDocument();
            XmlElement linesElement;
            IEnumerable<Geometry3D.Line> lines;
            try
            {
                document.Load(SelectedImage.Replace("JPG", "xml"));
                linesElement = document.GetElementsByTagName("Lines")[0] as XmlElement;
                lines = new List<Geometry3D.Line>() { newLine };
            }
            catch (Exception)
            {
                XmlDeclaration xmlDeclaration = document.CreateXmlDeclaration("1.0", "UTF-8", null);
                XmlElement root = document.DocumentElement;
                document.InsertBefore(xmlDeclaration, root);

                linesElement = document.CreateElement(string.Empty, "Lines", string.Empty);
                document.AppendChild(linesElement);
                lines = Lines.Lines;
            }

            foreach (var line in lines)
            {
                XmlElement lineElement = document.CreateElement(string.Empty, "Line", string.Empty);
                linesElement.AppendChild(lineElement);

                XmlElement pointsElement = document.CreateElement(string.Empty, "Points", string.Empty);
                lineElement.AppendChild(pointsElement);

                Vector3[] points = new Vector3[] { line.P0, line.P1 };

                foreach (var point in points)
                {
                    XmlElement pointElement = document.CreateElement(string.Empty, "Point", string.Empty);
                    pointsElement.AppendChild(pointElement);

                    XmlText xText = document.CreateTextNode(point.X.ToString());
                    XmlText yText = document.CreateTextNode(point.Y.ToString());

                    XmlElement xElement = document.CreateElement(string.Empty, "X", string.Empty);
                    pointElement.AppendChild(xElement);
                    xElement.AppendChild(xText);

                    XmlElement yElement = document.CreateElement(string.Empty, "Y", string.Empty);
                    pointElement.AppendChild(yElement);
                    yElement.AppendChild(yText);
                }
            }

            document.Save(SelectedImage.Replace("JPG", "xml"));
        }

        public void LoadPoints(string fullFileName)
        {
            XmlDocument document = new XmlDocument();

            try
            {
                document.Load(fullFileName.Replace("JPG", "xml"));

                XmlNodeList xCoordinates = document.GetElementsByTagName("X");
                XmlNodeList yCoordinates = document.GetElementsByTagName("Y");

                List<Vector3> poVectors = new List<Vector3>();
                List<Vector3> p1Vectors = new List<Vector3>();

                for (int i = 0; i < xCoordinates.Count; i++)
                {
                    float x = float.Parse(xCoordinates[i].InnerText);
                    float y = float.Parse(yCoordinates[i].InnerText);
                    Vector3 vector = new Vector3(x, y, 0);

                    if (i % 2 == 0)
                    {
                        poVectors.Add(vector);
                    }
                    else
                    {
                        p1Vectors.Add(vector);
                    }
                }

                var lineBuilder = new LineBuilder();

                for (int i = 0; i < poVectors.Count; i++)
                {
                    lineBuilder.AddLine(poVectors[i], p1Vectors[i]);
                }

                Lines = lineBuilder.ToLineGeometry3D();
            }
            catch (Exception)
            {
                Lines = new LineGeometry3D()
                {
                    Positions = new Vector3Collection(),
                    Indices = new IntCollection(),
                    Colors = new Color4Collection()
                };
            }
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
            files = Directory.GetFiles("../../Images", "*.JPG");
            //    }
            //}

            return files;
        }
    }
}
