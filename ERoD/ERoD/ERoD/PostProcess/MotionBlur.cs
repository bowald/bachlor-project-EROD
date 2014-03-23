using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    class MotionBlur : PostProcess
    {
        Matrix lastVP;

        public MotionBlur(ERoD game)
            : base(game)
        {
            UsesVertexShader = true;
            newSceneSurfaceFormat = SurfaceFormat.Vector4;
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
            {
                effect = Game.Content.Load<Effect>("Shaders/PostProcessing/MotionBlur");
                effect.CurrentTechnique = effect.Techniques["MotionBlur"];
            }

            effect.Parameters["mask"].SetValue(Game.Renderer.colorMap);
            effect.Parameters["depthMap"].SetValue(Game.Renderer.depthMap);
            effect.Parameters["g_ViewProjectionInverseMatrix"].SetValue(Matrix.Invert(camera.View * camera.Projection));
            effect.Parameters["g_previousViewProjectionMatrix"].SetValue(lastVP);
            effect.Parameters["halfPixel"].SetValue(HalfPixel);

            lastVP = camera.View * camera.Projection;

            Game.GraphicsDevice.BlendState = BlendState.Opaque;
            // Set Params.
            base.Draw(gameTime);
        }
    }
}
