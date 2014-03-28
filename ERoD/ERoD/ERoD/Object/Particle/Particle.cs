using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class Particle
    {

        public float BirthTime { get; set; }
        public float LifeSpan { get; set; }
        public bool IsAlive { get; set; }
        public Vector3 OrginalPosition { get; set; }
        public Vector3 Accelaration { get; set; }
        public Vector3 Velocity { get; set; }
        public Vector3 Position { get; set; }
        public float Scaling { get; set; }
        public Color ModColor { get; set; }
        public TextureQuad textureQuad { get; set; }

        public float Alpha
        {
            get { return textureQuad.Alpha; }
            set { textureQuad.Alpha = value; }
        }

        public Particle(int lifespan, Texture2D texture, GraphicsDevice graphicsDevice)
        {
            textureQuad = new TextureQuad(graphicsDevice, texture, texture.Width, texture.Height);
            LifeSpan = lifespan;
        }

        public void Update(double totalMilliseconds)
        {
            Position += Velocity;

            if (LifeSpan < (totalMilliseconds - BirthTime))
            {
                IsAlive = false;
            }
        }

        public void Draw(GraphicsDevice graphicsDevice, ICamera camera)
        {
            textureQuad.Draw(camera.View, camera.Projection
                , Matrix.CreateScale(Scaling) * Matrix.CreateBillboard(Position, camera.Position, camera.World.Up, camera.World.Forward)
                , camera.FarPlane);
        }
    }
}
