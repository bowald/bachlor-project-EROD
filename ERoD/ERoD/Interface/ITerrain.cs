using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public interface ITerrain
    {
        BEPUphysics.BroadPhaseEntries.StaticCollidable PhysicTerrain { get; }
    }
}
