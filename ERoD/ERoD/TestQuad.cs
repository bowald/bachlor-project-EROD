using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    class TestQuad
    {
        VertexPositionColor[] vertices;
        VertexBuffer vb;
        short[] ib;

        Game Game;

        public TestQuad(Game game)
        {
            Game = game;
            vertices = new VertexPositionColor[4];
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public virtual void Initialize()
        {
            vertices = new VertexPositionColor[]
                    {
                        new VertexPositionColor(
                            new Vector3(0, 0, 0),
                            new Color(0, 255, 0, 255)),
                        new VertexPositionColor(
                            new Vector3(0, 0, 0),
                            Color.Red),
                        new VertexPositionColor(
                            new Vector3(0, 0, 0),
                            Color.Blue),
                        new VertexPositionColor(
                            new Vector3(0, 0, 0),
                            Color.Blue)
                    };

            ib = new short[] { 0, 1, 2, 0, 3, 2 };
            vb = new VertexBuffer(Game.GraphicsDevice, typeof(VertexPositionColor), vertices.Length, BufferUsage.None);
        }

        public virtual void Draw(Vector2 v1, Vector2 v2)
        {
            vertices[0].Position.X = v2.X; // 1
            vertices[0].Position.Y = v1.Y; // -1

            vertices[1].Position.X = v1.X; // -1
            vertices[1].Position.Y = v1.Y; // -1

            vertices[2].Position.X = v1.X; // -1
            vertices[2].Position.Y = v2.Y; // 1

            vertices[3].Position.X = v2.X; // 1
            vertices[3].Position.Y = v2.Y; // 1

            vb.SetData(vertices);
            Game.GraphicsDevice.SetVertexBuffer(vb);

            Game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vertices, 0, 4, ib, 0, 2);
        }
    }
}
