using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    class ThrusterEmitter : BaseEmitter
    {
        public Entity Ship { get; set; }

        public ThrusterEmitter(int maxParticles, int particleLifespan, int emitAmount, float particleSpeed, float particleScaling, Entity ship) 
            : base(maxParticles, particleLifespan, emitAmount, particleSpeed, particleScaling, ConversionHelper.MathConverter.Convert(ship.Position))
        {
            Ship = ship;
        }

        public override void Emit(GameTime gameTime)
        {
            float totalMilliseconds = (float)gameTime.TotalGameTime.TotalMilliseconds;
            Matrix shipMatrix = ConversionHelper.MathConverter.Convert(Ship.OrientationMatrix);
            Vector3 shipBackwardVector = ConversionHelper.MathConverter.Convert(Ship.OrientationMatrix.Backward);
            Vector3 shipPos = ConversionHelper.MathConverter.Convert(Ship.Position);
            float offsetScale = 0.4f;
            

            for (int i = 0; i < emitAmount && freeParticles.Count > 0; i++)
            {
                Particle particle = freeParticles.Dequeue();
                particle.IsAlive = true;
                Vector3 offset = new Vector3(shipMatrix.Left.X * (float)random.NextDouble() * offsetScale + shipMatrix.Right.X * (float)random.NextDouble() * offsetScale + shipMatrix.Up.X * (float)random.NextDouble() * offsetScale/2 + shipMatrix.Down.X * (float)random.NextDouble() * offsetScale/2
                                            , shipMatrix.Left.Y * (float)random.NextDouble() * offsetScale + shipMatrix.Right.Y * (float)random.NextDouble() * offsetScale + shipMatrix.Up.Y * (float)random.NextDouble() * offsetScale/2 + shipMatrix.Down.Y * (float)random.NextDouble() * offsetScale/2,
                                              shipMatrix.Left.Z * (float)random.NextDouble() * offsetScale + shipMatrix.Right.Z * (float)random.NextDouble() * offsetScale + shipMatrix.Up.Z * (float)random.NextDouble() * offsetScale/2 + shipMatrix.Down.Z * (float)random.NextDouble() * offsetScale/2);
                particle.Position = shipPos + offset + shipBackwardVector * 2.8f + shipMatrix.Down * 0.17f;
                particle.BirthTime = totalMilliseconds;
                particle.Scaling = particleScaling;

                Vector3 velocity = (float)random.NextDouble() * new Vector3(particleSpeed, particleSpeed, particleSpeed) * shipBackwardVector;

                particle.Velocity = velocity;
                //particle.Velocity = Vector3.Transform(velocity, Matrix.CreateRotationZ(MathHelper.ToRadians(random.Next(360))));
                //particle.Velocity = Vector3.Transform(velocity, Matrix.CreateFromYawPitchRoll(shipBackwardVector.X,
                //                                                                              shipBackwardVector.Y,
                //                                                                              shipBackwardVector.Z));
                //Console.WriteLine(particle.Velocity);
            }
            
        }
    }
}
