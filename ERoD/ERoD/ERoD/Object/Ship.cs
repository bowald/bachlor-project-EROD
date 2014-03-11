using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.BroadPhaseEntries;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using BRay = BEPUutilities.Ray;
using BVector3 = BEPUutilities.Vector3;

namespace ERoD
{
    class Ship : EntityObject
    {
        private Space space;
        private ERoD erod;
        private float rollSpeed = 0.7f;
        private float maxSpeed = 50.0f;
        
        private StaticCollidable Terrain
        {
            get { return ((ITerrain)Game.Services.GetService(typeof(ITerrain))).PhysicTerrain; }
        }

        public Ship(Entity entity, Model model, Matrix world, Game game) 
            : base(entity, model, world, game)
        {
            erod = game as ERoD;
            space = erod.Space;
            entity.BecomeKinematic();
        }
        /// <summary>
        /// Returns the strafeing velocity
        /// </summary>
        /// <param name="Right"> True for Right turn</param>
        private BVector3 strafe(Boolean Right)
        {
            BVector3 velocity;
            BVector3 turn;
            if (Right)
            {
                turn = Entity.OrientationMatrix.Right;
            }
            else
            {
                turn = Entity.OrientationMatrix.Left;
            }
            velocity = turn * 15.0f;
            return new BVector3(velocity.X, 0, velocity.Z);
        }
        /// <summary>
        /// Returns the distance from the ship to the ground.
        /// </summary>
        private float distancefromGround()
        {
            BEPUutilities.RayHit hit;
            BRay ray = new BRay(Entity.Position, BVector3.Down);
            Terrain.RayCast(ray, 100.0f, out hit);
            return hit.T;
        }
        /// <summary>
        /// Given an entity.OriantationMatrix-vector, returns wich direction it facing
        /// Helper for debugging rayCasting,
        /// </summary>
        private String helper(BVector3 vec3){
            if (vec3 == Entity.OrientationMatrix.Forward)
                return "Forward";
            if (vec3 == Entity.OrientationMatrix.Right)
                return "Right";
            if (vec3 == Entity.OrientationMatrix.Left)
                return "Left";
            if (vec3 == Entity.OrientationMatrix.Up)
                return "Up";
            if (vec3 == Entity.OrientationMatrix.Down)
                return "Down";
            return "no match";
        }
        private void dontCollide(BRay ray, float rayLength, float gamePadDirection)
        {
            BEPUutilities.RayHit hit;
            if (Terrain.RayCast(ray, rayLength, out hit))
            {
                BRay rayRight = new BRay(Entity.Position, Entity.OrientationMatrix.Forward - 0.3f * ray.Direction);
                BRay rayLeft = new BRay(Entity.Position, Entity.OrientationMatrix.Forward + 0.3f * ray.Direction);
                BEPUutilities.RayHit hitRight;
                BEPUutilities.RayHit hitLeft;
                Boolean rayCastHitRight = Terrain.RayCast(rayRight, 4.0f, out hitRight);
                Boolean rayCastHitLeft = Terrain.RayCast(rayLeft, 4.0f, out hitLeft);
                float angularspeed = 0.5f;
                float directionBump = 0.2f;
                BVector3 velocityDirection = -ray.Direction;
                if (rayCastHitRight && rayCastHitLeft)
                {
                    if (hitRight.T > hitLeft.T)
                    {
                        //yaw = -yaw;
                        angularspeed = -angularspeed;
                        directionBump = -directionBump;
                    }
                    velocityDirection = Entity.OrientationMatrix.Forward * directionBump;
                }
                else if (rayCastHitRight)
                {
                    Debug.WriteLine("only right");
                    velocityDirection = Entity.OrientationMatrix.Forward * directionBump;
                }
                else if (rayCastHitLeft)
                {
                    Debug.WriteLine("only Left");
                    angularspeed = -angularspeed;
                    directionBump = -directionBump;
                    velocityDirection = Entity.OrientationMatrix.Forward * directionBump;
                }
                else
                {
                    angularspeed = 0;
                }
                if (gamePadDirection < 0) 
                {
                    gamePadDirection = -1.0f;
                }
                else
                {
                    gamePadDirection = 1.0f;
                }
                Entity.AngularVelocity = new BVector3(0, angularspeed * gamePadDirection, 0);
                Entity.LinearVelocity = velocityDirection * (Entity.LinearVelocity.Length() * 0.9f);
            }
        }
        /// <summary>
        /// Returns true if its above the ideal heigth
        /// </summary>
        /// <param name="idealHeight">Ships hovering distance from the ground</param>
        private bool fly(float idealHeight){
            float h = distancefromGround();
            if ((idealHeight - h) > 0) {
                Entity.Position = new BVector3(Entity.Position.X, Entity.Position.Y + (idealHeight - h), Entity.Position.Z);
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// Checks if ship is rolling right
        /// </summary>
        private bool ifRollRight()
        {
            BVector3 Right = Entity.Position + Entity.OrientationMatrix.Right;
            BVector3 Left = Entity.Position + Entity.OrientationMatrix.Left;
            return Right.Y < Left.Y;
        }
        /// <summary>
        /// Returns the ships new velocity.
        /// Used to make the ship accelerate.
        /// </summary>
        private BVector3 newVelocity(float dt, float strafeSpeed)
        {
            BVector3 currentSpeed = new BVector3(Entity.LinearVelocity.X,0, Entity.LinearVelocity.Z);
            float currentLength = currentSpeed.Length() - strafeSpeed;
            BVector3 newVelocity = BVector3.Zero;
            float accelerationLength;
            float a = currentLength / maxSpeed;
            if (a < 0.1f)
            {
                newVelocity = Entity.OrientationMatrix.Forward * maxSpeed * 0.12f;
            }
            else if (a < 0.7f)
            {
                accelerationLength = maxSpeed * 0.2f * dt;
                newVelocity = Entity.OrientationMatrix.Forward * (Entity.LinearVelocity.Length() + accelerationLength);
            }
            else if (a < 0.9f)
            {
                accelerationLength = maxSpeed * 0.1f * dt;
                newVelocity = Entity.OrientationMatrix.Forward * (Entity.LinearVelocity.Length() + accelerationLength);
            }
            else if (a < 1.0f)
            {
                accelerationLength = maxSpeed * 0.05f * dt;
                newVelocity = Entity.OrientationMatrix.Forward * (Entity.LinearVelocity.Length() + accelerationLength);
            }
            else
            {
                newVelocity = Entity.OrientationMatrix.Forward * maxSpeed;
            }
            return newVelocity;
        }
        /// <summary>
        /// Gets the speed on the ground plane
        /// </summary>
        private BVector3 getPlanarSpeedVector()
        {
            return new BVector3(Entity.LinearVelocity.X, 0, Entity.LinearVelocity.Z);
        }
        /// <summary>
        /// Returns the roll-factor for the ship, takes ships speed, current angle and gamepad in account
        /// </summary>
        private float rolling(float gamepadX, float dt)
        {
            float rollValue = 0;
            float angle = (float)(Math.Acos(BVector3.Dot(Entity.OrientationMatrix.Up, BVector3.Up)) * 180.0 / Math.PI);
            if (angle > 1.0f && gamepadX == 0)
            {
                rollValue = (Entity.LinearVelocity.Length() * 1.5f) / maxSpeed * rollSpeed * dt + 0.005f;
                if (ifRollRight())
                {
                    rollValue = -rollValue;
                }
            }
            else if (angle < 45.0f)
            {
                rollValue = (Entity.LinearVelocity.Length() * 1.5f) / maxSpeed * rollSpeed * dt * gamepadX + gamepadX / 500;
            }
            return rollValue;
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            BVector3 forward = BVector3.Zero;
            BVector3 shipStrafe = BVector3.Zero;
            BVector3 downward = BVector3.Zero;
            GamePadState gamePadState = ((ERoD)Game).GamePadState;
            Single roll = 0;

            //Aircontroll
            if (fly(4.0f))
            {
                downward = new BVector3(0, -16.0f, 0);
                downward.Y -= gamePadState.ThumbSticks.Left.Y * 5.0f;
            }

            // Turning, strafing and rolling the ship
            if (gamePadState.ThumbSticks.Left.X != 0)
            {
                roll = rolling(gamePadState.ThumbSticks.Left.X, dt);
                Entity.AngularVelocity = new BVector3(0,-gamePadState.ThumbSticks.Left.X * 1.3f,0);
                shipStrafe = strafe(gamePadState.ThumbSticks.Left.X > 0);
            }
            // stop turning and stablize the roll
            else {
                roll = rolling(gamePadState.ThumbSticks.Left.X, dt);
                shipStrafe = BVector3.Zero;
                Entity.AngularVelocity = BVector3.Zero;
            }
            // Gets forwad Acceleration
            if (gamePadState.IsButtonDown(Buttons.A))
            {
                forward = newVelocity(dt, shipStrafe.Length());
            }
            // Gets forwad and downward decrease
            else
            {
                forward = (getPlanarSpeedVector() * 0.98f) - shipStrafe + new BVector3(0, Entity.LinearVelocity.Y * 0.5f, 0);
            }
            // Applies the roll
            BEPUutilities.Quaternion AddRot = BEPUutilities.Quaternion.CreateFromYawPitchRoll(0, 0, -roll);
            Entity.Orientation *= AddRot;

            // Applies all speed
            Entity.LinearVelocity = shipStrafe + forward + downward;



            // Checks for collitions
            dontCollide(new BRay(Entity.Position, Entity.OrientationMatrix.Forward), 4.0f, gamePadState.ThumbSticks.Left.X);
            dontCollide(new BRay(Entity.Position, Entity.OrientationMatrix.Left), 2.0f, gamePadState.ThumbSticks.Left.X);
            dontCollide(new BRay(Entity.Position, Entity.OrientationMatrix.Right), 2.0f, gamePadState.ThumbSticks.Left.X);
            base.Update(gameTime);
        }
    }
}
