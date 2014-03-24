using BEPUphysics.Entities;
using ConversionHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ERoD
{
    class EntityObject : BaseObject
    {
        public Entity Entity;

        public EntityObject(Entity entity, Model model, Vector3 scale, Game game) 
            : base(model, game)
        {
            this.Entity = entity;
            this.scale = scale;
        }

        public override void Draw(GameTime gameTime, Effect effect)
        {
            Vector3 ignore;
            MathConverter.Convert(Entity.WorldTransform).Decompose(out ignore, out rotation, out position);

            base.Draw(gameTime, effect);
        }
    }
}
