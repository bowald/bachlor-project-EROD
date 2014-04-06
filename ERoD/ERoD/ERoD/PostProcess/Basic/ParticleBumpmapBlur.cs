using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    class ParticleBumpmapBlur : BumpmapBlur
    {
        public ParticleBumpmapBlur(Game game, bool high)
            : base(game, high)
        {
        }

        public override void Draw(GameTime gameTime)
        {
            Game.GraphicsDevice.Textures[0] = ParticleBuffer;

            base.Draw(gameTime);
        }
    }
}
