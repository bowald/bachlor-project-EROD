using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ERoD
{
    public class BasicSSAO : BasicPostProcess
    {
        public float Radius;
        public float Intensity;
        public float Scale;
        public float Bias;

        private Texture2D randomTexture;

        public BasicSSAO(ERoD game, float radius, float intensity, float scale, float bias)
            : base(game)
        {
            this.Radius = radius;
            this.Intensity = intensity;
            this.Scale = scale;
            this.Bias = bias;

            UsesVertexShader = true;
        }


        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
            {
                effect = Game.Content.Load<Effect>("Shaders/PostProcessing/SSAO");
                
            }
            if (randomTexture == null)
            { 
                randomTexture = Game.Content.Load<Texture2D>("Textures/random");
            }

            effect.CurrentTechnique = effect.Techniques["SSAO"];

            effect.Parameters["HalfPixel"].SetValue(HalfPixel);
            effect.Parameters["NormalMap"].SetValue(NormalBuffer);
            effect.Parameters["DepthMap"].SetValue(DepthBuffer);

            effect.Parameters["ScreenSize"].SetValue(new Vector2(Camera.Viewport.Width, Camera.Viewport.Height));
            effect.Parameters["Random"].SetValue(randomTexture);
            effect.Parameters["Random_size"].SetValue(new Vector2(randomTexture.Width, randomTexture.Height));
            //effect.Parameters["ViewProjectionInv"].SetValue(Matrix.Invert(camera.View
            //    * camera.Projection));

            effect.Parameters["ViewInverse"].SetValue(Matrix.Invert(Camera.View));
            effect.Parameters["Radius"].SetValue(Radius);
            effect.Parameters["Intensity"].SetValue(Intensity);
            effect.Parameters["Scale"].SetValue(Scale);
            effect.Parameters["Bias"].SetValue(Bias);

            effect.Parameters["SidesLengthVS"].SetValue(new Vector2(Camera.TanFovy * Camera.AspectRatio, -Camera.TanFovy));
            effect.Parameters["FarPlane"].SetValue(Camera.FarPlane);

            //Game.GraphicsDevice.BlendState = BlendState.Opaque;
            base.Draw(gameTime);

        }
    }
}
