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
using BEPUphysics.CollisionRuleManagement;


namespace ERoD
{
    class Powerup : Trigger
    {
        public Powerup(Game game, GameLogic gameLogic, BVector3 size, BVector3 position, BQuaternion rotation, CollisionGroup collisionGroup)
            :base(game, gameLogic, size, position, rotation, collisionGroup)
        {

        }
    }
}
