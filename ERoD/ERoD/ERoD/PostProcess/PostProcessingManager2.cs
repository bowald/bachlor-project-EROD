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

        //public RenderTarget2D newScene;

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
            BasicEffects.Add(ppEfect);
        }

        private void UpdateList<T>(GameTime gameTime, List<T> list) where T : IPPEffect
        {
            int max = list.Count;
            for (int e = 0; e < max; e++)
            {
                if (list[e].Enabled)
                {
                    list[e].Update(gameTime);
                }
            }
        }

        private Texture2D drawList<T>(GameTime gameTime, Texture2D scene, Vector2 HalfPixel, List<T> list) where T : IPPEffect
        {
            int maxEffect = list.Count;
            for (int e = 0; e < maxEffect; e++)
            {
                if (list[e].Enabled)
                {
                    if (list[e].HalfPixel == Vector2.Zero)
                        list[e].HalfPixel = HalfPixel;

                    list[e].OrgScene = scene;
                    if(typeof(T) == typeof(AdvancedPostProcess)){
                        list[e].Draw(gameTime, Scene);
                    }

                    Scene = list[e].NewScene;
                }
            }
            return Scene;
        }

        public virtual void Update(GameTime gameTime)
        {
            UpdateList(gameTime, AdvancedEffects);
            UpdateList(gameTime, BasicEffects);
        }

        public virtual void Draw(GameTime gameTime, Texture2D scene)
        {
            if (HalfPixel == Vector2.Zero)
                HalfPixel = -new Vector2(.5f / (float)Game.GraphicsDevice.Viewport.Width,
                                     .5f / (float)Game.GraphicsDevice.Viewport.Height);

            

            Scene = scene;
            int maxEffect = AdvancedEffects.Count;
            for (int e = 0; e < maxEffect; e++)
            {
                if (AdvancedEffects[e].Enabled)
                {
                    if (AdvancedEffects[e].HalfPixel == Vector2.Zero)
                        AdvancedEffects[e].HalfPixel = HalfPixel;

                    AdvancedEffects[e].orgScene = scene;
                    AdvancedEffects[e].Draw(gameTime, Scene);
                    Scene = AdvancedEffects[e].lastScene;
                }
            }

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            spriteBatch.Draw(Scene, new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), Color.White);
            spriteBatch.End();
        }
    }
}
