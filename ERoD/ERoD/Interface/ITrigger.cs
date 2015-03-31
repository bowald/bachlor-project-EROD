using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    interface ITrigger
    {
        void PairCreated(EntityCollidable sender, BroadPhaseEntry other, NarrowPhasePair pair);
    }
}
