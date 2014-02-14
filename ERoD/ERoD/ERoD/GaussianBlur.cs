using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class GaussianBlur : PostProcessor
    {
        float blurAmount;
        float[] weightsH, weightsV;
        Vector2[] offsetsH, offsetsV;

        RenderCapture capture;

        public GaussianBlur(GraphicsDevice graphicsDevice, ContentManager Content, float BlurAmount)
            : base(Content.Load<Effect>("Shaders/GBPostProcessor"), graphicsDevice)
        {
            this.blurAmount = BlurAmount;
            // Calculate weights/offsets for horizontal pass
            calcSettings(1.0f / (float)graphicsDevice.Viewport.Width, 0,
            out weightsH, out offsetsH);
            // Calculate weights/offsets for vertical pass
            calcSettings(0, 1.0f / (float)graphicsDevice.Viewport.Height,
            out weightsV, out offsetsV);
            capture = new RenderCapture(graphicsDevice);
        }

        void calcSettings(float w, float h, out float[] weights, out Vector2[] offsets)
        {
            // 15 Samples
            weights = new float[15];
            offsets = new Vector2[15];
            // Calculate values for center pixel
            weights[0] = gaussianFn(0);
            offsets[0] = new Vector2(0, 0);
            float total = weights[0];
            // Calculate samples in pairs
            for (int i = 0; i < 7; i++)
            {
                // Weight each pair of samples according to Gaussian function
                float weight = gaussianFn(i + 1);
                weights[i * 2 + 1] = weight;
                weights[i * 2 + 2] = weight;
                total += weight * 2;
                // Samples are offset by 1.5 pixels, to make use of
                // filtering halfway between pixels
                float offset = i * 2 + 1.5f;
                Vector2 offsetVec = new Vector2(w, h) * offset;
                offsets[i * 2 + 1] = offsetVec;
                offsets[i * 2 + 2] = -offsetVec;
            }
            // Divide all weights by total so they will add up to 1
            for (int i = 0; i < weights.Length; i++)
                weights[i] /= total;
        }

        float gaussianFn(float x)
        {
            return (float)((1.0f / Math.Sqrt(2 * Math.PI * blurAmount * blurAmount)) * Math.Exp(-(x * x) / (2 * blurAmount * blurAmount)));
        }

        public override void Draw()
        {
            // Set values for horizontal pass
            Effect.Parameters["Offsets"].SetValue(offsetsH);
            Effect.Parameters["Weights"].SetValue(weightsH);
            // Render this pass into the RenderCapture
            capture.Begin();
            base.Draw();
            capture.End();
            // Get the results of the first pass
            Input = capture.GetTexture();
            // Set values for the vertical pass
            Effect.Parameters["Offsets"].SetValue(offsetsV);
            Effect.Parameters["Weights"].SetValue(weightsV);
            // Render the final pass
            base.Draw();
        }
    }
}


