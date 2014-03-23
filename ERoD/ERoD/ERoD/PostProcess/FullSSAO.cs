using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ERoD
{
    public class FullSSAO : BasePostProcessingEffect
    {
        public BasicSSAO ssao;
        //public WorldPositionMap posMap;
        public BiliteradBlurV BlurV;
        public BiliteradBlurH BlurH;
        //public SceneBlend blend;

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

            //posMap = new WorldPositionMap(game);

            BlurV = new BiliteradBlurV(game, 1.5f);
            BlurH = new BiliteradBlurH(game, 1.5f);
            //blur.Sampler = SamplerState.PointClamp;

            //blend = new SceneBlend(game);
            //blend.Blend = true;
            //blend.Sampler = SamplerState.PointClamp;


            //AddPostProcess(posMap);
            AddPostProcess(ssao);
            AddPostProcess(BlurV);
            AddPostProcess(BlurH);
            //AddPostProcess(blend);
        }
        public override void Draw(GameTime gameTime, Texture2D scene)
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
                        //if (postProcesses[p] is WorldPositionMap)
                        //    postProcesses[p].newScene = new RenderTarget2D(Game.GraphicsDevice, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height, false, SurfaceFormat.Vector4, DepthFormat.None);
                        //else
                            postProcesses[p].newScene = new RenderTarget2D(Game.GraphicsDevice, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);
                    }

                    Game.GraphicsDevice.SetRenderTarget(postProcesses[p].newScene);

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
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;

//namespace ERoD
//{
//    public class FullSSAO : PostProcess
//    {
//        protected List<PostProcess> postProcesses = new List<PostProcess>();
//        public BasicSSAO ssao;
//        //public WorldPositionMap posMap;
//        //public PoissonDiscBlur blur;
//        //public Blend blend;

//        public float rad
//        {
//            get { return ssao.rad; }
//            set { ssao.rad = value; }
//        }
//        public float intensity
//        {
//            get { return ssao.intensity; }
//            set { ssao.intensity = value; }
//        }
//        public float scale
//        {
//            get { return ssao.scale; }
//            set { ssao.scale = value; }
//        }
//        public float bias
//        {
//            get { return ssao.bias; }
//            set { ssao.bias = value; }
//        }

//        public FullSSAO(ERoD game, float radius, float intensity, float scale, float bias)
//            : base(game)
//        {
//            ssao = new BasicSSAO(game, radius, intensity, scale, bias);

//            //posMap = new WorldPositionMap(game);

//            //blur = new PoissonDiscBlur(game);
//            //blur.Sampler = SamplerState.PointClamp;

//            //blend = new Blend(game);
//            //blend.Sampler = SamplerState.PointClamp;

//            //AddPostProcess(posMap);
//            postProcesses.Add(ssao);
//            //AddPostProcess(blur);
//            //postProcesses.Add(blend);

//        }

//        public override void Draw(GameTime gameTime)
//        {
//            orgBuffer = Game.Renderer.finalBackBuffer;

//            int maxProcess = postProcesses.Count;
//            BackBuffer = null;

//            for (int p = 0; p < maxProcess; p++)
//            {
//                if (postProcesses[p].Enabled)
//                {
//                    // Set Half Pixel value.
//                    if (postProcesses[p].HalfPixel == Vector2.Zero)
//                        postProcesses[p].HalfPixel = HalfPixel;

//                    // Set original scene
//                    postProcesses[p].orgBuffer = orgBuffer;

//                    // Ready render target if needed.
//                    if (postProcesses[p].newScene == null)
//                    {
//                        postProcesses[p].newScene = new RenderTarget2D(Game.GraphicsDevice, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);
//                    }

//                    Game.GraphicsDevice.SetRenderTarget(postProcesses[p].newScene);

//                    // Has the scene been rendered yet (first effect may be disabled)
//                    if (BackBuffer == null)
//                        BackBuffer = orgBuffer;

//                    postProcesses[p].BackBuffer = BackBuffer;

//                    //postProcesses[p].DepthBuffer = depth;
//                    //postProcesses[p].normalBuffer = normal;
//                    Game.GraphicsDevice.Textures[0] = postProcesses[p].BackBuffer;
//                    postProcesses[p].Draw(gameTime);

//                    Game.GraphicsDevice.SetRenderTarget(null);

//                    BackBuffer = postProcesses[p].newScene;
//                }
//            }

//            if (BackBuffer == null)
//                BackBuffer = orgBuffer;
//        }
//    }
//}
