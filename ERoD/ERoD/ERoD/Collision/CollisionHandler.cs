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

namespace ERoD
{
    class CollisionHandler
    {
        CollisionGroupPair groupPair;

        CollisionGroup shipgroup;
        CollisionGroup blockgroup;

        private Collidable acceptedTrigger;


        public CollisionHandler()
        {
            shipgroup = new CollisionGroup();
            blockgroup = new CollisionGroup();

            groupPair = new CollisionGroupPair(shipgroup, blockgroup);
            CollisionRules.CollisionGroupRules.Add(groupPair, CollisionRule.NoBroadPhase);
        }

        public void addShipGroup(Entity ship)
        {
            ship.CollisionInformation.CollisionRules.Group = shipgroup;
            ship.CollisionInformation.Events.InitialCollisionDetected += InitialCollisionDetected;
        }

        public void addBoxGroup(Entity block)
        {
            block.CollisionInformation.CollisionRules.Group = blockgroup;
            acceptedTrigger = block.CollisionInformation;
        }

        public void InitialCollisionDetected(EntityCollidable sender, Collidable other, CollidablePairHandler collisionPair)
        {
            Debug.WriteLine("WOOOP WOOP KROCK!");
        }
    }
}
