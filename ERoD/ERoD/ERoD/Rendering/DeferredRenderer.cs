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
        public RenderTarget2D SGRMap;
        public RenderTarget2D lightMap;
        public RenderTarget2D finalBackBuffer;
        public RenderTarget2D skyMap;

        SpriteBatch spriteBatch;

        ShadowRenderer shadowRenderer;

        // Skybox //
        Skybox skybox;


        // Particles //
        //private BaseEmitter particleEffect;

        Model pointLightMesh;
        Matrix[] boneTransforms;

        Effect pointLightShader;
        Effect directionalLightShader;
        Effect deferredShader;

        public List<IPointLight> PointLights = new List<IPointLight>();
        public List<IDirectionalLight> DirectionalLights = new List<IDirectionalLight>();

        public List<BaseEmitter> Emitters = new List<BaseEmitter>();

        Vector2 halfPixel;

        ScreenQuad sceneQuad;

        public DeferredRenderer(Game game) : base(game)
        {
            game.Components.Add(this);
            sceneQuad = new ScreenQuad(game);
            shadowRenderer = new ShadowRenderer(this, game);
            //particleEffect = new BaseEmitter(2000, 7500, 2, 2f, Vector3.Zero);
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

            SGRMap = new RenderTarget2D(GraphicsDevice, width, height, false,
                SurfaceFormat.Rgba1010102, DepthFormat.None);

            lightMap = new RenderTarget2D(GraphicsDevice, width, height, false,
                SurfaceFormat.Color, DepthFormat.None);

            skyMap = new RenderTarget2D(GraphicsDevice, width, height, false,
                SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

            finalBackBuffer = new RenderTarget2D(GraphicsDevice, width, height, false,
                SurfaceFormat.Color, DepthFormat.None);

            skybox = new Skybox("Skyboxes/skybox", Game.Content);

            directionalLightShader = Game.Content.Load<Effect>("Shaders/DirectionalLightShader");

            pointLightShader = Game.Content.Load<Effect>("Shaders/PointLightShader");
            deferredShader = Game.Content.Load<Effect>("Shaders/DeferredRender");


            TextureQuad.ParticleEffect = Game.Content.Load<Effect>("Shaders/ParticleEffect");
          
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

        public override void Update(GameTime gameTime)
        {
            foreach (BaseEmitter emitter in Emitters)
            {
                emitter.Emit(gameTime);
                emitter.Update(gameTime);
            }
        }

        private void RenderDeferred(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTargets(colorMap, normalMap, depthMap, SGRMap);

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

            GraphicsDevice.SetRenderTarget(skyMap);
            DrawSkybox();

            GraphicsDevice.SetRenderTargets(finalBackBuffer);

            DrawDeferred();

            DrawParticles();

            GraphicsDevice.SetRenderTarget(null);
        }

        private void DrawParticles()
        {
            // Particles
            TextureQuad.ParticleEffect.Parameters["DepthMap"].SetValue(depthMap);
            TextureQuad.ParticleEffect.Parameters["HalfPixel"].SetValue(halfPixel);

            foreach(BaseEmitter emitter in Emitters)
            {
                emitter.Draw(GraphicsDevice, Camera);
            }
        }

        private void DeferredShadows(GameTime gameTime)
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
                shadowRenderer.RenderCascadedShadowMap(light, Camera, gameTime);
            }
        }

        // Render all lights to a texture.
        private void DeferredLightning(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(lightMap);

            GraphicsDevice.Clear(Color.Transparent);
            GraphicsDevice.BlendState = BlendState.Additive;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

            // Render all directional lights
            foreach (IDirectionalLight dirLight in DirectionalLights)
            {
                RenderDirectionalLight(dirLight);
            }

            // Render all point-lights
            foreach (IPointLight pointLight in PointLights) 
            {
                RenderPointLight(pointLight);
            }

            GraphicsDevice.SetRenderTarget(null);
        }

        private void RenderDirectionalLight(IDirectionalLight directionalLight)
        {
            // Load Light Params
            directionalLightShader.Parameters["HalfPixel"].SetValue(halfPixel);
            directionalLightShader.Parameters["LightDirection"].SetValue(directionalLight.Direction);
            directionalLightShader.Parameters["Color"].SetValue(directionalLight.Color.ToVector3());

            directionalLightShader.Parameters["NormalMap"].SetValue(normalMap);
            directionalLightShader.Parameters["SGRMap"].SetValue(SGRMap);
            directionalLightShader.Parameters["DepthMap"].SetValue(depthMap);
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

        private void RenderPointLight(IPointLight pointLight)
        {
            pointLightShader.Parameters["HalfPixel"].SetValue(halfPixel);
            pointLightShader.Parameters["ColorMap"].SetValue(colorMap);
            pointLightShader.Parameters["NormalMap"].SetValue(normalMap);
            pointLightShader.Parameters["DepthMap"].SetValue(depthMap);

            Matrix sphereWorldMatrix = Matrix.CreateScale(pointLight.Radius) 
                * Matrix.CreateTranslation(pointLight.Position);

            pointLightShader.Parameters["World"].SetValue(sphereWorldMatrix);
            pointLightShader.Parameters["View"].SetValue(Camera.View);
            pointLightShader.Parameters["Projection"].SetValue(Camera.Projection);
            pointLightShader.Parameters["ViewInv"].SetValue(Matrix.Invert(Camera.View));

            pointLightShader.Parameters["LightPosition"].SetValue(pointLight.Position);

            pointLightShader.Parameters["SidesLengthVS"].SetValue(new Vector2(Camera.TanFovy * Camera.AspectRatio, -Camera.TanFovy));
            pointLightShader.Parameters["FarPlane"].SetValue(Camera.FarPlane);

            pointLightShader.Parameters["Color"].SetValue(pointLight.Color.ToVector3());
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

        private void DrawSkybox()
        {
            GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            skybox.Draw(Camera.View, Camera.Projection, Camera.Position);
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }

        private void DrawDeferred()
        {
            GraphicsDevice.Clear(Color.White);

            deferredShader.Parameters["HalfPixel"].SetValue(halfPixel);
            deferredShader.Parameters["ColorMap"].SetValue(colorMap);
            deferredShader.Parameters["LightMap"].SetValue(lightMap);
            deferredShader.Parameters["DepthMap"].SetValue(depthMap);
            deferredShader.Parameters["SkyMap"].SetValue(skyMap);

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
            //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);

            //spriteBatch.Draw(colorMap, new Rectangle(1, 1, w, h), Color.White);
            ////spriteBatch.Draw(SGRMap, new Rectangle((w * 4) + 4, 1, w, h), Color.White);
            //spriteBatch.Draw(normalMap, new Rectangle(w + 2, 1, w, h), Color.White);


            //GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

            //spriteBatch.Draw(lightMap, new Rectangle((w * 3) + 4, 1, w, h), Color.White);
            
            //spriteBatch.End();
            
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            DepthRender.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            spriteBatch.Draw(depthMap, new Rectangle((w * 3) + 4, 1, w, h), Color.White);
            spriteBatch.Draw(DirectionalLights[0].ShadowMapEntry.ShadowMap, new Rectangle(1, 1, w*3, h), Color.White);
            spriteBatch.End();
        }
    }
}
