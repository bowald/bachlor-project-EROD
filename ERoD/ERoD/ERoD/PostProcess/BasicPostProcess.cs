﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ERoD
{
    public class BasicPostProcess : IPPEffect
    {
        protected Boolean enable;
        public bool Enabled
        {
            get { return enable; }
            set { enable = value; }
        }
        public ICamera camera
        {
            get { return ((ICamera)Game.Services.GetService(typeof(ICamera))); }
        }

        public SpriteBatch spriteBatch
        {
            get { return (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch)); }
        }

        protected Vector2 halfPixel;
        public Vector2 HalfPixel
        {
            get { return halfPixel; }
            set { halfPixel = value; }
        }

        public Texture2D newScene;
        public Texture2D NewScene
        {
            get { return newScene; }
            set { newScene = value; }
        }

        protected Texture2D orgScene;
        public Texture2D OrgScene
        {
            get { return orgScene; }
            set { orgScene = value; }
        }
        public Texture2D DepthBuffer;
        public Texture2D normalBuffer;

        
        public SpriteSortMode SortMode = SpriteSortMode.Immediate;
        public BlendState Blend = BlendState.Opaque;
        public SamplerState Sampler = SamplerState.AnisotropicClamp;

        public SurfaceFormat newSceneSurfaceFormat = SurfaceFormat.Color;

        protected Effect effect;

        protected ERoD Game;

        ScreenQuad sq;

        public bool UsesVertexShader = false;

        public BasicPostProcess(ERoD game)
        {
            Game = game;
            enable = true;

        }
        public virtual void Update(GameTime gameTime)
        {

        }
        public virtual void Draw(GameTime gameTime)
        {
            if (Enabled)
            {
                if (sq == null)
                {
                    sq = new ScreenQuad(Game);
                    sq.Initialize();
                }
                if (!UsesVertexShader)
                    spriteBatch.Begin(SortMode, Blend, Sampler, DepthStencilState.None, RasterizerState.CullCounterClockwise);
                else
                {
                    Game.GraphicsDevice.SamplerStates[0] = Sampler;
                }

                effect.CurrentTechnique.Passes[0].Apply();

                if (UsesVertexShader)
                    sq.Draw(-Vector2.One, Vector2.One);
                else
                {
                    spriteBatch.Draw(newScene, new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), Color.White);
                    spriteBatch.End();
                }
            }
        }
    }
}
