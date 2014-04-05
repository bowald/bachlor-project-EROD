using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ERoD
{
    public class SSAO : AdvancedPostProcess
    {

        //Scale
        //Bias
        //Intensity
        //Radius
        public BasicSSAO ssao;
        //public BasicSSAO_SC2 ssao2;
        public Blur BlurV;
        public Blur BlurH;
        public SSAOBlend Blend;

        public SSAO(ERoD game, float radius, float intensity, float scale, float bias, int width, int heigth)
            : base(game)
        {
            ssao = new BasicSSAO(game, radius, intensity, scale, bias);
            //ssao2 = new BasicSSAO_SC2(game, 3f, 0.2f, 5.0f, 2f);
            BlurV = new Blur(game, 10.0f, true, false, width, heigth);
            BlurH = new Blur(game, 10.0f, true, true, width, heigth);
            Blend = new SSAOBlend(game);

            AddPostProcess(ssao);

            AddPostProcess(BlurV);
            AddPostProcess(BlurH);
            AddPostProcess(BlurV);
            AddPostProcess(BlurH);

            AddPostProcess(Blend);
        }
    }
}