using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ERoD
{
    class DirectionalLight : BaseLight, IDirectionalLight
    {
        protected Vector3 target;

        public new Matrix View
        {
            get
            {
                return Matrix.CreateLookAt(position, target, Vector3.Transform(Vector3.Forward, Matrix.Invert(Matrix.CreateTranslation(position))));
            }
        }

        public new Matrix Projection
        {
            get
            {
                return Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4 / 2, camera.Viewport.AspectRatio, camera.Viewport.MinDepth, camera.Viewport.MaxDepth);
            }
        }

        public DirectionalLight(Game game, Vector3 position, Vector3 target, Color color, float intensity, bool castShadow)
            : base(game, position, color, intensity, castShadow)
        {
            this.target = target;
        }

        #region IDirectionalLight Members

        public Vector3 Target
        {
            get { return target; }
            set { target = value; }
        }

        public Vector3 Direction
        {
            get { return target - position; }
        }

        #endregion
    }
}
