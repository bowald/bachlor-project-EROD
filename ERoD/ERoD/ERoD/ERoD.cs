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
        public enum GameState
        {
            MENU,
            GAME,
            LOAD_GAME
        }

        public GameState CurrentState = GameState.MENU;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private StartMenu Menu;
        private Texture2D LoadingTexture;

        private Space space;

        // Boolean for drawing the debug frame
        private bool RenderDebug;

        private int NumberOfPlayers;
        private PlayerView[] views;
        private Viewport original;
        private Viewport[][] ViewPorts;

        private RenderTarget2D finalScreenTarget;

        // GameLogic //
        GameLogic GameLogic;

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

        public ModelDrawer modelDrawer;  //Used to draw entities for debug.

        public KeyboardState KeyBoardState { get; set; }
        public KeyboardState LastKeyBoardState { get; set; }

        public ERoD()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = GameConstants.WindowWidth;
            graphics.PreferredBackBufferHeight = GameConstants.WindowHeight;
            
            Content.RootDirectory = "Content";

            LightHelper.Game = this;
            LightHelper.ToolEnabled = false;
            RenderDebug = false;

            ViewPorts = CreateViewPorts();
        }

        private Viewport[][] CreateViewPorts()
        {
            // Use different viewports depending on 1, 2, 3 or 4 players
            // 1: original
            // 2: left + right
            // 3: left + topright + bottomright
            // 4: topLeft, topRight, bottomLeft, bottomRight

            original = new Viewport();
            original.X = 0;
            original.Y = 0;
            original.Width = graphics.PreferredBackBufferWidth;
            original.Height = graphics.PreferredBackBufferHeight;
            original.MinDepth = 0;
            original.MaxDepth = 1;

            Viewport left = new Viewport();
            left.X = 0;
            left.Y = 0;
            left.Width = original.Width / 2;
            left.Height = original.Height;
            left.MinDepth = 0;
            left.MaxDepth = 1;

            Viewport right = new Viewport();
            right.X = original.Width / 2;
            right.Y = 0;
            right.Width = original.Width / 2;
            right.Height = original.Height;
            right.MinDepth = 0;
            right.MaxDepth = 1;

            Viewport topLeft = new Viewport();
            topLeft.X = 0;
            topLeft.Y = 0;
            topLeft.Width = original.Width / 2;
            topLeft.Height = original.Height / 2;
            topLeft.MinDepth = 0;
            topLeft.MaxDepth = 1;

            Viewport topRight = new Viewport();
            topRight.X = original.Width / 2;
            topRight.Y = 0;
            topRight.Width = original.Width / 2;
            topRight.Height = original.Height / 2;
            topRight.MinDepth = 0;
            topRight.MaxDepth = 1;

            Viewport bottomLeft = new Viewport();
            bottomLeft.X = 0;
            bottomLeft.Y = original.Height / 2;
            bottomLeft.Width = original.Width / 2;
            bottomLeft.Height = original.Height / 2;
            bottomLeft.MinDepth = 0;
            bottomLeft.MaxDepth = 1;

            Viewport bottomRight = new Viewport();
            bottomRight.X = original.Width / 2;
            bottomRight.Y = original.Height / 2;
            bottomRight.Width = original.Width / 2;
            bottomRight.Height = original.Height / 2;
            bottomRight.MinDepth = 0;
            bottomRight.MaxDepth = 1;

            // Assign viewports to number of players
            Viewport[][] viewPorts = new Viewport[4][];
            viewPorts[0] = new Viewport[1] { original };
            viewPorts[1] = new Viewport[2] { left, right };
            viewPorts[2] = new Viewport[3] { left, topRight, bottomRight };
            viewPorts[3] = new Viewport[4] { topLeft, topRight, bottomLeft, bottomRight };
            return viewPorts;
        }



        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Sprites/Lap1");

            finalScreenTarget = new RenderTarget2D(GraphicsDevice
                    , original.Width
                    , original.Height
                    , false
                    , SurfaceFormat.Color
                    , DepthFormat.None
                    , 0
                    , RenderTargetUsage.PreserveContents);

            fontPos = new Microsoft.Xna.Framework.Vector2(graphics.GraphicsDevice.Viewport.Width / 2,
                graphics.GraphicsDevice.Viewport.Height / 2);

            LoadingTexture = Content.Load<Texture2D>("Textures/loading");

            Menu = new StartMenu(this);
        }

        /// <summary>
        /// Load Game content that depends on the number of players selected
        /// 
        /// </summary>
        private void LoadGameContent(int numberOfPlayers)
        {

            views = new PlayerView[numberOfPlayers];

            for (int i = 0; i < numberOfPlayers; i++)
            {
                views[i] = new PlayerView(this, ViewPorts[numberOfPlayers - 1][i], i);
            }

            renderer = new DeferredRenderer(this, views);

            Effect objEffect = Content.Load<Effect>("Shaders/DeferredObjectRender");
            Effect objShadow = Content.Load<Effect>("Shaders/DeferredShadowShader");

            terrain = new HeightTerrainCDLOD(this, 7);
            Components.Add(terrain);
            Services.AddService(typeof(ITerrain), terrain);

            GameLogic = new GameLogic(this);
            this.Services.AddService(typeof(GameLogic), GameLogic);

            space = new Space();
            space.ForceUpdater.Gravity = new BVector3(0, GameConstants.Gravity, 0);
            Services.AddService(typeof(Space), space);

            space.Add(((ITerrain)Services.GetService(typeof(ITerrain))).PhysicTerrain);

            #region Bridge

            // Load the bridge model
            Model bridgeModel = Content.Load<Model>("Models/bridge");
            AffineTransform bridgeTransform = new AffineTransform(
                new BVector3(6f, 6f, 6f),
                BQuaternion.Identity,
                new BVector3(138.5f, -71, -145));
            var bridgeMesh = LoadStaticObject(bridgeModel, bridgeTransform);
            StaticObject bridge = new StaticObject(bridgeModel, bridgeMesh, this);
            bridge.SpecularMap = Content.Load<Texture2D>("Textures/Bridge/specular");
            bridge.Textures = new Texture2D[1] { Content.Load<Texture2D>("Textures/Bridge/diffuse") };
            bridge.BumpMap = Content.Load<Texture2D>("Textures/Bridge/normal");
            bridge.TextureEnabled = new bool[1] {true};
            bridge.TexMult = 3f; // make the texture cover 3x more area before repeating.
            bridge.BumpConstant = 1f;
            bridge.GlowMap = new Texture2D[1] { Content.Load<Texture2D>("Textures/Specular/specular_0") };
            bridge.standardEffect = objEffect;
            bridge.shadowEffect = objShadow;

            space.Add(bridgeMesh);
            Components.Add(bridge);

            #endregion

            #region Ship loading

            Model shipModel = Content.Load<Model>("Models/racer");
            Vector3 shipScale = new Vector3(0.14f, 0.14f, 0.14f);
            Vector3 shipPosition = new Vector3(865, -45, -255);

            // Ship parts
            /* 
             * 0: Front inside lights
             * 1: Door
             * 2: Green ring lights
             * 3: LightBlue side lights
             * 4: "Engine"
             * 5: Red engine things
             * 6: Window
             * 7: Window frame
             * 8: Body
             */

            Texture2D doorTexture = Content.Load<Texture2D>("Textures/Racer/door");
            Texture2D[] bodyColors = new Texture2D[4];
            bodyColors[0] = Content.Load<Texture2D>("Textures/Racer/body_orange");
            bodyColors[1] = Content.Load<Texture2D>("Textures/Racer/body_blue");
            bodyColors[2] = Content.Load<Texture2D>("Textures/Racer/body_lightgreen");
            bodyColors[3] = Content.Load<Texture2D>("Textures/Racer/body_red");
            
            Texture2D[] shipGlow = new Texture2D[9];
            Texture2D glow100 = Content.Load<Texture2D>("Textures/Specular/specular_100");
            shipGlow[0] = glow100;
            shipGlow[2] = glow100;
            shipGlow[3] = glow100;
            shipGlow[4] = glow100;
            shipGlow[5] = glow100;
            
            string[] names = new string[] { "Alex", "Anton", "Johan", "TheGovernator" };

            ConvexHullShape shipShape = CreateConvexHullShape(shipModel, shipScale);

            for (int i = 0; i < views.Length; i++)
            {
                Entity entity = new Entity(shipShape, 10);
                entity.Position = ConversionHelper.MathConverter.Convert(shipPosition + new Vector3(8 * i, 0, 0));
                Ship ship = new Ship(entity, shipModel, shipScale, this);
                space.Add(entity);
                Texture2D[] shipTextures = new Texture2D[9];
                shipTextures[1] = doorTexture;
                shipTextures[8] = bodyColors[i];
                ship.Textures = shipTextures;
                bool[] textureEnabled = new bool[9];
                textureEnabled[1] = true;
                textureEnabled[8] = true;
                ship.SpecularMap = glow100;
                ship.GlowMap = shipGlow;
                ship.TextureEnabled = textureEnabled;
                ship.standardEffect = objEffect;
                ship.shadowEffect = objShadow;

                ship.AddCollidable(bridgeMesh);

                // Add one thruster emitter for each ship.
                //renderer.Emitters.Add(new ThrusterEmitter(6000, 60, 800, 1.0f, 0.0005f, entity));

                Components.Add(ship);
                GameLogic.AddPlayer(ship, names[i]);
                views[i].SetChaseTarget(ship);
                Components.Add(views[i]);
            }

            #endregion

            List<Texture2D> textures = new List<Texture2D> { Content.Load<Texture2D>("Textures/Particles/Plasma") };
            foreach (BaseEmitter emitter in renderer.Emitters)
            {
                emitter.LoadContent(textures, GraphicsDevice);
            }

            Model cubeModel = Content.Load<Model>("Models/cube");
            CreateCheckPoints(objEffect, cubeModel);
            

            #region Add PostProcess

            for (int i = 0; i < views.Length; i++)
            {
                views[i].Manager = new PostProcessingManager(this, renderer.renderTargets[i]);
            }

            for (int i = 0; i < views.Length; i++)
            {
                views[i].Manager.AddEffect(new Bloom(this, 0.5f, views[i].Viewport.Width, views[i].Viewport.Height));
                views[i].Manager.AddEffect(new GodRays(this, new Vector3(100, 20, 100), 60.0f, 0.8f, 0.99f, 0.8f, 0.15f));
            }

            #endregion

            #region Lights

            renderer.DirectionalLights.Add(new DirectionalLight(this, new Vector3(2500, 2000, 2500), Vector3.Zero, Color.LightYellow, 0.4f, 7000.0f, GameConstants.ShadowsEnabled));

            renderer.PointLights.AddRange(LightHelper.ReadLights());

            #endregion
        }

        private void CreateCheckPoints(Effect effect, Model model)
        {
            int nbrPoints = GameConstants.NumberOfCheckpoints;
            int i = 0;
            Checkpoint[] points = new Checkpoint[nbrPoints];
            points[i++] = GameLogic.AddCheckpoint(new BVector3(450, 80, 5), new BVector3(886, -38, -632), BEPUutilities.MathHelper.ToRadians(9f));
            points[i++] = GameLogic.AddCheckpoint(new BVector3(250, 60, 5), new BVector3(-5, -14, -1178), BEPUutilities.MathHelper.ToRadians(83f));
            points[i++] = GameLogic.AddCheckpoint(new BVector3(300, 60, 5), new BVector3(-402, -27, -630), BEPUutilities.MathHelper.ToRadians(72f));
            points[i++] = GameLogic.AddCheckpoint(new BVector3(300, 50, 5), new BVector3(-456, -45, 37), BEPUutilities.MathHelper.ToRadians(-63f));
            points[i++] = GameLogic.AddCheckpoint(new BVector3(250, 50, 5), new BVector3(-97, -29, 782), BEPUutilities.MathHelper.ToRadians(45f));
            points[i++] = GameLogic.AddCheckpoint(new BVector3(150, 50, 5), new BVector3(846, -55, 842), BEPUutilities.MathHelper.ToRadians(16f));
            points[i++] = GameLogic.AddCheckpoint(new BVector3(80, 35, 5), new BVector3(3, 10, -85), BEPUutilities.MathHelper.ToRadians(-8f));
            points[i++] = GameLogic.AddCheckpoint(new BVector3(100, 50, 5), new BVector3(623, 43, -239), BEPUutilities.MathHelper.ToRadians(0f));

            foreach(Checkpoint p in points)
            {
                p.BasicEffect = effect;
                p.Model = model;
                //Components.Add(p); //Uncomment to draw checkpoints
            }
        }

        private ConvexHullShape CreateConvexHullShape(Model model, Vector3 scaling)
        {
            BVector3[] vertices;
            int[] indices;
            ModelDataExtractor.GetVerticesAndIndicesFromModel(model, out vertices, out indices);
            
            // Convert to list since array is read only.
            IList<BVector3> verts = new List<BVector3>(vertices);

            // Remove redundant vertices that causes the convexhullshape to crash.
            ConvexHullHelper.RemoveRedundantPoints(verts);
            vertices = verts.ToArray<BVector3>();

            return new ConvexHullShape(OurHelper.scaleVertices(vertices, scaling));
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
            KeyBoardState = Keyboard.GetState();

            if (CurrentState == GameState.GAME)
            {
                UpdateGameLoop(gameTime);
                base.Update(gameTime);
            }
            if (CurrentState == GameState.LOAD_GAME)
            {
                LoadGameContent(NumberOfPlayers);
                space.Update();
                CurrentState = GameState.GAME;
            }
            else if (CurrentState == GameState.MENU)
            {
                // Update Menu (Input)
                switch(Menu.Update(gameTime))
                {
                    case StartMenu.MenuState.EXIT_GAME:
                        this.Exit();
                        break;
                    case StartMenu.MenuState.START_GAME_1:
                        NumberOfPlayers = 1;
                        CurrentState = GameState.LOAD_GAME;
                        break;
                    case StartMenu.MenuState.START_GAME_2:
                        NumberOfPlayers = 2;
                        CurrentState = GameState.LOAD_GAME;
                        break;
                    case StartMenu.MenuState.START_GAME_3:
                        NumberOfPlayers = 3;
                        CurrentState = GameState.LOAD_GAME;
                        break;
                    case StartMenu.MenuState.START_GAME_4:
                        NumberOfPlayers = 4;
                        CurrentState = GameState.LOAD_GAME;
                        break;
                    default:
                        // Not an interesting case
                        break;
                }
            }

            LastKeyBoardState = KeyBoardState;
        }

        private void UpdateGameLoop(GameTime gameTime)
        {
            // Allows the game to exit
            // TODO: Change this before "release", close with menu
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


            if (KeyBoardState.IsKeyDown(Keys.F1) && LastKeyBoardState.IsKeyUp(Keys.F1))
            {
                // Show deferred debug
                RenderDebug = !RenderDebug;
            }

            if (KeyBoardState.IsKeyDown(Keys.F2) && LastKeyBoardState.IsKeyUp(Keys.F2))
            {
                // Swap cameras
                for (int i = 0; i < views.Length; i++)
                {
                    views[i].FreeCameraActive = !views[i].FreeCameraActive;
                }
            }

            space.Update();

            #region LightHelper

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

            #endregion
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
            if (CurrentState == GameState.GAME)
            {
                RenderGame(gameTime);
            }
            else if (CurrentState == GameState.LOAD_GAME)
            {
                GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                spriteBatch.Draw(LoadingTexture, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
                spriteBatch.End();
            }
            else if (CurrentState == GameState.MENU)
            {
                GraphicsDevice.Clear(Color.Black);
                Menu.Draw(gameTime);
            }
        }

        private void RenderGame(GameTime gameTime)
        {
            // Do Deferred Render pass for each viewport
            for (int i = 0; i < views.Length; i++)
            {
                Services.RemoveService(typeof(ICamera));
                Services.AddService(typeof(ICamera), views[i].Camera);
                renderer.Draw(gameTime, i);
            }
            // Clear final screen target
            GraphicsDevice.SetRenderTarget(finalScreenTarget);
            GraphicsDevice.Clear(Color.Black);

            // Draw the finalbackbuffer and postprocesses to the final screen
            for (int i = 0; i < views.Length; i++)
            {
                Services.RemoveService(typeof(ICamera));
                Services.AddService(typeof(ICamera), views[i].Camera);

                GraphicsDevice.Viewport = views[i].Viewport;

                views[i].Manager.Draw(gameTime, finalScreenTarget);

                //PrintMessage();
                //logFPS(gameTime);

                if (RenderDebug)
                {
                    renderer.RenderDebug(renderer.renderTargets[i]);
                }
            }

            // Draw the final screen to the backbuffer
            graphics.GraphicsDevice.SetRenderTarget(null);
            graphics.GraphicsDevice.Viewport = original;

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            spriteBatch.Draw(finalScreenTarget, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
            spriteBatch.End();
        }
    }
}
