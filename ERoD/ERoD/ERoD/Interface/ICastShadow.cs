using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public interface ICastShadow
    {
        void DrawShadow(GameTime gameTime, Matrix lightViewProjection);
    }
}
