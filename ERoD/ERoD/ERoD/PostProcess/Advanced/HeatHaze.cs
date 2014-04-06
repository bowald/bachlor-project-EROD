using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    class HeatHaze : AdvancedPostProcess
    {
        private Blend Blend;
        private BumpmapBlur Blur;

        public HeatHaze(Game game, bool highBlur) : base(game)
        {
            Blur = new BumpmapBlur(game, highBlur);
            Blend = new Blend(game, 1.0f, 1.6f, 1.0f, 1.3f);

            AddPostProcess(Blur);
            AddPostProcess(Blend);
        }
    }
}
