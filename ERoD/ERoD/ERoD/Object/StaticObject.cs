using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    class StaticObject : BaseObject
    {
        public StaticObject(Model model, Matrix transform, Game game) 
            : base(model, transform, game)
        {
            World = Transform;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
