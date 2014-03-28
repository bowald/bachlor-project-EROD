using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD.Object.Particle
{
    class TextureQuad
    {
        private static readonly Vector2 UpperLeft = new Vector2(0, 0);
        private static readonly Vector2 UpperRight = new Vector2(1, 0);
        private static readonly Vector2 BottomLeft = new Vector2(0, 1);
        private static readonly Vector2 BottomRight = new Vector2(1, 1);

        private readonly VertexBuffer vertexBuffer;
        private readonly BasicEffect effect;

        public TextureQuad(GraphicsDevice graphicsDevice, Texture2D texture, int width, int height)
        {
            VertexPositionTexture[] vertices = CreateQuadVertices(width, height);
            vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionTexture), vertices.Length, BufferUsage.None);
            vertexBuffer.SetData(vertices);

            effect = new BasicEffect(graphicsDevice) { TextureEnabled = true, Texture = texture };
        }

        private static VertexPositionTexture[] CreateQuadVertices(int width, int height)
        {
            int halfWidth = width / 2;
            int halfHeight = height / 2;

            VertexPositionTexture[] vertices = new VertexPositionTexture[4];

            vertices[0] = new VertexPositionTexture(new Vector3(-halfWidth, halfHeight, 0), UpperLeft);
            vertices[1] = new VertexPositionTexture(new Vector3(halfWidth, halfHeight, 0), UpperRight);
            vertices[2] = new VertexPositionTexture(new Vector3(-halfWidth, -halfHeight, 0), BottomLeft);
            vertices[3] = new VertexPositionTexture(new Vector3(halfWidth, -halfHeight, 0), BottomRight);

            return vertices;
        }

        public void Draw(Matrix viewMatrix, Matrix projectionMatrix, Matrix worldMatrix)
        {
            effect.GraphicsDevice.SetVertexBuffer(vertexBuffer);

            effect.World = worldMatrix;
            effect.View = viewMatrix;
            effect.Projection = projectionMatrix;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                effect.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }
        }

        public float Alpha
        {
            get { return effect.Alpha; }
            set { effect.Alpha = value; }
        }
    }
}
