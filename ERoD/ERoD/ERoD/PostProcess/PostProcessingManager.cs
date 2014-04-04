using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ERoD
{
    public class PostProcessingManager
    {
        protected Game Game;
        public Texture2D Scene;

        public DeferredRenderer.DeferredRenderTarget target;

        protected List<AdvancedPostProcess> postProcessingEffects = new List<AdvancedPostProcess>();

        public Vector2 HalfPixel;
        public SpriteBatch spriteBatch;

        public PostProcessingManager(Game game, DeferredRenderer.DeferredRenderTarget target)
        {
            Game = game;
            this.target = target;
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        }

        public void AddEffect(AdvancedPostProcess ppEffect)
        {
            postProcessingEffects.Add(ppEffect);
        }

        public void AddEffect(BasicPostProcess ppEffect)
        {
            postProcessingEffects.Add(AdvancedPostProcess.CreateFromBasic(Game, ppEffect));
        }

        public virtual void Update(GameTime gameTime)
        {
            int maxEffect = postProcessingEffects.Count;
            for (int e = 0; e < maxEffect; e++)
            {
                if (postProcessingEffects[e].Enabled)
                {
                    postProcessingEffects[e].Update(gameTime);
                }
            }
        }

        public virtual void Draw(GameTime gameTime, RenderTarget2D finalTarget)
        {
            if (HalfPixel == Vector2.Zero)
            {
                HalfPixel = -new Vector2(.5f / (float)Game.GraphicsDevice.Viewport.Width,
                                     .5f / (float)Game.GraphicsDevice.Viewport.Height);
            }
            
            int maxEffect = postProcessingEffects.Count;

            Scene = target.finalBackBuffer;
            for (int e = 0; e < maxEffect; e++)
            {
                if (postProcessingEffects[e].Enabled)
                {
                    if (postProcessingEffects[e].HalfPixel == Vector2.Zero)
                    {
                        postProcessingEffects[e].HalfPixel = HalfPixel;
                    }

                    postProcessingEffects[e].Draw(gameTime, Scene, target);
                    Scene = postProcessingEffects[e].lastScene;
                }
            }

            Viewport tmp = Game.GraphicsDevice.Viewport;
            Game.GraphicsDevice.SetRenderTarget(finalTarget);
            Game.GraphicsDevice.Viewport = tmp;

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            spriteBatch.Draw(Scene, new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), Color.White);
            spriteBatch.End();
        }
    }
}
