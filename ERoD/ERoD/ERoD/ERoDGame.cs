using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Jitter;
using Jitter.Dynamics;
using Jitter.Collision;
using Jitter.LinearMath;
using Jitter.Collision.Shapes;
using Jitter.Dynamics.Constraints;
using Jitter.Dynamics.Joints;
using Jitter.DataStructures;

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

        // Skybox //
        Skybox skybox;

        // Sounds //


        // Cameras //
        ChaseCamera camera;

        // Physics //
        World World;

        // Lights //

        // Render Matrices (Put inside Light/Camera class?)
        Matrix Projection;

        public ERoDGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            CollisionSystem collision = new CollisionSystemPersistentSAP();
            World = new World(collision);
        }

        protected override void Initialize()
        {
            float aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;

            // Camera view and projection matrices
            //View = Matrix.CreateLookAt(CameraLocation, CameraTarget, Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f), 
                aspectRatio,
                1.0f, 10000.0f);

            base.Initialize();
        }
        PrelightingRenderer renderer;
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);


            // Load Shaders
            Effect effect = Content.Load<Effect>("Shaders/PPModel");

            // Load Models
            ObjModel newModel = new ObjModel(Content.Load<Model>("Models/shrine"), new Vector3(-10, 50.0f, 0.8f),
                new Vector3(MathHelper.ToRadians(90), 0, 0), new Vector3(1.0f), GraphicsDevice, new BoxShape(new JVector(10.0f, 10.0f, 10.0f)));
            newModel.DiffuseTexture = Content.Load<Texture2D>("Textures/shrine_diff");
            newModel.NormalTexture = Content.Load<Texture2D>("Textures/shrine_normal");
            newModel.SetModelEffect(effect, true);
            World.AddBody(newModel.body);
            models.Add(newModel);


            newModel = new ObjModel(Content.Load<Model>("Models/ship"), new Vector3(-40.0f, 15.0f, 55.0f),
                new Vector3(0, MathHelper.ToRadians(-90), 0), new Vector3(0.002f), GraphicsDevice, new BoxShape(JVector.One));
            newModel.DiffuseTexture = Content.Load<Texture2D>("Textures/ship_diff");
            newModel.SetModelEffect(effect, true);
            World.AddBody(newModel.body);
            newModel.body.Mass = 100.0f;
            models.Add(newModel);

            newModel = new ObjModel(Content.Load<Model>("Models/ground"), new Vector3(0, 0, 0),
                new Vector3(MathHelper.ToRadians(90), 0, 0), new Vector3(1.0f), GraphicsDevice, new BoxShape(new JVector(400.0f, 1.0f, 400.0f)));
            newModel.DiffuseTexture = Content.Load<Texture2D>("Textures/ground");//max/diffuse");
            //newModel.NormalTexture = Content.Load<Texture2D>("Textures/Groundmax/normal");
            newModel.SetModelEffect(effect, true);
            newModel.body.IsStatic = true;
            World.AddBody(newModel.body);
            models.Add(newModel);
            // Load Textures
            
            
            //models[2].DiffuseTexture = Content.Load<Texture2D>("Textures/ground");


            // Load Skybox
            skybox = new Skybox("Skyboxes/Islands", Content);

            // Load Camera
            camera = new ChaseCamera(new Vector3(0, 0.5f, 20.0f),
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 0), GraphicsDevice);

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

            World.Step(1.0f/100.0f, true);

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
                   models[1].body.LinearVelocity = Conversion.ToJitterVector((((ChaseCamera)camera).Target - ((ChaseCamera)camera).Position) * new Vector3(3.0f, 0, 3.0f));
                   //models[1].body.AddForce(new JVector(100.0f, 100.0f, 100.0f), models[1].body.Position);

                }
                if (currentState.Triggers.Left > 0)
                {
                    // Determine what direction to move in
                    Matrix rotation = Matrix.CreateFromYawPitchRoll(
                    models[1].Rotation.Y, models[1].Rotation.X,
                    models[1].Rotation.Z);
                    // Move in the direction dictated by our rotation matrix
                    models[1].body.Position += Conversion.ToJitterVector(Vector3.Transform(Vector3.Forward,
                    rotation) * (float)gameTime.ElapsedGameTime.TotalMilliseconds * -0.03f);
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
                models[1].body.Position += Conversion.ToJitterVector(Vector3.Transform(Vector3.Forward,
                rotation) * (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.01f);
            }
        }

        void updateCamera(GameTime gameTime)
        {
            // Move the camera to the new model's position and orientation
            ((ChaseCamera)camera).Move(Conversion.ToXNAVector(models[1].body.Position),
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
