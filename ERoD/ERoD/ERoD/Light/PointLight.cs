using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    class PointLight : BaseLight, IPointLight
    {
        protected float radius;

        public PointLight(Vector3 position, Color color, float radius, float intensity)
            : base(position, color, intensity)
        {
            this.radius = radius;
        }

        public float Radius
        {
            get { return radius; }
            set { radius = value; }
        }
    }
}
