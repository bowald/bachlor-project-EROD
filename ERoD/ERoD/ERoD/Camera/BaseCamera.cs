using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class BaseCamera : GameComponent, ICamera
    {
        protected Matrix view;
        protected Vector3 position;
        protected Matrix projection;
        protected Quaternion rotation;
        protected Matrix world;
        protected Viewport viewport;
        protected float nearPlane;
        protected float farPlane;

        public Matrix View
        {
            get{ return view; }
            set{ view = value; }
        }

        public Matrix Projection
        {
            get { return projection; }
        }

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Quaternion Rotation
        {
            get{ return rotation; }
            set{ rotation = value; }
        }

        public Matrix World
        {
            get { return world; }
        }

        public Viewport Viewport
        {
            get { return viewport; }
            set { viewport = value; }
        }

        public float NearPlane
        {
            get { return nearPlane; }
        }

        public float FarPlane
        {
            get { return farPlane; }
        }

        protected BaseCamera(Game game, float nearPlane, float farPlane) : base(game)
        {
            this.nearPlane = nearPlane;
            this.farPlane = farPlane;

            game.Components.Add(this);
        }

        public override void Initialize()
        {
            viewport = Game.GraphicsDevice.Viewport;
            viewport.MinDepth = nearPlane;
            viewport.MaxDepth = farPlane;

            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4
                , viewport.AspectRatio, viewport.MinDepth, viewport.MaxDepth);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        //public override void Dispose()
        //{
        //    base.Dispose();
        //    Game.Components.Remove(this);
        //}


        
    }
}
