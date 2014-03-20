﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ERoD
{

    struct VertexPositionTextureCorner : IVertexType
    {
        public Vector3 Position;
        public Vector3 TexCoordAndCornerIndex;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
        );

        public VertexPositionTextureCorner(Vector3 pos, Vector3 texCoordCorner)
        {
            // TODO: Complete member initialization
            this.Position = pos;
            this.TexCoordAndCornerIndex = texCoordCorner;
        }

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    };

    public class ScreenQuad
    {
        VertexPositionTextureCorner[] corners;
        VertexBuffer vb;
        short[] ib;

        Game Game;

        public ScreenQuad(Game game)
        {
            Game = game;
            corners = new VertexPositionTextureCorner[4];
            corners[0].Position = new Vector3(0, 0, 0);
            corners[0].TexCoordAndCornerIndex = Vector3.Zero;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public virtual void Initialize()
        {
            corners = new VertexPositionTextureCorner[]
                    {
                        new VertexPositionTextureCorner(
                            new Vector3(0,0,0),
                            new Vector3(1,1,1)),
                        new VertexPositionTextureCorner(
                            new Vector3(0,0,0),
                            new Vector3(0,1,0)),
                        new VertexPositionTextureCorner(
                            new Vector3(0,0,0),
                            new Vector3(0,0,3)),
                        new VertexPositionTextureCorner(
                            new Vector3(0,0,0),
                            new Vector3(1,0,2))
                    };

            ib = new short[] { 0, 1, 2, 2, 3, 0 };
            vb = new VertexBuffer(Game.GraphicsDevice, typeof(VertexPositionTextureCorner), corners.Length, BufferUsage.None);
        }

        public virtual void Draw(Vector2 v1, Vector2 v2)
        {
            corners[0].Position.X = v2.X; // 1
            corners[0].Position.Y = v1.Y; // -1

            corners[1].Position.X = v1.X; // -1
            corners[1].Position.Y = v1.Y; // -1

            corners[2].Position.X = v1.X; // -1
            corners[2].Position.Y = v2.Y; // 1

            corners[3].Position.X = v2.X; // 1
            corners[3].Position.Y = v2.Y; // 1

            vb.SetData(corners);
            Game.GraphicsDevice.SetVertexBuffer(vb);

            Game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTextureCorner>(PrimitiveType.TriangleList, corners, 0, 4, ib, 0, 2);
        }
    }
}
