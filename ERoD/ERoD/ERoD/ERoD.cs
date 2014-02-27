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
        public ICamera Camera;
        public Boolean DebugEnabled;

        private DeferredRenderer renderer;

        public ModelDrawer modelDrawer;  //Used to draw entitis for debug.

        public GamePadState GamePadState { get; set; }
        Model CubeModel;

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
            Camera = new FreeCamera(this, 0.1f, 1000, new Vector3(0, 50, 40), 40.0f);
            this.Services.AddService(typeof(ICamera), Camera);
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Model shipModel = Content.Load<Model>("Models/space_frigate");
            Vector3 shipScale = new Vector3(0.07f, 0.07f, 0.07f);
            Vector3 shipPosition = new Vector3(0, 60, 0);

            Model groundModel = Content.Load<Model>("Models/Z3B0_Arena_alphaVersion");
            AffineTransform groundTransform = new AffineTransform(new BVector3(0.1f, 0.1f, 0.1f), new BQuaternion(0, 0, 0, 0), new BVector3(0, 0, 0));

            CubeModel = Content.Load<Model>("Models/cube");
            modelDrawer = new InstancedModelDrawer(this); // For debug
            space = new Space();

            foreach (Entity e in space.Entities)
            {
                Box box = e as Box;
                if (box != null) //This won't create any graphics for an entity that isn't a box since the model being used is a box.
                {

                    Matrix scaling = Matrix.CreateScale(box.Width, box.Height, box.Length); //Since the cube model is 1x1x1, it needs to be scaled to match the size of each individual box.
                    EntityObject model = new EntityObject(e, CubeModel, scaling, this);
                    //Add the drawable game component for this entity to the game.
                    Components.Add(model);
                    e.Tag = model; //set the object tag of this entity to the model so that it's easy to delete the graphics component later if the entity is removed.
                }
            }

            Effect objEffect = Content.Load<Effect>("Shaders/DeferredObjectRender");

            space.ForceUpdater.Gravity = new BVector3(0, -0.82f, 0);
            EntityObject eobj = LoadEntityObject(shipModel, shipPosition, shipScale);
            eobj.Texture = Content.Load<Texture2D>("Textures/Ship2/diffuse");
            eobj.TextureEnabled = false;
            eobj.standardEffect = objEffect;
            Components.Add(eobj);

            //Camera = new ChaseCamera(eobj.entity, new BEPUutilities.Vector3(0.0f, 5.0f, 0.0f), true, 20.0f, 0.1f, 2000.0f, this);
            //this.Services.AddService(typeof(ICamera), Camera);
            //((ChaseCamera)Camera).Initialize();

            StaticObject sobj = LoadStaticObject(groundModel, groundTransform);
            sobj.Texture = Content.Load<Texture2D>("Textures/Ground/diffuse");
            sobj.TextureEnabled = true;
            sobj.standardEffect = objEffect;
            Components.Add(sobj);

            renderer.DirectionalLights.Add(new DirectionalLight(this, new Vector3(50, 250, 250), Vector3.Zero, Color.LightYellow, 1.0f, true));

            renderer.PointLights.Add(new PointLight(new Vector3(10, 10, 10), Color.White, 25.0f, 1.0f));
            renderer.PointLights.Add(new PointLight(new Vector3(-10, 10, -10), Color.Red, 25.0f, 1.0f));
            renderer.PointLights.Add(new PointLight(new Vector3(95, 17, 70), Color.Blue, 10.0f, 1.0f));
            renderer.PointLights.Add(new PointLight(new Vector3(110, 17, 55), Color.Cyan, 10.0f, 1.0f));
            renderer.PointLights.Add(new PointLight(new Vector3(115, 17, 45), Color.Red, 10.0f, 1.0f));
        }

        private EntityObject LoadEntityObject(Model model, Vector3 position, Vector3 scaling)
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
            space.Add(entity);

            if (DebugEnabled) 
            {
                modelDrawer.Add(entity);
            }
            return new EntityObject(entity, model, Matrix.CreateScale(scaling), this);
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
            if (DebugEnabled)
            {
                modelDrawer.Update(); // For debug
            }
            GamePadState = GamePad.GetState(PlayerIndex.One);

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                this.Exit();
            }

            if (GamePadState.Triggers.Right > 0)
            {
                //If the user is holding down the trigger, start firing some boxes.
                //First, create a new dynamic box at the camera's location.
                Box toAdd = new Box(ConversionHelper.MathConverter.Convert(Camera.Position), 1, 1, 1, 1);
                //Set the velocity of the new box to fly in the direction the camera is pointing.
                //Entities have a whole bunch of properties that can be read from and written to.
                //Try looking around in the entity's available properties to get an idea of what is available.
                toAdd.LinearVelocity = ConversionHelper.MathConverter.Convert(Camera.World.Forward * 10);
                //Add the new box to the simulation.
                space.Add(toAdd);

                //Add a graphical representation of the box to the drawable game components.
                EntityObject obj = new EntityObject(toAdd, CubeModel, Matrix.Identity, this);
                obj.TextureEnabled = false;
                Components.Add(obj);
                toAdd.Tag = obj;  //set the object tag of this entity to the model so that it's easy to delete the graphics component later if the entity is removed.
            }
            
            space.Update();

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

            renderer.RenderDebug();

            //if (DebugEnabled)
            //{
            //    modelDrawer.Draw(ConversionHelper.MathConverter.Convert(Camera.View), ConversionHelper.MathConverter.Convert(Camera.Projection));
            //}
        }
    }
}
