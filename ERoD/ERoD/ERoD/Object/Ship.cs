﻿using BEPUphysics;
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

        List<StaticCollidable> Collidables = new List<StaticCollidable>();
        BVector3 angularVelocity;
        
        private Matrix baseTransform;
        Matrix BaseTransform
        {
            get { return baseTransform; }
        }

        public Ship(Entity entity, Model model, Matrix world, Game game) 
            : base(entity, model, world, game)
        {
            entity.BecomeKinematic();
            AddCollidable(((ITerrain)Game.Services.GetService(typeof(ITerrain))).PhysicTerrain);
            baseTransform = world;
        }

        public void AddCollidable(StaticCollidable c)
        {
            Collidables.Add(c);
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
            velocity = turn * ObjectConstants.StrafeSpeed;
            return new BVector3(velocity.X, 0, velocity.Z);
        }

        private float verticalDistance(StaticCollidable staticObject)
        {
            return verticalDistance(staticObject, BVector3.Zero);
        }

        /// <summary>
        /// Returns the distance from the ship to the ground.
        /// </summary>
        private float verticalDistance(StaticCollidable staticObject, BVector3 offset)
        {
            BEPUutilities.RayHit hit;
            BRay ray = new BRay(Entity.Position + offset, Entity.OrientationMatrix.Down);
            staticObject.RayCast(ray, 100.0f, out hit);
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

        private void dontCollide(BRay ray, float rayLength, float gamePadDirection, StaticCollidable staticObject)
        {
            BEPUutilities.RayHit hit;
            if (staticObject.RayCast(ray, rayLength, out hit))
            {
                BRay rayRight = new BRay(Entity.Position, Entity.OrientationMatrix.Forward - 0.3f * ray.Direction);
                BRay rayLeft = new BRay(Entity.Position, Entity.OrientationMatrix.Forward + 0.3f * ray.Direction);
                BEPUutilities.RayHit hitRight;
                BEPUutilities.RayHit hitLeft;

                Boolean rayCastHitRight = staticObject.RayCast(rayRight, ObjectConstants.RayLengthSide, out hitRight);
                Boolean rayCastHitLeft = staticObject.RayCast(rayLeft, ObjectConstants.RayLengthSide, out hitLeft);
                float angularspeed = ObjectConstants.AngularSpeed;
                float directionBump = ObjectConstants.DirectionBump;

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
                    velocityDirection = Entity.OrientationMatrix.Forward * directionBump;
                }
                else if (rayCastHitLeft)
                {
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
                Entity.LinearVelocity = velocityDirection * (Entity.LinearVelocity.Length() * ObjectConstants.SpeedDecrease);
            }
        }

        /// <summary>
        /// Returns true if its above the ideal heigth
        /// </summary>
        /// <param name="idealHeight">Ships hovering distance from the ground</param>
        private bool fly()
        {
            if (Collidables.Count < 1)
            {
                return false;
            }

            float h = verticalDistance(Collidables[0]);
            // Check a little in front and to the back to check rotation
            float front = 0f, back = 0f;

            front = verticalDistance(Collidables[0], Entity.OrientationMatrix.Forward * 0.5f);
            back = verticalDistance(Collidables[0], Entity.OrientationMatrix.Forward * -0.5f);


            for (int i = 1; i < Collidables.Count; i++ )
            {
                float val = verticalDistance(Collidables[i]);
                if (val <= 0)
                {
                    continue;
                }

                h = Math.Min(h, val);
                
                if (h == val)
                {
                    // Update front and back as well
                    front = verticalDistance(Collidables[i], Entity.OrientationMatrix.Forward * 0.5f);
                    back = verticalDistance(Collidables[i], Entity.OrientationMatrix.Forward * -0.5f);
                }
            }

            if (h < 1.2f * ObjectConstants.IdealHeight) 
            {
                float diff = front - back;
                // experimental values
                float rad = (float)Math.Atan(diff / 1.0f);
                rad = Math.Max(Math.Min(rad, 0.5235988f), -0.5235988f); // +-30deg

                // compares current angle to up angle, could compare current to ground angle as well
                float dot = BEPUutilities.Vector3.Dot(Entity.OrientationMatrix.Up, BEPUutilities.Vector3.Up);

                //Console.WriteLine((1 - dot));

                angularVelocity = Entity.OrientationMatrix.Right * (-rad / 0.5235988f) * ((1 / 0.15f) * Math.Max(0, (0.15f - (1 - dot))));
            }

            if ((ObjectConstants.IdealHeight - h) > 0) 
            {
                Entity.Position = new BVector3(Entity.Position.X, Entity.Position.Y + (ObjectConstants.IdealHeight - h), Entity.Position.Z);
                return false;
            }

            return true;
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
            float a = currentLength / ObjectConstants.MaxSpeed;
            if (a < 0.1f)
            {
                newVelocity = Entity.OrientationMatrix.Forward * ObjectConstants.MaxSpeed * 0.12f;
            }
            else if (a < ObjectConstants.FirstCase)
            {
                accelerationLength = ObjectConstants.MaxSpeed * ObjectConstants.StartAcceleration * dt;
                newVelocity = Entity.OrientationMatrix.Forward * (Entity.LinearVelocity.Length() + accelerationLength);
            }
            else if (a < ObjectConstants.SecondCase)
            {
                accelerationLength = ObjectConstants.MaxSpeed * ObjectConstants.MidAcceleration * dt;
                newVelocity = Entity.OrientationMatrix.Forward * (Entity.LinearVelocity.Length() + accelerationLength);
            }
            else if (a < 1.0f)
            {
                accelerationLength = ObjectConstants.MaxSpeed * ObjectConstants.EndAcceleration * dt;
                newVelocity = Entity.OrientationMatrix.Forward * (Entity.LinearVelocity.Length() + accelerationLength);
            }
            else
            {
                newVelocity = Entity.OrientationMatrix.Forward * ObjectConstants.MaxSpeed;
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
                rollValue = (Entity.LinearVelocity.Length() * 1.5f) / ObjectConstants.MaxSpeed * ObjectConstants.RollSpeed * dt + 0.005f;
                if (ifRollRight())
                {
                    rollValue = -rollValue;
                }
            }
            else if (angle < ObjectConstants.MaxRollAngle)
            {
                rollValue = (Entity.LinearVelocity.Length() * 1.5f) / ObjectConstants.MaxSpeed * ObjectConstants.RollSpeed * dt * gamepadX + gamepadX / 500;
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
            //Debug.WriteLine(Entity.Position);

            //Aircontroll
            if (fly())
            {
                downward = new BVector3(0, -ObjectConstants.FallingSpeed, 0);
                downward.Y -= gamePadState.ThumbSticks.Left.Y * ObjectConstants.ControllSpeed;
            }

            // Turning, strafing and rolling the ship
            if (gamePadState.ThumbSticks.Left.X != 0)
            {
                roll = rolling(gamePadState.ThumbSticks.Left.X, dt);
                angularVelocity += new BVector3(0, -gamePadState.ThumbSticks.Left.X * ObjectConstants.TurningSpeed, 0);
                Entity.AngularVelocity = angularVelocity;
                shipStrafe = strafe(gamePadState.ThumbSticks.Left.X > 0);
            }
            else // stop turning and stablize the roll 
            {
                roll = rolling(gamePadState.ThumbSticks.Left.X, dt);
                shipStrafe = BVector3.Zero;
                Entity.AngularVelocity = angularVelocity;
            }
            // Gets forwad Acceleration
            if (gamePadState.IsButtonDown(Buttons.A))
            {
                forward = newVelocity(dt, shipStrafe.Length());
            }
            // Gets forwad and downward decrease
            else
            {
                forward = (getPlanarSpeedVector() * ObjectConstants.Decceleration) - shipStrafe + new BVector3(0, Entity.LinearVelocity.Y * 0.5f, 0);
            }
            // Applies the roll
            BEPUutilities.Quaternion AddRot = BEPUutilities.Quaternion.CreateFromYawPitchRoll(0, 0, -roll);
            Entity.Orientation *= AddRot;

            // Applies all speed
            Entity.LinearVelocity = shipStrafe + forward + downward;



            // Checks for collitions
            foreach(StaticCollidable c in Collidables)
            {
                dontCollide(new BRay(Entity.Position, Entity.OrientationMatrix.Forward), ObjectConstants.ForwardCollideLength, gamePadState.ThumbSticks.Left.X, c);
                dontCollide(new BRay(Entity.Position, Entity.OrientationMatrix.Left), ObjectConstants.SideCollideLength, gamePadState.ThumbSticks.Left.X, c);
                dontCollide(new BRay(Entity.Position, Entity.OrientationMatrix.Right), ObjectConstants.SideCollideLength, gamePadState.ThumbSticks.Left.X, c);
            }

            angularVelocity = BVector3.Zero;
            base.Update(gameTime);
        }

        public Matrix GroundRotation { get; set; }
    }
}
