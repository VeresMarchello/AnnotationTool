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
using System.Windows.Controls;
using System.Windows;

namespace AnnotationTool.ViewModel
{
    class ViewModel2D : ViewModelBase
    {
        private LineGeometry3D _lines;
        private string _selectedImage;
        private string[] _images;
        private Geometry3D.Line _selectedLine;


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

            ChangeSelectedImage(Images[0]);

            SelectImageCommand = new RelayCommand<object>(ChangeSelectedImage);
            LeftClickCommand = new RelayCommand<object>(AddLine);
            CTRLLeftClickCommand = new RelayCommand<object>(SelectLine);
            CTRLRigtClickCommand = new RelayCommand<object>(DeleteLine);
            MiddleClickCommand = new RelayCommand<object>(CancelLine);
        }

        private void SelectLine(object parameter)
        {
            SelectedLine = GetNearestLine(GetVector(parameter));
        }

        private void CancelLine(object parameter)
        {
            if (!IsFirstPoint)
            {
                IsFirstPoint = true;
            }
        }

        private void AddLine(object parameter)
        {
            var vector = GetVector(parameter);
            vector.Z = 0;

            if (IsFirstPoint)
            {
                FirstPoint = vector;
            }
            else
            {
                var lineBuilder = new LineBuilder();
                //foreach (var item in Lines.Lines)
                //{
                //    lineBuilder.AddLine(item.P0, item.P1);
                //}

                //var center = new Vector3((FirstPoint.X + vector.X) / 2, (FirstPoint.Y + vector.Y) / 2, 0);
                //lineBuilder.AddCircle(center, new Vector3(0, 0, 1), 0.05f, 360);
                lineBuilder.AddLine(FirstPoint, vector);
                Lines = lineBuilder.ToLineGeometry3D();
                SaveLine(Lines.Lines.Last());
            }

            IsFirstPoint = !IsFirstPoint;
        }
        private void DeleteLine(object parameter)
        {
            var selectedLine = GetNearestLine(GetVector(parameter));

            var lineBuilder = new LineBuilder();
            foreach (var line in Lines.Lines)
            {
                if (!line.Equals(selectedLine))
                {
                    lineBuilder.AddLine(line.P0, line.P1);
                }
            }

            Lines = lineBuilder.ToLineGeometry3D();
            DeleteLine(selectedLine);
        }
        private Vector3 GetVector(object parameter)
        {
            var viewPort = (Viewport3DX)parameter;
            var point = viewPort.FindNearestPoint(Mouse.GetPosition(viewPort));
            var vector = new Vector3();

            if (point.HasValue)
            {
                vector = point.Value.ToVector3();
                //vector.Z = 0;
            }

            return vector;
        }

        public LineGeometry3D Lines
        {
            get { return _lines; }
            set
            {
                _lines = value;
                NotifyPropertyChanged();
            }
        }
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

        public Geometry3D.Line SelectedLine
        {
            get { return _selectedLine; }
            set
            {
                _selectedLine = value;
                NotifyPropertyChanged();

                SetCameraTarget();
            }
        }
        public ICommand LeftClickCommand { get; private set; }
        public ICommand CTRLLeftClickCommand { get; private set; }
        public ICommand CTRLRigtClickCommand { get; private set; }
        public ICommand MiddleClickCommand { get; private set; }
        public ICommand SelectImageCommand { get; private set; }

