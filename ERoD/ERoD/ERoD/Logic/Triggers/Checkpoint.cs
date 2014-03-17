using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BVector3 = BEPUutilities.Vector3;
using BQuaternion = BEPUutilities.Quaternion;
using BMatrix = BEPUutilities.Matrix;
using BMatrix3x3 = BEPUutilities.Matrix3x3;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Quaternion = Microsoft.Xna.Framework.Quaternion;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Microsoft.Xna.Framework;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.CollisionRuleManagement;

namespace ERoD
{
    class Checkpoint : Trigger
    {
        private int ID;

        public Checkpoint(Game game, GameLogic gameLogic, BVector3 size, BVector3 position, BQuaternion rotation, int ID, CollisionGroup collisionGroup) 
            : base(game, gameLogic, size, position, rotation, collisionGroup)
        {
            this.ID = ID;
            Entity.CollisionInformation.CollisionRules.Group = collisionGroup;
        }

        public Checkpoint(Game game, GameLogic gameLogic, BVector3 size, BVector3 position, float rotation, int ID, CollisionGroup collisionGroup)
            : this(game, gameLogic, size, position, BQuaternion.CreateFromAxisAngle(BVector3.Up, rotation), ID, collisionGroup)
        {
        }

        public override void PairCreated(EntityCollidable sender, BroadPhaseEntry other, NarrowPhasePair pair)
        {
            if(other.Tag is Ship)
            {
                Player p = GameLogic.GetPlayer((Ship)other.Tag);
                GameLogic.CheckpointPassed(ID, p);
            }
            base.PairCreated(sender, other, pair);
        }
    }
}
