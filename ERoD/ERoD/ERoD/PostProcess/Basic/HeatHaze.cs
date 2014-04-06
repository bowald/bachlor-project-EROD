using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ERoD
{
    public class HeatHaze : BasicPostProcess
    {
        private double elapsedTime = 0;
        public bool High = true;

        public HeatHaze(Game game, bool high)
            : base(game)
        {
            High = high;
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
            {
                effect = Game.Content.Load<Effect>("Shaders/PostProcessing/HeatHaze");
            }

            if (High)
            {
                effect.CurrentTechnique = effect.Techniques["High"];
            }
            else
            {
                effect.CurrentTechnique = effect.Techniques["Low"];
            }

            elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;

            if (elapsedTime >= 10.0f)
                elapsedTime = 0.0f;

            effect.Parameters["Offset"].SetValue((float)elapsedTime * .1f);
            effect.Parameters["Bumpmap"].SetValue(Game.Content.Load<Texture2D>("Textures/Particles/bumpmap"));

            effect.Parameters["halfPixel"].SetValue(HalfPixel);

            // Set Params.
            base.Draw(gameTime);
        }
    }
}
