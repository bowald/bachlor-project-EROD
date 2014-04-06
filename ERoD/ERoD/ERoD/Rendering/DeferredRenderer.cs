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
            get 
            {
                return (ICamera)Game.Services.GetService(typeof(ICamera));
            }
        }

        public struct DeferredRenderTarget
        {
            public int width, height;
            public int w, h;

            public RenderTarget2D depthMap;
            public RenderTarget2D colorMap;
            public RenderTarget2D normalMap;
            public RenderTarget2D particleMap;
            public RenderTarget2D SGRMap;
            public RenderTarget2D lightMap;
            public RenderTarget2D finalBackBuffer;
            public RenderTarget2D skyMap;

            public Vector2 HalfPixel;
        }

        public DeferredRenderTarget[] renderTargets;

        SpriteBatch spriteBatch;

        ShadowRenderer shadowRenderer;

        // Skybox //
        Skybox skybox;


        // Particles //

        BumpmapBlur heatHaze;

        Model pointLightMesh;
        Matrix[] boneTransforms;

        Effect pointLightShader;
        Effect directionalLightShader;
        Effect deferredShader;

        Effect deferredShadowShader;

        Effect DepthRender;

        public List<IPointLight> PointLights = new List<IPointLight>();
        public List<IDirectionalLight> DirectionalLights = new List<IDirectionalLight>();

        public List<BaseEmitter> Emitters = new List<BaseEmitter>();

        ScreenQuad sceneQuad;

        public DeferredRenderer(Game game, PlayerView[] playerViews) 
            : base(game)
        {
            sceneQuad = new ScreenQuad(game);
            shadowRenderer = new ShadowRenderer(this, game);

            renderTargets = new DeferredRenderTarget[playerViews.Length];
            for (int i = 0; i < playerViews.Length; i++)
            {
                renderTargets[i].width = playerViews[i].Viewport.Width;
                renderTargets[i].height = playerViews[i].Viewport.Height;

                // Debug render
                renderTargets[i].w = playerViews[i].Viewport.Width / 6;
                renderTargets[i].h = playerViews[i].Viewport.Height / 4;
            }
            game.Components.Add(this);
        }

        protected override void LoadContent()
        {
            for (int i = 0; i < renderTargets.Length; i++)
            {
                int width = renderTargets[i].width;
                int height = renderTargets[i].height;

                renderTargets[i].HalfPixel = -new Vector2(0.5f / (float)width, 0.5f / (float)height);

                renderTargets[i].depthMap = new RenderTarget2D(GraphicsDevice, width, height, false,
                    SurfaceFormat.Single, DepthFormat.Depth24Stencil8);

                renderTargets[i].colorMap = new RenderTarget2D(GraphicsDevice, width, height, false,
                    SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

                renderTargets[i].normalMap = new RenderTarget2D(GraphicsDevice, width, height, false,
                    SurfaceFormat.Rgba1010102, DepthFormat.None);

                renderTargets[i].particleMap = new RenderTarget2D(GraphicsDevice, width, height, false,
                    SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

                renderTargets[i].SGRMap = new RenderTarget2D(GraphicsDevice, width, height, false,
                    SurfaceFormat.Rgba1010102, DepthFormat.None);

                renderTargets[i].lightMap = new RenderTarget2D(GraphicsDevice, width, height, false,
                    SurfaceFormat.Color, DepthFormat.None);

                renderTargets[i].skyMap = new RenderTarget2D(GraphicsDevice, width, height, false,
                    SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

                renderTargets[i].finalBackBuffer = new RenderTarget2D(GraphicsDevice, width, height, false,
                    SurfaceFormat.Color, DepthFormat.None);
            }

            skybox = new Skybox("Skyboxes/skybox", Game.Content);

            directionalLightShader = Game.Content.Load<Effect>("Shaders/DirectionalLightShader");

            pointLightShader = Game.Content.Load<Effect>("Shaders/PointLightShader");
            deferredShader = Game.Content.Load<Effect>("Shaders/DeferredRender");

            TextureQuad.ParticleEffect = Game.Content.Load<Effect>("Shaders/ParticleEffect");

            heatHaze = new BumpmapBlur(Game, true);

            deferredShadowShader = Game.Content.Load<Effect>("Shaders/DeferredShadowShader");

            // Debug depth renderer
            DepthRender = Game.Content.Load<Effect>("Shaders/depthRender");

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

        public void Draw(GameTime gameTime, int renderTargetIndex)
        {
            RenderDeferred(gameTime, renderTargets[renderTargetIndex]);
        }

        public override void Update(GameTime gameTime)
        {
            foreach (ThrusterEmitter emitter in Emitters)
            {
                emitter.Emit(gameTime);
                emitter.Update(gameTime);
            }
        }

        private void RenderDeferred(GameTime gameTime, DeferredRenderTarget target)
        {
            GraphicsDevice.SetRenderTargets(target.colorMap, target.normalMap
                , target.depthMap, target.SGRMap);

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
            DeferredShadows(gameTime, Camera);

            GraphicsDevice.SetRenderTarget(target.lightMap);
            DeferredLightning(gameTime, target);

            GraphicsDevice.SetRenderTarget(target.skyMap);
            DrawSkybox(Camera);

            GraphicsDevice.SetRenderTargets(target.finalBackBuffer);
            DrawDeferred(target);

            DrawParticles(gameTime, target);

            GraphicsDevice.SetRenderTarget(null);
        }

        private void DrawParticles(GameTime gameTime, DeferredRenderTarget target)
        {
            // Particles
            TextureQuad.ParticleEffect.Parameters["DepthMap"].SetValue(target.depthMap);
            TextureQuad.ParticleEffect.Parameters["HalfPixel"].SetValue(target.HalfPixel);

            GraphicsDevice.SetRenderTarget(target.particleMap);
            GraphicsDevice.Clear(Color.Black);

            foreach(ThrusterEmitter emitter in Emitters)
            {
                emitter.Draw(GraphicsDevice, Camera);
            }
            GraphicsDevice.SetRenderTarget(null);
        }

        private void DeferredShadows(GameTime gameTime, ICamera camera)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            List<ILight> lights = new List<ILight>(DirectionalLights.Where(entity => entity.Intensity > 0 && entity.Color != Color.Black && entity.CastShadow));

            List<ILight> needShadowMaps = new List<ILight>(lights.Where(entity => entity.ShadowMapEntry.ShadowMap == null));

            int width = GraphicsDevice.Viewport.Width;
            int height = GraphicsDevice.Viewport.Height;

            foreach (ILight light in needShadowMaps)
            {
                light.ShadowMapEntry.ShadowMap = shadowRenderer.AssignShadowMap();
            }

            foreach (IDirectionalLight light in lights)
            {
                shadowRenderer.RenderCascadedShadowMap(light, camera, gameTime);
            }
        }

        // Render all lights to a texture.
        private void DeferredLightning(GameTime gameTime, DeferredRenderTarget target)
        {
            GraphicsDevice.Clear(Color.Transparent);
            GraphicsDevice.BlendState = BlendState.Additive;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

            // Render all directional lights
            foreach (IDirectionalLight dirLight in DirectionalLights)
            {
                RenderDirectionalLight(dirLight, target);
            }

            // Render all point-lights
            foreach (IPointLight pointLight in PointLights) 
            {
                RenderPointLight(pointLight, target);
            }
            if (LightHelper.ToolEnabled)
            {
                RenderPointLight(LightHelper.Light, target);
            }
            GraphicsDevice.SetRenderTarget(null);
        }

        private void RenderDirectionalLight(IDirectionalLight directionalLight, DeferredRenderTarget target)
        {
            // Load Light Params
            directionalLightShader.Parameters["HalfPixel"].SetValue(target.HalfPixel);
            directionalLightShader.Parameters["LightDirection"].SetValue(directionalLight.Direction);
            directionalLightShader.Parameters["LightColor"].SetValue(directionalLight.Color.ToVector3());

            directionalLightShader.Parameters["NormalMap"].SetValue(target.normalMap);
            directionalLightShader.Parameters["ColorMap"].SetValue(target.colorMap);
            directionalLightShader.Parameters["SGRMap"].SetValue(target.SGRMap);
            directionalLightShader.Parameters["DepthMap"].SetValue(target.depthMap);
            directionalLightShader.Parameters["Power"].SetValue(directionalLight.Intensity);

            directionalLightShader.Parameters["CameraPosition"].SetValue(Camera.Position);
            directionalLightShader.Parameters["ViewInv"].SetValue(Matrix.Invert(Camera.View));

            directionalLightShader.Parameters["SidesLengthVS"].SetValue(new Vector2(Camera.TanFovy * Camera.AspectRatio, -Camera.TanFovy));
            directionalLightShader.Parameters["FarPlane"].SetValue(Camera.FarPlane);

            directionalLightShader.Parameters["LightView"].SetValue(directionalLight.View);
            directionalLightShader.Parameters["LightProj"].SetValue(directionalLight.Projection);

            directionalLightShader.Parameters["CastShadow"].SetValue(directionalLight.CastShadow);

            if (directionalLight.CastShadow)
            {
                directionalLightShader.Parameters["ShadowMapSize"].SetValue(new Vector2(directionalLight.ShadowMapEntry.ShadowMap.Width, directionalLight.ShadowMapEntry.ShadowMap.Height));
                directionalLightShader.Parameters["ShadowMap"].SetValue(directionalLight.ShadowMapEntry.ShadowMap);
                directionalLightShader.Parameters["ClipPlanes"].SetValue(directionalLight.ShadowMapEntry.LightClipPlanes);
                Matrix[] viewMatrices = new Matrix[3];
                Matrix[] projectionMatrices = new Matrix[3];
                for (int i = 0; i < 3; i++)
                {
                    viewMatrices[i] = directionalLight.ShadowMapEntry.LightViewProjectionMatrices[i].LightView;
                    projectionMatrices[i] = directionalLight.ShadowMapEntry.LightViewProjectionMatrices[i].LightProjection;
                }
                directionalLightShader.Parameters["ViewMatrices"].SetValue(viewMatrices);
                directionalLightShader.Parameters["ProjectionMatrices"].SetValue(projectionMatrices);
                Vector4 cascadeDistances = Vector4.Zero;
                cascadeDistances.X = directionalLight.ShadowMapEntry.LightClipPlanes[0].X;
                cascadeDistances.Y = directionalLight.ShadowMapEntry.LightClipPlanes[1].X;
                cascadeDistances.Z = directionalLight.ShadowMapEntry.LightClipPlanes[2].X;
                cascadeDistances.W = directionalLight.ShadowMapEntry.LightClipPlanes[3].X;
                directionalLightShader.Parameters["CascadeDistances"].SetValue(cascadeDistances);
            }

            directionalLightShader.Techniques[0].Passes[0].Apply();

            sceneQuad.Draw(-Vector2.One, Vector2.One);
        }

        private void RenderPointLight(IPointLight pointLight, DeferredRenderTarget target)
        {
            // TODO: Remove debug hack for release, also in shader
            pointLightShader.Parameters["DebugPosition"].SetValue(LightHelper.DebugPosition);

            pointLightShader.Parameters["HalfPixel"].SetValue(target.HalfPixel);
            pointLightShader.Parameters["SGRMap"].SetValue(target.SGRMap);
            pointLightShader.Parameters["ColorMap"].SetValue(target.colorMap);
            pointLightShader.Parameters["NormalMap"].SetValue(target.normalMap);
            pointLightShader.Parameters["DepthMap"].SetValue(target.depthMap);

            pointLightShader.Parameters["CameraPosition"].SetValue(Camera.Position);

            Matrix sphereWorldMatrix = Matrix.CreateScale(pointLight.Radius) 
                * Matrix.CreateTranslation(pointLight.Position);

            pointLightShader.Parameters["World"].SetValue(sphereWorldMatrix);
            pointLightShader.Parameters["View"].SetValue(Camera.View);
            pointLightShader.Parameters["Projection"].SetValue(Camera.Projection);
            pointLightShader.Parameters["ViewInv"].SetValue(Matrix.Invert(Camera.View));

            pointLightShader.Parameters["LightPosition"].SetValue(pointLight.Position);

            pointLightShader.Parameters["SidesLengthVS"].SetValue(new Vector2(Camera.TanFovy * Camera.AspectRatio, -Camera.TanFovy));
            pointLightShader.Parameters["FarPlane"].SetValue(Camera.FarPlane);

            pointLightShader.Parameters["LightColor"].SetValue(pointLight.Color.ToVector3());
            pointLightShader.Parameters["LightRadius"].SetValue(pointLight.Radius);
            pointLightShader.Parameters["LightIntensity"].SetValue(pointLight.Intensity);

            float dist = Vector3.Distance(Camera.Position, pointLight.Position);

            if (dist < pointLight.Radius)
            {
                GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            }
            else
            {
                GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            }

            // Draw the point-light-sphere
            pointLightMesh.Meshes[0].Draw();

            // Revert the cull mode
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            
        }

        private void DrawSkybox(ICamera camera)
        {
            GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            skybox.Draw(camera.View, camera.Projection, camera.Position);
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }

        private void DrawDeferred(DeferredRenderTarget target)
        {
            GraphicsDevice.Clear(Color.White);

            deferredShader.Parameters["HalfPixel"].SetValue(target.HalfPixel);
            deferredShader.Parameters["ColorMap"].SetValue(target.colorMap);
            deferredShader.Parameters["LightMap"].SetValue(target.lightMap);
            deferredShader.Parameters["DepthMap"].SetValue(target.depthMap);
            deferredShader.Parameters["SkyMap"].SetValue(target.skyMap);

            deferredShader.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            sceneQuad.Draw(-Vector2.One, Vector2.One); 

        }

        public void RenderDebug(DeferredRenderTarget target)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            spriteBatch.Draw(target.particleMap, new Rectangle(1, 1, target.w, target.h), Color.White);
            spriteBatch.Draw(target.SGRMap, new Rectangle((target.w * 4) + 4, 1, target.w, target.h), Color.White);
            spriteBatch.Draw(target.normalMap, new Rectangle(target.w + 2, 1, target.w, target.h), Color.White);

            spriteBatch.Draw(target.lightMap, new Rectangle((target.w * 2) + 3, 1, target.w, target.h), Color.White);
            
            spriteBatch.End();
            
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            DepthRender.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            DepthRender.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(target.depthMap, new Rectangle((target.w * 3) + 4, 1, target.w, target.h), Color.White);
            //spriteBatch.Draw(DirectionalLights[0].ShadowMapEntry.ShadowMap, new Rectangle(1, 1, w*3, h), Color.White);
            spriteBatch.End();
        }
    }
}
