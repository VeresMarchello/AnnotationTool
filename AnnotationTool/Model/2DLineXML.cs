using System.Xml.Serialization;
using System.Collections.Generic;
namespace AnnotationTool.Model
{
    [XmlRoot(ElementName = "Point")]
    public class Point
    {
        public Point()
        {

        }
        public Point(float x, float y)
        {
            X = x;
            Y = y;
        }

        [XmlElement(ElementName = "X")]
        public float X { get; set; }
        [XmlElement(ElementName = "Y")]
        public float Y { get; set; }
    }

    [XmlRoot(ElementName = "Points")]
    public class Points
    {
        [XmlElement(ElementName = "Point")]
        public List<Point> Point { get; set; }
    }

    [XmlRoot(ElementName = "Line")]
    public class Line
    {
        [XmlElement(ElementName = "Points")]
        public Points Points { get; set; }
        [XmlElement(ElementName = "Type")]
        public MarkingType Type { get; set; }
    }

    [XmlRoot(ElementName = "Lines")]
    public class Lines
    {
        [XmlElement(ElementName = "Line")]
        public List<Line> Line { get; set; }
    }
}

