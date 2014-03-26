using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ERoD
{
    public class PostProcessingManager2
    {
        protected Game Game;
        public Texture2D Scene;
        public Texture2D DepthBuffer;

        protected List<AdvancedPostProcess> AdvancedEffects = new List<AdvancedPostProcess>();
        protected List<BasicPostProcess> BasicEffects = new List<BasicPostProcess>();
        public Vector2 HalfPixel;

        public SpriteBatch spriteBatch;
        //{
        //    get { return (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch)); }
        //}

        public PostProcessingManager2(Game game)
        {
            Game = game;
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        }

        public void AddEffect(AdvancedPostProcess ppEfect)
        {
            AdvancedEffects.Add(ppEfect);
        }

        public void AddEffect(BasicPostProcess ppEfect)
        {
            AdvancedEffects.Add(new AdvancedPostProcess(Game, ppEfect));
        }

        public virtual void Update(GameTime gameTime)
        {
            int max = AdvancedEffects.Count;
            for (int e = 0; e < max; e++)
            {
                if (AdvancedEffects[e].Enable)
                {
                    AdvancedEffects[e].Update(gameTime);
                }
            }
        }

        public virtual void Draw(GameTime gameTime, Texture2D scene)
        {
            if (HalfPixel == Vector2.Zero)
                HalfPixel = -new Vector2(.5f / (float)Game.GraphicsDevice.Viewport.Width,
                                     .5f / (float)Game.GraphicsDevice.Viewport.Height);

            int maxEffect = AdvancedEffects.Count;

            Scene = scene;

            for (int e = 0; e < maxEffect; e++)
            {
                if (AdvancedEffects[e].Enable)
                {
                    if (AdvancedEffects[e].HalfPixel == Vector2.Zero)
                        AdvancedEffects[e].HalfPixel = HalfPixel;

                    AdvancedEffects[e].OrgScene = scene;
                    AdvancedEffects[e].Draw(gameTime, Scene);
                    Scene = AdvancedEffects[e].LastScene;
                }
            }

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            spriteBatch.Draw(Scene, new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), Color.White);
            spriteBatch.End();
        }
    }
}
