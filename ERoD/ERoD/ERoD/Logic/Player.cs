using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    class Player
    {
        public int Lap;
        public int LastCheckpoint;
        public Ship Ship;
        public String Name;

        // How many checkpoints the player has passed towards a full lap
        public int CheckpointsPassed;

        public Player(Ship ship, String name)
        {
            Name = name;
            Ship = ship;
            Lap = 1;
            LastCheckpoint = 0;
            CheckpointsPassed = 0;
        }
    }
}
