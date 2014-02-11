using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class LightingMaterial : Material
    {
        public Vector3 AmbientColor { get; set; }
        public Vector3 LightDirection { get; set; }
        public Vector3 LightColor { get; set; }
        public Vector3 SpecularColor { get; set; }
        public LightingMaterial()
        {
            AmbientColor = new Vector3(.1f, .1f, .1f);
            LightDirection = new Vector3(1, 1, 1);
            LightColor = new Vector3(0.9f, 0.9f, 0.9f);
            SpecularColor = new Vector3(1, 1, 1);
        }

        public override void SetEffectParameters(Effect effect)
        {
            if (effect.Parameters["AmbientColor"] != null)
            {
                effect.Parameters["AmbientColor"].SetValue(AmbientColor);
            }
            if (effect.Parameters["LightDirection"] != null)
            {
                effect.Parameters["LightDirection"].SetValue(LightDirection);
            }
            if (effect.Parameters["LightColor"] != null)
            {
                effect.Parameters["LightColor"].SetValue(LightColor);
            }
            if (effect.Parameters["SpecularColor"] != null)
            {
                effect.Parameters["SpecularColor"].SetValue(SpecularColor);
            }
        }
    }
}
