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
using BEPUphysics;

namespace ERoD
{
    // Class for handling game logic based on Collision Events
    class GameLogic
    {
        CollisionHandler CollisionHandler;

        // A list of all the players currently racing
        List<Player> Players;

        List<Trigger> Triggers; // No use at the moment.

        Game Game;

        Space Space
        {
            get { return (Space)Game.Services.GetService(typeof(Space)); }
        }

        public GameLogic(Game game)
        {
            CollisionHandler = new CollisionHandler(game);
            Game = game;
            Players = new List<Player>();
            Triggers = new List<Trigger>();
        }

        public void AddPlayer(Ship ship, String name)
        {
            Players.Add(new Player(ship, name));
            CollisionHandler.addShipGroup(ship);
        }

        /// <summary>
        ///  Add Checkpoints, the first on is the goal line.
        ///  All checkpoints added make one lap.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="position"></param>
        public Checkpoint AddCheckpoint(BEPUutilities.Vector3 size, BEPUutilities.Vector3 position, float rotation)
        {
            Checkpoint point = new Checkpoint(
                Game, 
                this, 
                size,
                position, 
                rotation,
                CollisionHandler.CheckpointGroup
                );
            Triggers.Add(point);
            Space.Add(point.Entity);
            return point;
        }

        // Increments the counter on each ship for each trigger they pass
        // Decrements if going the wrong way
        // A ship needs to pass 4 triggers in the right order to run a full lap
        // When a ship has run 3 laps, it wins the game. 

        public void CheckpointPassed(int checkpointID, Player player)
        {
            Console.WriteLine("{0} has passed checkpoint {1}", player.Name, checkpointID);
            int lastID = player.LastCheckpoint;

            // If the Checkpoint hit is the next Checkpoint
            if (checkpointID == lastID + 1 || (lastID == GameConstants.NumberOfCheckpoints && checkpointID == 1))
            {
                player.LastCheckpoint = checkpointID;
                // If the player have passed enough Checkpoints to make a full lap
                if (checkpointID == 1)
                {
                    player.Lap++;
                    // If the player have ran enough Laps to finish the race
                    if (player.Lap == GameConstants.NumberOfLaps + 1)
                    {
                        // TODO: Need to check which player was first in goal
                        Debug.WriteLine("{0} has finished, was he first???", player.Name);
                    }
                    else
                    {
                        Debug.WriteLine("{0} has started his {1} Lap", player.Name, player.Lap);
                    }
                }
            }
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
