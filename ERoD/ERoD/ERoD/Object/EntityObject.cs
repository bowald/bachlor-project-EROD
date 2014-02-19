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
        protected Entity entity;

        public EntityObject(Entity entity, Model model, Matrix world, Game game) 
            : base(model, world, game)
        {
            this.entity = entity;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            World = Transform * MathConverter.Convert(entity.WorldTransform);

            base.Update(gameTime);
        }
    }
}
