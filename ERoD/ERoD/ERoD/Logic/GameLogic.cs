using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ERoD
{
    // Class for handling game logic based on Collision Events
    class GameLogic
    {
        int CurrentLap = 0;
        ERoD Game;

        public GameLogic(ERoD Game)
        {
            this.Game = Game;
        }

        // Increments the lap counter
        public void IncrementLap()
        {
            CurrentLap++;
            Debug.WriteLine("CurrentLap is now: " + CurrentLap);

            if (CurrentLap == 3)
            {
                Debug.WriteLine("Woop Woop! You've won the game!");
            }
        }

        // Removes an entity from the world space
        public void RemoveObject(Entity entity, EntityObject entityObject)
        {
            Debug.WriteLine("I just removed an object!");
            Game.Space.Remove(entity);
            Game.Components.Remove(entityObject);
        }
    }
}
