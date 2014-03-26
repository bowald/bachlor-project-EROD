﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ERoD
{
    public class LightSourceMask : BasicPostProcess
    {
        Texture lishsourceTexture;
        public Vector3 lighSourcePos;
        public string lightSourceasset;
        public float lightSize = 1500;

        public LightSourceMask(ERoD game, Vector3 sourcePos, string lightSourceasset, float lightSize)
            : base(game)
        {
            UsesVertexShader = true;
            lighSourcePos = sourcePos;
            this.lightSourceasset = lightSourceasset;
            this.lightSize = lightSize;
        }

        public override void Draw(GameTime gameTime)
        {

            if (effect == null)
            {
                effect = Game.Content.Load<Effect>("Shaders/PostProcessing/LightSourceMask");
            }

            lishsourceTexture = Game.Content.Load<Texture2D>(lightSourceasset);
            effect.CurrentTechnique = effect.Techniques["LightSourceMask"];

            effect.Parameters["flare"].SetValue(lishsourceTexture);

            effect.Parameters["SunSize"].SetValue(lightSize);
            effect.Parameters["lightPosition"].SetValue(lighSourcePos);
            effect.Parameters["cameraPosition"].SetValue(camera.Position);
            effect.Parameters["matVP"].SetValue(camera.View * camera.Projection);

            // Set Params.
            base.Draw(gameTime);

        }
    }
}