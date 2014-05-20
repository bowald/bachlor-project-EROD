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
    public partial class Ship : EntityObject
    {
        //Variables
        List<StaticCollidable> Collidables = new List<StaticCollidable>();
        BVector3 angularVelocity;
        ShipState State;
        float airTime = 0;
        float bestAirTime = 0;
        public float boostTimer = 0;
        float currentVelocity = 0;
        public Boolean AllowedToBoost = true;
        public Boolean Boosting = false;
        BVector3 shipVelocity = BVector3.Zero;

        enum ShipState
        {
            Normal,
            Destroyed
        };


        #region Controls and Physics

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
            BRay ray = new BRay(Entity.Position + offset, BVector3.Down);
            staticObject.RayCast(ray, 100.0f, out hit);
            return hit.T;
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

            front = verticalDistance(Collidables[0], Entity.OrientationMatrix.Forward * ObjectConstants.OrientationRayLength);
            back = verticalDistance(Collidables[0], Entity.OrientationMatrix.Forward * -ObjectConstants.OrientationRayLength);


            for (int i = 1; i < Collidables.Count; i++)
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
                    front = verticalDistance(Collidables[i], Entity.OrientationMatrix.Forward * ObjectConstants.OrientationRayLength);
                    back = verticalDistance(Collidables[i], Entity.OrientationMatrix.Forward * -ObjectConstants.OrientationRayLength);
                }
            }

            if (h < 1.2f * ObjectConstants.IdealHeight)
            {
                float diff = front - back;
                // experimental values
                float rad = (float)Math.Atan(diff / (2 * ObjectConstants.OrientationRayLength));
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
        private BVector3 newVelocity(float dt)
        {
            BVector3 newVelocity = BVector3.Zero;
            float accelerationLength;
            float a = shipVelocity.Length() / ObjectConstants.MaxSpeed;
            if (a < 0.1f)
            {
                newVelocity = Entity.OrientationMatrix.Forward * ObjectConstants.MaxSpeed * 0.12f;
            }
            else if (a < ObjectConstants.FirstCase)
            {
                accelerationLength = ObjectConstants.MaxSpeed * ObjectConstants.StartAcceleration * dt;
                newVelocity = Entity.OrientationMatrix.Forward * (shipVelocity.Length() + accelerationLength);
            }
            else if (a < ObjectConstants.SecondCase)
            {
                accelerationLength = ObjectConstants.MaxSpeed * ObjectConstants.MidAcceleration * dt;
                newVelocity = Entity.OrientationMatrix.Forward * (shipVelocity.Length() + accelerationLength);
            }
            else if (a < 1.0f)
            {
                accelerationLength = ObjectConstants.MaxSpeed * ObjectConstants.EndAcceleration * dt;
                newVelocity = Entity.OrientationMatrix.Forward * (shipVelocity.Length() + accelerationLength);
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
            float angle = getYAngle();
            if (angle > 4.0f && gamepadX == 0)
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

        public float getYAngle(){
            return (float)(Math.Acos(BVector3.Dot(Entity.OrientationMatrix.Up, BVector3.Up)) * 180.0 / Math.PI);
        }

        #endregion

        public void makeNormal(){
            if (getYAngle() > 100)
            {
                Entity.Position = new BVector3(Entity.Position.X, Entity.Position.Y + 5f, Entity.Position.Z);
                BEPUutilities.Matrix3x3 Orientation = new BEPUutilities.Matrix3x3(Entity.OrientationMatrix.M11, Entity.OrientationMatrix.M12, Entity.OrientationMatrix.M13, 
                                                                                  0f,1f,0f,
                                                                                  Entity.OrientationMatrix.M31, Entity.OrientationMatrix.M32, Entity.OrientationMatrix.M33);
                Entity.OrientationMatrix = Orientation;
            }
            Entity.BecomeKinematic();
            State = ShipState.Normal;
        }

        public void makeDestroyed()
        {
            Entity.BecomeDynamic(100);
            State = ShipState.Destroyed;
        }
        public void NormalUpdate(GameTime gameTime, GamePadState gamePadState)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            BVector3 forward = BVector3.Zero;
            BVector3 shipStrafe = BVector3.Zero;
            BVector3 downward = BVector3.Zero;
            BVector3 boostSpeed = BVector3.Zero;
            Single roll = 0;

            //Aircontroll
            if (fly())
            {
                //Velocity downwards based on airtime.
                airTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                float downwardSpeed = Math.Max(ObjectConstants.FallingSpeed * airTime, ObjectConstants.FallingSpeed);
                downward = BVector3.Down * downwardSpeed;

                //Yaw ship each 0.1 second of airtime
                int round =(int) Math.Round(airTime, 1) * 10;
                if (round % 2 == 0 && airTime > 0.3f)
                {
                    BEPUutilities.Quaternion yaw = BEPUutilities.Quaternion.CreateFromYawPitchRoll(-0.002f, 0, 0);
                    Entity.Orientation *= yaw;
                }
            }
            else
            {
                if (airTime > bestAirTime)
                {
                    bestAirTime = airTime;
                }
                airTime = 0;
            }
            //Debug.WriteLine("Airtime: " + airTime);
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
                shipVelocity = newVelocity(dt);
            }
            // Gets forwad decrease
            else
            {
                shipVelocity *= ObjectConstants.Decceleration;
            }
            //Boost
            if (gamePadState.IsButtonDown(Buttons.RightShoulder) && AllowedToBoost)
            {
                if (!(currentVelocity > ObjectConstants.MaxSpeed + GameConstants.BoostSpeed))
                {
                    boostSpeed = Entity.OrientationMatrix.Forward * GameConstants.BoostSpeed;
                }
                boostTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                Boosting = true;
            }
            else
            {
                if (currentVelocity > ObjectConstants.MaxSpeed)
                 {
                     boostSpeed = Entity.OrientationMatrix.Forward * -GameConstants.BoostSpeed;
                 }
                Boosting = false;
            }
            Debug.WriteLine(boostTimer);
            // Applies the roll
            BEPUutilities.Quaternion AddRot = BEPUutilities.Quaternion.CreateFromYawPitchRoll(0, 0, -roll);
            Entity.Orientation *= AddRot;
            // Applies all speed
            Entity.LinearVelocity = shipStrafe + shipVelocity + downward + boostSpeed;
            currentVelocity = shipVelocity.Length() + boostSpeed.Length();
            angularVelocity = BVector3.Zero;
            // Checks for collitions
            foreach (StaticCollidable c in Collidables)
            {
                dontCollide(new BRay(Entity.Position, Entity.OrientationMatrix.Forward), ObjectConstants.ForwardCollideLength, gamePadState.ThumbSticks.Left.X, c);
                dontCollide(new BRay(Entity.Position, Entity.OrientationMatrix.Left), ObjectConstants.SideCollideLength, gamePadState.ThumbSticks.Left.X, c);
                dontCollide(new BRay(Entity.Position, Entity.OrientationMatrix.Right), ObjectConstants.SideCollideLength, gamePadState.ThumbSticks.Left.X, c);
            }
        }


        public void DestroyedUpdate(GameTime gameTime, GamePadState gamePadState)
        {
        }
    }
}
