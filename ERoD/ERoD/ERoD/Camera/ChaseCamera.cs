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

        private BEPUutilities.Vector3 viewDirection = BEPUutilities.Vector3.Forward;
        /// <summary>
        /// Gets or sets the view direction of the camera.
        /// </summary>
        public BEPUutilities.Vector3 ViewDirection
        {
            get { return viewDirection; }
            set
            {
                float lengthSquared = value.LengthSquared();
                if (lengthSquared > Toolbox.Epsilon)
                {
                    BEPUutilities.Vector3.Divide(ref value, (float) Math.Sqrt(lengthSquared), out value);
                    //Validate the input. A temporary violation of the maximum pitch is permitted as it will be fixed as the user looks around.
                    //However, we cannot allow a view direction parallel to the locked up direction.
                    float dot;
                    BEPUutilities.Vector3.Dot(ref value, ref lockedUp, out dot);
                    if (Math.Abs(dot) > 1 - Toolbox.BigEpsilon)
                    {
                        //The view direction must not be aligned with the locked up direction.
                        //Silently fail without changing the view direction.
                        return;
                    }
                    viewDirection = value;
                }
            }
        }

        private BEPUutilities.Vector3 lockedUp = BEPUutilities.Vector3.Up;
        /// <summary>
        /// Gets or sets the current locked up vector of the camera.
        /// </summary>
        public BEPUutilities.Vector3 LockedUp
        {
            get { return lockedUp; }
            set
            {
                var oldUp = lockedUp;
                float lengthSquared = value.LengthSquared();
                if (lengthSquared > Toolbox.Epsilon)
                {
                    BEPUutilities.Vector3.Divide(ref value, (float)Math.Sqrt(lengthSquared), out lockedUp);
                    //Move the view direction with the transform. This helps guarantee that the view direction won't end up aligned with the up vector.
                    BEPUutilities.Quaternion rotation;
                    BEPUutilities.Quaternion.GetQuaternionBetweenNormalizedVectors(ref oldUp, ref lockedUp, out rotation);
                    BEPUutilities.Quaternion.Transform(ref viewDirection, ref rotation, out viewDirection);
                }
                //If the new up vector was a near-zero vector, silently fail without changing the up vector.
            }
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

            //Find the earliest ray hit that isn't the chase target to position the camera appropriately.
            RayCastResult result;
            float cameraDistance = ChasedEntity.Space.RayCast(new BEPUutilities.Ray(lookAt, backwards), DistanceToTarget, rayCastFilter, out result) ? result.HitData.T : DistanceToTarget;

            BEPUutilities.Vector3 pos = lookAt + (Math.Max(cameraDistance - ChaseCameraMargin, 0)) * backwards;
            Position = ConversionHelper.MathConverter.Convert(pos); //Put the camera just before any hit spot.
            //Set the Up direction to be the same as the chased entity's
            LockedUp = ChasedEntity.OrientationMatrix.Up;
            View = ConversionHelper.MathConverter.Convert(BEPUutilities.Matrix.CreateViewRH(pos, viewDirection, lockedUp));

            base.Update(gameTime);
        }
    }
}
