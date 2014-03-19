using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using System;
using ConversionHelper;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.Vehicle;
using BEPUutilities;

namespace ERoD
{
    public class ChaseCamera : BaseCamera
    {
        /// <summary>
        /// Entity to follow around and point at.
        /// </summary>
        public Entity ChasedEntity { get; set; }

        /// <summary>
        /// Gets or sets the offset vector from the center of the target chase entity to look at.
        /// </summary>
        public BEPUutilities.Vector3 OffsetFromChaseTarget { get; set; }

        /// <summary>
        /// Gets or sets whether or not to transform the offset vector with the rotation of the entity.
        /// </summary>
        private bool TransformOffset { get; set; }

        /// <summary>
        /// Gets or sets the distance away from the target entity to try to maintain.  The distance will be shorter at times if the ray hits an object.
        /// </summary>
        public float DistanceToTarget { get; set; }
        
        /// <summary>
        /// Gets or sets the margin of the camera. The camera will be placed no closer to any obstacle than this margin along the ray cast.
        /// </summary>
        public float ChaseCameraMargin { get; set; }

        /// <summary>
        /// Gets or sets the view direction of the camera.
        /// </summary>
        public BEPUutilities.Vector3 ViewDirection
        {
            get 
            {
                // Find horizontal speed.
                float x = ChasedEntity.LinearVelocity.X;
                float z = ChasedEntity.LinearVelocity.Z;
                float speed = Math.Min(ObjectConstants.MaxSpeed, (new BEPUutilities.Vector2(x, z)).Length());
                
                // Use horizontal speed to apply different amounts of angle to the viewdirection
                BEPUutilities.Vector3 result = ChasedEntity.OrientationMatrix.Forward;
                result += ((((ObjectConstants.MaxSpeed - speed) / ObjectConstants.MaxSpeed) * ObjectConstants.ChaseCameraSpeedAngle + ObjectConstants.ChaseCameraBaseAngle) * BEPUutilities.Vector3.Down);
                result.Normalize();
                return result;
            }
        }

        /// <summary>
        /// Gets or sets the current locked up vector of the camera.
        /// </summary>
        public BEPUutilities.Vector3 Up
        {
            //get { return ChasedEntity.OrientationMatrix.Up; }
            get { return BEPUutilities.Vector3.Up; }
        }
        //The raycast filter limits the results retrieved from the Space.RayCast while in chase camera mode.
        Func<BroadPhaseEntry, bool> rayCastFilter;
        bool RayCastFilter(BroadPhaseEntry entry)
        {
            return entry != ChasedEntity.CollisionInformation && (entry.CollisionRules.Personal <= CollisionRule.Normal);
        }

        /// <summary>
        /// Sets up all the information required by the chase camera.
        /// </summary>
        /// <param name="chasedEntity">Target to follow.</param>
        /// <param name="offsetFromChaseTarget">Offset from the center of the entity target to point at.</param>
        /// <param name="transformOffset">Whether or not to transform the offset with the target entity's rotation.</param>
        /// <param name="distanceToTarget">Distance from the target position to try to maintain.</param>
        /// <param name="nearPlane">NearPlane for the projection.</param>
        /// <param name="farPlane">FarPlane for the projection.</param>
        /// <param name="game">Running game.</param>
        public ChaseCamera(Entity chasedEntity, BEPUutilities.Vector3 offsetFromChaseTarget, bool transformOffset, float distanceToTarget, float nearPlane, float farPlane, ERoD game)
            : base(game, nearPlane, farPlane)
        {
            ChasedEntity = chasedEntity;
            OffsetFromChaseTarget = offsetFromChaseTarget;
            TransformOffset = transformOffset;
            DistanceToTarget = distanceToTarget;
            ChaseCameraMargin = 1;
            rayCastFilter = RayCastFilter;
        }

        public override void Update(GameTime gameTime)
        {
            BEPUutilities.Vector3 offset = TransformOffset ? Matrix3x3.Transform(OffsetFromChaseTarget, ChasedEntity.BufferedStates.InterpolatedStates.OrientationMatrix) : OffsetFromChaseTarget;
            BEPUutilities.Vector3 lookAt = ChasedEntity.BufferedStates.InterpolatedStates.Position + offset;
            BEPUutilities.Vector3 backwards = -ViewDirection;

            //Test against everything in the world
            //RayCastResult result;
            //float cameraDistance = ChasedEntity.Space.RayCast(new BEPUutilities.Ray(lookAt, backwards), DistanceToTarget, rayCastFilter, out result) ? result.HitData.T : DistanceToTarget;

            //Test only against the ground
            RayHit hit;
            BEPUutilities.Ray ray = new BEPUutilities.Ray(lookAt, backwards);
            float cameraDistance = 
                ((ITerrain)Game.Services.GetService(typeof(ITerrain))).PhysicTerrain
                .RayCast(ray, DistanceToTarget, out hit) ? hit.T : DistanceToTarget;

            BEPUutilities.Vector3 pos = lookAt + (Math.Max(cameraDistance - ChaseCameraMargin, 0)) * backwards;
            Position = ConversionHelper.MathConverter.Convert(pos); //Put the camera just before any hit spot.
            //Set the Up direction to be the same as the chased entity's
            View = ConversionHelper.MathConverter.Convert(BEPUutilities.Matrix.CreateViewRH(pos, ViewDirection, Up));

            base.Update(gameTime);
        }
    }
}
