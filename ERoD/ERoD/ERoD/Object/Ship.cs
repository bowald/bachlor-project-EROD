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

namespace ERoD
{
    class Ship : EntityObject
    {
        private Space space;
        private Model model = null;
        private ERoD erod;
        private float f_max;
        private float IDEAL_HEIGHT;
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
            IDEAL_HEIGHT = 7.0f;
            max_speed = 20.0f; //max velocity
        }

        public void move()
        {
            float h = distancefromG();
            //float acc = accelarationY(h);
           // float FY = ((entity.LinearMomentum.Y / entity.Mass) + acc);
            if (debugEnable)
            {
                Debug.WriteLine("entity.posistion: " + entity.Position);
                Debug.WriteLine("down: " + entity.OrientationMatrix.Down);
                Debug.WriteLine("forward: " + world.Down);
                Debug.WriteLine("right:" + entity.OrientationMatrix.Right);
                Debug.WriteLine("left: " + entity.OrientationMatrix.Left);
            }
            entity.LinearVelocity = new BEPUutilities.Vector3(vX(), vY(10.0f), 0.0f);
        }
        private void hover()
        {
            if(distancefromG() < 3.0f)
                entity.LinearVelocity = new BEPUutilities.Vector3(0.0f, vY(2.0f), 0.0f);
        }
        private float vX(){
            if (entity.LinearVelocity.X < 2.0f)
                return 3.0f;
            else if (entity.LinearVelocity.X >= 15.0f)
            {
                return 15.0f;
            }
            else {
                return entity.LinearVelocity.X + 1.0001f;
            }
            
        }
        
        private float distancefromG()
        {
            BEPUutilities.RayHit hit;
            BRay ray = new BRay(entity.Position, BEPUutilities.Vector3.Down);
            erod.testVarGround.RayCast(ray, 100.0f, out hit);
            return hit.T;
        }
        private float vY(float IDEAL_HEIGHT){
            float h = distancefromG();
            if (h < IDEAL_HEIGHT)
            {
                return 9.82f;
            }
            else if (h < IDEAL_HEIGHT + 4 && h > IDEAL_HEIGHT - 4)
            {
                return 0.0f;
            }
            else if (h == IDEAL_HEIGHT)
            {
                return 0;
            }
            else
                return Math.Min(entity.LinearVelocity.Y * 1.01f, - 9.82f);
        }

        //private float strafe(float stick)
        //{
        //    if(stick != 0)
        //    return stick * 3.0f;
        //}

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            GamePadState gamePadState = ((ERoD)Game).GamePadState;
            if (gamePadState.IsButtonDown(Buttons.A))
            {
                move();
            }
            else
            {
                hover();
            }
            if(gamePadState.IsButtonDown(Buttons.DPadRight))
            {
                if (entity.LinearVelocity.Z < 10.0f)
                {
                    entity.LinearVelocity = new BEPUutilities.Vector3(entity.LinearVelocity.X, entity.LinearVelocity.Y, 10.0f);
                    entity.AngularVelocity = new BEPUutilities.Vector3(0, -1.0f, 0);
                }
                else if (entity.LinearVelocity.Z > 40.0f)
                {
                    entity.LinearVelocity = new BEPUutilities.Vector3(entity.LinearVelocity.X, entity.LinearVelocity.Y, 40.0f);
                    entity.AngularVelocity = new BEPUutilities.Vector3(0, -1.0f, 0);
                }
                else
                {
                    entity.LinearVelocity = new BEPUutilities.Vector3(entity.LinearVelocity.X, entity.LinearVelocity.Y, entity.LinearVelocity.Z * 1.1f);
                    entity.AngularVelocity = new BEPUutilities.Vector3(0, -1.0f, 0);
                }
            }
            else if (gamePadState.IsButtonDown(Buttons.DPadLeft))
            {
                entity.LinearVelocity = new BEPUutilities.Vector3(entity.LinearVelocity.X, entity.LinearVelocity.Y, -10.0f);
                entity.AngularVelocity = new BEPUutilities.Vector3(0,1.0f,0);
            }
            base.Update(gameTime);
        }
    }
}
