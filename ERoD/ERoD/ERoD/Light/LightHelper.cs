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
        public const string LightSources = "lightSources.txt";

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
    }
}
