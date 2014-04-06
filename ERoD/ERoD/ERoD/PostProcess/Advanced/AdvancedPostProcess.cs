using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ERoD
{
    public class AdvancedPostProcess
    {
        public Vector2 HalfPixel;
        public Texture2D lastScene;

        public Texture2D originalScene;
        protected List<BasicPostProcess> postProcesses = new List<BasicPostProcess>();

        public bool Enabled = true;
        protected Game Game;

        public ICamera camera
        {
            get { return ((ICamera)Game.Services.GetService(typeof(ICamera))); }
        }

        public SpriteBatch spriteBatch
        {
            get { return (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch)); }
        }

        public AdvancedPostProcess(Game game)
        {
            Game = game;
        }

        public static AdvancedPostProcess CreateFromBasic(Game game, BasicPostProcess basic)
        {
            AdvancedPostProcess process = new AdvancedPostProcess(game);
            process.AddPostProcess(basic);
            return process;
        }

        public void AddPostProcess(BasicPostProcess postProcess)
        {
            postProcesses.Add(postProcess);
        }

        public virtual void Update(GameTime gameTime)
        {
            int maxProcess = postProcesses.Count;
            for (int p = 0; p < maxProcess; p++)
            {
                if (postProcesses[p].Enabled)
                {
                    postProcesses[p].Update(gameTime);
                }
            }
        }

        public virtual void Draw(GameTime gameTime, Texture2D scene, DeferredRenderer.DeferredRenderTarget target)
        {
            if (!Enabled)
                return;

            originalScene = scene;

            int maxProcess = postProcesses.Count;
            lastScene = null;

            for (int p = 0; p < maxProcess; p++)
            {
                if (postProcesses[p].Enabled)
                {
                    // Set Half Pixel value.
                    if (postProcesses[p].HalfPixel == Vector2.Zero)
                    {
                        postProcesses[p].HalfPixel = HalfPixel;
                    }
                    // Has the scene been rendered yet
                    if (lastScene == null)
                    {
                        lastScene = originalScene;
                    }

                    // Set G-buffers
                    postProcesses[p].DepthBuffer = target.depthMap;
                    postProcesses[p].NormalBuffer = target.normalMap;
                    postProcesses[p].ParticleBuffer = target.particleMap;

                    // Set original scene
                    postProcesses[p].originalBuffer = originalScene;

                    if (postProcesses[p].NewScene == null)
                    {
                        postProcesses[p].NewScene = new RenderTarget2D(Game.GraphicsDevice
                            , target.width
                            , target.height
                            , false
                            , SurfaceFormat.Color
                            , DepthFormat.None);
                    }
                    Viewport original = Game.GraphicsDevice.Viewport;
                    Game.GraphicsDevice.SetRenderTarget(postProcesses[p].NewScene);
                    Game.GraphicsDevice.Clear(Color.Black);

                    postProcesses[p].BackBuffer = lastScene;
                    Game.GraphicsDevice.Textures[0] = postProcesses[p].BackBuffer;
                    postProcesses[p].Draw(gameTime);

                    Game.GraphicsDevice.SetRenderTarget(null);
                    Game.GraphicsDevice.Viewport = original;
                    lastScene = postProcesses[p].NewScene;
                }
            }

            if (lastScene == null)
            {
                lastScene = scene;
            }
        }
    }
}
