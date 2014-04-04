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

        // Boolean for drawing the debug frame
        private bool RenderDebug;
        // Camera Variables
        public BaseCamera ChaseCamera;
        public BaseCamera FreeCamera;
        Boolean FreeCameraActive;


        // GameLogic //
        GameLogic GameLogic;

        public Boolean DebugEnabled;
        StaticMesh rockMesh;
        
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

        protected PostProcessingManager manager;


        public ModelDrawer modelDrawer;  //Used to draw entities for debug.

        public GamePadState GamePadState { get; set; }
        public GamePadState LastGamePadState { get; set; }
        public KeyboardState KeyBoardState { get; set; }
        public KeyboardState LastKeyBoardState { get; set; }

        public ERoD()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 768;
            graphics.PreferredBackBufferWidth = 1360;
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

            FreeCamera = new FreeCamera(this, 0.1f, 7000.0f, new Vector3(25f, 150.0f, 25f), 270.0f);

            this.Services.AddService(typeof(ICamera), FreeCamera);
            FreeCameraActive = true;

            manager = new PostProcessingManager(this);

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

            Effect objEffect = Content.Load<Effect>("Shaders/DeferredObjectRender");
            Effect objShadow = Content.Load<Effect>("Shaders/DeferredShadowShader");
            
            space = new Space();
            Services.AddService(typeof(Space), space);

            LightHelper.Game = this;

            space.Add(((ITerrain)Services.GetService(typeof(ITerrain))).PhysicTerrain);

            manager.AddEffect(new Bloom(this, 0.5f));
            manager.AddEffect(new GodRays(this, new Vector3(100,20,100), 60.0f, 0.8f, 0.99f, 0.8f, 0.15f));


            #region Ship loading

            Model shipModel = Content.Load<Model>("Models/space_frigate");
            Model shipModelT = Content.Load<Model>("Models/space_frigate_tangentOn");
            Vector3 shipScale = new Vector3(0.06f, 0.06f, 0.06f);
            Vector3 shipPosition = new Vector3(150, 20, 300);


            Entity entity = LoadEntityObject(shipModel, shipPosition, shipScale);

            Ship ship = new Ship(entity, shipModelT, shipScale, this);

            space.Add(entity);
            ship.Texture = Content.Load<Texture2D>("Textures/Ship/diffuse");
            ship.SpecularMap = Content.Load<Texture2D>("Textures/Ship/specular");
            ship.TextureEnabled = true;
            ship.standardEffect = objEffect;
            ship.shadowEffect = objShadow;
            ship.Mask = true;
            Components.Add(ship);
            GameLogic.AddPlayer(ship, "Anton");

            #endregion

            ChaseCamera = new ChaseCamera(ship.Entity, new BEPUutilities.Vector3(0.0f, 0.0f, 0.0f), true, 25.0f, 0.1f, 3000.0f, this);
            ((ChaseCamera)ChaseCamera).Initialize();

            #region Bridge

            // Load the bridge model
            Model bridgeModel = Content.Load<Model>("Models/bridge");
            AffineTransform bridgeTransform = new AffineTransform(
                new BVector3(6f, 6f, 6f), 
                BQuaternion.Identity, 
                new BVector3(138.5f, -71, -145));
            var bridgeMesh = LoadStaticObject(bridgeModel, bridgeTransform);
            StaticObject bridge = new StaticObject(bridgeModel, bridgeMesh, this);
            bridge.SpecularMap  = Content.Load<Texture2D>("Textures/Bridge/specular");
            bridge.Texture = Content.Load<Texture2D>("Textures/Bridge/diffuse");
            bridge.BumpMap = Content.Load<Texture2D>("Textures/Bridge/normal");
            bridge.TextureEnabled = true;
            bridge.TexMult = 3f; // make the texture cover 3x more area before repeating.
            bridge.BumpConstant = 1f;
            bridge.standardEffect = objEffect;
            bridge.shadowEffect = objShadow;

            space.Add(bridgeMesh);
            Components.Add(bridge);
            ship.AddCollidable(bridgeMesh);

            #endregion

            #region Rock

            Model rockModel = Content.Load<Model>("Models/rock");
            AffineTransform rockTransform = new AffineTransform(
                new BVector3(4, 4, 4),
                BQuaternion.Identity,
                new BVector3(0, 0, 0));
            //var rockMesh
            rockMesh = LoadStaticObject(rockModel, rockTransform);
            StaticObject rock = new StaticObject(rockModel, rockMesh, this);
            rock.SpecularMap = Content.Load<Texture2D>("Textures/Rock/specular");
            rock.Texture = Content.Load<Texture2D>("Textures/Rock/diffuse");
            rock.BumpMap = Content.Load<Texture2D>("Textures/Rock/normal");
            rock.TextureEnabled = true;
            rock.BumpConstant = 1f;
            rock.standardEffect = objEffect;
            rock.shadowEffect = objShadow;

            //space.Add(rockMesh);
            //Components.Add(rock);

            #endregion

            CreateCheckPoints(objEffect, cubeModel);

            space.ForceUpdater.Gravity = new BVector3(0, GameConstants.Gravity, 0);

            renderer.DirectionalLights.Add(new DirectionalLight(this, new Vector3(2500, 2000, 2500), Vector3.Zero, Color.LightYellow, 0.4f, 7000.0f, true));

            LightHelper.ToolEnabled = false;
            renderer.PointLights.AddRange(LightHelper.ReadLights());
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

        private StaticMesh LoadStaticObject(Model model, AffineTransform transform) 
        {
            BVector3[] vertices;
            int[] indices;
            ModelDataExtractor.GetVerticesAndIndicesFromModel(model, out vertices, out indices);
            return new StaticMesh(vertices, indices, transform);
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
            KeyBoardState = Keyboard.GetState();

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                if (LightHelper.ToolEnabled) 
                {
                    Console.WriteLine("All lights:");
                    foreach (IPointLight light in renderer.PointLights)
                    {
                        Console.WriteLine(light);
                    }
                }
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

            if (LightHelper.ToolEnabled) 
            {
                LightHelper.PlaceLightUpdate(KeyBoardState, LastKeyBoardState);

                if (KeyBoardState.IsKeyDown(Keys.M) && !LastKeyBoardState.IsKeyDown(Keys.M))
                {
                    LightHelper.DebugPosition = !LightHelper.DebugPosition;
                }
                if (KeyBoardState.IsKeyDown(Keys.U) && !LastKeyBoardState.IsKeyDown(Keys.U))
                {
                    renderer.PointLights.Add(LightHelper.Light);
                }
                if (KeyBoardState.IsKeyDown(Keys.Y) && !LastKeyBoardState.IsKeyDown(Keys.Y))
                {
                    Console.WriteLine("All lights:");
                    foreach (IPointLight light in renderer.PointLights)
                    {
                        Console.WriteLine(light);
                    }
                }
            }

            LastGamePadState = GamePadState;
            LastKeyBoardState = KeyBoardState;
            base.Update(gameTime);
        }

        private float ChangeStrength = 1.0f;
        private void PlaceRockUpdate()
        {
            KeyboardState keyState = Keyboard.GetState();
            BVector3 translation = rockMesh.WorldTransform.Translation;

            if (keyState.IsKeyDown(Keys.Z))
            {
                ChangeStrength = 0.1f;
            }
            if (keyState.IsKeyDown(Keys.X))
            {
                ChangeStrength = 1.0f;
            }

            if (keyState.IsKeyDown(Keys.W))
            {
                //+z
                translation.Z += ChangeStrength;
            }
            if (keyState.IsKeyDown(Keys.S))
            {
                //-z
                translation.Z -= ChangeStrength;
            }
            if (keyState.IsKeyDown(Keys.A))
            {
                //-x
                translation.X += ChangeStrength;
            }
            if (keyState.IsKeyDown(Keys.D))
            {
                //+x
                translation.X -= ChangeStrength;
            }
            if (keyState.IsKeyDown(Keys.Q))
            {
                //+y
                translation.Y += ChangeStrength;
            }
            if (keyState.IsKeyDown(Keys.E))
            {
                //-y
                translation.Y -= ChangeStrength;
            }
            rockMesh.WorldTransform = new AffineTransform(new BVector3(4,4,4), BQuaternion.Identity, translation);
        }

        #region Message and FPS

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

        private void PrintMessage()
        {
            if (startTime < endTime)
            {

                spriteBatch.Begin();
                // Draw sprites
                Microsoft.Xna.Framework.Vector2 FontOrigin = font.MeasureString(message) / 2;
                spriteBatch.DrawString(font, message, fontPos, Color.DeepPink,
                    0, FontOrigin, 1.0f, SpriteEffects.None, 0.5f);

                spriteBatch.End();
            }
        }

        int x = 0;
        double totTime = 0;
        double time2 = 0;
        private void logFPS(GameTime gameTime)
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
        }

        #endregion

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            renderer.Draw(gameTime);

            //GraphicsDevice.Clear(Color.Coral);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque,
            SamplerState.PointClamp, DepthStencilState.Default,
            RasterizerState.CullCounterClockwise);
            spriteBatch.Draw(renderer.finalBackBuffer, new Rectangle(0, 0, GraphicsDevice.Viewport.Width,
            GraphicsDevice.Viewport.Height), Color.White);
            spriteBatch.End();

            logFPS(gameTime);

            PrintMessage();

            manager.Draw(gameTime, renderer.finalBackBuffer, renderer.depthMap, renderer.normalMap);
            //manager.Draw(gameTime);

            if (RenderDebug)
            {
                renderer.RenderDebug();
            }
        }
    }
}
