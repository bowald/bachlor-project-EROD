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
        protected List<BasicPostProcess> postProcesses = new List<BasicPostProcess>();

        protected Game Game;
        public Texture2D LastScene;
        public Texture2D OrgScene;
        public Boolean Enable;
        public Vector2 HalfPixel;

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
            Enable = true;
        }
        public AdvancedPostProcess(Game game, BasicPostProcess BPP)
        {
            Game = game;
            Enable = true;
            AddPostProcess(BPP);
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
                if (postProcesses[p].Enable)
                {
                    postProcesses[p].Update(gameTime);
                }
            }
        }

        public virtual void Draw(GameTime gameTime, Texture2D scene)
        {
            if (!Enable)
                return;

            OrgScene = scene;

            int maxProcess = postProcesses.Count;
            LastScene = null;

            for (int p = 0; p < maxProcess; p++)
            {
                if (postProcesses[p].Enable)
                {
                    // Set Half Pixel value.
                    if (postProcesses[p].HalfPixel == Vector2.Zero)
                        postProcesses[p].HalfPixel = HalfPixel;

                    // Set original scene
                    postProcesses[p].Original = OrgScene;

                    // Ready render target if needed.
                    if (postProcesses[p].Target == null)
                        postProcesses[p].Target = new RenderTarget2D(Game.GraphicsDevice, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height, false, postProcesses[p].newSceneSurfaceFormat, DepthFormat.None);

                    Game.GraphicsDevice.SetRenderTarget(postProcesses[p].Target);
                    
                    Game.GraphicsDevice.Clear(Color.Black);
                    
                    // Has the scene been rendered yet (first effect may be disabled)
                    if (LastScene == null)
                        LastScene = OrgScene;

                    postProcesses[p].AfterEffect = LastScene;

                    //postProcesses[p].DepthBuffer = depth;
                    //postProcesses[p].normalBuffer = normal;
                    Game.GraphicsDevice.Textures[0] = postProcesses[p].AfterEffect;
                    postProcesses[p].Draw(gameTime);

                    Game.GraphicsDevice.SetRenderTarget(null);

                    LastScene = postProcesses[p].Target;
                }
            }

            if (LastScene == null)
                LastScene = scene;
        }
    }
}
