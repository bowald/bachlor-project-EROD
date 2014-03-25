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

            effect.Parameters["halfPixel"].SetValue(HalfPixel);
            effect.Parameters["normalMap"].SetValue(Game.Renderer.normalMap);
            effect.Parameters["depthMap"].SetValue(Game.Renderer.depthMap);
            effect.Parameters["screenSize"].SetValue(new Vector2(camera.Viewport.Width, camera.Viewport.Height));
            effect.Parameters["random"].SetValue(randomTexture);
            effect.Parameters["random_size"].SetValue(new Vector2(randomTexture.Width, randomTexture.Height));
            effect.Parameters["viewProjectionInv"].SetValue(Matrix.Invert(camera.View
                * camera.Projection));
            effect.Parameters["g_sample_rad"].SetValue(rad);
            effect.Parameters["g_intensity"].SetValue(intensity);
            effect.Parameters["g_scale"].SetValue(scale);
            effect.Parameters["g_bias"].SetValue(bias);

            //Game.GraphicsDevice.BlendState = BlendState.Opaque;
            base.Draw(gameTime);

        }
    }
}
