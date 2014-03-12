using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        CollisionHandler CollisionHandler;
        // The number of checkpoints per lap
        int NRofCheckpoints;

        // Dictionary with Ship ID's and what Lap they are on.
        Dictionary<int, int> IDtoLap;
        // Dictionary with Ship ID's and the ID of the last passed checkpoint
        Dictionary<int, int> IDtoCurrentCheckpoint;
        // Dictionary with Ship ID's and the number of passed checkpoints towards a full lap
        Dictionary<int, int> IDtoCheckpointNR;

        ERoD Game;

        public GameLogic(int nrOfCheckpoints, ERoD game)
        {
            CollisionHandler = new CollisionHandler(game);
            this.Game = game;
            this.NRofCheckpoints = nrOfCheckpoints;
            IDtoLap = new Dictionary<int, int>();
            IDtoCurrentCheckpoint = new Dictionary<int, int>();
            IDtoCheckpointNR = new Dictionary<int, int>();
        }

        // When a ship is created, adds it to the dictionaries
        public void CreateShip(Ship ship)
        {
            IDtoLap.Add(ship.ID, 1);
            IDtoCurrentCheckpoint.Add(ship.ID, 0);
            IDtoCheckpointNR.Add(ship.ID, 0);
            CollisionHandler.addShipGroup(ship);
            
        }

        // Increments the counter on each ship for each trigger they pass
        // Decrements if going the wrong way
        // A ship needs to pass 4 triggers in the right order to run a full lap
        // When a ship has run 3 laps, it wins the game. 
        public void IncrementLap(int shipID, int ID)
        {
            if(IDtoCurrentCheckpoint[shipID] == (ID - 1))
            {
                IDtoCurrentCheckpoint[shipID] = ID;
                IDtoCheckpointNR[shipID]++;
                Debug.WriteLine("Ship nr: " + shipID + " just passed checkpoint nr: " + ID);
            }

            else if(IDtoCurrentCheckpoint[shipID] <= ID ){
                IDtoCheckpointNR[shipID]--;
                Debug.WriteLine("Ship nr: " + shipID + " just passed checkpoint nr: " + ID + ", is going the wrong way!" );
            }

            if (IDtoCheckpointNR[shipID] == NRofCheckpoints)
            {
                IDtoLap[shipID]++;
                Debug.WriteLine("Ship nr: " + shipID + " has ran a full lap! Is now on lap: " + IDtoLap[shipID]);
                IDtoCheckpointNR[shipID] = 0;
                IDtoCurrentCheckpoint[shipID] = 0;
            }
  
            if (IDtoLap[shipID] == 3)
            {
                Debug.WriteLine("Woop Woop! Ship nr: " + shipID + " won the game!");
            }
        }

        // Removes an entity and its EntityObject from the world space
        public void RemoveObject(Entity entity, EntityObject entityObject)
        {
            Debug.WriteLine("I just removed an object!");
            Game.Space.Remove(entity);
            Game.Components.Remove(entityObject);
        }
    }
}
