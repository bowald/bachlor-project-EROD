using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class LightRays : BasicPostProcess
    {

        private Vector3 LightPosition;
        private float Density;
        private float Decay;
        private float Weight;
        private float Exposure;

        public LightRays(Game game, Vector3 lightPosition, float density, float decay, float weight, float exposure) 
            : base(game)
        {
            this.LightPosition = lightPosition;
            this.Density = density;
            this.Decay = decay;
            this.Weight = weight;
            this.Exposure = exposure;

            UsesVertexShader = true;
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
            {
                effect = Game.Content.Load<Effect>("Shaders/PostProcessing/LightRays");
            }

            effect.CurrentTechnique = effect.Techniques["LightRays"];
            effect.Parameters["LightPosition"].SetValue(LightPosition);
            effect.Parameters["Density"].SetValue(Density);
            effect.Parameters["Decay"].SetValue(Decay);
            effect.Parameters["Weight"].SetValue(Weight);
            effect.Parameters["Exposure"].SetValue(Exposure);
            effect.Parameters["ViewProjection"].SetValue(Camera.View * Camera.Projection);
            effect.Parameters["HalfPixel"].SetValue(HalfPixel);

            base.Draw(gameTime);
        }
    }
}
