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

        // Game Variables //
        // Models //
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
        ChaseCamera camera;

        // Lights //

        // Render Matrices (Put inside Light/Camera class?)
        Matrix World; // Inside model?
        Matrix Projection;

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
        PrelightingRenderer renderer;
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
       
            // Load Models
            models.Add(new ObjModel(Content.Load<Model>("Models/shrine"), new Vector3(-10, 50.0f, 0.8f),
            new Vector3(MathHelper.ToRadians(90), 0, 0), new Vector3(1.0f), GraphicsDevice));
            models.Add(new ObjModel(Content.Load<Model>("Models/ship"), new Vector3(-40.0f, 15.0f, 55.0f),
            new Vector3(0, MathHelper.ToRadians(-90), 0), new Vector3(0.002f), GraphicsDevice));
            models.Add(new ObjModel(Content.Load<Model>("Models/groundmax"), new Vector3(0, 0, 0),
            new Vector3(MathHelper.ToRadians(90), 0, 0), new Vector3(1.0f), GraphicsDevice));

            // Load Textures
            models[0].DiffuseTexture = Content.Load<Texture2D>("Textures/shrine_diff");
            models[0].NormalTexture = Content.Load<Texture2D>("Textures/shrine_normal");
            models[1].DiffuseTexture = Content.Load<Texture2D>("Textures/ship_diff");
            //models[2].DiffuseTexture = Content.Load<Texture2D>("Textures/ground");
            models[2].DiffuseTexture = Content.Load<Texture2D>("Textures/Groundmax/diffuse");
            models[2].NormalTexture = Content.Load<Texture2D>("Textures/Groundmax/normal");

            // Load Skybox
            skybox = new Skybox("Skyboxes/SkyBox", Content);

            // Load Shaders
            BumpShader = Content.Load<Effect>("Shaders/NormalMap");
            TextureShader = Content.Load<Effect>("Shaders/Textured");
            ShadowMap = Content.Load<Effect>("Shaders/ShadowMap");
            Effect effect = Content.Load<Effect>("Shaders/PPModel");

            // Load Camera
            camera = new ChaseCamera(new Vector3(0, 0.5f, 20.0f),
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 0), GraphicsDevice);

            // Assign shaders to models
            models[0].SetModelEffect(effect, true);
            models[1].SetModelEffect(effect, true);
            models[2].SetModelEffect(effect, true);

            ProjectedTextureMaterial mat = new ProjectedTextureMaterial(
                Content.Load<Texture2D>("Textures/horde"), GraphicsDevice);
            mat.ProjectorPosition = new Vector3(10, 40, 0);
            mat.ProjectorTarget = new Vector3(10, 20, -50);
            mat.Scale = 0.05f;

            models[0].Material = mat;
            models[1].Material = mat;
            models[2].Material = mat;


            renderer = new PrelightingRenderer(GraphicsDevice, Content);
            renderer.Models = models;
            renderer.Camera = camera;
            renderer.Lights = new List<PointLightMaterial>()
            {
                new PointLightMaterial(new Vector3(10, 10, 55), Color.White.ToVector3() * .85f, 40.0f),
                new PointLightMaterial(new Vector3(-40, 15, 55), Color.White.ToVector3() * .85f, 40.0f),
                new PointLightMaterial(new Vector3(43.0f, 7.0f, 40.0f), Color.Red.ToVector3() * .85f, 20.0f),
                new PointLightMaterial(new Vector3(40, 20, 0), Color.LightGreen.ToVector3() * .85f, 40.0f),
                new PointLightMaterial(new Vector3(-30, 15, -30), Color.Navy.ToVector3() * .85f, 75.0f),
                new PointLightMaterial(new Vector3(-10, 15, -50), Color.Pink.ToVector3() * .85f, 25.0f)
            };

            // Load Sounds

        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

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

                if (currentState.Triggers.Right > 0)
                {
                    // Determine what direction to move in
                    Matrix rotation = Matrix.CreateFromYawPitchRoll(
                    models[1].Rotation.Y, models[1].Rotation.X,
                    models[1].Rotation.Z);
                    // Move in the direction dictated by our rotation matrix
                    models[1].Position += Vector3.Transform(Vector3.Forward,
                    rotation) * (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.01f;
                }
                if (currentState.Triggers.Left > 0)
                {
                    // Determine what direction to move in
                    Matrix rotation = Matrix.CreateFromYawPitchRoll(
                    models[1].Rotation.Y, models[1].Rotation.X,
                    models[1].Rotation.Z);
                    // Move in the direction dictated by our rotation matrix
                    models[1].Position += Vector3.Transform(Vector3.Forward,
                    rotation) * (float)gameTime.ElapsedGameTime.TotalMilliseconds * -0.01f;
                }

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
            renderer.Draw();

            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Invert the culling for the skybox cube and draw the skybox
            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            skybox.Draw(camera.View, Projection, camera.Position);
            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            // Draw the Modes
            foreach (ObjModel model in models)
            {
                model.Draw(camera.View, Projection, camera.Position); 
            }

            base.Draw(gameTime);
        }
    }
}
