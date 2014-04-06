using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ERoD
{
    public class Blend : BasicPostProcess
    {
        private float OriginalSaturation;
        private float LastSceneSaturation;
        private float OriginalIntensity;
        private float LastSceneIntensity;

        public Blend(Game game, float OriginalSaturation, float LastSceneSaturation, float OriginalIntensity, float LastSceneIntensity)
            : base(game)
        {
            this.OriginalSaturation = OriginalSaturation;
            this.LastSceneSaturation = LastSceneSaturation;
            this.OriginalIntensity = OriginalIntensity;
            this.LastSceneIntensity = LastSceneIntensity;
            Sampler = SamplerState.PointClamp;
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
                effect = Game.Content.Load<Effect>("Shaders/PostProcessing/Blend");

            effect.CurrentTechnique = effect.Techniques["Blend"];

            effect.Parameters["OriginalSaturation"].SetValue(OriginalSaturation);
            effect.Parameters["LastSceneSaturation"].SetValue(LastSceneSaturation);
            effect.Parameters["OriginalIntensity"].SetValue(OriginalIntensity);
            effect.Parameters["LastSceneIntensity"].SetValue(LastSceneIntensity);
            effect.Parameters["OriginalScene"].SetValue(originalBuffer);

            base.Draw(gameTime);

        }
    }
}
