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
using AnnotationTool.Model;

namespace AnnotationTool.ViewModel
{
    class ViewModel2D : ViewModelBase
    {
        private MeshGeometry3D _plane;
        private PhongMaterial _planeMaterial;
        private Media3D.Transform3D _planeTransform;
        private LineGeometry3D _lines;
        private LineGeometry3D _newLine;
        private string _selectedImage;
        private string[] _images;
        private _2DLine _selected2dLine;
        private List<_2DLine> _2dLineList;


        public ViewModel2D()
        {
            ResetLines();
            ResetNewLine();

            var box = new MeshBuilder();
            box.AddBox(new Vector3(0, 0, 0), 10, 10, 0, BoxFaces.PositiveZ);
            _plane = box.ToMeshGeometry3D();
            _planeMaterial = PhongMaterials.Blue;
            _planeTransform = new Media3D.TranslateTransform3D(0, 0, 0);

            _2dLineList = new List<_2DLine>();

            IsFirstPoint = true;

            _images = GetFolderFiles();

            ChangeSelectedImage(Images[0]);

            SelectImageCommand = new RelayCommand<object>(ChangeSelectedImage);
            LeftClickCommand = new RelayCommand<object>(AddLine);
            CTRLLeftClickCommand = new RelayCommand<object>(SelectLine);
            CTRLRigtClickCommand = new RelayCommand<object>(DeleteLine);
            ESCCommand = new RelayCommand<object>(CancelLine);
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

        public LineGeometry3D Lines
        {
            get { return _lines; }
            set
            {
                _lines = value;
                NotifyPropertyChanged();
            }
        }
        public LineGeometry3D NewLine
        {
            get { return _newLine; }
            set
            {
                _newLine = value;
                NotifyPropertyChanged();
            }
        }

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

        public _2DLine Selected2dLine
        {
            get { return _selected2dLine; }
            set
            {
                _selected2dLine = value;
                NotifyPropertyChanged();

                if (value != null)
                {
                    var target = GetVectorFromPixel(value.SelectedPoint);
                    SetCameraTarget(target);
                }
            }
        }
        public List<_2DLine> _2DLineList
        {
            get { return _2dLineList; }
            set
            {
                _2dLineList = value;
                NotifyPropertyChanged();
            }
        }


        public Vector3 FirstPoint { get; set; }
        public bool IsFirstPoint { get; set; }

        public ICommand SelectImageCommand { get; private set; }


        private void SelectLine(object parameter)
        {
            var vector = GetVector(parameter);
            if (vector == new Vector3(1000))
            {
                return;
            }

            var index = Lines.Lines.ToList().IndexOf(GetNearestLine(vector));

            if (index > -1)
                Selected2dLine = _2DLineList[index];
        }
        private void CancelLine(object parameter)
        {
            if (!IsFirstPoint)
            {
                IsFirstPoint = true;
                ResetNewLine();
            }
        }
        private void AddLine(object parameter)
        {
            var vector = GetVector(parameter);
            if (vector == new Vector3(1000))
            {
                return;
            }
            vector.Z = 0;
            if (IsFirstPoint)
            {
                FirstPoint = vector;
                SetCameraTarget(vector);
            }
            else
            {
                var newVector = new Vector3(FirstPoint.X + (FirstPoint.X - vector.X), FirstPoint.Y + (FirstPoint.Y - vector.Y), 0);

                var p1 = GetPixelFromVector(vector);
                var p2 = GetPixelFromVector(FirstPoint);
                var p3 = GetPixelFromVector(newVector);

                var newLine = new _2DLine(p1, p2, p3, MarkingType);
                _2DLineList = _2DLineList.Append(newLine).ToList();

                Lines.Positions.Add(newVector);
                Lines.Positions.Add(vector);
                Lines.Indices.Add(Lines.Indices.Count);
                Lines.Indices.Add(Lines.Indices.Count);
                Lines.Colors.Add(GetColor(MarkingType).ToColor4());
                Lines.Colors.Add(GetColor(MarkingType).ToColor4());

                Lines = new LineGeometry3D()
                {
                    Positions = Lines.Positions,
                    Indices = Lines.Indices,
                    Colors = Lines.Colors
                };

                SaveLineToXML(newLine);

                ResetNewLine();
            }

            IsFirstPoint = !IsFirstPoint;
        }
        private void DeleteLine(object parameter)
        {
            var vector = GetVector(parameter);
            if (vector == new Vector3(1000))
            {
                return;
            }

            var nearest = GetNearestLine(vector);
            var remainingPositions = Lines.Positions;
            var index = remainingPositions.IndexOf(nearest.P0);

            if (index > -1)
            {
                var remainingIndices = Lines.Indices;
                var remainingColors = Lines.Colors;

                DeleteLineFromXML(_2DLineList[index / 2]);

                remainingPositions.RemoveRange(index, 2);
                remainingIndices.RemoveRange(remainingIndices.Count - 2, 2);
                remainingColors.RemoveRange(index, 2);

                _2DLineList = _2DLineList.Where((v, i) => i != index / 2).ToList();

                Lines = new LineGeometry3D()
                {
                    Positions = remainingPositions,
                    Indices = remainingIndices,
                    Colors = remainingColors
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

        private void ChangeSelectedImage(object newPath)
        {
            if (SelectedImage == (string)newPath)
            {
                return;
            }

            SelectedImage = (string)newPath;

            var image = new BitmapImage(new Uri(SelectedImage, UriKind.RelativeOrAbsolute));
            SetImage(image);

            LoadLinesFromXML(SelectedImage);

            ResetCamera();
            
            IsFirstPoint = true;
            ResetNewLine();
        }
        private void SetImage(BitmapSource image)
        {
            var ratio = image.PixelWidth / (double)image.PixelHeight;
            var transform = Media3D.Transform3D.Identity;
            transform = transform.AppendTransform(new Media3D.ScaleTransform3D(ratio, 1.0, 1.0));

            PlaneTransform = transform;
            var material = new PhongMaterial()
            {
                DiffuseColor = Color.White,
                AmbientColor = Color.Black,
                ReflectiveColor = Color.Black,
                EmissiveColor = Color.Black,
                SpecularColor = Color.Black,
                DiffuseMap = new MemoryStream(image.ToByteArray()),
            };

            PlaneMaterial = material;
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

        public void MouseMove3DHandler(object sender, MouseMove3DEventArgs e)
        {
            if (IsFirstPoint)
            {
                return;
            }

            var vector = GetVector(sender);

            if (vector == new Vector3(1000))
            {
                return;
            }
            vector.Z = 0;
            var lineBuilder = new LineBuilder();

            var newVector = new Vector3(FirstPoint.X - (vector.X - FirstPoint.X), FirstPoint.Y - (vector.Y - FirstPoint.Y), 0);

            lineBuilder.AddLine(newVector, vector);
            lineBuilder.AddCircle(FirstPoint, new Vector3(0, 0, 1), 0.04f, 360);
            var lineGeometry = lineBuilder.ToLineGeometry3D();

            bool newLineIsValid = true;
            for (int i = 0; i < Lines.Positions.Count - 1; i += 2)
            {
                newLineIsValid = !LineIntersect(lineGeometry.Positions[0], lineGeometry.Positions[1], Lines.Positions[i], Lines.Positions[i + 1]);
                if (!newLineIsValid)
                {
                    break;
                }
            }

            if (newLineIsValid)
            {
                lineGeometry.Colors = new Color4Collection();
                for (int i = 0; i < lineGeometry.Positions.Count; i++)
                {
                    lineGeometry.Colors.Add(GetColor(MarkingType).ToColor4());
                }
                NewLine = lineGeometry;
            }
        }
        public void SelectionChangedHandler(object sender, SelectionChangedEventArgs e)
        {
            if (e != null && e.AddedItems != null && e.AddedItems.Count >= 1 && e.AddedItems[0] is _2DLine)
                Selected2dLine = (_2DLine)e.AddedItems[0];
        }

        private void ResetLines()
        {
            Lines = new LineGeometry3D()
            {
                Positions = new Vector3Collection(),
                Indices = new IntCollection(),
                Colors = new Color4Collection()
            };

            _2DLineList = new List<_2DLine>();
        }
        private void ResetNewLine()
        {
            NewLine = new LineGeometry3D()
            {
                Positions = new Vector3Collection(),
                Indices = new IntCollection(),
                Colors = new Color4Collection()
            };
        }

        static bool LineIntersect(Vector3 p01, Vector3 p11, Vector3 p02, Vector3 p12)
        {
            var dx = p11.X - p01.X;
            var dy = p11.Y - p01.Y;
            var da = p12.X - p02.X;
            var db = p12.Y - p02.Y;

            if (da * dy - db * dx == 0)
            {
                // The segments are parallel.
                return false;
            }

            var s = (dx * (p02.Y - p01.Y) + dy * (p01.X - p02.X)) / (da * dy - db * dx);
            var t = (da * (p01.Y - p02.Y) + db * (p02.X - p01.X)) / (db * dx - da * dy);

            if ((s >= 0) & (s <= 1) & (t >= 0) & (t <= 1))
                return true;
            else
                return false;
        }

        private Vector2 GetPixelFromVector(Vector3 vector)
        {
            var image = new BitmapImage(new Uri(SelectedImage, UriKind.RelativeOrAbsolute));
            int imageWidth = image.PixelWidth;
            int imageHeight = image.PixelHeight;

            double vertical = 5.0;
            double horizontal = imageWidth / (imageHeight / vertical);
            Vector2 center = new Vector2(imageWidth / 2, imageHeight / 2);
            Vector2 computedPoint = new Vector2();

            double computedX = Math.Abs(center.X / vertical * vector.X);
            if (vector.X >= 0)
                computedPoint.X = Convert.ToInt32(center.X + computedX);
            else
                computedPoint.X = Convert.ToInt32(center.X - computedX);

            double computedY = Math.Abs(center.Y / horizontal * vector.Y);
            if (vector.Y >= 0)
                computedPoint.Y = Convert.ToInt32(center.Y - computedY);
            else
                computedPoint.Y = Convert.ToInt32(center.Y + computedY);

            return computedPoint;
        }
        private Vector3 GetVectorFromPixel(Vector2 vector)
        {
            var image = new BitmapImage(new Uri(SelectedImage, UriKind.RelativeOrAbsolute));
            int imageWidth = image.PixelWidth;
            int imageHeight = image.PixelHeight;

            double vertical = 5.0;
            double horizontal = imageWidth / (imageHeight / vertical);
            Vector2 center = new Vector2(imageWidth / 2, imageHeight / 2);
            double computedX = Math.Abs(vector.X - center.X);
            double computedY = Math.Abs(vector.Y - center.Y);

            double computedPointX;
            if (vector.X >= center.X)
                computedPointX = computedX / (center.X / vertical);
            else
                computedPointX = -computedX / (center.X / vertical);

            double computedPointY;
            if (vector.Y >= center.Y)
                computedPointY = -computedY / (center.Y / horizontal);
            else
                computedPointY = computedY / (center.Y / horizontal);

            return new Vector3((float)computedPointX, (float)computedPointY, 0);
        }

        private void DeleteLineFromXML(_2DLine line)
        {
            XmlDocument document = new XmlDocument();
            try
            {
                document.Load(SelectedImage.Replace("JPG", "xml"));

                bool equal = false;
                var nodelist = document.SelectNodes("/Lines/Line[Type = '" + line.Type + "']");
                //var nodelist = document.SelectNodes("/Lines/Line");
                foreach (XmlNode node in nodelist)
                {
                    var asd = node.SelectNodes("//Points/Point");
                    foreach (XmlNode item in asd)
                    {
                        if (item["X"].InnerText == line.FirstPoint.X.ToString() && item["Y"].InnerText == line.FirstPoint.Y.ToString())
                        {
                            equal = true;
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
            catch (Exception)
            {
                return;
            }
        }
        private void SaveLineToXML(_2DLine newLine)
        {
            XmlDocument document = new XmlDocument();
            XmlElement linesElement;
            IEnumerable<_2DLine> _2DLines;

            try
            {
                document.Load(SelectedImage.Replace("JPG", "xml"));
                linesElement = document.GetElementsByTagName("Lines")[0] as XmlElement;
                _2DLines = new List<_2DLine>() { newLine };
            }
            catch (Exception)
            {
                XmlDeclaration xmlDeclaration = document.CreateXmlDeclaration("1.0", "UTF-8", null);
                XmlElement root = document.DocumentElement;
                document.InsertBefore(xmlDeclaration, root);

                linesElement = document.CreateElement(string.Empty, "Lines", string.Empty);
                document.AppendChild(linesElement);
                _2DLines = _2DLineList;
            }

            foreach (var line in _2DLines)
            {
                XmlElement lineElement = document.CreateElement(string.Empty, "Line", string.Empty);
                linesElement.AppendChild(lineElement);

                XmlElement pointsElement = document.CreateElement(string.Empty, "Points", string.Empty);
                lineElement.AppendChild(pointsElement);

                Vector2Collection points = new Vector2Collection() { line.FirstPoint, line.SelectedPoint, line.MirroredPoint };

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

                XmlText typeText = document.CreateTextNode(line.Type.ToString());
                XmlElement typeElement = document.CreateElement(string.Empty, "Type", string.Empty);
                lineElement.AppendChild(typeElement);
                typeElement.AppendChild(typeText);
            }

            document.Save(SelectedImage.Replace("JPG", "xml"));
        }
        private void LoadLinesFromXML(string fullFileName)
        {
            XmlDocument document = new XmlDocument();

            var lineList = new List<_2DLine>();

            try
            {
                document.Load(fullFileName.Replace("JPG", "xml"));

                XmlNodeList lines = document.GetElementsByTagName("Line");

                foreach (XmlNode line in lines)
                {
                    var firstPointX = Convert.ToInt32(line.ChildNodes[0].ChildNodes[0].FirstChild.InnerText);
                    var firstPointY = Convert.ToInt32(line.ChildNodes[0].ChildNodes[0].LastChild.InnerText);

                    var selectedPointX = Convert.ToInt32(line.ChildNodes[0].ChildNodes[1].FirstChild.InnerText);
                    var selectedPointY = Convert.ToInt32(line.ChildNodes[0].ChildNodes[1].LastChild.InnerText);

                    var mirroredPointX = Convert.ToInt32(line.ChildNodes[0].ChildNodes[2].FirstChild.InnerText);
                    var mirroredPointY = Convert.ToInt32(line.ChildNodes[0].ChildNodes[2].LastChild.InnerText);

                    string type = line.ChildNodes[1].InnerText;

                    lineList.Add(new _2DLine(new Vector2(firstPointX, firstPointY), new Vector2(selectedPointX, selectedPointY), new Vector2(mirroredPointX, mirroredPointY), (MarkingType)Enum.Parse(typeof(MarkingType), type)));
                }

                _2DLineList = lineList;


                LineGeometry3D lineGeometry = new LineGeometry3D()
                {
                    Positions = new Vector3Collection(),
                    Indices = new IntCollection(),
                    Colors = new Color4Collection()
                };

                foreach (var line in _2DLineList)
                {
                    var v1 = GetVectorFromPixel(line.FirstPoint);
                    var v2 = GetVectorFromPixel(line.MirroredPoint);

                    lineGeometry.Positions.Add(v1);
                    lineGeometry.Positions.Add(v2);
                    lineGeometry.Indices.Add(lineGeometry.Indices.Count);
                    lineGeometry.Indices.Add(lineGeometry.Indices.Count);
                    lineGeometry.Colors.Add(GetColor(line.Type).ToColor4());
                    lineGeometry.Colors.Add(GetColor(line.Type).ToColor4());
                }

                Lines = lineGeometry;
            }
            catch (Exception)
            {
                ResetLines();
            }
        }
    }
}
