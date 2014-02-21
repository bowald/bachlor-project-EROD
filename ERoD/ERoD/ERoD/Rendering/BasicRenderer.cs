using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class BasicRenderer : DrawableGameComponent
    {
        public RenderTarget2D depthMap;
        public RenderTarget2D colorMap;
        public RenderTarget2D normalMap;
        public RenderTarget2D lightMap;
        public RenderTarget2D finalBackBuffer;
        public RenderTarget2D blendedDepthBuffer;

        public Model pointLightMesh;

        public BasicRenderer(Game game) : base(game)
        {
            game.Components.Add(this);
        }

        protected override void LoadContent()
        {
            int width = GraphicsDevice.Viewport.Width;
            int height = GraphicsDevice.Viewport.Height;

            depthMap = new RenderTarget2D(GraphicsDevice, width, height, false,
                SurfaceFormat.Single, DepthFormat.Depth24Stencil8);

            colorMap = new RenderTarget2D(GraphicsDevice, width, height, false,
                SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

            normalMap = new RenderTarget2D(GraphicsDevice, width, height, false,
                SurfaceFormat.Rgba1010102, DepthFormat.None);

            lightMap = new RenderTarget2D(GraphicsDevice, width, height, false,
                SurfaceFormat.Color, DepthFormat.None);

            finalBackBuffer = new RenderTarget2D(GraphicsDevice, width, height, false,
                SurfaceFormat.Color, DepthFormat.None);

            blendedDepthBuffer = new RenderTarget2D(GraphicsDevice, width, height, false,
                SurfaceFormat.Rg32, DepthFormat.None);
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Draw(GameTime gameTime)
        {
            RenderDeferred(gameTime);
        }

        private void RenderDeferred(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTargets(colorMap, normalMap, depthMap);

            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            foreach (GameComponent component in Game.Components)
            {
                if (component is IDeferredRender)
                {
                    ((IDeferredRender)component).Draw(gameTime);
                }
            }

            GraphicsDevice.SetRenderTarget(null);

            //DeferredLightning(gameTime);

            //GraphicsDevice.SetRenderTargets(finalBackBuffer, blendedDepthBuffer);

            //GraphicsDevice.Clear(Color.Black);

            //DrawDeferred();
            //draw non deferred.. transparent objs??

            GraphicsDevice.SetRenderTarget(null);
        }

        private void DeferredLightning(GameTime gameTime)
        {

        }

        private void DrawDeferred()
        {

        }
    }
}
