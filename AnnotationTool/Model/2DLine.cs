using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public enum MarkingType
    {
        /// <summary>
        /// Általános metszés
        /// </summary>
        GeneralPruning = 1,
        /// <summary>
        /// Bizonytalan metszés
        /// </summary>
        UncertainPruning,
        /// <summary>
        /// Tőből metszés
        /// </summary>
        PruningFromStems,
    }
}
