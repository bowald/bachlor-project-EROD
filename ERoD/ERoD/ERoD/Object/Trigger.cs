using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    class Trigger : EntityObject
    {
        public Trigger(Entity entity, Model model, Matrix world, Game game) 
            : base(entity, model, world, game)
        {
            this.entity = entity;
        }
    }
}
