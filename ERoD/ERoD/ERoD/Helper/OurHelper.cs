using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD.Helper
{
    public static class OurHelper
    {

        public static BEPUutilities.Vector3[] scaleVertices(BEPUutilities.Vector3[] vertices, BEPUutilities.AffineTransform scaling)
        {
            BEPUutilities.Vector3[] scaled = new BEPUutilities.Vector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                scaled[i] = BEPUutilities.AffineTransform.Transform(vertices[i], scaling);
            }
            return scaled;
        }

    }
}
