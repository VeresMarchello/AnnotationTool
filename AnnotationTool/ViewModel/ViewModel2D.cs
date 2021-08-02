using SharpDX;
using System.Windows.Input;
using System.IO;
using AnnotationTool.Commands;
using System.Windows.Media.Imaging;
using HelixToolkit.Wpf.SharpDX;
using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using AnnotationTool.Model;

namespace AnnotationTool.ViewModel
{
    class ViewModel2D : ViewModelBase
    {
        private string _selectedLeftImage;
        private string _selectedRightImage;
        private string[] _images;

        private _2DLine _selected2dLine;
        private List<_2DLine> _2dLeftLineList;
        private List<_2DLine> _2dRightLineList;


        public ViewModel2D()
        {
            _images = GetFolderFiles();
            ChangeSelectedImage(_images[0]);

            SelectImageCommand = new RelayCommand<object>(ChangeSelectedImage);
        }


        public string SelectedLeftImage
        {
            get { return _selectedLeftImage; }
            set
            {
                _selectedLeftImage = value;
                NotifyPropertyChanged();

                SelectedRightImage = value.Replace("Left", "Right");
            }
        }
        public string SelectedRightImage
        {
            get { return _selectedRightImage; }
            set
            {
                _selectedRightImage = value;
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
                    var target = GetVectorFromPixel(value.CenterPoint);
                    SetCameraTarget(target);
                }
            }
        }
        public List<_2DLine> _2DLeftLineList
        {
            get { return _2dLeftLineList; }
            set
            {
                _2dLeftLineList = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("LeftLines");
            }
        }
        public List<_2DLine> _2DRightLineList
        {
            get { return _2dRightLineList; }
            set
            {
                _2dRightLineList = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("RightLines");
            }
        }
        public LineGeometry3D LeftLines
        {
            get { return GetLineGeometry(_2DLeftLineList); }
            set
            {
                var lineList = Get_2DLineList(value);
                if (lineList.Count > _2DLeftLineList.Count)
                {
                    var newItem = lineList[lineList.Count-1];
                    SaveLineToXML(newItem, SelectedLeftImage);
                }
                else
                {
                    var deletedItem = _2DLeftLineList.Except(lineList).First();
                    DeleteLineFromXML(deletedItem, SelectedLeftImage);
                }
                _2DLeftLineList = lineList;
            }
        }
        public LineGeometry3D RightLines
        {
            get { return GetLineGeometry(_2DRightLineList); }
            set
            {
                var lineList = Get_2DLineList(value);
                if (lineList.Count > _2DRightLineList.Count)
                {
                    var newItem = lineList[lineList.Count - 1];
                    SaveLineToXML(newItem, SelectedRightImage);
                }
                else
                {
                    var deletedItem = _2DRightLineList.Except(lineList).First();
                    DeleteLineFromXML(deletedItem, SelectedRightImage);
                }
                _2DRightLineList = lineList;
            }
        }

        public ICommand SelectImageCommand { get; private set; }


        private LineGeometry3D GetLineGeometry(List<_2DLine> _2DLines)
        {
            LineGeometry3D lineGeometry = new LineGeometry3D()
            {
                Positions = new Vector3Collection(),
                Indices = new IntCollection(),
                Colors = new Color4Collection()
            };

            foreach (var line in _2DLines)
            {
                var v1 = GetVectorFromPixel(line.FirstPoint);
                var v2 = GetVectorFromPixel(line.MirroredPoint);

                lineGeometry.Positions.Add(v1);
                lineGeometry.Positions.Add(v2);
                lineGeometry.Indices.Add(lineGeometry.Indices.Count);
                lineGeometry.Indices.Add(lineGeometry.Indices.Count);
                lineGeometry.Colors.Add(GetColor(line.Type));
                lineGeometry.Colors.Add(GetColor(line.Type));
            }

            return lineGeometry;
        }
        private List<_2DLine> Get_2DLineList(LineGeometry3D lineGeometry)
        {
            var lineList = new List<_2DLine>();
            var count = lineGeometry.Positions.Count;

            for (int i = 0; i < count; i += 2)
            {
                //var center = new Vector3((lineGeometry.Positions[i].X + lineGeometry.Positions[i+1].X) / 2, (lineGeometry.Positions[i].Y + lineGeometry.Positions[i + 1].Y) / 2, 0);
                var center = (lineGeometry.Positions[i] + lineGeometry.Positions[i + 1]) / 2;
                var newLine = new _2DLine(GetPixelFromVector(center), GetPixelFromVector(lineGeometry.Positions[i]), GetMarkingType(lineGeometry.Colors[i]));
                lineList.Add(newLine);
            }

            return lineList;
        }


