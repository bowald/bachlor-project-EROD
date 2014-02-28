using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.Entities;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;

namespace ERoD
{
    class CollisionHandler
    {
        Game Game;

        Dictionary<Entity, EntityObject> Triggers;
        Dictionary<Entity, EntityObject> Powerups;

        // Pairs for the ship+trigger and for the ship+powerup
        CollisionGroupPair ShipTriggerPair;
        CollisionGroupPair ShipPowerupPair;

        // Sets up collisiongroups for different objects
        CollisionGroup shipGroup;
        CollisionGroup triggerGroup;
        CollisionGroup powerupGroup;

        public CollisionHandler(Game game)
        {
            // The game.
            // Used to access the GameLogic service
            this.Game = game;

            // Maps all trigger-entities and all powerup-entities together with their respective entityobjects.
            // Necessary when you want to remove an entityobject from the game
            Triggers = new Dictionary<Entity, EntityObject>();
            Powerups = new Dictionary<Entity, EntityObject>();

            // Setting up collisiongroups
            shipGroup = new CollisionGroup();
            triggerGroup = new CollisionGroup();
            powerupGroup = new CollisionGroup();

            // Adding collisiongroups into pairs
            ShipPowerupPair = new CollisionGroupPair(shipGroup, powerupGroup);
            ShipTriggerPair = new CollisionGroupPair(shipGroup, triggerGroup);
  

            //Tells that the groups in our pair should not be able to collide.
            CollisionRules.CollisionGroupRules.Add(ShipPowerupPair, CollisionRule.NoSolver);
            CollisionRules.CollisionGroupRules.Add(ShipTriggerPair, CollisionRule.NoSolver);
        }

        // Add an entityobject to the "ship" collision group
        // Also sets its tag to be equal to its entityobject
        public void addShipGroup(EntityObject ship)
        {
            ship.Entity.CollisionInformation.Tag = ship;
            ship.Entity.CollisionInformation.CollisionRules.Group = shipGroup;
            ship.Entity.CollisionInformation.Events.PairCreated += Events_PairCreated;
        }

        // Add an entityobject to the "trigger" collision group
        // Also sets its tag to be equal to its entityobject
        public void addTriggerGroup(EntityObject trigger)
        {
            trigger.Entity.CollisionInformation.Tag = trigger;
            trigger.Entity.CollisionInformation.CollisionRules.Group = triggerGroup;
            Triggers.Add(trigger.Entity, trigger);
        }

        // Add an entityobject to the "powerup" collision group
        // Also sets its tag to be equal to its entityobject.
        public void addPowerupGroup(EntityObject powerup)
        {
            powerup.Entity.CollisionInformation.Tag = powerup;
            powerup.Entity.CollisionInformation.CollisionRules.Group = powerupGroup;
            Powerups.Add(powerup.Entity, powerup);
        }

        // If a pair was created with the ship and something else,
        // check what the other thing is and do the appropriate action.
        void Events_PairCreated(EntityCollidable sender, BroadPhaseEntry other, NarrowPhasePair pair)
        {
            GameLogic gameLogic = (GameLogic)Game.Services.GetService(typeof (GameLogic));
            EntityObject entityObject = (EntityObject)other.Tag;
            
            try{
                // Checks the Tag of the collided object
                Entity entity = entityObject.Entity;
                Debug.WriteLine("Checking what the tag was!");

                // If it is a trigger, increment the lap counter.
                if (Triggers.ContainsKey(entity))
                {
                    Debug.WriteLine("It was a trigger!");
                    gameLogic.IncrementLap();
                }

                // If it is a powerup, remove the entityobject and its entity from the game.
                else if (Powerups.ContainsKey(entity))
                {
                    Debug.WriteLine("It was a powerup!");
                    gameLogic.RemoveObject(entity, Powerups[entity]);
                    Powerups.Remove(entity);
                }  
            }
            catch (NullReferenceException e)
            {
                Debug.WriteLine(e);
            }              
        }
    }
}
