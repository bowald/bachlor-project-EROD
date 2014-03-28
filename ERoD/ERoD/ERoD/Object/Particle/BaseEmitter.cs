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
        protected int particleLifeSpan;
        protected int emitAmount;
        protected float particleSpeed;
        protected float particleScaling;
        protected Vector3 Position;
        protected Queue<Particle> freeParticles;
        protected Particle[] particles;
        //private readonly List<Modifier> modifiers = new List<Modifier>();

        protected Random random = new Random();

        public BaseEmitter(int maxParticles, int particleLifespan, int emitAmount, float particleSpeed, float scaling, Vector3 position)
        {
            this.particleLifeSpan = particleLifespan;
            this.emitAmount = emitAmount;
            this.particleSpeed = particleSpeed;
            this.Position = position;
            this.particleScaling = scaling;

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

        public virtual void Emit(GameTime gameTime)
        {
            float totalMilliseconds = (float)gameTime.TotalGameTime.TotalMilliseconds;

            for(int i = 0; i < emitAmount && freeParticles.Count > 0; i++)
            {
                Particle particle = freeParticles.Dequeue();
                particle.IsAlive = true;
                particle.Position = Position;
                particle.BirthTime = totalMilliseconds;
                particle.Scaling = particleScaling;

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

        public void Draw(GraphicsDevice graphicsDevice, ICamera camera)
        {
            ConfigureEffectGraphics(graphicsDevice);

            foreach (Particle particle in particles)
            {
                if(particle.IsAlive)
                    particle.Draw(graphicsDevice, camera);
            }

            ResetGraphicsDevice(graphicsDevice);
        }

        private static void ConfigureEffectGraphics(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.BlendState = BlendState.Additive;
            graphicsDevice.DepthStencilState = DepthStencilState.None;
            graphicsDevice.RasterizerState = RasterizerState.CullClockwise;
        }

        private static void ResetGraphicsDevice(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }

        //public List<Modifier> Modifiers
        //{
        //    get { return modifiers; }
        //}
    }
}


