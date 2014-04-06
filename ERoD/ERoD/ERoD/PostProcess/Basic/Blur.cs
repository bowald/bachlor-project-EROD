﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ERoD
{
    public class Blur : BasicPostProcess
    {

        private Boolean BiliteralBlur;
        private Boolean Horizontal;

        private Vector4[] sampleOffsets;
        private float[] sampleWeights;

        private DeferredRenderer renderer;

        private const int Sample_Count = 11;

        protected float blurAmount;
        public float BlurAmount
        {
            get { return blurAmount; }
            set
            {
                blurAmount = value;
                if (sampleOffsets != null)
                {
                    if (Horizontal)
                        SetBlurEffectParameters(1.0f / (float)(this.Game.GraphicsDevice.Viewport.Width / 2f), 0, ref sampleOffsets, ref sampleWeights);
                    else
                    {
                        SetBlurEffectParameters(0, 1.0f / (float)(this.Game.GraphicsDevice.Viewport.Width / 2f), ref sampleOffsets, ref sampleWeights);
                    }
                }
            }
        }

        //If BiliteralBlur is false, Guassian is Used.
        //If Horisantal is false, Vertical is Used.
        public Blur(Game game, float amount, Boolean BiliteralBlur, Boolean Horizontal, DeferredRenderer renderer)
            : base(game)
        {
            this.BiliteralBlur = BiliteralBlur;
            this.Horizontal = Horizontal;

            blurAmount = amount;
            newSceneSurfaceFormat = SurfaceFormat.Vector4;
            Sampler = SamplerState.PointClamp;

            if (BiliteralBlur)
            {
                UsesVertexShader = true;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
            {
                if (BiliteralBlur)
                {
                    effect = Game.Content.Load<Effect>("Shaders/PostProcessing/BiliteralBlur");
                    effect.CurrentTechnique = effect.Techniques["BiliteralBlur"];
                    effect.Parameters["DepthMap"].SetValue(renderer.depthMap);
                    effect.Parameters["NormalMap"].SetValue(renderer.normalMap);
                }
                else
                {
                    effect = Game.Content.Load<Effect>("Shaders/PostProcessing/GuassianBlur");
                    effect.CurrentTechnique = effect.Techniques["GuassianBlur"];
                }

                sampleOffsets = new Vector4[Sample_Count];
                sampleWeights = new float[Sample_Count];
                if (Horizontal)
                    SetBlurEffectParameters(1.0f / (float)(this.Game.GraphicsDevice.Viewport.Width / 2f), 0, ref sampleOffsets, ref sampleWeights);
                else
                {
                    SetBlurEffectParameters(0, 1.0f / (float)(this.Game.GraphicsDevice.Viewport.Width / 2f), ref sampleOffsets, ref sampleWeights);
                }

            }

            effect.Parameters["SampleOffsets"].SetValue(sampleOffsets);
            effect.Parameters["SampleWeights"].SetValue(sampleWeights);
            effect.Parameters["HalfPixel"].SetValue(HalfPixel);

            Game.GraphicsDevice.BlendState = BlendState.Opaque;
            
            base.Draw(gameTime);
        }

        /// <summary>
        /// Computes sample weightings and texture coordinate offsets
        /// for one pass of a separable gaussian blur filter.
        /// </summary>
        private void SetBlurEffectParameters(float dx, float dy, ref Vector4[] offsets, ref float[] weights)
        {
            // The first sample always has a zero offset.
            weights[0] = ComputeGaussian(0);
            offsets[0] = new Vector4();

            // Maintain a sum of all the weighting values.
            float totalWeights = weights[0];

            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            for (int i = 0; i < Sample_Count / 2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = ComputeGaussian(i + 1);

                weights[i * 2 + 1] = weight;
                weights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float sampleOffset = i * 2 + 1.5f;

                Vector4 delta = new Vector4(dx, dy, 1.0f, 1.0f) * sampleOffset;

                // Store texture coordinate offsets for the positive and negative taps.
                offsets[i * 2 + 1] = delta;
                offsets[i * 2 + 2] = -delta;
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (int i = 0; i < Sample_Count; i++)
            {
                weights[i] /= totalWeights;
            }
        }

        /// <summary>
        /// Evaluates a single point on the gaussian falloff curve.
        /// Used for setting up the blur filter weightings.
        /// </summary>
        private float ComputeGaussian(float n)
        {
            float theta = blurAmount;

            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                           Math.Exp(-(n * n) / (2 * theta * theta)));
        }
    }
}
