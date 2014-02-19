using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD.Helper
{
    public static class OurHelper
    {

        public static BEPUutilities.Vector3[] scaleVertices(BEPUutilities.Vector3[] vertices, Microsoft.Xna.Framework.Vector3 scaling)
        {
            BEPUutilities.Vector3[] scaled = new BEPUutilities.Vector3[vertices.Length];
            BEPUutilities.AffineTransform scalingAffine = new BEPUutilities.AffineTransform(ConversionHelper.MathConverter.Convert(scaling), 
                new BEPUutilities.Quaternion(0,0,0,0), BEPUutilities.Vector3.Zero);
            for (int i = 0; i < vertices.Length; i++)
            {
                scaled[i] = BEPUutilities.AffineTransform.Transform(vertices[i], scalingAffine);
            }
            return scaled;
        }

    }
}
