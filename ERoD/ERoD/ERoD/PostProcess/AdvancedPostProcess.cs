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

        public RenderTarget2D NewScene;

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

        public bool Enabled = true;

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

        public virtual void Draw(GameTime gameTime, Texture2D scene, Texture2D depth, Texture2D normal)
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

                    // Set G-buffers
                    postProcesses[p].DepthBuffer = depth;
                    postProcesses[p].NormalBuffer = normal;

                    // Set original scene
                    postProcesses[p].originalBuffer = originalScene;

                    Game.GraphicsDevice.SetRenderTarget(NewScene);
                    
                    Game.GraphicsDevice.Clear(Color.Black);
                    
                    // Has the scene been rendered yet
                    if (lastScene == null)
                    {
                        lastScene = originalScene;
                    }

                    postProcesses[p].BackBuffer = lastScene;

                    Game.GraphicsDevice.Textures[0] = postProcesses[p].BackBuffer;
                    Console.WriteLine("--------------");
                    Console.WriteLine("before {0}", Game.GraphicsDevice.Viewport.Bounds);
                    postProcesses[p].Draw(gameTime);

                    Console.WriteLine("after {0}", Game.GraphicsDevice.Viewport.Bounds);
                    Game.GraphicsDevice.SetRenderTarget(null);

                    lastScene = NewScene;
                }
            }

            if (lastScene == null)
            {
                lastScene = scene;
            }
        }
    }
}
