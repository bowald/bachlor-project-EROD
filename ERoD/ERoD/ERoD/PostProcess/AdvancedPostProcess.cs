using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ERoD
{
    public class AdvancedPostProcess : IPPEffect
    {
        protected List<BasicPostProcess> postProcesses = new List<BasicPostProcess>();

        protected Game Game;

        public ICamera camera
        {
            get { return ((ICamera)Game.Services.GetService(typeof(ICamera))); }
        }

        public SpriteBatch spriteBatch
        {
            get { return (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch)); }
        }

        protected Texture2D newScene;
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

        protected Vector2 halfPixel;
        public Vector2 HalfPixel
        {
            get { return halfPixel; }
            set { halfPixel = value; }
        }
        protected Boolean enable;
        public bool Enabled
        {
            get { return enable; }
            set { enable = value; }
        }

        public AdvancedPostProcess(Game game)
        {
            Game = game;
            enable = true;
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

        public virtual void Draw(GameTime gameTime, Texture2D scene)
        {
            if (!Enabled)
                return;

            orgScene = scene;

            int maxProcess = postProcesses.Count;
            newScene = null;

            for (int p = 0; p < maxProcess; p++)
            {
                if (postProcesses[p].Enabled)
                {
                    // Set Half Pixel value.
                    if (postProcesses[p].HalfPixel == Vector2.Zero)
                        postProcesses[p].HalfPixel = HalfPixel;

                    // Set original scene
                    postProcesses[p].OrgScene = orgScene;

                    // Ready render target if needed.
                    if (postProcesses[p].newScene == null)
                        postProcesses[p].newScene = new RenderTarget2D(Game.GraphicsDevice, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height, false, postProcesses[p].newSceneSurfaceFormat, DepthFormat.None);

                    Game.GraphicsDevice.SetRenderTarget(postProcesses[p].newScene);
                    
                    Game.GraphicsDevice.Clear(Color.Black);
                    
                    // Has the scene been rendered yet (first effect may be disabled)
                    if (newScene == null)
                        newScene = orgScene;

                    postProcesses[p].OrgScene = newScene;

                    //postProcesses[p].DepthBuffer = depth;
                    //postProcesses[p].normalBuffer = normal;
                    Game.GraphicsDevice.Textures[0] = postProcesses[p].OrgScene;
                    postProcesses[p].Draw(gameTime);

                    Game.GraphicsDevice.SetRenderTarget(null);

                    newScene = postProcesses[p].newScene;
                }
            }

            if (newScene == null)
                newScene = scene;
        }
    }
}
