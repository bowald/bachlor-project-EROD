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
        private Space space;
        
        // Camera Variables
        public BaseCamera ChaseCamera;
        public BaseCamera FreeCamera;
        Boolean FreeCameraActive;
        Entity ShipEntity;

        public Boolean DebugEnabled;
        public StaticMesh testVarGround;

        public ModelDrawer modelDrawer;  //Used to draw entities for debug.

        public Space Space
        {
            get { return space; }
        }

        public GamePadState GamePadState { get; set; }
        public GamePadState LastGamePadState { get; set; }
        Model CubeModel;

        public ERoD()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Model shipModel = Content.Load<Model>("Models/ship");
            Vector3 shipScale = new Vector3(0.002f, 0.002f, 0.002f);
            Vector3 shipPosition = new Vector3(-24, 15, 22);
            Quaternion shipRotation = Quaternion.CreateFromAxisAngle(Vector3.Up, Microsoft.Xna.Framework.MathHelper.ToRadians(-90));

            Model groundModel = Content.Load<Model>("Models/ground");
            AffineTransform groundTransform = new AffineTransform(new BVector3(10, 10, 10), new BEPUutilities.Quaternion(0,0,0,0), BVector3.Zero);

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

            space.ForceUpdater.Gravity = new BVector3(0, -9.82f, 0);
            AddStaticObject(groundModel, groundTransform);
            AddShip(shipModel, shipPosition, shipRotation, shipScale);
        }

        private void AddEntityObject(Model model, Vector3 position, Vector3 scaling)
        {            
            BVector3[] vertices;
            int[] indices;
            ModelDataExtractor.GetVerticesAndIndicesFromModel(model, out vertices, out indices);
            ConvexHullShape CHS = new ConvexHullShape(OurHelper.scaleVertices(vertices, scaling));
            Entity entity = new Entity(CHS, 10);
            entity.Position = ConversionHelper.MathConverter.Convert(position);
            space.Add(entity);

            if (DebugEnabled) 
            {
                modelDrawer.Add(entity);
            }
            Components.Add(new EntityObject(entity, model, Matrix.CreateScale(scaling), this));        
        }

        private void AddShip(Model model, Vector3 position, Quaternion shipRotation, Vector3 scaling)
        {
            BVector3[] vertices;
            int[] indices;
            ModelDataExtractor.GetVerticesAndIndicesFromModel(model, out vertices, out indices);
            ConvexHullShape CHS = new ConvexHullShape(OurHelper.scaleVertices(vertices, scaling));
            Entity entity = new Entity(CHS, 250);
            entity.Orientation = ConversionHelper.MathConverter.Convert(shipRotation);
            entity.Position = ConversionHelper.MathConverter.Convert(position);
            space.Add(entity);
            if (DebugEnabled)
            {
                modelDrawer.Add(entity);
            }
            Components.Add(new Ship(entity, model, Matrix.CreateScale(scaling), this));

            ShipEntity = entity;

            // Should not be done here, need to move
            ChaseCamera = new ChaseCamera(entity, new BEPUutilities.Vector3(0.0f, 5.0f, 0.0f), true, 20.0f, 0.1f, 2000.0f, this);
            ChaseCamera.Initialize();
        }

        private void AddStaticObject(Model model, AffineTransform transform) 
        {
            BVector3[] vertices;
            int[] indices;
            ModelDataExtractor.GetVerticesAndIndicesFromModel(model, out vertices, out indices);
            var mesh = new StaticMesh(vertices, indices, transform);
            testVarGround = mesh;
            space.Add(mesh);
            Components.Add(new StaticObject(model, MathConverter.Convert(mesh.WorldTransform.Matrix), this));
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
                Box toAdd = new Box(ConversionHelper.MathConverter.Convert(FreeCamera.Position), 1, 1, 1, 1);
                //Set the velocity of the new box to fly in the direction the camera is pointing.
                //Entities have a whole bunch of properties that can be read from and written to.
                //Try looking around in the entity's available properties to get an idea of what is available.
                toAdd.LinearVelocity = ConversionHelper.MathConverter.Convert(FreeCamera.World.Forward * 10);
                //Add the new box to the simulation.
                space.Add(toAdd);

                //Add a graphical representation of the box to the drawable game components.
                EntityObject obj = new EntityObject(toAdd, CubeModel, Matrix.Identity, this);
                Components.Add(obj);
                toAdd.Tag = obj;  //set the object tag of this entity to the model so that it's easy to delete the graphics component later if the entity is removed.
            }

            if ((GamePadState.Buttons.B == ButtonState.Pressed) && (LastGamePadState.Buttons.B == ButtonState.Released ))
            {
                Services.RemoveService(typeof(ICamera));
                if (FreeCameraActive)
                {
                    FreeCameraActive = false;
                    Console.WriteLine("Freecameractive = false");
                    Services.AddService(typeof(ICamera), ChaseCamera);
                }
                else
                {
                    FreeCameraActive = true;
                    Services.AddService(typeof(ICamera), FreeCamera);
                }
            }

            space.Update();

            //FreeCamera.Update(gameTime);
            //ChaseCamera.Update(gameTime);

            base.Update(gameTime);
            LastGamePadState = GamePadState;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            if (DebugEnabled)
            {
                if (FreeCameraActive)
                {
                    modelDrawer.Draw(ConversionHelper.MathConverter.Convert(FreeCamera.View), ConversionHelper.MathConverter.Convert(FreeCamera.Projection));
                }
                else
                {                 
                    modelDrawer.Draw(ConversionHelper.MathConverter.Convert(ChaseCamera.View), ConversionHelper.MathConverter.Convert(ChaseCamera.Projection));
                }
            }
            base.Draw(gameTime);
        }
    }
}
