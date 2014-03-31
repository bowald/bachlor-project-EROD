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
        public float rad = .1f;
        public float intensity = 1;//2.5f;
        public float scale = .5f;//5;
        public float bias = 1f;
        private Texture2D randomTexture;

        public BasicSSAO(ERoD game, float radius, float intensity, float scale, float bias)
            : base(game)
        {
            rad = radius;
            this.intensity = intensity;
            this.scale = scale;
            this.bias = bias;

            UsesVertexShader = true;
            newSceneSurfaceFormat = SurfaceFormat.Vector4; //kolla
        }


        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
            {
                effect = Game.Content.Load<Effect>("Shaders/SSAO");
                
            }
            if (randomTexture == null)
            { 
                randomTexture = Game.Content.Load<Texture2D>("Textures/random");
            }

            effect.CurrentTechnique = effect.Techniques["SSAO"];
            float rad = .2f;
            float intensity = 1.0f;//2.5f;
            float scale = 1.5f;//5;
            float bias = 1f;

            effect.Parameters["HalfPixel"].SetValue(HalfPixel);
            effect.Parameters["NormalMap"].SetValue(NormalBuffer);
            effect.Parameters["DepthMap"].SetValue(DepthBuffer);
            effect.Parameters["ScreenSize"].SetValue(new Vector2(camera.Viewport.Width, camera.Viewport.Height));
            effect.Parameters["Random"].SetValue(randomTexture);
            effect.Parameters["Random_size"].SetValue(new Vector2(randomTexture.Width, randomTexture.Height));
            //effect.Parameters["ViewProjectionInv"].SetValue(Matrix.Invert(camera.View
            //    * camera.Projection));
            effect.Parameters["ViewInverse"].SetValue(Matrix.Invert(camera.View));
            effect.Parameters["Sample_rad"].SetValue(rad);
            effect.Parameters["Intensity"].SetValue(intensity);
            effect.Parameters["Scale"].SetValue(scale);
            effect.Parameters["Bias"].SetValue(bias);
            effect.Parameters["SidesLengthVS"].SetValue(new Vector2(camera.TanFovy * camera.AspectRatio, -camera.TanFovy));
            effect.Parameters["FarPlane"].SetValue(camera.FarPlane);

            //Game.GraphicsDevice.BlendState = BlendState.Opaque;
            base.Draw(gameTime);

        }
    }
}
