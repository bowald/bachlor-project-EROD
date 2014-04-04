using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class PointLight : BaseLight, IPointLight
    {
        protected float radius;

        public PointLight(Vector3 position, Color color, float radius, float intensity)
            : base(position, color, intensity, false)
        {
            this.radius = radius;
        }

        public float Radius
        {
            get { return radius; }
            set { radius = value; }
        }

        public override string ToString()
        {
            return position.X + " "
                + position.Y + " "
                + position.Z + " "
                + radius + " "
                + color.R + " "
                + color.G + " "
                + color.B + " "
                + intensity;
        }
    }
}
