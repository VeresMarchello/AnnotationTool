using SharpDX;
namespace AnnotationTool.Model
{
    public class _2DLine
    {
        public Vector2 FirstPoint { get; set; }
        public Vector2 SelectedPoint { get; set; }
        public Vector2 MirroredPoint { get; set; }
        public MarkingType Type { get; set; }

        public _2DLine(Vector2 firstPoint, Vector2 selectedPoint, Vector2 mirroredPoint, MarkingType type)
        {
            FirstPoint = firstPoint;
            SelectedPoint = selectedPoint;
            MirroredPoint = mirroredPoint;
            Type = type;
        }
    }
}
