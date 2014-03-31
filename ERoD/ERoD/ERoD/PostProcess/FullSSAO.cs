using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ERoD
{
    public class FullSSAO : AdvancedPostProcess
    {
        public BasicSSAO ssao;
        public BasicSSAO_SC2 ssao2;
        public BiliteradBlurV BlurV;
        public BiliteradBlurH BlurH;
        public Blend blend;

        public float rad
        {
            get { return ssao.rad; }
            set { ssao.rad = value; }
        }
        public float intensity
        {
            get { return ssao.intensity; }
            set { ssao.intensity = value; }
        }
        public float scale
        {
            get { return ssao.scale; }
            set { ssao.scale = value; }
        }
        public float bias
        {
            get { return ssao.bias; }
            set { ssao.bias = value; }
        }

        public FullSSAO(ERoD game, float radius, float intensity, float scale, float bias)
            : base(game)
        {
            ssao = new BasicSSAO(game, radius, intensity, scale, bias);
            ssao2 = new BasicSSAO_SC2(game, 3f, 0.2f, 5.0f, 2f);

            BlurV = new BiliteradBlurV(game, 2.0f);
            BlurH = new BiliteradBlurH(game, 2.0f);

            blend = new Blend(game);

            AddPostProcess(ssao2);
            //AddPostProcess(BlurV);
            //AddPostProcess(BlurH);
            //AddPostProcess(blend);
        }
        public override void Draw(GameTime gameTime, Texture2D scene, Texture2D depth, Texture2D normal)
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
                    {
                        postProcesses[p].newScene = new RenderTarget2D(Game.GraphicsDevice, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);
                    }

                    Game.GraphicsDevice.SetRenderTarget(postProcesses[p].newScene);

                    // Has the scene been rendered yet (first effect may be disabled)
                    if (lastScene == null)
                        lastScene = orgScene;

                    postProcesses[p].BackBuffer = lastScene;

                    postProcesses[p].DepthBuffer = depth;
                    postProcesses[p].NormalBuffer = normal;
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