using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUutilities;
using ConversionHelper;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;

using BVector3 = BEPUutilities.Vector3;
using BQuaternion = BEPUutilities.Quaternion;
using BMatrix = BEPUutilities.Matrix;
using BMatrix3x3 = BEPUutilities.Matrix3x3;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Quaternion = Microsoft.Xna.Framework.Quaternion;
using Matrix = Microsoft.Xna.Framework.Matrix;
using BEPUphysics.DataStructures;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysicsDrawer.Models;
using ERoD.Helper;

namespace ERoD
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ERoD : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private Space space;

        // Boolean for drawing the debug frame
        private bool RenderDebug;
        // Camera Variables
        public BaseCamera ChaseCamera;
        public BaseCamera FreeCamera;
        Boolean FreeCameraActive;


        // GameLogic //
        GameLogic GameLogic;

        public Boolean DebugEnabled;
        public StaticMesh testVarGround;

        HeightTerrainCDLOD terrain;

        public Space Space
        {
            get { return space; }
        }

        protected DeferredRenderer renderer;
        public DeferredRenderer Renderer
        {
            get { return renderer; }
        }

        protected List<PostProcess> postProcesses = new List<PostProcess>();

        public ModelDrawer modelDrawer;  //Used to draw entities for debug.

        public GamePadState GamePadState { get; set; }
        public GamePadState LastGamePadState { get; set; }

        private LightRay lightRay;

        public ERoD()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 768;
            graphics.PreferredBackBufferWidth = 1366;
            //graphics.IsFullScreen = true;

            Content.RootDirectory = "Content";

            renderer = new DeferredRenderer(this);

            RenderDebug = false;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            terrain = new HeightTerrainCDLOD(this, 7);
            Components.Add(terrain);
            Services.AddService(typeof(ITerrain), terrain);

            FreeCamera = new FreeCamera(this, 0.1f, 1000.0f, new Vector3(25f, 150.0f, 25f), 70.0f);
            this.Services.AddService(typeof(ICamera), FreeCamera);
            FreeCameraActive = true;

            GameLogic = new GameLogic(this);
            this.Services.AddService(typeof(GameLogic), GameLogic);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Sprites/Lap1");

            // TODO: Load your game content here            
            fontPos = new Microsoft.Xna.Framework.Vector2(graphics.GraphicsDevice.Viewport.Width / 2,
                graphics.GraphicsDevice.Viewport.Height / 2);

            Model cubeModel = Content.Load<Model>("Models/cube");

            Model shipModel = Content.Load<Model>("Models/space_frigate");
            Model shipModelT = Content.Load<Model>("Models/space_frigate_tangentOn");
            Vector3 shipScale = new Vector3(0.01f, 0.01f, 0.01f);
            Vector3 shipPosition = new Vector3(10, 20, 30);

            Effect objEffect = Content.Load<Effect>("Shaders/DeferredObjectRender");
            Effect objShadow = Content.Load<Effect>("Shaders/DeferredShadowShader");
            
            space = new Space();
            Services.AddService(typeof(Space), space);

            space.Add(((ITerrain)Services.GetService(typeof(ITerrain))).PhysicTerrain);

            //Console.WriteLine("Max {0}, Min {1}", terrain.PhysicTerrain.BoundingBox.Max, terrain.PhysicTerrain.BoundingBox.Min);

            // Fix ship loading
            Entity entity = LoadEntityObject(shipModel, shipPosition, shipScale);

            Ship ship = new Ship(entity, shipModelT, Matrix.CreateScale(shipScale), this);

            lightRay = new LightRay(this, new Vector3(0, 50, 0), 1, 1, 1, 1);

            space.Add(entity);
            ship.Texture = Content.Load<Texture2D>("Textures/Ship2/diffuse");
            ship.SpecularMap = Content.Load<Texture2D>("Textures/Ship2/specular");
            ship.TextureEnabled = true;
            ship.standardEffect = objEffect;
            ship.shadowEffect = objShadow;
            Components.Add(ship);
            GameLogic.AddPlayer(ship, "Anton");

            ChaseCamera = new ChaseCamera(ship.Entity, new BEPUutilities.Vector3(0.0f, 0.7f, 0.0f), true, 4.0f, 0.1f, 1000.0f, this);
            ((ChaseCamera)ChaseCamera).Initialize();

            CreateCheckPoints(objEffect, cubeModel);

            space.ForceUpdater.Gravity = new BVector3(0, -20.0f, 0);

            renderer.DirectionalLights.Add(new DirectionalLight(this, new Vector3(50, 550, 450), Vector3.Zero, Color.LightYellow, 0.5f, true));

            renderer.PointLights.Add(new PointLight(new Vector3(0, 25, 50), Color.Blue, 50.0f, 1.0f));
            renderer.PointLights.Add(new PointLight(new Vector3(50, 25, 0), Color.Red, 50.0f, 1.0f));
            renderer.PointLights.Add(new PointLight(new Vector3(-50, 25, 0), Color.Green, 50.0f, 1.0f));

            renderer.PointLights.Add(new PointLight(new Vector3(170, 25, -175), Color.Goldenrod, 50.0f, 1.0f));
            renderer.PointLights.Add(new PointLight(new Vector3(130, 25, -172), Color.Goldenrod, 50.0f, 1.0f));
            renderer.PointLights.Add(new PointLight(new Vector3(90, 25, -160), Color.Goldenrod, 50.0f, 1.0f));
        }

        private void CreateCheckPoints(Effect effect, Model model)
        {
            int nbrPoints = GameConstants.NumberOfCheckpoints; // Update!!
            int i = 0;
            Checkpoint[] points = new Checkpoint[nbrPoints];
            points[i++] = GameLogic.AddCheckpoint(new BVector3(30, 20, 5), new BVector3(-5, 0, 0), BEPUutilities.MathHelper.ToRadians(10f));
            points[i++] = GameLogic.AddCheckpoint(new BVector3(40, 30, 5), new BVector3(165, 10, -150), BEPUutilities.MathHelper.ToRadians(-20f));
            points[i++] = GameLogic.AddCheckpoint(new BVector3(30, 20, 5), new BVector3(-175, 5, -15), BEPUutilities.MathHelper.ToRadians(-15f));
            points[i++] = GameLogic.AddCheckpoint(new BVector3(30, 20, 5), new BVector3(0, 5, 115), BEPUutilities.MathHelper.ToRadians(75f));

            foreach(Checkpoint p in points)
            {
                p.BasicEffect = effect;
                p.Model = model;
                //Components.Add(p); //Uncomment to draw checkpoints
            }
        }

        private Entity LoadEntityObject(Model model, Vector3 position, Vector3 scaling)
        {
            BVector3[] vertices;
            int[] indices;
            ModelDataExtractor.GetVerticesAndIndicesFromModel(model, out vertices, out indices);
            
            // Convert to list since array is read only.
            IList<BVector3> verts = new List<BVector3>(vertices);
            // Remove redundant vertices that causes the convexhullshape to crash.
            ConvexHullHelper.RemoveRedundantPoints(verts);
            vertices = verts.ToArray<BVector3>();

            ConvexHullShape CHS = new ConvexHullShape(OurHelper.scaleVertices(vertices, scaling));
            Entity entity = new Entity(CHS, 10);
            entity.Position = ConversionHelper.MathConverter.Convert(position);
            return entity;
        }

        private StaticObject LoadStaticObject(Model model, AffineTransform transform) 
        {
            BVector3[] vertices;
            int[] indices;
            ModelDataExtractor.GetVerticesAndIndicesFromModel(model, out vertices, out indices);
            var mesh = new StaticMesh(vertices, indices, transform);
            space.Add(mesh);
            return new StaticObject(model, MathConverter.Convert(mesh.WorldTransform.Matrix), this);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent() 
        {
            ;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            GamePadState = GamePad.GetState(PlayerIndex.One);

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                this.Exit();
            }

            if ((GamePadState.Buttons.B == ButtonState.Pressed) && (LastGamePadState.Buttons.B == ButtonState.Released ))
            {
                Services.RemoveService(typeof(ICamera));
                if (FreeCameraActive)
                {
                    FreeCameraActive = false;
                    Services.AddService(typeof(ICamera), ChaseCamera);
                }
                else
                {
                    FreeCameraActive = true;
                    Services.AddService(typeof(ICamera), FreeCamera);
                }
            }

            if ((GamePadState.Buttons.Y == ButtonState.Pressed) && (LastGamePadState.Buttons.Y == ButtonState.Released))
            {
                RenderDebug = !RenderDebug;
            }

            space.Update();

            LastGamePadState = GamePadState;
            base.Update(gameTime);
        }

        SpriteFont font;
        Microsoft.Xna.Framework.Vector2 fontPos;
        private string message = "";
        private float startTime;
        private float endTime = -1;
        public void DisplayMessage(string message, float seconds)
        {
            this.message = message;
            endTime = startTime + seconds;
            Console.WriteLine(startTime);
            Console.WriteLine(endTime);
            Console.WriteLine("---");
        }

        int x = 0;
        double totTime = 0;
        double time2 = 0;

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            startTime = (float)gameTime.TotalGameTime.TotalSeconds; 
            totTime += dt;
            if (totTime > 2)
            {
                time2 += dt;
                x++;
            }
            if (time2 > 5)
            {
                //Console.WriteLine(x / time2);
            }

            renderer.Draw(gameTime);

            lightRay.Draw(gameTime);

            GraphicsDevice.Clear(Color.Coral);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque,
            SamplerState.PointClamp, DepthStencilState.Default,
            RasterizerState.CullCounterClockwise);
            spriteBatch.Draw(renderer.finalBackBuffer, new Rectangle(0, 0, GraphicsDevice.Viewport.Width,
            GraphicsDevice.Viewport.Height), Color.White);
            spriteBatch.End();

            if (startTime < endTime) 
            {

                spriteBatch.Begin();
                // Draw sprites
                Microsoft.Xna.Framework.Vector2 FontOrigin = font.MeasureString(message) / 2;
                spriteBatch.DrawString(font, message, fontPos, Color.DeepPink,
                    0, FontOrigin, 1.0f, SpriteEffects.None, 0.5f);

                spriteBatch.End();
            }


            foreach (PostProcess postProcess in postProcesses)
            {
                //postProcess.Draw(gameTime);
            }

            if (RenderDebug)
            {
                renderer.RenderDebug();
            }
        }
    }
}
