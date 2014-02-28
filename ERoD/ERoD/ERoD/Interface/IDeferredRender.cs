using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public interface IDeferredRender
    {
        void Draw(GameTime gameTime);
        void Draw(GameTime gameTime, Effect effect);
    }
}
