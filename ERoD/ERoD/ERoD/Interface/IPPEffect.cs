using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public interface IPPEffect
    {
        Vector2 HalfPixel { get; set; }
        bool Enabled { get; set; }
        Texture2D OrgScene { get; set; }
        Texture2D NewScene { get; set; }

        void Update(GameTime gameTime);
        void Draw(GameTime gameTime, Texture2D scene);
    }
}
