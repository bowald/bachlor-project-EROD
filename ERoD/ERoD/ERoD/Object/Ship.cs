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
        private Model model = null;
        private ERoD erod;
        private float f_max;
        private float max_speed;
        private Boolean debugEnable = true;

        public Ship(Entity entity, Model model, Matrix world, Game game) 
            : base(entity, model, world, game)
        {
            erod = game as ERoD;
            space = erod.Space;
            build();
        }

        private void build()
        {
            Debug.WriteLine(entity.Mass);
            entity.Mass = 250.0f;
            Debug.WriteLine(entity.Mass);
            f_max = 2 * (entity.Mass * 9.8f); // change to space.gravity
            max_speed = 20.0f; //max velocity
        }

        //public BVector3 move()
        //{
        //    float h = distancefromG();
        //    entity.LinearVelocity = new BVector3(forward().X, vY(10.0f), forward().Z);
        //}
        //private void hover()
        //{
        //    if(distancefromG() < 3.0f)
        //        entity.LinearVelocity = new BVector3(0.0f, vY(2.0f), 0.0f);
        //}

        private BVector3 movment(float scale){
            BEPUutilities.Vector2 constrain = new BEPUutilities.Vector2(entity.LinearVelocity.X,entity.LinearVelocity.Z);
            Debug.WriteLine("constran.lenght = " + constrain.Length());
            if(constrain.Length() >= 30.0f){
                BVector3 currentVelocity = entity.LinearVelocity;
                currentVelocity = entity.OrientationMatrix.Forward * 30.0f;
                return new BVector3(entity.LinearVelocity.X, 0, entity.LinearVelocity.Z);
            }
            else
            {
                BVector3 currentVelocity = entity.OrientationMatrix.Forward * scale * entity.LinearVelocity.Length();
                return new BVector3(currentVelocity.X, 0, currentVelocity.Z);
            }
        }

        private BVector3 strafe(Boolean Right)
        {
            BVector3 turn;
            if (Right)
            {
                turn = entity.OrientationMatrix.Right;
            }
            else
            {
                turn = entity.OrientationMatrix.Left;
            }
            BVector3 velocity = turn * 8.0f;
            return new BVector3(velocity.X, 0, velocity.Z);
        }
        
        private float distancefromG()
        {
            BEPUutilities.RayHit hit;
            BRay ray = new BRay(entity.Position,BVector3.Down);
            erod.testVarGround.RayCast(ray, 100.0f, out hit);
            return hit.T;
        }
        private void fly(float idealHeight){
            float h = distancefromG();

            entity.Position = new BVector3(entity.Position.X, entity.Position.Y + (idealHeight - h), entity.Position.Z);
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
                forward = entity.LinearVelocity * 0.95f;
                height = 4.0f;
            }
            if(gamePadState.IsButtonDown(Buttons.DPadRight))
            {
                //shipStrafe = strafe(true);
                entity.AngularVelocity = new BVector3(0, -0.7f, 0);

            }
            else if (gamePadState.IsButtonDown(Buttons.DPadLeft))
            {
                //shipStrafe = strafe(false);
                entity.AngularVelocity = new BVector3(0,0.7f,0);
            }
            else if (gamePadState.IsButtonUp(Buttons.DPadLeft) && gamePadState.IsButtonUp(Buttons.DPadRight))
            {
                entity.AngularVelocity = BVector3.Zero;
            }
            fly(height);
            entity.LinearVelocity = shipStrafe + forward;

            base.Update(gameTime);
        }
    }
}
