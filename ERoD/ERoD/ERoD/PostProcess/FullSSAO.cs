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

            BlurV = new BiliteradBlurV(game, 1.5f);
            BlurH = new BiliteradBlurH(game, 1.5f);

            blend = new Blend(game);

            AddPostProcess(ssao);
            //AddPostProcess(BlurV);
            //AddPostProcess(BlurH);
            //AddPostProcess(blend);
        }
        public override void Draw(GameTime gameTime, Texture2D scene)
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
                    {
                        postProcesses[p].Target = new RenderTarget2D(Game.GraphicsDevice, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);
                    }

                    Game.GraphicsDevice.SetRenderTarget(postProcesses[p].Target);

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