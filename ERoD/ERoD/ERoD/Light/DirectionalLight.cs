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

        public DirectionalLight(Game game, Vector3 position, Vector3 target, Color color, float intensity)
            : base(game, position, color, intensity)
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
