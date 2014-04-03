using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ERoD
{
    public class BasicSSAO_SC2 : BasicPostProcess
    {
        private float OcclusionRadious;
        private float FullOcclusionThreshold;
        private float NoOcclusionThreshold;
        private float OcclusionPower;
        private Vector3[] SamplePoints;
        private Vector2 NoiseScale;

        private Texture2D NoiseTexture;

        public BasicSSAO_SC2(ERoD game, float OcclusionRadious, float FullOcclusionThreshold, float NoOcclusionThreshold, float OcclusionPower)
            : base(game)
        {
            this.OcclusionRadious = OcclusionRadious;
            this.FullOcclusionThreshold = FullOcclusionThreshold;
            this.NoOcclusionThreshold = NoOcclusionThreshold;
            this.OcclusionPower = OcclusionPower;
            Random random = new Random();
            int w = 1;// Game.GraphicsDevice.Viewport.Width;
            int h = 1;// Game.GraphicsDevice.Viewport.Height;
            SamplePoints = generateSamplePoints(10, random);
            NoiseTexture = generateNoiseTexture(w, h, random);
            NoiseScale = new Vector2(Game.GraphicsDevice.Viewport.Width / w, Game.GraphicsDevice.Viewport.Height / h);//192 108

            Debug.WriteLine(NoiseScale);

            UsesVertexShader = true;
            newSceneSurfaceFormat = SurfaceFormat.Vector4; //kolla
        }


        //Generates samplepoints, more samples closer to the origio of the kernel, also the kernel is orientet against the z-axis
        private Vector3[] generateSamplePoints(int SampleSize, Random random)
        {
            Vector3[] kernel = new Vector3[SampleSize];
            for (int i = 0; i < SampleSize; i++)
            {
                kernel[i] = new Vector3(Nextfloat(random, -1.0, 1.0), Nextfloat(random, -1.0, 1.0), Nextfloat(random, -1.0, 1.0));
                kernel[i].Normalize();
                //Debug.WriteLine("Kernel[" + i + "] =" + kernel[i]);
                float scale = (float) i / SampleSize;
                scale = MathHelper.Lerp(0.1f, 1.0f, scale * scale);
                kernel[i] *= scale;
                Debug.WriteLine(kernel[i].Length());
            }
            return kernel;
        
        }

        //Generates a noisetexter given a widh and heigth,  Z-component is zero because our kernel is orientet aligning the Z-axis
        private Texture2D generateNoiseTexture(int w, int h, Random random)
        {
            int noiseSize = w * h;
            Color[] noise = new Color[noiseSize];

            for (int i = 0; i < noiseSize; ++i)
            {
                Vector2 vec2 = new Vector2(Nextfloat(random, -1.0, 1.0), Nextfloat(random, -1.0, 1.0));
                vec2.Normalize();
                //noise[i] = new Color(vec2.X, vec2.Y, 0.0f);
                noise[i] = new Color(0, 0, 1.0f);
            }

            Texture2D noiseTexture = new Texture2D(Game.GraphicsDevice, w, h, false, SurfaceFormat.Color);
            noiseTexture.SetData(noise);
            return noiseTexture;
        }

        //Calculates a random float between min and max.
        private float Nextfloat(Random rng, double min, double max)
        {
            return (float) ( min + (rng.NextDouble() * (max - min)));
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
            {
                effect = Game.Content.Load<Effect>("Shaders/PostProcessing/SSAO_SC2");

            }
            
            effect.CurrentTechnique = effect.Techniques["SSAO"];

            effect.Parameters["HalfPixel"].SetValue(HalfPixel);
            effect.Parameters["OcclusionRadious"].SetValue(OcclusionRadious);
            effect.Parameters["FullOcclusionThreshold"].SetValue(FullOcclusionThreshold);
            effect.Parameters["NoOcclusionThreshold"].SetValue(NoOcclusionThreshold);
            effect.Parameters["OcclusionPower"].SetValue(OcclusionPower);
            effect.Parameters["SSAOSamplePoints"].SetValue(SamplePoints);
            effect.Parameters["DepthMap"].SetValue(DepthBuffer);
            effect.Parameters["NormalMap"].SetValue(NormalBuffer);
            effect.Parameters["NoiseScale"].SetValue(NoiseScale);
            effect.Parameters["NoiseMap"].SetValue(NoiseTexture);
            effect.Parameters["SidesLengthVS"].SetValue(new Vector2(camera.TanFovy * camera.AspectRatio, -camera.TanFovy));
            effect.Parameters["FarPlane"].SetValue(camera.FarPlane);

            base.Draw(gameTime);

        }
    }
}
