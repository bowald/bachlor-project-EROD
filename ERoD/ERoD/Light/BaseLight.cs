﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class BaseLight : ILight
    {

        protected Vector3 position;
        protected Color color;
        protected float intensity;
        protected bool castShadow;
        protected ShadowRenderer.CascadeShadowMapEntry shadowMapEntry;
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

        public bool CastShadow
        {
            get { return castShadow; }
            set { castShadow = value; }
        }

        public ShadowRenderer.CascadeShadowMapEntry ShadowMapEntry
        {
            get { return shadowMapEntry; }
            set { shadowMapEntry = value; }
        }

        public BaseLight(Vector3 position, Color color, float intensity, bool castShadow)
        {
            this.Position = position;
            this.Color = color;
            this.Intensity = intensity;
            this.CastShadow = castShadow;
            shadowMapEntry = new ShadowRenderer.CascadeShadowMapEntry();
        }

        // Use this?? do we need Game here?
        public BaseLight(Game game, Vector3 position, Color color, float intensity, bool castShadow)
            : this(position, color, intensity, castShadow)
        {
            Game = game;
        }
    }
}
