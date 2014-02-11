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
        //Vector3 CameraLocation = new Vector3(0.0f, 0.5f, 20.0f);
        //Vector3 CameraTarget = new Vector3(0, 0, 0);
        //float CameraRotation = 0.5f;
        ChaseCamera camera;

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
            //View = Matrix.CreateLookAt(CameraLocation, CameraTarget, Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f), 
                aspectRatio,
                1.0f, 10000.0f);

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

            // Assign shaders to models
            models[0].SetModelEffect(TextureShader, true);
            models[1].SetModelEffect(TextureShader, true);

            PointLightMaterial mat = new PointLightMaterial();
            mat.LightPosition = new Vector3(0, 100, 100);
            mat.LightAttenuation = 300;

            models[0].Material = mat;
            models[1].Material = mat;

            // Load Camera
            camera = new ChaseCamera(new Vector3(0, 0.5f, 20.0f),
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 0), GraphicsDevice);

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
            //CameraRotation += 0.01f;
            //CameraLocation = new Vector3((float)Math.Sin(CameraRotation) * 20.0f,
            //    5.0f, (float)Math.Cos(CameraRotation) * 20.0f);

            updateModel(gameTime);
            updateCamera(gameTime);

            base.Update(gameTime);
        }

        private void updateModel(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();
            GamePadState currentState = GamePad.GetState(PlayerIndex.One);

            if (currentState.IsConnected)
            {
                Vector3 rotChange = new Vector3(0, 0, 0);

                rotChange.X = -currentState.ThumbSticks.Left.Y * 0.05f;
                rotChange.Y = -currentState.ThumbSticks.Left.X * 0.05f;

                models[1].Rotation += rotChange;

                if (!(currentState.Triggers.Right > 0))
                {
                    return;
                }
                // Determine what direction to move in
                Matrix rotation = Matrix.CreateFromYawPitchRoll(
                models[1].Rotation.Y, models[1].Rotation.X,
                models[1].Rotation.Z);
                // Move in the direction dictated by our rotation matrix
                models[1].Position += Vector3.Transform(Vector3.Forward,
                rotation) * (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.01f;

            }
            else
            {
                Vector3 rotChange = new Vector3(0, 0, 0);
                // Determine on which axes the ship should be rotated on, if any
                if (keyState.IsKeyDown(Keys.W))
                    rotChange += new Vector3(1, 0, 0);
                if (keyState.IsKeyDown(Keys.S))
                    rotChange += new Vector3(-1, 0, 0);
                if (keyState.IsKeyDown(Keys.A))
                    rotChange += new Vector3(0, 1, 0);
                if (keyState.IsKeyDown(Keys.D))
                    rotChange += new Vector3(0, -1, 0);
                models[1].Rotation += rotChange * .025f;
                // If space isn't down, the ship shouldn't move
                if (!keyState.IsKeyDown(Keys.Space))
                    return;
                // Determine what direction to move in
                Matrix rotation = Matrix.CreateFromYawPitchRoll(
                models[1].Rotation.Y, models[1].Rotation.X,
                models[1].Rotation.Z);
                // Move in the direction dictated by our rotation matrix
                models[1].Position += Vector3.Transform(Vector3.Forward,
                rotation) * (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.01f;
            }
        }

        void updateCamera(GameTime gameTime)
        {
            // Move the camera to the new model's position and orientation
            ((ChaseCamera)camera).Move(models[1].Position,
            models[1].Rotation);
            // Update the camera
            camera.Update();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //DrawSceneToTexture(shadowRenderTarget);

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
                        model.Draw(BumpShader, camera.View, Projection, camera.Position);
                    }
                    else
                    {
                        model.Draw(TextureShader, camera.View, Projection, camera.Position);
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
            //DrawShadowCaster(shrine);
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
