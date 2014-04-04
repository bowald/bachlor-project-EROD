using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class SceneMask : BasicPostProcess
    {

        private Vector3 LightPosition;

        public SceneMask(ERoD game, Vector3 lightPosition)
            :base(game)
        {
            this.LightPosition = lightPosition;
            UsesVertexShader = true;
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
                effect = Game.Content.Load<Effect>("Shaders/PostProcessing/SceneMask");

            effect.CurrentTechnique = effect.Techniques["SceneMask"];
            effect.Parameters["DepthBuffer"].SetValue(DepthBuffer);
            effect.Parameters["HalfPixel"].SetValue(HalfPixel);
            effect.Parameters["ViewProjection"].SetValue(camera.View * camera.Projection);
            effect.Parameters["InverseViewProjection"].SetValue(Matrix.Invert(camera.View * camera.Projection));
            effect.Parameters["LightPosition"].SetValue(LightPosition);

            base.Draw(gameTime);
        }

    }
}
