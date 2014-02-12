using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    class MultiLightingMaterial : Material
    {
        public Vector4 AmbientColor { get; set; }
        public Vector3[] LightDirection { get; set; }
        public Vector4[] LightColor { get; set; }
        public Vector3[] LightPosition { get; set; }
        public Vector4 SpecularColor { get; set; }

        public MultiLightingMaterial()
        {
            AmbientColor = new Vector4(.1f, .1f, .1f, 1.0f);
            LightDirection = new Vector3[3];
            LightPosition = new Vector3[3];
            LightColor = new Vector4[] { new Vector4(1, 0, 0, 1), new Vector4(0, 1, 0, 1), new Vector4(0, 0, 1, 1) };
            SpecularColor = new Vector4(1, 1, 1, 1);
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
            if (effect.Parameters["LightPosition"] != null)
            {
                effect.Parameters["LightPosition"].SetValue(LightPosition);
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
