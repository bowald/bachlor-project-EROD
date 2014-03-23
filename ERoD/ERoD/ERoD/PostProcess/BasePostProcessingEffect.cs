using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ERoD
{
    public class BasePostProcessingEffect
    {
        public Vector2 HalfPixel;
        public Texture2D lastScene;
        public Texture2D orgScene;
        protected List<PostProcess> postProcesses = new List<PostProcess>();

        protected Game Game;

        public ICamera camera
        {
            get { return ((ICamera)Game.Services.GetService(typeof(ICamera))); }
        }

        public SpriteBatch spriteBatch
        {
            get { return (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch)); }
        }

        public BasePostProcessingEffect(Game game)
        {
            Game = game;
        }

        public bool Enabled = true;

        public void AddPostProcess(PostProcess postProcess)
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

        public virtual void Draw(GameTime gameTime, Texture2D scene)
        {
            if (!Enabled)
                return;

            orgScene = scene;

            int maxProcess = postProcesses.Count;
            lastScene = null;

            for (int p = 0; p < maxProcess; p++)
            {
                if (postProcesses[p].Enabled)
                {
                    // Set Half Pixel value.
                    if (postProcesses[p].HalfPixel == Vector2.Zero)
                        postProcesses[p].HalfPixel = HalfPixel;

                    // Set original scene
                    postProcesses[p].orgBuffer = orgScene;

                    // Ready render target if needed.
                    if (postProcesses[p].newScene == null)
                        postProcesses[p].newScene = new RenderTarget2D(Game.GraphicsDevice, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height, false, postProcesses[p].newSceneSurfaceFormat, DepthFormat.None);

                    Game.GraphicsDevice.SetRenderTarget(postProcesses[p].newScene);
                    
                    Game.GraphicsDevice.Clear(Color.Black);
                    
                    // Has the scene been rendered yet (first effect may be disabled)
                    if (lastScene == null)
                        lastScene = orgScene;

                    postProcesses[p].BackBuffer = lastScene;

                    //postProcesses[p].DepthBuffer = depth;
                    //postProcesses[p].normalBuffer = normal;
                    Game.GraphicsDevice.Textures[0] = postProcesses[p].BackBuffer;
                    postProcesses[p].Draw(gameTime);

                    Game.GraphicsDevice.SetRenderTarget(null);

                    lastScene = postProcesses[p].newScene;
                }
            }

            if (lastScene == null)
                lastScene = scene;
        }
    }
}
