using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.BroadPhaseEntries;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using BRay = BEPUutilities.Ray;
using BVector3 = BEPUutilities.Vector3;

namespace ERoD
{
    public partial class Ship : EntityObject
    {
        public Ship(Entity entity, Model model, Vector3 scale, Game game) 
            : base(entity, model, scale, game)
        {
            makeNormal();
            AddCollidable(((ITerrain)Game.Services.GetService(typeof(ITerrain))).PhysicTerrain);
        }

        public void Update(GameTime gameTime, GamePadState gamePadState)
        {
            if (gamePadState.IsButtonDown(Buttons.Y))
            {
                makeDestroyed();
            }
            else if (gamePadState.IsButtonDown(Buttons.X))
            {
                makeNormal();
            }
            if (State == ShipState.Normal)
            {
                NormalUpdate(gameTime, gamePadState);
            }
            else
            {
                DestroyedUpdate(gameTime, gamePadState);
            }
            base.Update(gameTime);
        }
    }
}
