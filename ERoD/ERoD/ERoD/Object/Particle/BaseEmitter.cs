using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class BaseEmitter
    {
        private readonly int particleLifeSpan;
        private readonly int emitAmount;
        private readonly float particleSpeed;
        private readonly Queue<Particle> freeParticles;
        private readonly Particle[] particles;
        //private readonly List<Modifier> modifiers = new List<Modifier>();

        private readonly Random random = new Random();

        public BaseEmitter(int maxParticles, int particleLifespan, int emitAmount, float particleSpeed)
        {
            this.particleLifeSpan = particleLifespan;
            this.emitAmount = emitAmount;
            this.particleSpeed = particleSpeed;

            particles = new Particle[maxParticles];
            freeParticles = new Queue<Particle>(maxParticles);
        }

        public void LoadContent(List<Texture2D> textures, GraphicsDevice graphicsDevice)
        {
            if(textures.Count == 0)
                throw new InvalidOperationException("Cannot load a particle effect without a list of textures.");

            for(int i = 0; i < particles.Length; i++)
            {
                particles[i] = new Particle(particleLifeSpan, textures[random.Next(textures.Count)], graphicsDevice);
                freeParticles.Enqueue(particles[i]);
            }
        }

        public void Emit(GameTime gameTime, Vector3 position)
        {
            float totalMilliseconds = (float)gameTime.TotalGameTime.TotalMilliseconds;

            for(int i = 0; i < emitAmount && freeParticles.Count > 0; i++)
            {
                Particle particle = freeParticles.Dequeue();
                particle.IsAlive = true;
                particle.Position = position;
                particle.BirthTime = totalMilliseconds;

                Vector3 velocity = new Vector3((float)random.NextDouble() * particleSpeed, 0, 0);

                velocity = Vector3.Transform(velocity, Matrix.CreateRotationZ(MathHelper.ToRadians(random.Next(360))));
                particle.Velocity = Vector3.Transform(velocity, Matrix.CreateRotationX(MathHelper.ToRadians(random.Next(360))));
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach (Particle particle in particles)
            {
                if(particle.IsAlive)
                {
                    float particleAge = (float)(gameTime.TotalGameTime.TotalMilliseconds - particle.BirthTime) / particle.LifeSpan;

                    //foreach (Modifier modifier in modifiers)
                    //    modifier.Update(particle, particleAge);

                    particle.Alpha = MathHelper.Lerp(1, 0, particleAge);

                    particle.Update(gameTime.TotalGameTime.TotalMilliseconds);

                    if(!particle.IsAlive)
                        freeParticles.Enqueue(particle);
                }
            }
        }

        public void Draw(GraphicsDevice graphicsDevice, Matrix viewMatrix, Matrix projectionMatrix)
        {
            ConfigureEffectGraphics(graphicsDevice);

            foreach (Particle particle in particles)
            {
                if(particle.IsAlive)
                    particle.Draw(graphicsDevice, viewMatrix, projectionMatrix);
            }

            ResetGraphicsDevice(graphicsDevice);
        }

        private static void ConfigureEffectGraphics(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.BlendState = BlendState.Additive;
            graphicsDevice.DepthStencilState = DepthStencilState.None;
        }

        private static void ResetGraphicsDevice(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        //public List<Modifier> Modifiers
        //{
        //    get { return modifiers; }
        //}
    }
}


