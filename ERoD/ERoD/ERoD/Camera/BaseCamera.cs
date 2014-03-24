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
        protected BoundingFrustum frustum;
        protected float fieldOfView;
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

        public BoundingFrustum Frustum
        {
            get { return frustum; }
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

        protected float tanFovy;
        public float TanFovy
        {
            get { return tanFovy; }
        }

        public float AspectRatio
        {
            get { return Game.GraphicsDevice.Viewport.AspectRatio; }
        }

        protected BaseCamera(Game game, float nearPlane, float farPlane) : base(game)
        {
            this.nearPlane = nearPlane;
            this.farPlane = farPlane;

            this.fieldOfView = MathHelper.PiOver4;
            this.tanFovy = (float)Math.Tan(fieldOfView / 2);

            game.Components.Add(this);
        }

        public override void Initialize()
        {
            viewport = Game.GraphicsDevice.Viewport;
            viewport.MinDepth = nearPlane;
            viewport.MaxDepth = farPlane;

            projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView
                , viewport.AspectRatio, viewport.MinDepth, viewport.MaxDepth);

            frustum = new BoundingFrustum(View * Projection);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
