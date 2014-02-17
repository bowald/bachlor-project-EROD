using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    interface IObject
    {
        Matrix World { get; set; }
        Model Model { get; set; }
        Texture2D Texture { get; set; }
    }
}
