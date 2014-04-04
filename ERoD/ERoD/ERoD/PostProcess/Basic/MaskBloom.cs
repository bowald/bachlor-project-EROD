using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class BloomMask : BasicPostProcess
    {
        public float Threshold;

        public BloomMask(ERoD game, float Threshold) 
            : base(game)
        {
            this.Threshold = Threshold;
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
            {
                effect = Game.Content.Load<Effect>("Shaders/PostProcessing/BloomMask");
            }

            effect.CurrentTechnique = effect.Techniques["BloomMask"];
            effect.Parameters["Threshold"].SetValue(Threshold);

            base.Draw(gameTime);
        }
    }
}
