using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnotationTool.Model
{
    public class _3DPlane
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Size { get; set; }
        public MarkingType Type { get; set; }


        public _3DPlane()
        {
            X = 0;
            Y = 0;
            Z = 0;
            Size = 0;
            Type = MarkingType.GeneralPruning;
        }

        public _3DPlane(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
            Size = 5;
            Type = MarkingType.GeneralPruning;
        }

        public _3DPlane(float x, float y, float z, float size, MarkingType type) : this(x, y, z)
        {
            Size = size;
            Type = type;
        }
    }
}
