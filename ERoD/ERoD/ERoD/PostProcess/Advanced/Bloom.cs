// Implementet using this Guide:
//http://digitalerr0r.wordpress.com/2009/10/04/xna-shader-programming-tutorial-24-bloom/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class Bloom : AdvancedPostProcess
    {
        //Globals
        public BloomMask mask;
        public Blur BlurV;
        public Blur BlurH;
        public Blend blend;

        public float Threshold;

        

        public Bloom(ERoD game, float Threshold, int width, int height)
            : base(game)
        {
            mask = new BloomMask(game, Threshold);
            BlurV = new Blur(game, 2.0f, false, false, width, height);
            BlurH = new Blur(game, 2.0f, false, true, width, height);
            blend = new Blend(game, 1.0f, 1.6f, 1.0f, 1.3f);

            AddPostProcess(mask);
            AddPostProcess(BlurV);
            AddPostProcess(BlurH);
            AddPostProcess(blend);
        }

    }
}
