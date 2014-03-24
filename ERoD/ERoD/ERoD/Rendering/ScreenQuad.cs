﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ERoD
{

    public class ScreenQuad
    {
        VertexPositionTexture[] vertices;
        VertexBuffer vb;
        short[] ib;

        Game Game;

        public ScreenQuad(Game game)
        {
            Game = game;
            vertices = new VertexPositionTexture[4];
            vertices[0].Position = new Vector3(0, 0, 0);
            vertices[0].TextureCoordinate = Vector2.Zero;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public virtual void Initialize()
        {
            vertices = new VertexPositionTexture[]
                    {
                        new VertexPositionTexture(
                            new Vector3(0,0,0),
                            new Vector2(1,1)),
                        new VertexPositionTexture(
                            new Vector3(0,0,0),
                            new Vector2(0,1)),
                        new VertexPositionTexture(
                            new Vector3(0,0,0),
                            new Vector2(0,0)),
                        new VertexPositionTexture(
                            new Vector3(0,0,0),
                            new Vector2(1,0))
                    };

            ib = new short[] { 0, 1, 2, 2, 3, 0 };
            vb = new VertexBuffer(Game.GraphicsDevice, typeof(VertexPositionTexture), vertices.Length, BufferUsage.None);
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

            Game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, vertices, 0, 4, ib, 0, 2);
        }
    }
}
