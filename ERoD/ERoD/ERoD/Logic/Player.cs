using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class Player
    {
        public int Lap;
        public int LastCheckpoint;
        public Ship Ship;
        public String Name;
        public int PlayerIndex;

        public Player(Ship ship, int playerIndex)
        {
            PlayerIndex = playerIndex;
            Name = GameConstants.PlayerNames[playerIndex];
            Ship = ship;
            Lap = 0; //start before 1st checkpt
            LastCheckpoint = 0;
        }
    }
}
