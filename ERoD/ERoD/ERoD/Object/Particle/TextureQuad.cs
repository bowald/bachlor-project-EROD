using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class TextureQuad
    {
        private static readonly Vector2 UpperLeft = new Vector2(0, 0);
        private static readonly Vector2 UpperRight = new Vector2(1, 0);
        private static readonly Vector2 BottomLeft = new Vector2(0, 1);
        private static readonly Vector2 BottomRight = new Vector2(1, 1);

        private readonly VertexBuffer vertexBuffer;

        public static Effect ParticleEffect { get; set; }

        private float alpha;
        public float Alpha
        {
            get { return alpha; }
            set 
            {
                alpha = value;
            }
        }

        private Texture2D texture;
        public Texture2D Texture
        {
            get { return texture; }
            set 
            {
                texture = value;
                ParticleEffect.Parameters["Diffuse"].SetValue(texture);
            }
        }

        public TextureQuad(GraphicsDevice graphicsDevice, Texture2D texture, int width, int height)
        {
            VertexPositionTexture[] vertices = CreateQuadVertices(width, height);
            vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionTexture), vertices.Length, BufferUsage.None);
            vertexBuffer.SetData(vertices);
            this.Texture = texture;
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

        public void Draw(Matrix viewMatrix, Matrix projectionMatrix, Matrix worldMatrix, float farPlane)
        {
            ParticleEffect.GraphicsDevice.SetVertexBuffer(vertexBuffer);

            ParticleEffect.Parameters["World"].SetValue(worldMatrix);
            ParticleEffect.Parameters["View"].SetValue(viewMatrix);
            ParticleEffect.Parameters["Projection"].SetValue(projectionMatrix);
            ParticleEffect.Parameters["Alpha"].SetValue(alpha);
            ParticleEffect.Parameters["FarPlane"].SetValue(farPlane);

            foreach (EffectPass pass in ParticleEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                ParticleEffect.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }
        }
    }
}