        private void ChangeSelectedImage(object newPath)
        {
            if (SelectedImage == (string)newPath)
            {
                return;
            }

            SelectedImage = (string)newPath;

            var image = new BitmapImage(new Uri(SelectedImage, UriKind.RelativeOrAbsolute));
            SetImage(image);

            LoadLines(SelectedImage);

            ResetCamera();

            SelectedLine = new Geometry3D.Line();
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

        //public void MouseDown3DHandler(object sender, MouseDown3DEventArgs e)
        //{
        //    if (e.HitTestResult == null)
        //    {
        //        return;
        //    }
        //    var originalEvent = e.OriginalInputEventArgs as MouseButtonEventArgs;
        //    var pressedMouseButton = originalEvent.ChangedButton;

        //    var vector = e.HitTestResult.PointHit;
        //    vector.Z = 0;

        //    if (pressedMouseButton == MouseButton.Left)
        //    {
        //        if (IsFirstPoint)
        //        {
        //            FirstPoint = vector;
        //        }
        //        else
        //        {
        //            var lineBuilder = new LineBuilder();
        //            foreach (var item in Lines.Lines)
        //            {
        //                lineBuilder.AddLine(item.P0, item.P1);
        //            }

        //            lineBuilder.AddLine(FirstPoint, vector);

        //            Lines = lineBuilder.ToLineGeometry3D();
        //            SaveLine(Lines.Lines.Last());
        //        }
        //        IsFirstPoint = !IsFirstPoint;
        //    }
        //    else if (pressedMouseButton == MouseButton.Right)
        //    {
        //        SelectedLine = GetNearestLine(vector);
        //    }
        //    else if (pressedMouseButton == MouseButton.Middle)
        //    {
        //        var selectedLine = GetNearestLine(vector);

        //        var lineBuilder = new LineBuilder();
        //        foreach (var line in Lines.Lines)
        //        {
        //            if (!line.Equals(selectedLine))
        //            {
        //                lineBuilder.AddLine(line.P0, line.P1);
        //            }
        //        }

        //        Lines = lineBuilder.ToLineGeometry3D();
        //        DeleteLine(selectedLine);
        //    }
        //}
        public void MouseMove3DHandler(object sender, MouseMove3DEventArgs e)
        {
            if (IsFirstPoint)
            {
                return;
            }
            var vector = GetVector(sender);
            var lineBuilder = new LineBuilder();
            lineBuilder.AddLine(FirstPoint, vector);
            Lines = lineBuilder.ToLineGeometry3D();
        }

        public void MouseUp3DHandler(object sender, MouseUp3DEventArgs e)
        {
            //var originalEvent = e.OriginalInputEventArgs as MouseButtonEventArgs;
            //var pressedMouseButton = originalEvent.ChangedButton;
            //if (pressedMouseButton != MouseButton.Left)
            //{
            //    return;
            //}
            //var vector = GetVector(sender);
            //var lineBuilder = new LineBuilder();
            //lineBuilder.AddLine(FirstPoint, vector);
            //Lines = lineBuilder.ToLineGeometry3D();
            //IsFirstPoint = true;
        }
        public void SelectionChangedHandler(object sender, SelectionChangedEventArgs e)
        {
            if (e != null && e.AddedItems != null && e.AddedItems.Count >= 1 && e.AddedItems[0] is Geometry3D.Line)
            {
                SelectedLine = (Geometry3D.Line)e.AddedItems[0];
            }
        }

        private void ResetCamera()
        {
            Camera = new PerspectiveCamera
            {
                Position = new Media3D.Point3D(0, 0, 10),
                LookDirection = new Media3D.Vector3D(0, 0, -5),
                UpDirection = new Media3D.Vector3D(0, 1, 0),
                NearPlaneDistance = 0.5,
                FarPlaneDistance = 150,
            };
        }
        private void SetCameraTarget()
        {
            var target = new Vector3((SelectedLine.P1.X + SelectedLine.P0.X) / 2, (SelectedLine.P1.Y + SelectedLine.P0.Y) / 2, 0);
            Camera.Position = new Media3D.Point3D(target.X, target.Y, 5);
            Camera.LookDirection = new Media3D.Vector3D(0, 0, -5);
            NotifyPropertyChanged("Camera");
        }
        private void DeleteLine(Geometry3D.Line line)
        {
            XmlDocument document = new XmlDocument();
            try
            {
                document.Load(SelectedImage.Replace("JPG", "xml"));

                bool equal = true;
                foreach (XmlNode node in document.SelectNodes("/Lines/Line"))
                {
                    foreach (XmlNode item in node.SelectNodes("/Points/Point"))
                    {
                        if (item["X"].InnerText != line.P0.X.ToString() || node["Y"].InnerText != line.P0.Y.ToString() || node["X"].InnerText != line.P1.X.ToString() || node["Y"].InnerText != line.P1.Y.ToString())
                        {
                            equal = false;
                            break;
                        }

                    }

                    if (equal)
                    {
                        node.ParentNode.RemoveChild(node);
                        document.Save(SelectedImage.Replace("JPG", "xml"));
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }
        private void SaveLine(Geometry3D.Line newLine)
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
        private void LoadLines(string fullFileName)
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
        private Geometry3D.Line GetNearestLine(Vector3 vector)
        {
            Dictionary<Geometry3D.Line, float> lineDistancePairs = new Dictionary<Geometry3D.Line, float>();
            foreach (var line in Lines.Lines)
            {
                var dxc = vector.X - line.P0.X;
                var dyc = vector.Y - line.P0.Y;
                var dxl = line.P1.X - line.P0.X;
                var dyl = line.P1.Y - line.P0.Y;
                var cross = dxc * dyl - dyc * dxl;

                lineDistancePairs.Add(line, Math.Abs(cross));
            }

            if (lineDistancePairs.Count < 1)
            {
                return new Geometry3D.Line();
            }

            return lineDistancePairs.OrderBy(x => x.Value).First().Key;
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
