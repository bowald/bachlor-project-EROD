using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ERoD
{
    public partial class LightHelper
    {
        private static Vector3 lightPosition = Vector3.Zero;
        private static float lightRadius = 30.0f;
        private static float lightIntensity = 5.0f;

        private static float ChangeStrength = 1.0f;

        public static Game Game { get; set; }
        private static ICamera Camera
        {
            get { return (ICamera)Game.Services.GetService(typeof(ICamera)); }
        }

        private static float red = 1.0f;
        private static float green = 1.0f;
        private static float blue = 1.0f;
        private static Color LightColor
        {
            get { return new Color(red, green, blue); }
        }

        public static PointLight Light
        {
            get { return new PointLight(lightPosition, LightColor, lightRadius, lightIntensity); }
        }

        private static void SetColor(ref float color, float diff)
        {
            color = Math.Max(0, Math.Min(1, color + diff));
        }

        private static bool KeyPressed(Keys key, KeyboardState currentKeyState, KeyboardState lastKeyState)
        {
            return currentKeyState.IsKeyDown(key) && lastKeyState.IsKeyUp(key);
        }

        public static void PlaceLightUpdate(KeyboardState currentKeyState, KeyboardState lastKeyState)
        {
            
            // ZX  ChangeStrength
            if (KeyPressed(Keys.Z, currentKeyState, lastKeyState))
            {
                ChangeStrength = 0.1f;
            }
            if (KeyPressed(Keys.X, currentKeyState, lastKeyState))
            {
                ChangeStrength = 1.0f;
            }
            
            // WS +-z
            // AD +-x
            // QE +-y
            // BN size+-
            #region Position & Size

            if (currentKeyState.IsKeyDown(Keys.W))
            {
                //+z
                //lightPosition.Z += ChangeStrength;
                lightPosition += ChangeStrength * Camera.World.Forward;
            }
            if (currentKeyState.IsKeyDown(Keys.S))
            {
                //-z
                lightPosition -= ChangeStrength * Camera.World.Forward;
            }
            if (currentKeyState.IsKeyDown(Keys.A))
            {
                //-x
                lightPosition += ChangeStrength * Camera.World.Left;
            }
            if (currentKeyState.IsKeyDown(Keys.D))
            {
                //+x
                lightPosition -= ChangeStrength * Camera.World.Left;
            }
            if (currentKeyState.IsKeyDown(Keys.Q))
            {
                //+y
                lightPosition += ChangeStrength * Camera.World.Up;
            }
            if (currentKeyState.IsKeyDown(Keys.E))
            {
                //-y
                lightPosition -= ChangeStrength * Camera.World.Up;
            }
            if (currentKeyState.IsKeyDown(Keys.B))
            {
                lightRadius += ChangeStrength;
            }
            if (currentKeyState.IsKeyDown(Keys.N))
            {
                lightRadius = Math.Max(0, lightRadius - ChangeStrength);
            }
            #endregion

            // FG +-red
            // HJ +-green
            // KL +-blue
            // CV +-Intensity
            #region Color & Intensity

            if (currentKeyState.IsKeyDown(Keys.F))
            {
                SetColor(ref red, ChangeStrength * 0.3f);
            }
            if (currentKeyState.IsKeyDown(Keys.G))
            {
                SetColor(ref red, -ChangeStrength * 0.3f);
            }
            if (currentKeyState.IsKeyDown(Keys.H))
            {
                SetColor(ref green, ChangeStrength * 0.3f);
            }
            if (currentKeyState.IsKeyDown(Keys.J))
            {
                SetColor(ref green, -ChangeStrength * 0.3f);
            }
            if (currentKeyState.IsKeyDown(Keys.K))
            {
                SetColor(ref blue, ChangeStrength * 0.3f);
            }
            if (currentKeyState.IsKeyDown(Keys.L))
            {
                SetColor(ref blue, -ChangeStrength * 0.3f);
            }
            if (currentKeyState.IsKeyDown(Keys.C))
            {
                lightIntensity = lightIntensity + ChangeStrength * 0.05f;
            }
            if (currentKeyState.IsKeyDown(Keys.V))
            {
                lightIntensity = Math.Max(0, lightIntensity - ChangeStrength * 0.05f);
            }

            #endregion


            // OP
            if (KeyPressed(Keys.O, currentKeyState, lastKeyState))
            {
                Console.WriteLine("X Y Z | radius | R G B | intensity");
            }
            if (KeyPressed(Keys.P, currentKeyState, lastKeyState))
            {
                Console.WriteLine(Light);
            }
        }
    }
}
