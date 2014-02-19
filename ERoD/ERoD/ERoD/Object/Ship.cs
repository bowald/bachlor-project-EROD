using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    class Ship : EntityObject
    {
        public Ship(Entity entity, Model model, Matrix world, Game game) 
            : base(entity, model, world, game)
        {
        }

        public void MoveForward()
        {
            entity.LinearVelocity = entity.WorldTransform.Forward * 20;
        }

        public override void Update(GameTime gameTime)
        {
            GamePadState gamePadState = ((ERoD)Game).GamePadState;
            if (gamePadState.IsButtonDown(Buttons.A))
            {
                MoveForward();
            }
            base.Update(gameTime);
        }



    }
}
