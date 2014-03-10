using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    class LapTrigger : EntityObject
    {

        public int ID { get; set; }

        public LapTrigger(int ID, Entity entity, Model model, Matrix world, Vector3 nodeRotation, Game game) 
            : base(entity, model, world , nodeRotation, game)
        {
            this.ID = ID;
            entity.BecomeKinematic();
        }
    }
}
