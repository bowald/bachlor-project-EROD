using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ERoD
{
    public class ERoDGame : Microsoft.Xna.Framework.Game
    {
        // Graphics Variables // 
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        RasterizerState StandardCull, CullClockwiseFace;

        // Game Variables //
        // Models //
        //Model shrine;
        List<ObjModel> models = new List<ObjModel>();


        // Textures // 
        Texture2D shrineTexture;
        Texture2D shrineNormal;

        // Shaders //
        Effect BumpShader;
        Effect TextureShader;
        Effect ShadowMap;

        // ShadowMap //
        RenderTarget2D shadowRenderTarget;

        // Skybox //
        Skybox skybox;

        // Sounds //


        // Cameras //
        // TODO: Replace with camera class
        Vector3 CameraLocation = new Vector3(0.0f, 0.5f, 20.0f);
        Vector3 CameraTarget = new Vector3(0, 0, 0);
        float CameraRotation = 0.5f;
        Vector3 viewVector;

        // Lights //
        // TODO: Replace with light class
        Vector3 LightLocation = new Vector3(20.0f, 10.0f, 0.0f);
        Vector3 LightTarget = new Vector3(0, 0, 0);

        // Render Matrices (Put inside Light/Camera class?)
        Matrix World; // Inside model?
        Matrix View, Projection;
        Matrix LightView, LightProjection;

        public ERoDGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            World = Matrix.Identity;
            
            float aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;

            // Camera view and projection matrices
            View = Matrix.CreateLookAt(CameraLocation, CameraTarget, Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f), 
                aspectRatio,
                1.0f, 1000.0f);

            // Light view and projection matrices
            LightView = Matrix.CreateLookAt(LightLocation, LightTarget, Vector3.Up);
            LightProjection = Matrix.CreateOrthographic(aspectRatio * 20.0f,
                20.0f, 10.0f, 50.0f);

            // Create custom rasterizerstates for the culling of the 
            // vertices on the skybox, only show the inside triangles.
            StandardCull = new RasterizerState();
            CullClockwiseFace = new RasterizerState();
            CullClockwiseFace.CullMode = CullMode.CullClockwiseFace;

            // Set up renderTarget for the shadowMap
            shadowRenderTarget = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth16);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Load Models
            models.Add(new ObjModel(Content.Load<Model>("Models/shrine"), new Vector3(0, 0, 0),
            new Vector3(MathHelper.ToRadians(90), 0, 0), new Vector3(1.0f), GraphicsDevice));
            models.Add(new ObjModel(Content.Load<Model>("Models/ship"), new Vector3(0, 0, 20.0f),
            new Vector3(0, 0, 0), new Vector3(0.002f), GraphicsDevice));
            
            // Load Textures
            models[0].DiffuseTexture = Content.Load<Texture2D>("Textures/shrine_diff");
            models[0].NormalTexture = Content.Load<Texture2D>("Textures/shrine_normal");
            models[1].DiffuseTexture = Content.Load<Texture2D>("Textures/ship_diff");

            // Load Skybox
            skybox = new Skybox("Skyboxes/SkyBox", Content);

            // Load Shaders
            BumpShader = Content.Load<Effect>("Shaders/NormalMap");
            TextureShader = Content.Load<Effect>("Shaders/Textured");
            ShadowMap = Content.Load<Effect>("Shaders/ShadowMap");
            
            // Load Sounds

        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // Update camera and view vector
            CameraRotation += 0.01f;
            CameraLocation = new Vector3((float)Math.Sin(CameraRotation) * 20.0f,
                5.0f, (float)Math.Cos(CameraRotation) * 20.0f);
            View = Matrix.CreateLookAt(CameraLocation, CameraTarget, Vector3.Up);
            // view vector is used for specular light
            viewVector = Vector3.Transform(CameraTarget - CameraLocation, Matrix.CreateRotationY(0));
            viewVector.Normalize();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            DrawSceneToTexture(shadowRenderTarget);

            // Commented code will draw the shadow depth texture to screen
            //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque,
            //    SamplerState.LinearClamp, DepthStencilState.Default,
            //    RasterizerState.CullNone);

            //spriteBatch.Draw(shadowRenderTarget, new Rectangle(0, 0, 800, 480), Color.White);

            //spriteBatch.End();

            // Comment the following 4 lines to draw depth texture instead

            // Invert the culling for the skybox cube and draw the skybox
            graphics.GraphicsDevice.RasterizerState = CullClockwiseFace;
            skybox.Draw(camera.View, Projection, camera.Position);
            graphics.GraphicsDevice.RasterizerState = StandardCull;

            // draw the shrine
            foreach (ObjModel model in models)
            {
                if (model.DiffuseExists)
                {
                    if (model.NormalExists)
                    {
                        model.Draw(BumpShader, camera.View, Projection, viewVector);
                    }
                    else
                    {
                        model.Draw(TextureShader, camera.View, Projection, viewVector);
                    }
                }
            }
            //DrawModelWithEffect(shrine, World, View, Projection);

            base.Draw(gameTime);
        }

        // Draw the depth from the lights perspective to a texture.
        protected void DrawSceneToTexture(RenderTarget2D renderTarget)
        {
            // Set the render target
            GraphicsDevice.SetRenderTarget(renderTarget);

            GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };

            // Draw the scene
            GraphicsDevice.Clear(Color.CornflowerBlue);
            DrawShadowCaster(shrine);
            //DrawModelWithEffect(model, World, View, LightProjection);

            GraphicsDevice.SetRenderTarget(null);
        }

        // Draw a shadowcaster object from the lights perspective, saving the depth value.
        private void DrawShadowCaster(Model model)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    // Set the part to use the shadowmap shader.
                    part.Effect = ShadowMap;
                    // Set parameters of the shadowmap shader.
                    ShadowMap.Parameters["World"].SetValue(World * mesh.ParentBone.Transform);
                    ShadowMap.Parameters["LightView"].SetValue(LightView);
                    ShadowMap.Parameters["LightProjection"].SetValue(LightProjection);

                }
                mesh.Draw();
            }
        }

        // Draw a model with a shader, now hardcoded to use the BumpShader with normalmaps
        
    }
}
