using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public interface IObject
    {
        Matrix World { get; }
        Model Model { get; set; }
        Texture2D[] Textures { get; set; }
        Texture2D BumpMap { get; set; }
        Texture2D SpecularMap { get; set; }
        Texture2D[] GlowMap { get; set; }
        Boolean[] TextureEnabled { get; set; }
    }
}
