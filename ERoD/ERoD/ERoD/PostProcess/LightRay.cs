
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ERoD
{
    public class LightRay : BasicPostProcess
    {
        public Vector3 lightSourcePos;
        // 
        public float Intensity = 1.0f;
        public float Density = .5f;
        public float Decay = .95f;
        public float Weight = 0.1f;
        public float Exposure = .15f;

        public LightRay(ERoD game, Vector3 sourcePos, float density, float decay, float weight, float exposure)
            : base(game)
        {
            lightSourcePos = sourcePos;

            Density = density;
            Decay = decay;
            Weight = weight;
            Exposure = exposure;
            UsesVertexShader = true;
        }

        public LightRay(ERoD game, Vector3 sourcePos)
            : base(game)
        {
            lightSourcePos = sourcePos;
            UsesVertexShader = true;
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
                effect = Game.Content.Load<Effect>("Shaders/PostProcessing/LightRay");

            effect.CurrentTechnique = effect.Techniques["LightRayFX"];

            effect.Parameters["halfPixel"].SetValue(HalfPixel);

            effect.Parameters["Density"].SetValue(Density);
            // Decay: 0-1. Dissipates each sample's contribution as the ray progresses away from the light source.
            effect.Parameters["Decay"].SetValue(Decay);
            //Weight controls the intensity of each sample
            effect.Parameters["Weight"].SetValue(Weight);
            // Exposure controls the overall intensity of the post-process,
            effect.Parameters["Exposure"].SetValue(Exposure);

            effect.Parameters["lightPosition"].SetValue(lightSourcePos);
            effect.Parameters["cameraPosition"].SetValue(camera.Position);
            effect.Parameters["matVP"].SetValue(camera.View * camera.Projection);

            // Set Params.
            base.Draw(gameTime);

        }
    }
}