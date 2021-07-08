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

        public _3DPlane()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public _3DPlane(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
