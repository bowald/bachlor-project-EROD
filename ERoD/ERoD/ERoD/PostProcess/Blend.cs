using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ERoD
{
    public class Blend : PostProcess
    {
        public Blend(ERoD game)
            : base(game)
        {
            UsesVertexShader = true;
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
                effect = Game.Content.Load<Effect>("Shaders/PostProcessing/Blend");

                effect.CurrentTechnique = effect.Techniques["Blend"];

            effect.Parameters["OrgScene"].SetValue(orgBuffer);
            // Set Params.
            base.Draw(gameTime);

        }
    }
}
