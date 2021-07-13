using SharpDX;
namespace AnnotationTool.Model
{
    public class _2DLine
    {
        public Vector3 CenterPoint { get; set; }
        public Vector3 FirstPoint { get; set; }
        public Vector3 MirroredPoint { get; set; }
        public MarkingType Type { get; set; }

        public _2DLine(Vector3 centerPoint, Vector3 firstPoint, Vector3 mirroredPoint, MarkingType type)
        {
            CenterPoint = centerPoint;
            FirstPoint = firstPoint;
            MirroredPoint = mirroredPoint;
            Type = type;
        }

        public _2DLine(Vector3 centerPoint, Vector3 firstPoint, MarkingType type)
        {
            CenterPoint = centerPoint;
            FirstPoint = firstPoint;
            MirroredPoint = centerPoint + (centerPoint - firstPoint);
            Type = type;
        }
    }
}
