using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
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

        // Camera Variables
        public BaseCamera ChaseCamera;
        public BaseCamera FreeCamera;
        Boolean FreeCameraActive;

        //Collision rules handler
        CollisionHandler CollisionHandler;

        GameLogic GameLogic;

        public Boolean DebugEnabled;
        public StaticMesh testVarGround;

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
        Model cubeModel;

        public ERoD()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredBackBufferWidth = 1280;

            Content.RootDirectory = "Content";

            renderer = new DeferredRenderer(this);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            FreeCamera = new FreeCamera(this, 0.01f, 10000.0f, new Vector3(0, 20.0f, 0), 50.0f);
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

            // Loading the collision rules handler
            CollisionHandler = new CollisionHandler(this);

            spriteBatch = new SpriteBatch(GraphicsDevice);

            cubeModel = Content.Load<Model>("Models/cube");

            Model shipModel = Content.Load<Model>("Models/space_frigate");
            Model shipModelT = Content.Load<Model>("Models/space_frigate_tangentOn");
            Vector3 shipScale = new Vector3(0.07f, 0.07f, 0.07f);
            Vector3 shipPosition = new Vector3(0, 60, 0);

            Model groundModel = Content.Load<Model>("Models/Z3B0_Arena_alphaVersion");
            AffineTransform groundTransform = new AffineTransform(new BVector3(0.15f, 0.15f, 0.15f), new BQuaternion(0, 0, 0, 0), new BVector3(0, 0, 0));
            
            Effect objEffect = Content.Load<Effect>("Shaders/DeferredObjectRender");

            space = new Space();

            // Fix ship loading
            Entity entity = LoadEntityObject(shipModel, shipPosition, shipScale);

            Ship ship = new Ship(1, entity, shipModelT, Matrix.CreateScale(shipScale), new Vector3(Microsoft.Xna.Framework.MathHelper.ToRadians(-90.0f), 0.0f, 0.0f), this);

            space.Add(entity);
            ship.Texture = Content.Load<Texture2D>("Textures/Ship2/diffuse");
            ship.SpecularMap = Content.Load<Texture2D>("Textures/Ship2/specular");
            ship.TextureEnabled = true;
            ship.standardEffect = objEffect;
            CollisionHandler.addShipGroup(ship);
            Components.Add(ship);

            CollisionHandler.addShipGroup(ship);

            ChaseCamera = new ChaseCamera(ship.Entity, new BEPUutilities.Vector3(0.0f, 5.0f, 0.0f), true, 20.0f, 0.1f, 2000.0f, this);
            ((ChaseCamera)ChaseCamera).Initialize();

            StaticObject sobj = LoadStaticObject(groundModel, groundTransform);
            sobj.Texture = Content.Load<Texture2D>("Textures/Ground/diffuse");
            sobj.SpecularMap = Content.Load<Texture2D>("Textures/Ground/specular");
            sobj.BumpMap = Content.Load<Texture2D>("Textures/Ground/normal");
            sobj.TextureEnabled = true;
            sobj.standardEffect = objEffect;
            Components.Add(sobj);
            
            space.ForceUpdater.Gravity = new BVector3(0, -9.82f, 0);

            postProcesses.Add(new MotionBlur(this));

            //Adds the test triggers
            Vector3 pwrScale = new Vector3(20, 20, 2);

            Vector3 pwrLocation1 = new Vector3(75, 27.0f, 114);
            Vector3 pwrLocation2 = new Vector3(-114, 4.0f, -141); 
            Vector3 pwrLocation3 = new Vector3(120, 4.0f, -130);                 
            Vector3 pwrLocation4 = new Vector3(40, 4, -35);

            Entity entity1 = LoadEntityObject(cubeModel, pwrLocation1, pwrScale);
            Entity entity2 = LoadEntityObject(cubeModel, pwrLocation2, pwrScale);
            Entity entity3 = LoadEntityObject(cubeModel, pwrLocation3, pwrScale);
            Entity entity4 = LoadEntityObject(cubeModel, pwrLocation4, pwrScale);
            BEPUutilities.Quaternion AddRot = BEPUutilities.Quaternion.CreateFromAxisAngle(BVector3.Up, -90);
            entity4.Orientation *= AddRot;
            entity1.Orientation *= AddRot;

            LapTrigger trigger1 = new LapTrigger(1, entity1, cubeModel, Matrix.CreateScale(pwrScale), Vector3.Zero, this);
            LapTrigger trigger2 = new LapTrigger(2, entity2, cubeModel, Matrix.CreateScale(pwrScale), Vector3.Zero, this);
            LapTrigger trigger3 = new LapTrigger(3, entity3, cubeModel, Matrix.CreateScale(pwrScale), Vector3.Zero, this);
            LapTrigger trigger4 = new LapTrigger(4, entity4, cubeModel, Matrix.CreateScale(pwrScale), Vector3.Zero, this);
 
            trigger1.standardEffect = objEffect;
            trigger2.standardEffect = objEffect;
            trigger3.standardEffect = objEffect;
            trigger4.standardEffect = objEffect;
            CollisionHandler.addTriggerGroup(trigger1);
            CollisionHandler.addTriggerGroup(trigger2);
            CollisionHandler.addTriggerGroup(trigger3);
            CollisionHandler.addTriggerGroup(trigger4);
            space.Add(entity1);
            space.Add(entity2);
            space.Add(entity3);
            space.Add(entity4);
            Components.Add(trigger1);
            Components.Add(trigger2);
            Components.Add(trigger3);
            Components.Add(trigger4);

            renderer.DirectionalLights.Add(new DirectionalLight(this, new Vector3(50, 250, 250), Vector3.Zero, Color.LightYellow, 1.0f, true));

            renderer.PointLights.Add(new PointLight(new Vector3(10, 10, 10), Color.White, 25.0f, 1.0f));
            renderer.PointLights.Add(new PointLight(new Vector3(-10, 10, -10), Color.Red, 25.0f, 1.0f));
            renderer.PointLights.Add(new PointLight(new Vector3(95, 17, 70), Color.Blue, 10.0f, 1.0f));
            renderer.PointLights.Add(new PointLight(new Vector3(110, 17, 55), Color.Cyan, 10.0f, 1.0f));
            renderer.PointLights.Add(new PointLight(new Vector3(115, 17, 45), Color.Red, 10.0f, 1.0f));
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
            testVarGround = mesh;
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

            space.Update();

            LastGamePadState = GamePadState;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            renderer.Draw(gameTime);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque,
                SamplerState.PointClamp, DepthStencilState.Default,
                RasterizerState.CullCounterClockwise);
            spriteBatch.Draw(renderer.finalBackBuffer, new Rectangle(0, 0, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height), Color.White);
            spriteBatch.End();

            //foreach (PostProcess postProcess in postProcesses)
            //{
            //    postProcess.Draw(gameTime);
            //}

            renderer.RenderDebug();
        }
    }
}
