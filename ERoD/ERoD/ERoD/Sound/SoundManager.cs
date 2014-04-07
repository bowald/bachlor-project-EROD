using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class SoundManager
    {
        private static SoundEffect menuSelection;
        public static SoundEffect MenuSelection
        {
            get { return menuSelection; }
        }

        public static void Initialize(Game game)
        {
            menuSelection = game.Content.Load<SoundEffect>("Sound/MenuSelection");
        }
    }
}
