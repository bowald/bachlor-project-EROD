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
        SpriteFont Font1;
        Vector2 FontPos;
        SpriteBatch spriteBatch;

        int CurrentLap = 0;
        // Dictionary with Ship ID's and what lap they are currently on.
        Dictionary<int, int> IDtoLap;
        Dictionary<int, int> IDtoCurrentCheckpoint;
        Dictionary<int, int> IDtoCheckpointNR;

        ERoD Game;

        public GameLogic(ERoD Game)
        {
            this.Game = Game;
            IDtoLap = new Dictionary<int, int>();
            IDtoCurrentCheckpoint = new Dictionary<int, int>();
            IDtoCheckpointNR = new Dictionary<int, int>();

            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            Font1 = Game.Content.Load<SpriteFont>("Sprites/Lap1");
        }

        public void CreateShip(int shipID)
        {
            IDtoLap.Add(shipID, 1);
            IDtoCurrentCheckpoint.Add(shipID, 0);
            IDtoCheckpointNR.Add(shipID, 0);
        }

        // Increments the lap counter
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

            if (IDtoCheckpointNR[shipID] == 4)
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

        // Removes an entity from the world space
        public void RemoveObject(Entity entity, EntityObject entityObject)
        {
            Debug.WriteLine("I just removed an object!");
            Game.Space.Remove(entity);
            Game.Components.Remove(entityObject);
        }
    }
}
