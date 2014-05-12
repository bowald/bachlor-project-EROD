﻿using BEPUphysics.BroadPhaseEntries.MobileCollidables;
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

        List<Trigger> Triggers;

        Game Game;

        public int WinnerIndex = -1;
        public float RaceTime = 0.0f;

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
            Checkpoint.resetID();
        }

        public void AddPlayer(Ship ship, int playerIndex)
        {
            Players.Add(new Player(ship, playerIndex));
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

        public void CheckpointPassed(int checkpointID, Player player)
        {
            Console.WriteLine("{0} has passed checkpoint {1}", player.Name, checkpointID);
            int lastID = player.LastCheckpoint;

            // If the Checkpoint hit is the next Checkpoint
            // Checkpoints start with ID = 1 and go up to ID = NumberOfCheckpoints
            // The player starts with lastID = 0
            // When you hit the start/goal, ID = 1 the lastID need to be 0 or Numberofcheckpoints
            // lastID % NumberOfCheckpoints is 0 in both cases.
            // LastID = 0 or nbr gives checkpointID = 1 for true
            // 0 < LastID < Nbr gives checkpointID = lastId + 1
            if (checkpointID == (lastID % GameConstants.NumberOfCheckpoints) + 1)
            {
                // Update LastCheckpoint when passed in correct order.
                player.LastCheckpoint = checkpointID;

                // If the player have passed enough Checkpoints to make a full lap
                // ID = 1 is the first/goal/start checkpoint
                if (checkpointID == 1)
                {
                    player.Lap++;

                    // If the player have ran enough Laps to finish the race
                    if (player.Lap == GameConstants.NumberOfLaps + 1)
                    {
                        // A player finished the race.
                        Console.WriteLine("A player finished the race");
                        WinnerIndex = player.PlayerIndex;
                        // Set game state to GAME_OVER
                    }
                }
            }
        }

        public void DrawTriggers(GameTime gameTime)
        {
           foreach(Trigger t in Triggers){
               t.Draw(gameTime);
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
