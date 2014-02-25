using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class DeferredRenderer : DrawableGameComponent
    {
        public ICamera Camera
        {
            get { return ((ICamera)Game.Services.GetService(typeof(ICamera))); }
        }

        public RenderTarget2D depthMap;
        public RenderTarget2D colorMap;
        public RenderTarget2D normalMap;
        public RenderTarget2D lightMap;
        public RenderTarget2D finalBackBuffer;
        //public RenderTarget2D blendedDepthBuffer;

        SpriteBatch spriteBatch;

        Model pointLightMesh;
        Matrix[] boneTransforms;

        Effect pointLightShader;
        Effect directionalLightShader;
        Effect deferredShader;
        Effect deferredShadowShader;

        public List<IPointLight> PointLights = new List<IPointLight>();
        public List<IDirectionalLight> DirectionalLights = new List<IDirectionalLight>();

        Vector2 halfPixel;
        private int shadowMapSize = 3;

        ScreenQuad sceneQuad;

        public DeferredRenderer(Game game) : base(game)
        {
            game.Components.Add(this);
            sceneQuad = new ScreenQuad(game);
        }

        protected override void LoadContent()
        {
            int width = GraphicsDevice.Viewport.Width;
            int height = GraphicsDevice.Viewport.Height;

            halfPixel = -new Vector2(0.5f / (float)width, 0.5f / (float)height);

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

            //blendedDepthBuffer = new RenderTarget2D(GraphicsDevice, width, height, false,
            //    SurfaceFormat.Rg32, DepthFormat.None);

            directionalLightShader = Game.Content.Load<Effect>("Shaders/DirectionalLightShader");

            pointLightShader = Game.Content.Load<Effect>("Shaders/PointLightShader");
            deferredShader = Game.Content.Load<Effect>("Shaders/DeferredRender");
            deferredShadowShader = Game.Content.Load<Effect>("Shaders/DeferredShadowShader");

            // Debug depth renderer
            DepthRender = Game.Content.Load<Effect>("Shaders/depthRender");
            w = GraphicsDevice.Viewport.Width / 5;
            h = (int) (GraphicsDevice.Viewport.Height / 3.5f);

            pointLightMesh = Game.Content.Load<Model>("Models/lightmesh");
            pointLightMesh.Meshes[0].MeshParts[0].Effect = pointLightShader;

            boneTransforms = new Matrix[pointLightMesh.Bones.Count];
            pointLightMesh.CopyAbsoluteBoneTransformsTo(boneTransforms);

            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        }

        public override void Initialize()
        {
            sceneQuad.Initialize();
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

            DeferredShadows(gameTime);
            DeferredLightning(gameTime);

            GraphicsDevice.SetRenderTargets(finalBackBuffer);

            DrawDeferred();

            GraphicsDevice.SetRenderTarget(null);
        }

        private void DeferredShadows(GameTime gameTime)
        {
            List<ILight> lights = new List<ILight>(DirectionalLights.Where(entity => entity.Intensity > 0 && entity.Color != Color.Black && entity.CastShadow));

            List<ILight> needShadowMaps = new List<ILight>(lights.Where(entity => entity.ShadowMap == null));

            int width = GraphicsDevice.Viewport.Width;
            int height = GraphicsDevice.Viewport.Height;

            foreach (ILight light in needShadowMaps)
            {
                light.ShadowMap = new RenderTarget2D(GraphicsDevice, width * shadowMapSize, height * shadowMapSize,
                    false, SurfaceFormat.Single, DepthFormat.None);
                //light.SoftShadowMap = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None);
            }

            foreach (ILight light in lights)
            {
                // Clear shadow map..
                GraphicsDevice.SetRenderTarget(light.ShadowMap);
                GraphicsDevice.Clear(Color.Transparent);

                deferredShadowShader.Parameters["vp"].SetValue(light.View * light.Projection);

                foreach (GameComponent component in Game.Components)
                {
                    if (component is IDeferredRender)
                    {
                        ((IDeferredRender)component).Draw(gameTime, deferredShadowShader);
                    }
                }
            }
        }

              
        private void DeferredLightning(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(lightMap);

            GraphicsDevice.Clear(Color.Transparent);
            GraphicsDevice.BlendState = BlendState.Additive;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

            foreach (IDirectionalLight dirLight in DirectionalLights)
            {
                RenderDirectionalLight(dirLight);
            }

            // for all lights, render lights
            foreach (IPointLight pointLight in PointLights) 
            {
                RenderPointLight(pointLight);
            }

            GraphicsDevice.SetRenderTarget(null);
        }

        private void RenderDirectionalLight(IDirectionalLight directionalLight)
        {
            // Load Light Params
            directionalLightShader.Parameters["halfPixel"].SetValue(halfPixel);
            directionalLightShader.Parameters["lightDirection"].SetValue(directionalLight.Direction);
            directionalLightShader.Parameters["color"].SetValue(directionalLight.Color.ToVector3());

            directionalLightShader.Parameters["normalMap"].SetValue(normalMap);
            directionalLightShader.Parameters["depthMap"].SetValue(depthMap);
            directionalLightShader.Parameters["power"].SetValue(directionalLight.Intensity);

            directionalLightShader.Parameters["cameraPosition"].SetValue(Camera.Position);
            directionalLightShader.Parameters["viewProjectionInv"].SetValue(Matrix.Invert(Camera.View 
                * Camera.Projection));
            directionalLightShader.Parameters["lightViewProjection"].SetValue(directionalLight.View
                * directionalLight.Projection);

            directionalLightShader.Parameters["castShadow"].SetValue(directionalLight.CastShadow);
            if (directionalLight.CastShadow)
            {
                directionalLightShader.Parameters["shadowMap"].SetValue(directionalLight.ShadowMap);
            }

            directionalLightShader.Techniques[0].Passes[0].Apply();

            sceneQuad.Draw(-Vector2.One, Vector2.One);
        }

        private void RenderPointLight(IPointLight pointLight)
        {
            pointLightShader.Parameters["halfPixel"].SetValue(halfPixel);
            pointLightShader.Parameters["colorMap"].SetValue(colorMap);
            pointLightShader.Parameters["normalMap"].SetValue(normalMap);
            pointLightShader.Parameters["depthMap"].SetValue(depthMap);

            Matrix sphereWorldMatrix = Matrix.CreateScale(pointLight.Radius) 
                * Matrix.CreateTranslation(pointLight.Position);

            pointLightShader.Parameters["World"].SetValue(sphereWorldMatrix);
            pointLightShader.Parameters["View"].SetValue(Camera.View);
            pointLightShader.Parameters["Projection"].SetValue(Camera.Projection);

            pointLightShader.Parameters["lightPosition"].SetValue(pointLight.Position);

            pointLightShader.Parameters["Color"].SetValue(pointLight.Color.ToVector3());
            pointLightShader.Parameters["lightRadius"].SetValue(pointLight.Radius);
            pointLightShader.Parameters["lightIntensity"].SetValue(pointLight.Intensity);

            pointLightShader.Parameters["CameraPosition"].SetValue(Camera.Position);
            pointLightShader.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(Camera.View * 
                Camera.Projection));

            foreach (ModelMesh mesh in pointLightMesh.Meshes)
            {
                Matrix meshWorld = boneTransforms[mesh.ParentBone.Index];
                Matrix wvp = meshWorld * Camera.View * Camera.Projection;

                if (pointLightShader.Parameters["world"] != null)
                {
                    pointLightShader.Parameters["world"].SetValue(meshWorld);
                }
                if (pointLightShader.Parameters["wvp"] != null)
                {
                    pointLightShader.Parameters["wvp"].SetValue(wvp);
                }

                float dist = Vector3.Distance(Camera.Position, pointLight.Position);


                if (dist < pointLight.Radius)
                {
                    GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
                }

                // Draw the point-light-sphere
                mesh.Draw();

                // Revert the cull mode
                GraphicsDevice.RasterizerState =
                RasterizerState.CullCounterClockwise;
            }
        }

        private void DrawDeferred()
        {
            GraphicsDevice.Clear(Color.White);

            deferredShader.Parameters["halfPixel"].SetValue(halfPixel);
            deferredShader.Parameters["colorMap"].SetValue(colorMap);
            deferredShader.Parameters["lightMap"].SetValue(lightMap);

            deferredShader.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            sceneQuad.Draw(-Vector2.One, Vector2.One); 

        }

        int w, h;
        Effect DepthRender;
        public void RenderDebug()
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);

            spriteBatch.Draw(colorMap, new Rectangle(1, 1, w, h), Color.White);
            spriteBatch.Draw(normalMap, new Rectangle(w + 2, 1, w, h), Color.White);

            GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

            spriteBatch.Draw(lightMap, new Rectangle((w * 3) + 4, 1, w, h), Color.White);
            
            spriteBatch.End();
            
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            DepthRender.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            spriteBatch.Draw(depthMap, new Rectangle((w * 2) + 3, 1, w, h), Color.White);
            spriteBatch.End();
        }
    }
}
