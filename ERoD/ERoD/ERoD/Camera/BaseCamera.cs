using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    class BaseCamera : GameComponent, ICamera
    {
        protected Matrix view;
        protected Vector3 position;
        protected Matrix projection;
        protected Quaternion rotation;
        protected Matrix world;
        protected Viewport viewport;
        protected float nearPlane;
        protected float farPlane;

        Matrix ICamera.View
        {
            get{ return view; }
            set{ view = value; }
        }

        Matrix ICamera.Projection
        {
            get { return projection; }
        }

        Vector3 ICamera.Position
        {
            get { return position; }
            set { position = value; }
        }

        Quaternion ICamera.Rotation
        {
            get{ return rotation; }
            set{ rotation = value; }
        }

        Matrix ICamera.World
        {
            get { return world; }
        }

        Viewport ICamera.Viewport
        {
            get { return viewport; }
            set { viewport = value; }
        }

        public BaseCamera(Game game, float nearPlane, float farPlane) : base(game)
        {
            position = Vector3.Zero;
            this.nearPlane = nearPlane;
            this.farPlane = farPlane;

            game.Components.Add(this);
        }

        public override void Initialize()
        {
            viewport = Game.GraphicsDevice.Viewport;
            viewport.MinDepth = nearPlane;
            viewport.MaxDepth = farPlane;
        }
        public override void Update(GameTime gameTime)
        {
            world = Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(position);
            view = Matrix.Invert(world);

            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, viewport.AspectRatio, viewport.MinDepth, viewport.MaxDepth);

            base.Update(gameTime);
        }
        public virtual void Dispose()
        {
            base.Dispose();
            Game.Components.Remove(this);
        }
    }
}
