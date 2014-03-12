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

        GameLogic gameLogic;

        Dictionary<Entity, EntityObject> Checkpoints;
        Dictionary<Entity, EntityObject> Powerups;
        Dictionary<Entity, EntityObject> Ships;

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
            gameLogic = (GameLogic)Game.Services.GetService(typeof(GameLogic));

            // Maps all trigger-entities and all powerup-entities together with their respective entityobjects.
            // Necessary when you want to remove an entityobject from the game
            Checkpoints = new Dictionary<Entity, EntityObject>();
            Powerups = new Dictionary<Entity, EntityObject>();
            Ships = new Dictionary<Entity, EntityObject>();

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
        public void addShipGroup(Ship ship)
        {
            Debug.WriteLine("Added the ship to the collision group!");
            ship.Entity.CollisionInformation.Tag = ship;
            ship.Entity.CollisionInformation.CollisionRules.Group = shipGroup;
            Ships.Add(ship.Entity, ship);
        }

        // Add an entityobject to the "trigger" collision group
        // Also sets its tag to be equal to its entityobject
        public void addTriggerGroup(EntityObject trigger)
        {
            Debug.WriteLine("Added the trigger to the collision group!");
            trigger.Entity.CollisionInformation.Tag = trigger;
            trigger.Entity.CollisionInformation.CollisionRules.Group = triggerGroup;
            trigger.Entity.CollisionInformation.Events.PairCreated += TriggerPairCreated;
            Checkpoints.Add(trigger.Entity, trigger);
        }

        // Add an entityobject to the "powerup" collision group
        // Also sets its tag to be equal to its entityobject.
        public void addPowerupGroup(EntityObject powerup)
        {
            powerup.Entity.CollisionInformation.Tag = powerup;
            powerup.Entity.CollisionInformation.CollisionRules.Group = powerupGroup;
            powerup.Entity.CollisionInformation.Events.PairCreated += PowerupPairCreated;
            Powerups.Add(powerup.Entity, powerup);
        }

        public void TriggerPairCreated(EntityCollidable sender, BroadPhaseEntry other, NarrowPhasePair pair)
        {
            if (other.Tag is Ship)
            {
                Debug.WriteLine("A ship hit the trigger!");
            }       
        }

        public void PowerupPairCreated(EntityCollidable sender, BroadPhaseEntry other, NarrowPhasePair pair)
        {
            Debug.WriteLine("A ship hit the powerup!");
            if (other.Tag is Ship)
            {
                // If it is a powerup, remove the entityobject and its entity from the game.
                EntityObject entityObject = (EntityObject)sender.Tag;
                Entity entity = entityObject.Entity;
                gameLogic.RemoveObject(entity, Powerups[entity]);
            }
            
        }
    }
}
