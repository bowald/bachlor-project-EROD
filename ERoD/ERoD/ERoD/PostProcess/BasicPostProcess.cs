﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ERoD
{
    public class BasicPostProcess
    {
        public Boolean Enable;

        public ICamera camera
        {
            get { return ((ICamera)Game.Services.GetService(typeof(ICamera))); }
        }

        public SpriteBatch spriteBatch
        {
            get { return (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch)); }
        }
        public Vector2 HalfPixel;
        public Texture2D AfterEffect;    //
        public Texture2D Original;
        public RenderTarget2D Target;

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
            Enable = true;

        }
        public virtual void Update(GameTime gameTime)
        {

        }
        public virtual void Draw(GameTime gameTime)
        {
            if (Enable)
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
                    spriteBatch.Draw(AfterEffect, new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), Color.White);
                    spriteBatch.End();
                }
            }
        }
    }
}
