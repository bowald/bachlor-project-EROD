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

        private PlayerView[] views;
        private Viewport original;

        private RenderTarget2D finalScreenTarget;
        // GameLogic //
        GameLogic GameLogic;

        //public Boolean DebugEnabled;
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

        public ModelDrawer modelDrawer;  //Used to draw entities for debug.

        public KeyboardState KeyBoardState { get; set; }
        public KeyboardState LastKeyBoardState { get; set; }

        public ERoD()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = GameConstants.WindowWidth;
            graphics.PreferredBackBufferHeight = GameConstants.WindowHeight;
            
            Content.RootDirectory = "Content";

            Viewport[][] viewPorts = CreateViewPorts();

            views = new PlayerView[GameConstants.NumberOfPlayers];

            for (int i = 0; i < GameConstants.NumberOfPlayers; i++)
            {
                views[i] = new PlayerView(this, viewPorts[GameConstants.NumberOfPlayers - 1][i], i);
            }

            renderer = new DeferredRenderer(this, views);

            RenderDebug = false;
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
            terrain = new HeightTerrainCDLOD(this, 7);
            Components.Add(terrain);
            Services.AddService(typeof(ITerrain), terrain);

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

            Model cubeModel = Content.Load<Model>("Models/cube");

            Effect objEffect = Content.Load<Effect>("Shaders/DeferredObjectRender");
            Effect objShadow = Content.Load<Effect>("Shaders/DeferredShadowShader");

            LoadingTexture = Content.Load<Texture2D>("Textures/loading");

            space = new Space();
            space.ForceUpdater.Gravity = new BVector3(0, GameConstants.Gravity, 0);
            Services.AddService(typeof(Space), space);

            LightHelper.Game = this;

            space.Add(((ITerrain)Services.GetService(typeof(ITerrain))).PhysicTerrain);


            Menu = new StartMenu(this);

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
            bridge.Texture = Content.Load<Texture2D>("Textures/Bridge/diffuse");
            bridge.BumpMap = Content.Load<Texture2D>("Textures/Bridge/normal");
            bridge.TextureEnabled = true;
            bridge.TexMult = 3f; // make the texture cover 3x more area before repeating.
            bridge.BumpConstant = 1f;
            bridge.standardEffect = objEffect;
            bridge.shadowEffect = objShadow;

            space.Add(bridgeMesh);
            Components.Add(bridge);

            #endregion

            #region Ship loading

            Model shipModel = Content.Load<Model>("Models/space_frigate");
            Model shipModelT = Content.Load<Model>("Models/space_frigate_tangentOn");
            Vector3 shipScale = new Vector3(0.06f, 0.06f, 0.06f);
            Vector3 shipPosition = new Vector3(865, -45, -255);

            string[] names = new string[] { "Alex", "Anton", "Johan", "TheGovernator"};

            for (int i = 0; i < views.Length; i++)
            {
                Entity entity = LoadEntityObject(shipModel, shipPosition + new Vector3(8 * i, 0, 0), shipScale);
                Ship ship = new Ship(entity, shipModelT, shipScale, this);
                space.Add(entity);
                ship.Texture = Content.Load<Texture2D>("Textures/Ship/diffuse");
                ship.SpecularMap = Content.Load<Texture2D>("Textures/Ship/specular");
                ship.TextureEnabled = true;
                ship.standardEffect = objEffect;
                ship.shadowEffect = objShadow;
                ship.Mask = true;

                ship.AddCollidable(bridgeMesh);

                Components.Add(ship);
                GameLogic.AddPlayer(ship, names[i]);
                views[i].SetChaseTarget(ship);
                Components.Add(views[i]);
            }

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

            #region Add PostProcess

            for (int i = 0; i < views.Length; i++)
            {
                views[i].Manager = new PostProcessingManager(this, renderer.renderTargets[i]);
            }

            for (int i = 0; i < views.Length; i++ )
            {
                views[i].Manager.AddEffect(new Bloom(this, 0.5f, views[i].Viewport.Width, views[i].Viewport.Height));
                views[i].Manager.AddEffect(new GodRays(this, new Vector3(100, 20, 100), 60.0f, 0.8f, 0.99f, 0.8f, 0.15f));
            }

            #endregion

            #region Lights

            renderer.DirectionalLights.Add(new DirectionalLight(this, new Vector3(2500, 2000, 2500), Vector3.Zero, Color.LightYellow, 0.4f, 7000.0f, GameConstants.ShadowsEnabled));
            
            LightHelper.ToolEnabled = false;
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
            KeyBoardState = Keyboard.GetState();

            if (CurrentState == GameState.GAME || CurrentState == GameState.LOAD_GAME)
            {
                UpdateGameLoop(gameTime);
                base.Update(gameTime);
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
                    case StartMenu.MenuState.START_GAME:
                        // Get number of players
                        // Initiate game stuff
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