        private void ChangeSelectedImage(object newPath)
        {
            if (SelectedLeftImage == (string)newPath)
            {
                return;
            }

            SelectedLeftImage = (string)newPath;

            _2DLeftLineList = LoadLinesFromXML(SelectedLeftImage);
            _2DRightLineList = LoadLinesFromXML(SelectedRightImage);

            ResetCamera();
        }

        private string[] GetFolderFiles()
        {
            string[] files = null;
            files = Directory.GetFiles($@"{AppDomain.CurrentDomain.BaseDirectory}\Images\Left\Unpruned", "*.JPG");

            return files;
        }


        private Vector3 GetVectorFromPixel(Vector3 vector)
        {
            var image = new BitmapImage(new Uri(SelectedLeftImage, UriKind.RelativeOrAbsolute));
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
        private Vector3 GetPixelFromVector(Vector3 vector)
        {
            var image = new BitmapImage(new Uri(SelectedLeftImage, UriKind.RelativeOrAbsolute));
            int imageWidth = image.PixelWidth;
            int imageHeight = image.PixelHeight;

            double vertical = 5.0;
            double horizontal = imageWidth / (imageHeight / vertical);
            Vector2 center = new Vector2(imageWidth / 2, imageHeight / 2);
            Vector3 computedPoint = new Vector3(0);

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
        private void DeleteLineFromXML(_2DLine line, string fullFileName)
        {
            XmlDocument document = new XmlDocument();
            try
            {
                document.Load(fullFileName.Replace("JPG", "xml"));

                bool equal = false;
                var nodelist = document.SelectNodes("/Lines/Line[Type = '" + line.Type + "']");
                //var nodelist = document.SelectNodes("/Lines/Line");
                foreach (XmlNode node in nodelist)
                {
                    var asd = node.SelectNodes("//Points/Point");
                    foreach (XmlNode item in asd)
                    {
                        if (item["X"].InnerText == line.CenterPoint.X.ToString() && item["Y"].InnerText == line.CenterPoint.Y.ToString())
                        {
                            equal = true;
                            break;
                        }
                    }

                    if (equal)
                    {
                        node.ParentNode.RemoveChild(node);
                        document.Save(fullFileName.Replace("JPG", "xml"));
                        break;
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }
        private void SaveLineToXML(_2DLine newLine, string fullFileName)
        {
            XmlDocument document = new XmlDocument();
            XmlElement linesElement;

            try
            {
                document.Load(fullFileName.Replace("JPG", "xml"));
                linesElement = document.GetElementsByTagName("Lines")[0] as XmlElement;
            }
            catch (Exception)
            {
                XmlDeclaration xmlDeclaration = document.CreateXmlDeclaration("1.0", "UTF-8", null);
                XmlElement root = document.DocumentElement;
                document.InsertBefore(xmlDeclaration, root);

                linesElement = document.CreateElement(string.Empty, "Lines", string.Empty);
                document.AppendChild(linesElement);
            }

            XmlElement lineElement = document.CreateElement(string.Empty, "Line", string.Empty);
            linesElement.AppendChild(lineElement);

            XmlElement pointsElement = document.CreateElement(string.Empty, "Points", string.Empty);
            lineElement.AppendChild(pointsElement);

            Vector3Collection points = new Vector3Collection() { newLine.CenterPoint, newLine.FirstPoint, newLine.MirroredPoint };

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

            XmlText typeText = document.CreateTextNode(newLine.Type.ToString());
            XmlElement typeElement = document.CreateElement(string.Empty, "Type", string.Empty);
            lineElement.AppendChild(typeElement);
            typeElement.AppendChild(typeText);

            document.Save(fullFileName.Replace("JPG", "xml"));
        }
        private List<_2DLine> LoadLinesFromXML(string fullFileName)
        {
            XmlDocument document = new XmlDocument();

            var lineList = new List<_2DLine>();

            try
            {
                document.Load(fullFileName.Replace("JPG", "xml"));

                XmlNodeList lines = document.GetElementsByTagName("Line");

                foreach (XmlNode line in lines)
                {
                    var centerPointX = Convert.ToInt32(line.ChildNodes[0].ChildNodes[0].FirstChild.InnerText);
                    var centerPointY = Convert.ToInt32(line.ChildNodes[0].ChildNodes[0].LastChild.InnerText);

                    var firstPointX = Convert.ToInt32(line.ChildNodes[0].ChildNodes[1].FirstChild.InnerText);
                    var firstPointY = Convert.ToInt32(line.ChildNodes[0].ChildNodes[1].LastChild.InnerText);

                    string type = line.ChildNodes[1].InnerText;

                    lineList.Add(new _2DLine(new Vector3(centerPointX, centerPointY, 0), new Vector3(firstPointX, firstPointY, 0), (MarkingType)Enum.Parse(typeof(MarkingType), type)));
                }
            }
            catch
            {
            }

            return lineList;
        }
    }
}
