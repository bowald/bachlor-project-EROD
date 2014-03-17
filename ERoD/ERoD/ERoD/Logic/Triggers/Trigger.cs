using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionShapes;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUutilities;
using Microsoft.Xna.Framework;
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
using BEPUphysics.CollisionRuleManagement;

namespace ERoD
{
    class Trigger : DrawableGameComponent, ITrigger
    {

        protected GameLogic GameLogic;
        protected Entity Entity;

        public Trigger(Game game, GameLogic gameLogic, BVector3 size, BVector3 position, BQuaternion rotation, CollisionGroup collisionGroup)
            : base(game)
        {
            GameLogic = gameLogic;
            Entity = new Box(position, size.X, size.Y, size.Z);
            Entity.Orientation = rotation;
            Entity.CollisionInformation.Tag = this;
            Entity.CollisionInformation.CollisionRules.Group = collisionGroup;
            Entity.CollisionInformation.Events.PairCreated += PairCreated;
        }

        public Trigger(Game game, GameLogic gameLogic, BVector3 size, BVector3 position, float rotation, CollisionGroup collisionGroup)
            : this(game, gameLogic, size, position, BQuaternion.CreateFromAxisAngle(BVector3.Up, rotation), collisionGroup)
        {

        }

        public virtual void PairCreated(EntityCollidable sender, BroadPhaseEntry other, NarrowPhasePair pair)
        {
            // sender is this?
            // other is ship?
            // do gamelogic stuff
            Console.WriteLine("Sender: {0} paired with Other: {1}", sender, other);
        }

        public override void Draw(GameTime gameTime)
        {
            // Do not draw base Trigger.
        }
    }
}
