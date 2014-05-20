using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    class GameConstants
    {

        public static string[] PlayerNames = new string[] { "Alex", "Anton", "Johan", "TheGovernator" };
        public static Color[] PlayerColors = new Color[] { Color.Orange, Color.Blue, Color.LightGreen, Color.Red };

        public const int WindowWidth = 1366;
        public const int WindowHeight = 768;
        //public const int WindowWidth = 1024;
        //public const int WindowHeight = 576;

        public const bool ShadowsEnabled = true;
        public const int ShadowMapSize = 2048;

        public const int NumberOfLaps = 1;
        public const int NumberOfCheckpoints = 8;

        public const float Gravity = -9.82f;

        public const float BoostMaxTime = 7.0f; //in secondes
        public const float BoostSpeed = 30.0f; //Amount of speed added during boost.
    }
}
