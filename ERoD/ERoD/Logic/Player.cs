using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public float Boost; //Boosttime

        public Player(Ship ship, int playerIndex)
        {
            PlayerIndex = playerIndex;
            Name = GameConstants.PlayerNames[playerIndex];
            Ship = ship;
            Lap = 0; //start before 1st checkpt
            LastCheckpoint = 0;
            Boost = GameConstants.BoostMaxTime;
        }

        public void AllowedToBoost(bool active)
        {
            Ship.AllowedToBoost = active;
        }

        public void Update(GameTime gameTime)
        {
            Boost -= Ship.boostTimer;
            Ship.boostTimer = 0;
            if (Boost < 0)
            {
                Boost = 0;
                Ship.AllowedToBoost = false;
            }
        }
    }
}
