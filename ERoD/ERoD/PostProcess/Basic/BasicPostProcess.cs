﻿﻿using System;
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
        
        protected Game Game;

        public ICamera Camera
        {
            get { return ((ICamera)Game.Services.GetService(typeof(ICamera))); }
        }

        //Drawing globals
        public Texture2D DepthBuffer;
        public Texture2D NormalBuffer;
        public Texture2D BackBuffer;
        public Texture2D ParticleBuffer;

        public Texture2D originalBuffer;

        //Enable effect
        public bool Enabled = true;

        public RenderTarget2D NewScene;
       
        public bool UsesVertexShader = false;
        //Spritebatch used if there is no use of a VertexShader
        public SpriteBatch spriteBatch;
        public SpriteSortMode SortMode = SpriteSortMode.Immediate;
        public BlendState Blend = BlendState.Opaque;
        public SamplerState Sampler = SamplerState.AnisotropicClamp;
        public SurfaceFormat newSceneSurfaceFormat = SurfaceFormat.Color;

        protected Effect effect;

        ScreenQuad screenQuad;


        public BasicPostProcess(Game game)
        {
            Game = game;
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        }

        public virtual void Update(GameTime gameTime) { }

        public virtual void Draw(GameTime gameTime)
        {
            if (Enabled)
            {
                if (screenQuad == null)
                {
                    screenQuad = new ScreenQuad(Game);
                    screenQuad.Initialize();
                }
                if (!UsesVertexShader)
                {
                    spriteBatch.Begin(SortMode, Blend, Sampler, DepthStencilState.None, RasterizerState.CullCounterClockwise);
                }
                else
                {
                    Game.GraphicsDevice.SamplerStates[0] = Sampler;
                }

                effect.CurrentTechnique.Passes[0].Apply();

                if (UsesVertexShader)
                {
                    screenQuad.Draw(-Vector2.One, Vector2.One);
                }
                else
                {
                    spriteBatch.Draw(BackBuffer, new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), Color.White);
                    spriteBatch.End();
                }
            }
       }
    }
}
