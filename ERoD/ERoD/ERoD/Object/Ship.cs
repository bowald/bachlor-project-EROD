using BEPUphysics;
using BEPUphysics.Entities;
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
    class Ship : EntityObject
    {
        private Space space;
        private ERoD erod;


        public Ship(Entity Entity, Model model, Matrix world, Game game) 
            : base(Entity, model, world, game)
        {
            erod = game as ERoD;
            space = erod.Space;
            build();
        }

        private void build()
        {
        }

        private BVector3 movment(float scale){
            BVector3 currentVelocity;
            BEPUutilities.Vector2 constrain = new BEPUutilities.Vector2(Entity.LinearVelocity.X,Entity.LinearVelocity.Z);
            if(constrain.Length() >= 30.0f){
                currentVelocity = Entity.OrientationMatrix.Forward * 30.0f;
                return new BVector3(currentVelocity.X, 0, currentVelocity.Z);
            }
            else
            {
                currentVelocity = Entity.OrientationMatrix.Forward * scale * Entity.LinearVelocity.Length();
                return new BVector3(currentVelocity.X, 0, currentVelocity.Z);
            }
        }

        private BVector3 strafe(Boolean Right)
        {
            BVector3 velocity;
            BVector3 turn;
            if (Right)
            {
                turn = Entity.OrientationMatrix.Right;
            }
            else
            {
                turn = Entity.OrientationMatrix.Left;
            }
            velocity = turn * 8.0f;
            return new BVector3(velocity.X, 0, velocity.Z);
        }
        
        private float distancefromG()
        {
            BEPUutilities.RayHit hit;
            BRay ray = new BRay(Entity.Position,BVector3.Down);
            erod.testVarGround.RayCast(ray, 100.0f, out hit);
            return hit.T;
        }
        private void fly(float idealHeight){
            float h = distancefromG();

            Entity.Position = new BVector3(Entity.Position.X, Entity.Position.Y + (idealHeight - h), Entity.Position.Z);
        }

        public override void Update(GameTime gameTime)
        {
            //float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            BVector3 forward = BVector3.Zero;
            float height;
            BVector3 shipStrafe = BVector3.Zero;
            GamePadState gamePadState = ((ERoD)Game).GamePadState;
            if (gamePadState.IsButtonDown(Buttons.A))
            {
                forward = movment(1.1f);
                height = 4.5f;
            }
            else
            {
                Entity.LinearVelocity *= 0.95f;
                height = 4.0f;
            }
            if(gamePadState.IsButtonDown(Buttons.DPadRight))
            {
                shipStrafe = strafe(true);
                Entity.AngularVelocity = new BVector3(0, -0.7f, 0);

            }
            else if (gamePadState.IsButtonDown(Buttons.DPadLeft))
            {
                shipStrafe = strafe(false);
                Entity.AngularVelocity = new BVector3(0,0.7f,0);
            }
            else if (gamePadState.IsButtonUp(Buttons.DPadLeft) && gamePadState.IsButtonUp(Buttons.DPadRight))
            {
                shipStrafe = BVector3.Zero;
                Entity.AngularVelocity = BVector3.Zero;
            }
            fly(height);
            Entity.LinearVelocity = shipStrafe + forward;

            base.Update(gameTime);
        }
    }
}
