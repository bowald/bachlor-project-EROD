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

            this.Game = game;

            // Setting up collisiongroups and the list of accepted collidables
            Triggers = new Dictionary<Entity, EntityObject>();
            Powerups = new Dictionary<Entity, EntityObject>();

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

        public void addShipGroup(EntityObject ship)
        {
            ship.Entity.CollisionInformation.Tag = ship;
            ship.Entity.CollisionInformation.CollisionRules.Group = shipGroup;
            ship.Entity.CollisionInformation.Events.PairCreated += Events_PairCreated;
        }

        public void addTriggerGroup(EntityObject trigger)
        {
            trigger.Entity.CollisionInformation.Tag = trigger;
            trigger.Entity.CollisionInformation.CollisionRules.Group = triggerGroup;
            Triggers.Add(trigger.Entity, trigger);
        }

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
                Entity entity = entityObject.Entity;
                Debug.WriteLine("Checking what the tag was!");
                if (Triggers.ContainsKey(entity))
                {
                    Debug.WriteLine("It was a trigger!");
                    gameLogic.IncrementLap();
                }

                else if (Powerups.ContainsKey(entity))
                {
                    Debug.WriteLine("It was a powerup!");
                    gameLogic.RemoveObject(entity, Powerups[entity]);
                    Powerups.Remove(entity);
                }  
            }
            catch (NullReferenceException e){}              
        }
    }
}
