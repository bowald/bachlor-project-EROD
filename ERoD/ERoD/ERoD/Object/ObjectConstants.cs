﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    abstract class ObjectConstants
    {
        //Ship
            // *** Velocity *** ///
            public const float MaxSpeed = 60.0f;
            public const float Decceleration = 0.98f;   // must be less then 1.0f
            public const float FirstCase = 0.7f;        // Acceleration case, obs 0.1 < FirstCase < SecondCase < 1.0f
            public const float SecondCase = 0.9f;   
            public const float StartAcceleration = 0.2f;  // Acceleration between 10% and FirstCase
            public const float MidAcceleration = 0.1f;    // Acceleration between FirstCase and SecondCase
            public const float EndAcceleration = 0.05f;   // Acceleration between SecondCase and MaxSpeed

            // *** Collision ***//
            public const float RayLengthSide = 4.0f;
            public const float AngularSpeed = 0.5f;     //Rotationspeed of the ship when hitting a wall    
            public const float DirectionBump = 0.2f;    //Change of direction
            public const float SpeedDecrease = 0.9f;    //Amount of velocity that will be kept when coliding
            
            // *** Fly *** //
            public const float IdealHeight = 4.0f;
            public const float FallingSpeed = 16.0f;
            public const float ControllSpeed = 5.0f;    //Amount of speed the player is able to change in Y-direction

            // *** Turning *** //
            public const float RollSpeed = 0.7f;
            public const float StrafeSpeed = 15.0f;
            public const float TurningSpeed = 1.3f;     //Angular speed when turning
            public const float MaxRollAngle = 45.0f; 
    }
}
