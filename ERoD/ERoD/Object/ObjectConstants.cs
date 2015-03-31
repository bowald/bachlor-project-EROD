using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    abstract class ObjectConstants
    {
        //World
        public const float WorldSize = 3000.0f;  // Size of the world in world size units

        //Ship
        // *** Velocity *** ///
        public const float MaxSpeed = 130.0f;
        public const float Decceleration = 0.98f;   // must be less then 1.0f
        public const float FirstCase = 0.7f;        // Acceleration case, obs 0.1 < FirstCase < SecondCase < 1.0f
        public const float SecondCase = 0.9f;   
        public const float StartAcceleration = 0.20f;  // Acceleration between 10% and FirstCase
        public const float MidAcceleration =   0.15f;    // Acceleration between FirstCase and SecondCase
        public const float EndAcceleration =   0.10f;   // Acceleration between SecondCase and MaxSpeed

        // *** Collision ***//
        public const float RayLengthSide = 1.0f;
        public const float AngularSpeed = 0.5f;     //Rotationspeed of the ship when hitting a wall    
        public const float DirectionBump = 0.2f;    //Change of direction
        public const float SpeedDecrease = 0.9f;    //Amount of velocity that will be kept when coliding
        public const float SideCollideLength = 1.8f;
        public const float ForwardCollideLength = 2.0f;
        public const float OrientationRayLength = 2.5f;

        // *** Fly *** //
        public const float IdealHeight = 2.5f;
        public const float FallingSpeed = 20.0f;
        public const float ControllSpeed = 2.0f;    //Amount of speed the player is able to change in Y-direction

        // *** Turning *** //
        public const float RollSpeed = 0.3f;
        public const float StrafeSpeed = 4.0f;
        public const float TurningSpeed = 1.8f;     //Angular speed when turning
        public const float MaxRollAngle = 20.0f;

        // *** Chase Camera *** //
        public static float ChaseCameraSpeedAngle = 0.12f;
        public static float ChaseCameraBaseAngle = 0.18f;
            
    }
}
