using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ERoD
{
    class LightHelper
    {
        public const string LightSources = "lightSources.txt";

        private static Vector3 lightPosition = Vector3.Zero;
        private static float lightRadius = 50.0f;
        private static float lightIntensity = 5.0f;

        private static float ChangeStrength = 1.0f;

        private static Color LightColor
        {
            get { return new Color(red, green, blue); }
        }

        public static PointLight Light
        {
            get { return new PointLight(lightPosition, LightColor, lightRadius, lightIntensity); }
        }

        private static float red = 1.0f;
        private static float green = 1.0f;
        private static float blue = 1.0f;
        private static void SetColor(ref float color, float diff)
        {
            color = Math.Max(0, Math.Min(1, color + diff));
        }

        public static void PlaceLightUpdate()
        {
            KeyboardState keyState = Keyboard.GetState();
            
            // ZX  ChangeStrength
            if (keyState.IsKeyDown(Keys.Z))
            {
                ChangeStrength = 0.1f;
            }
            if (keyState.IsKeyDown(Keys.X))
            {
                ChangeStrength = 1.0f;
            }
            
            // WS +-z
            // AD +-x
            // QE +-y
            // BN size+-
            #region Position & Size
            
            if (keyState.IsKeyDown(Keys.W))
            {
                //+z
                lightPosition.Z += ChangeStrength;
            }
            if (keyState.IsKeyDown(Keys.S))
            {
                //-z
                lightPosition.Z -= ChangeStrength;
            }
            if (keyState.IsKeyDown(Keys.A))
            {
                //-x
                lightPosition.X += ChangeStrength;
            }
            if (keyState.IsKeyDown(Keys.D))
            {
                //+x
                lightPosition.X -= ChangeStrength;
            }
            if (keyState.IsKeyDown(Keys.Q))
            {
                //+y
                lightPosition.Y += ChangeStrength;
            }
            if (keyState.IsKeyDown(Keys.E))
            {
                //-y
                lightPosition.Y -= ChangeStrength;
            }
            if (keyState.IsKeyDown(Keys.B))
            {
                lightRadius += ChangeStrength;
            }
            if (keyState.IsKeyDown(Keys.N))
            {
                lightRadius = Math.Max(0, lightRadius - ChangeStrength);
            }
            #endregion

            // FG +-red
            // HJ +-green
            // KL +-blue
            // CV +-Intensity
            #region Color & Intensity

            if (keyState.IsKeyDown(Keys.F))
            {
                SetColor(ref red, ChangeStrength * 0.3f);
            }
            if (keyState.IsKeyDown(Keys.G))
            {
                SetColor(ref red, -ChangeStrength * 0.3f);
            }
            if (keyState.IsKeyDown(Keys.H))
            {
                SetColor(ref green, ChangeStrength * 0.3f);
            }
            if (keyState.IsKeyDown(Keys.J))
            {
                SetColor(ref green, -ChangeStrength * 0.3f);
            }
            if (keyState.IsKeyDown(Keys.K))
            {
                SetColor(ref blue, ChangeStrength * 0.3f);
            }
            if (keyState.IsKeyDown(Keys.L))
            {
                SetColor(ref blue, -ChangeStrength * 0.3f);
            }
            if (keyState.IsKeyDown(Keys.C))
            {
                lightIntensity = lightIntensity + ChangeStrength * 0.05f;
            }
            if (keyState.IsKeyDown(Keys.V))
            {
                lightIntensity = Math.Max(0, lightIntensity - ChangeStrength * 0.05f);
            }

            #endregion


            // OP
            if (keyState.IsKeyDown(Keys.O))
            {
                Console.WriteLine("X Y Z | radius | R G B | intensity");
            }
            if (keyState.IsKeyDown(Keys.P))
            {
                Console.WriteLine(GetDesc());
            }
        }

        public static List<IPointLight> ReadLights()
        {
            var stream = TitleContainer.OpenStream(LightSources);
            var reader = new StreamReader(stream);
            string data = reader.ReadToEnd();

            char[] spaceSeparator =  new char[] { ' ' };
            char[] endLineSeparator = new char[] { '\n' };
            string[] lines = data.Split(endLineSeparator, StringSplitOptions.RemoveEmptyEntries);

            List<IPointLight> pointLights = new List<IPointLight>();
            foreach (string line in lines)
            {
                if (line.StartsWith("/")) // comment
                {
                    continue;
                }
                string[] values = line.Split(spaceSeparator, StringSplitOptions.None);
                float x = float.Parse(values[0]);
                float y = float.Parse(values[1]);
                float z = float.Parse(values[2]);

                float radius = float.Parse(values[3]);

                float r = float.Parse(values[4]);
                float g = float.Parse(values[5]);
                float b = float.Parse(values[6]);

                float intensity = float.Parse(values[7]);

                pointLights.Add(new PointLight(new Vector3(x, y, z), new Color(r, g, b), radius, intensity));
            }

            return pointLights;
        }

        private static string GetDesc()
        {
            return lightPosition.X + " " 
                + lightPosition.Y + " " 
                + lightPosition.Z + " " 
                + lightRadius + " " 
                + LightColor.R + " " 
                + LightColor.G + " " 
                + LightColor.B + " " 
                + lightIntensity;
        }
    }
}
