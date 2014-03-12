using BEPUphysics.Entities;
using ConversionHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BMatrix = BEPUutilities.Matrix;


namespace ERoD
{
    class EntityObject : BaseObject
    {
        public Entity Entity;

        public EntityObject(Entity entity, Model model, Matrix world, Vector3 nodeRotation, Game game) 
            : base(model, world, nodeRotation, game)
        {   
            this.Entity = entity;
        }

        public override void Draw(GameTime gameTime, Effect effect)
        {
            World = Transform * MathConverter.Convert(Entity.WorldTransform);

            base.Draw(gameTime, effect);
        }
    }
}
