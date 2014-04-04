using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    // Combines horizontal and vertical blur for testing purposes.
    public class BlurAdvanced : AdvancedPostProcess
    {
        public BlurAdvanced(Game game, int width, int height) 
            : base(game)
        {
            Game = game;
            AddPostProcess(new BiliteralBlurH(game, 2.0f, width));
            AddPostProcess(new BiliteralBlurV(game, 2.0f, height));
        }
    }
}
