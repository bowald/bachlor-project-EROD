using BEPUphysics;
using BEPUphysics.Entities;
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
                turn = entity.OrientationMatrix.Right;
            }
            else
            {
                turn = entity.OrientationMatrix.Left;
            }
            velocity = turn * 15.0f;
            return new BVector3(velocity.X, 0, velocity.Z);
        }
        /// <summary>
        /// Returns the distance from the ship to the ground.
        /// </summary>
        private float distancefromG()
        {
            BEPUutilities.RayHit hit;
            BRay ray = new BRay(entity.Position,BVector3.Down);
            erod.testVarGround.RayCast(ray, 100.0f, out hit);
            return hit.T;
        }
        /// <summary>
        /// Given an entity.OriantationMatrix-vector, returns wich direction it facing
        /// Helper for debugging rayCasting,
        /// </summary>
        private String helper(BVector3 vec3){
            if (vec3 == entity.OrientationMatrix.Forward)
                return "forward";
            if (vec3 == entity.OrientationMatrix.Right)
                return "Right";
            if (vec3 == entity.OrientationMatrix.Left)
                return "Left";
            if (vec3 == entity.OrientationMatrix.Up)
                return "Up";
            if (vec3 == entity.OrientationMatrix.Down)
                return "Down";
            return "no match";
        }
        private void dontCollide(BRay ray, float rayLength, float gamePadDirection)
        {
            BEPUutilities.RayHit hit;
            if (erod.testVarGround.RayCast(ray, rayLength, out hit))
            {
                Debug.WriteLine(helper(ray.Direction));
                BRay rayRight = new BRay(entity.Position, entity.OrientationMatrix.Forward - 0.3f * ray.Direction);
                BRay rayLeft = new BRay(entity.Position, entity.OrientationMatrix.Forward + 0.3f * ray.Direction);
                BEPUutilities.RayHit hitRight;
                BEPUutilities.RayHit hitLeft;
                Boolean rayCastHitRight = erod.testVarGround.RayCast(rayRight, 4.0f, out hitRight);
                Boolean rayCastHitLeft = erod.testVarGround.RayCast(rayLeft, 4.0f, out hitLeft);
                float angularspeed = 0.5f;
                float directionBump = 0.2f;
                BVector3 velocityDirection = -ray.Direction;
                if (rayCastHitRight && rayCastHitLeft)
                {
                    Debug.WriteLine("Both Left & Right");
                    if (hitRight.T > hitLeft.T)
                    {
                        //yaw = -yaw;
                        angularspeed = -angularspeed;
                        directionBump = -directionBump;
                        Debug.WriteLine("Turning Left");
                    }
                    velocityDirection = entity.OrientationMatrix.Forward * directionBump;
                }
                else if (rayCastHitRight)
                {
                    Debug.WriteLine("only right");
                    velocityDirection = entity.OrientationMatrix.Forward * directionBump;
                }
                else if (rayCastHitLeft)
                {
                    Debug.WriteLine("only Left");
                    angularspeed = -angularspeed;
                    directionBump = -directionBump;
                    velocityDirection = entity.OrientationMatrix.Forward * directionBump;
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
                entity.AngularVelocity = new BVector3(0, angularspeed * gamePadDirection, 0);
                entity.LinearVelocity = velocityDirection * (entity.LinearVelocity.Length() * 0.9f);
            }
        }
        /// <summary>
        /// Returns true if its above the ideal heigth
        /// </summary>
        /// <param name="idealHeight">Ships hovering distance from the ground</param>
        private bool fly(float idealHeight){
            float h = distancefromG();
            if ((idealHeight - h) > 0) {
                entity.Position = new BVector3(entity.Position.X, entity.Position.Y + (idealHeight - h), entity.Position.Z);
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
            BVector3 Right = entity.Position + entity.OrientationMatrix.Right;
            BVector3 Left = entity.Position + entity.OrientationMatrix.Left;
            return Right.Y < Left.Y;
        }
        /// <summary>
        /// Returns the ships new velocity.
        /// Used to make the ship accelerate.
        /// </summary>
        private BVector3 newVelocity(float dt, float strafeSpeed)
        {
            BVector3 currentSpeed = new BVector3(entity.LinearVelocity.X,0, entity.LinearVelocity.Z);
            float currentLength = currentSpeed.Length() - strafeSpeed;
            BVector3 newVelocity = BVector3.Zero;
            float accelerationLength;
            float a = currentLength / maxSpeed;
            if (a < 0.1f)
            {
                newVelocity = entity.OrientationMatrix.Forward * maxSpeed * 0.12f;
            }
            else if (a < 0.7f)
            {
                accelerationLength = maxSpeed * 0.2f * dt;
                newVelocity = entity.OrientationMatrix.Forward * (entity.LinearVelocity.Length() + accelerationLength);
            }
            else if (a < 0.9f)
            {
                accelerationLength = maxSpeed * 0.1f * dt;
                newVelocity = entity.OrientationMatrix.Forward * (entity.LinearVelocity.Length() + accelerationLength);
            }
            else if (a < 1.0f)
            {
                accelerationLength = maxSpeed * 0.05f * dt;
                newVelocity = entity.OrientationMatrix.Forward * (entity.LinearVelocity.Length() + accelerationLength);
            }
            else
            {
                newVelocity = entity.OrientationMatrix.Forward * maxSpeed;
            }
            return newVelocity;
        }
        /// <summary>
        /// Gets the speed on the ground plane
        /// </summary>
        private BVector3 getPlanarSpeedVector()
        {
            return new BVector3(entity.LinearVelocity.X, 0, entity.LinearVelocity.Z);
        }
        /// <summary>
        /// Returns the roll-factor for the ship, takes ships speed, current angle and gamepad in account
        /// </summary>
        private float rolling(float gamepadX, float dt)
        {
            float rollValue = 0;
            float angle = (float)(Math.Acos(BVector3.Dot(entity.OrientationMatrix.Up, BVector3.Up)) * 180.0 / Math.PI);
            if (angle > 1.0f && gamepadX == 0)
            {
                rollValue = (entity.LinearVelocity.Length() * 1.5f) / maxSpeed * rollSpeed * dt + 0.005f;
                if (ifRollRight())
                {
                    rollValue = -rollValue;
                }
            }
            else if (angle < 45.0f)
            {
                rollValue = (entity.LinearVelocity.Length() * 1.5f) / maxSpeed * rollSpeed * dt * gamepadX + gamepadX / 500;
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
                entity.AngularVelocity = new BVector3(0,-gamePadState.ThumbSticks.Left.X * 1.3f,0);
                shipStrafe = strafe(gamePadState.ThumbSticks.Left.X > 0);
            }
            // stop turning and stablize the roll
            else {
                roll = rolling(gamePadState.ThumbSticks.Left.X, dt);
                shipStrafe = BVector3.Zero;
                entity.AngularVelocity = BVector3.Zero;
            }
            // Gets forwad Acceleration
            if (gamePadState.IsButtonDown(Buttons.A))
            {
                forward = newVelocity(dt, shipStrafe.Length());
            }
            // Gets forwad and downward decrease
            else
            {
                forward = (getPlanarSpeedVector() * 0.98f) - shipStrafe + new BVector3 (0, entity.LinearVelocity.Y * 0.5f, 0);
            }
            // Applies the roll
            BEPUutilities.Quaternion AddRot = BEPUutilities.Quaternion.CreateFromYawPitchRoll(0, 0, -roll);
            entity.Orientation *= AddRot;

            // Applies all speed
            entity.LinearVelocity = shipStrafe + forward + downward;
            
            // Checks for collitions
            dontCollide(new BRay(entity.Position, entity.OrientationMatrix.Forward), 4.0f, gamePadState.ThumbSticks.Left.X);
            dontCollide(new BRay(entity.Position, entity.OrientationMatrix.Left), 2.0f, gamePadState.ThumbSticks.Left.X);
            dontCollide(new BRay(entity.Position, entity.OrientationMatrix.Right), 2.0f, gamePadState.ThumbSticks.Left.X);
            base.Update(gameTime);
        }
    }
}
