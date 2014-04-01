using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    class SceneMask : BasicPostProcess
    {

        public SceneMask(ERoD game)
            : base(game)
        {
            UsesVertexShader = true;
        }


        public override void Draw(GameTime gameTime)
        {

            if (effect == null)
                effect = Game.Content.Load<Effect>("Shaders/PostProcessing/SceneMask");

            effect.CurrentTechnique = effect.Techniques["ConvertDepth"];
            effect.Parameters["DepthBuffer"].SetValue(DepthBuffer);
            effect.Parameters["HalfPixel"].SetValue(HalfPixel);

            effect.Parameters["cameraPosition"].SetValue(camera.Position);
            effect.Parameters["matVP"].SetValue(camera.View * camera.Projection);

            // Set Params.
            base.Draw(gameTime);
        }
    }
}
