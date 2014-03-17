using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.GamerServices;

namespace ERoD
{
    // Class for handling game logic based on Collision Events
    class GameLogic
    {
        CollisionHandler CollisionHandler;

        // A list of all the players currently racing
        List<Player> Players;

        ERoD Game;

        public GameLogic(ERoD game)
        {
            CollisionHandler = new CollisionHandler(game);
            this.Game = game;
            Players = new List<Player>();
        }

        public void AddPlayer(Ship ship, String name)
        {
            Players.Add(new Player(ship, name));
        }

        // Increments the counter on each ship for each trigger they pass
        // Decrements if going the wrong way
        // A ship needs to pass 4 triggers in the right order to run a full lap
        // When a ship has run 3 laps, it wins the game. 

        public void CheckpointPassed(int checkpointID, Player player)
        {
            int lastID = player.LastCheckpoint;

            // If the Checkpoint hit is the next Checkpoint
            if (checkpointID == lastID + 1)
            {
                player.LastCheckpoint = checkpointID;
                player.CheckpointsPassed++;
                // If the player have passed enough Checkpoints to make a full lap
                if (player.CheckpointsPassed == GameConstants.NumberOfCheckpoints)
                {
                    player.Lap++;
                    // If the player have ran enough Laps to finish the race
                    if(player.Lap == GameConstants.NumberOfLaps)
                    {
                        // TODO: Need to check which player was first in goal
                        Debug.WriteLine("{0} has finished, was he first???", player.Name);
                    }
                }
            }
            // If the Checkpoint hit is a previous Checkpoint
            else if(lastID <= checkpointID)
            {
                player.CheckpointsPassed--;             
            }
            // To make sure the number of Checkpoints passed never exceeds the number of Checkpoints on the lap
            player.CheckpointsPassed %= GameConstants.NumberOfCheckpoints;
        }

        // Removes an entity and its EntityObject from the world space
        public void RemoveObject(Entity entity, EntityObject entityObject)
        {
            Debug.WriteLine("I just removed an object!");
            Game.Space.Remove(entity);
            Game.Components.Remove(entityObject);
        }

        // Returns a Player from a Ship
        public Player GetPlayer(Ship ship)
        {
            foreach(Player p in Players)
            {
                if (p.Ship == ship)
                {
                    return p;
                }
            }
            return null;
        }
    }
}
