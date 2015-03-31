using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public struct VertexMatrixNormal : IVertexType
    {
        public Matrix Matrix;
        public Vector3 Normal;

        public VertexMatrixNormal(Matrix matrix, Vector3 normal)
        {
            Matrix = matrix;
            Normal = normal;
        }

        public readonly static VertexDeclaration VertexDeclaration
            = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Fog, 0)
                ,
                new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.Fog, 1)
                ,
                new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.Fog, 2)
                ,
                new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.Fog, 3)
                ,
                new VertexElement(64, VertexElementFormat.Vector3, VertexElementUsage.Normal, 1)
                );

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }

    }
}
