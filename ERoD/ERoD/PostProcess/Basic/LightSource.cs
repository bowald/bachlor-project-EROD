using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class LightSource : BasicPostProcess
    {

        private float SunSize;
        private Vector3 LightPosition;

        public LightSource(Game game, Vector3 lightPosition, float sunSize)
            : base(game)
        {
            this.SunSize = sunSize;
            this.LightPosition = lightPosition;

            UsesVertexShader = true;
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
                effect = Game.Content.Load<Effect>("Shaders/PostProcessing/LightSource");

            effect.CurrentTechnique = effect.Techniques["LightSource"];

            effect.Parameters["LightPosition"].SetValue(LightPosition);
            effect.Parameters["HalfPixel"].SetValue(HalfPixel);
            effect.Parameters["ViewProjection"].SetValue(Camera.View * Camera.Projection);
            effect.Parameters["SunSize"].SetValue(SunSize);
            effect.Parameters["SunSource"].SetValue(Game.Content.Load<Texture2D>("Textures/sunFlare"));

            base.Draw(gameTime);
        }


    }
}
