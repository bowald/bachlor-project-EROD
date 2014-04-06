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
        public Vector2 HalfPixel;

        public ICamera camera
        {
            get { return ((ICamera)Game.Services.GetService(typeof(ICamera))); }
        }

        //Drawing globals
        public Texture2D DepthBuffer;
        public Texture2D BackBuffer;
        public Texture2D orgBuffer;
        public Texture2D NormalBuffer;
        public RenderTarget2D newScene;

        //Enable effect
        public bool Enabled = true;

        public bool UsesVertexShader = false;
        //Spritebatch used if there is no use of a VertexShader
        public SpriteBatch spriteBatch;
        public SpriteSortMode SortMode = SpriteSortMode.Immediate;
        public BlendState Blend = BlendState.Opaque;
        public SamplerState Sampler = SamplerState.AnisotropicClamp;
        public SurfaceFormat newSceneSurfaceFormat = SurfaceFormat.Color;


        ScreenQuad sq;

        protected Effect effect;
        protected Game Game;
        

        public BasicPostProcess(Game game)
        {
            Game = game;
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
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
                    spriteBatch.Draw(BackBuffer, new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), Color.White);
                    spriteBatch.End();
                }
            }
       }
    }
}
