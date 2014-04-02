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
        private float OcclusionRadius;
        private float FullOcclusionThreshold;
        private float NoOcclusionThreshold;
        private float OcclusionPower;
        private Vector3[] SamplePoints;

        public BasicSSAO_SC2(ERoD game, float OcclusionRadious, float FullOcclusionThreshold, float NoOcclusionThreshold, float OcclusionPower)
            : base(game)
        {
            this.OcclusionRadius = OcclusionRadious;
            this.FullOcclusionThreshold = FullOcclusionThreshold;
            this.NoOcclusionThreshold = NoOcclusionThreshold;
            this.OcclusionPower = OcclusionPower;
            SamplePoints = generateSamplePoints(10);
            UsesVertexShader = true;
            newSceneSurfaceFormat = SurfaceFormat.Vector4;
        }

        private Vector3[] generateSamplePoints(int SampleSize)
        {
            Vector3[] points = new Vector3[SampleSize];
            Random random = new Random();
            for (int i = 0; i < SampleSize; i++)
            {
                points[i] = new Vector3(Nextfloat(random, -1.0, 1.0), Nextfloat(random, -1.0, 1.0), Nextfloat(random, -1.0, 1.0));
                points[i].Normalize();
            }
            return points;
        
        }

        //generate_Normal texture here

        //Only use for debugging
        private void debugWriter(Vector3[] list)
        {
            for (int i = 0; i < list.Length; i++)
            {
                Debug.WriteLine("list[" + i + "] =" + list[i]);
            }
        }

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

            //effect.Parameters["HalfPixel"].SetValue(HalfPixel);
            effect.Parameters["CamWorld"].SetValue(camera.World);
            effect.Parameters["ViewProjection"].SetValue(camera.View * camera.Projection);
            //effect.Parameters["SSAOSamplePoints"].SetValue(SamplePoints);
            //effect.Parameters["DepthMap"].SetValue(DepthBuffer);
            //effect.Parameters["SidesLengthVS"].SetValue(new Vector2(camera.TanFovy * camera.AspectRatio, -camera.TanFovy));
            //effect.Parameters["FarPlane"].SetValue(camera.FarPlane);

            effect.Parameters["HalfPixel"].SetValue(HalfPixel);
            effect.Parameters["OcclusionRadius"].SetValue(OcclusionRadius);
            effect.Parameters["FullOcclusionThreshold"].SetValue(FullOcclusionThreshold);
            effect.Parameters["NoOcclusionThreshold"].SetValue(NoOcclusionThreshold);
            effect.Parameters["OcclusionPower"].SetValue(OcclusionPower);
            effect.Parameters["SSAOSamplePoints"].SetValue(SamplePoints);
            effect.Parameters["DepthMap"].SetValue(DepthBuffer);
            effect.Parameters["SidesLengthVS"].SetValue(new Vector2(camera.TanFovy * camera.AspectRatio, -camera.TanFovy));
            effect.Parameters["FarPlane"].SetValue(camera.FarPlane);

            //Game.GraphicsDevice.BlendState = BlendState.Opaque;
            base.Draw(gameTime);

        }
    }
}
