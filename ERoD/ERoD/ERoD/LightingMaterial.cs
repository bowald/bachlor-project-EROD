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
        public Vector4 AmbientColor { get; set; }
        public Vector3 LightDirection { get; set; }
        public Vector4 LightColor { get; set; }
        public Vector4 SpecularColor { get; set; }
        public LightingMaterial()
        {
            AmbientColor = new Vector4(.1f, .1f, .1f, 1.0f);
            LightDirection = new Vector3(1, 1, 1);
            LightColor = new Vector4(0.9f, 0.9f, 0.9f, 1.0f);
            SpecularColor = new Vector4(1, 1, 1, 1);
        }

        public override void SetEffectParameters(Effect effect)
        {
            if (effect.Parameters["AmbientColor"] != null)
            {
                effect.Parameters["AmbientColor"].SetValue(AmbientColor);
            }
            if (effect.Parameters["DiffuseLightDirection"] != null)
            {
                effect.Parameters["DiffuseLightDirection"].SetValue(LightDirection);
            }
            if (effect.Parameters["DiffuseColor"] != null)
            {
                effect.Parameters["DiffuseColor"].SetValue(LightColor);
            }
            if (effect.Parameters["SpecularColor"] != null)
            {
                effect.Parameters["SpecularColor"].SetValue(SpecularColor);
            }
        }
    }
}
