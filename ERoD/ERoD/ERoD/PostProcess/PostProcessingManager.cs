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
        public Texture2D DepthBuffer;

        protected List<AdvancedPostProcess> postProcessingEffects = new List<AdvancedPostProcess>();

        public Vector2 HalfPixel;
        public SpriteBatch spriteBatch;

        public PostProcessingManager(Game game)
        {
            Game = game;
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        }

        public void AddEffect(AdvancedPostProcess ppEfect)
        {
            postProcessingEffects.Add(ppEfect);
        }
        public void AddEffect(BasicPostProcess ppEfect)
        {
            postProcessingEffects.Add(new AdvancedPostProcess(Game, ppEfect));
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

        public virtual void Draw(GameTime gameTime, Texture2D scene, Texture2D depth, Texture2D normal)
        {
            if (HalfPixel == Vector2.Zero)
                HalfPixel = -new Vector2(.5f / (float)Game.GraphicsDevice.Viewport.Width,
                                     .5f / (float)Game.GraphicsDevice.Viewport.Height);

            int maxEffect = postProcessingEffects.Count;

            Scene = scene;

            for (int e = 0; e < maxEffect; e++)
            {
                if (postProcessingEffects[e].Enabled)
                {
                    if (postProcessingEffects[e].HalfPixel == Vector2.Zero)
                        postProcessingEffects[e].HalfPixel = HalfPixel;

                    postProcessingEffects[e].orgScene = scene;
                    postProcessingEffects[e].Draw(gameTime, Scene, depth, normal);
                    Scene = postProcessingEffects[e].lastScene;
                }
            }

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            spriteBatch.Draw(Scene, new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), Color.White);
            spriteBatch.End();
        }
    }
}
