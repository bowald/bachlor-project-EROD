using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    class ThrusterEmitter : BaseEmitter
    {
        public Entity Ship { get; set; }
        public Vector3 EmitPosOffsets { get; set; }
        private Vector3 lastEmitPos;

        public ThrusterEmitter(int maxParticles, int particleLifespan, int emitAmount, float particleSpeed, float particleScaling, Entity ship, Vector3 emitPosOffsets) 
            : base(maxParticles, particleLifespan, emitAmount, particleSpeed, particleScaling, ConversionHelper.MathConverter.Convert(ship.Position))
        {
            Ship = ship;
            EmitPosOffsets = emitPosOffsets;
        }

        public override void LoadContent(List<Texture2D> textures, GraphicsDevice graphicsDevice)
        {
            if (textures.Count == 0)
                throw new InvalidOperationException("Cannot load a particle effect without a list of textures.");

            for (int i = 0; i < particles.Length; i++)
            {
                Texture2D tex = textures[random.Next(textures.Count)];
                particles[i] = new Particle(particleLifeSpan, tex, graphicsDevice);
                particles[i].ModColor = new Color(0.0f, 150.0f, 255.0f);
                freeParticles.Enqueue(particles[i]);
            }
        }

        public override void Emit(GameTime gameTime)
        {
            float totalMilliseconds = (float)gameTime.TotalGameTime.TotalMilliseconds;
            Matrix shipMatrix = ConversionHelper.MathConverter.Convert(Ship.OrientationMatrix);
            Vector3 shipBackwardVector = ConversionHelper.MathConverter.Convert(Ship.OrientationMatrix.Backward);
            Vector3 shipPos = ConversionHelper.MathConverter.Convert(Ship.Position);
            float offsetScale = 0.4f;
            Vector3 emitPos = shipPos + shipMatrix.Right * EmitPosOffsets.X + shipMatrix.Up * EmitPosOffsets.Y + shipBackwardVector * EmitPosOffsets.Z;

            int maxParticles = Math.Min(emitAmount, freeParticles.Count);

            for (int i = 0; i < emitAmount && freeParticles.Count > 0; i++)
            {
                float timeWeight = (float)i / maxParticles; 
                Particle particle = freeParticles.Dequeue();
                particle.IsAlive = true;
                Vector3 offset = new Vector3(shipMatrix.Left.X * (float)random.NextDouble() * offsetScale + shipMatrix.Right.X * (float)random.NextDouble() * offsetScale + shipMatrix.Up.X * (float)random.NextDouble() * offsetScale/2 + shipMatrix.Down.X * (float)random.NextDouble() * offsetScale/2
                                            , shipMatrix.Left.Y * (float)random.NextDouble() * offsetScale + shipMatrix.Right.Y * (float)random.NextDouble() * offsetScale + shipMatrix.Up.Y * (float)random.NextDouble() * offsetScale/2 + shipMatrix.Down.Y * (float)random.NextDouble() * offsetScale/2,
                                              shipMatrix.Left.Z * (float)random.NextDouble() * offsetScale + shipMatrix.Right.Z * (float)random.NextDouble() * offsetScale + shipMatrix.Up.Z * (float)random.NextDouble() * offsetScale/2 + shipMatrix.Down.Z * (float)random.NextDouble() * offsetScale/2);

                particle.Position = Vector3.Lerp(emitPos, lastEmitPos, timeWeight) + offset;
                
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
            lastEmitPos = emitPos;

        }
    }
}
