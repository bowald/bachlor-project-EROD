using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ERoD
{
    public class SSAOBlend : BasicPostProcess
    {
        public SSAOBlend(ERoD game)
            : base(game)
        {
            //Sampler = SamplerState.PointClamp;
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
                effect = Game.Content.Load<Effect>("Shaders/PostProcessing/Blend");

            effect.CurrentTechnique = effect.Techniques["SSAOBlend"];

            effect.Parameters["OriginalScene"].SetValue(originalBuffer);

            base.Draw(gameTime);

        }
    }
}
