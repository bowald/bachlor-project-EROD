using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    class BaseLight : ILight
    {

        protected Vector3 position;
        protected Color color;
        protected float intensity;
        protected string name;

        protected Game Game;

        protected ICamera camera
        {
            get { return ((ICamera)Game.Services.GetService(typeof(ICamera))); }
        }

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public float Intensity
        {
            get { return intensity; }
            set { intensity = value; }
        }

        public Matrix View
        {
            get { return Matrix.Identity; }
        }

        public Matrix Projection
        {
            get { return Matrix.Identity; }
        }

        public BaseLight(Vector3 position, Color color, float intensity)
        {
            this.Position = position;
            this.Color = color;
            this.Intensity = intensity;
        }

        // Use this?? do we need Game here?
        public BaseLight(Game game, Vector3 position, Color color, float intensity)
            : this(position, color, intensity)
        {
            Game = game;
        }
    }
}
