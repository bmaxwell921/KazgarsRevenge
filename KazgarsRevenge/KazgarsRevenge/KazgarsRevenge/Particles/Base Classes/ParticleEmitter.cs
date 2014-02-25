using System;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    /// <summary>
    /// Helper for objects that want to leave particles behind them as they
    /// move around the world. This emitter implementation solves two related
    /// problems:
    /// 
    /// If an object wants to create particles very slowly, less than once per
    /// frame, it can be a pain to keep track of which updates ought to create
    /// a new particle versus which should not.
    /// 
    /// If an object is moving quickly and is creating many particles per frame,
    /// it will look ugly if these particles are all bunched up together. Much
    /// better if they can be spread out along a line between where the object
    /// is now and where it was on the previous frame. This is particularly
    /// important for leaving trails behind fast moving objects such as rockets.
    /// 
    /// This emitter class keeps track of a moving object, remembering its
    /// previous position so it can calculate the velocity of the object. It
    /// works out the perfect locations for creating particles at any frequency
    /// you specify, regardless of whether this is faster or slower than the
    /// game update rate.
    /// </summary>
    public class ParticleEmitter
    {
        #region Fields
        public int BoneIndex { get; private set; }

        ParticleSystem particleSystem;
        float timeBetweenParticles;
        Vector3 previousPosition;
        float timeLeftOver;
        Random rand;
        int maxOffset;
        Vector3 offset;

        public bool Timed { get; private set; }
        public bool Dead{get; private set;}
        private double timeLeft = 0;
        #endregion


        /// <summary>
        /// Constructs a new particle emitter object.
        /// </summary>
        public ParticleEmitter(ParticleSystem particleSystem,
                               float particlesPerSecond, Vector3 initialPosition, int maxOffset, Vector3 offset)
        {
            this.particleSystem = particleSystem;

            timeBetweenParticles = 1.0f / particlesPerSecond;
            
            previousPosition = initialPosition;

            rand = new Random();
            this.maxOffset = maxOffset;
            this.offset = offset;

            this.BoneIndex = -1;

            Timed = false;
            Dead = false;
        }
        public ParticleEmitter(ParticleSystem particleSystem,
                       float particlesPerSecond, Vector3 initialPosition, int maxOffset, Vector3 offset, int attachIndex)
        {
            this.particleSystem = particleSystem;

            timeBetweenParticles = 1.0f / particlesPerSecond;

            previousPosition = initialPosition;

            rand = new Random();
            this.maxOffset = maxOffset;
            this.offset = offset;

            this.BoneIndex = attachIndex;

            Timed = false;
            Dead = false;
        }

        /// <summary>
        /// Updates the emitter, creating the appropriate number of particles
        /// in the appropriate positions.
        /// </summary>
        public void Update(GameTime gameTime, Vector3 newPosition, Matrix rotation)
        {
            if (Timed)
            {
                timeLeft -= gameTime.ElapsedGameTime.TotalMilliseconds;
                if (timeLeft <= 0)
                {
                    Dead = true;
                }
            }

            // Work out how much time has passed since the previous update.
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (elapsedTime > 0)
            {
                // Work out how fast we are moving.
                Vector3 velocity = (newPosition - previousPosition) / elapsedTime;

                // If we had any time left over that we didn't use during the
                // previous update, add that to the current elapsed time.
                float timeToSpend = timeLeftOver + elapsedTime;
                
                // Counter for looping over the time interval.
                float currentTime = -timeLeftOver;

                // Create particles as long as we have a big enough time interval.
                while (timeToSpend > timeBetweenParticles)
                {
                    currentTime += timeBetweenParticles;
                    timeToSpend -= timeBetweenParticles;

                    // Work out the optimal position for this particle. This will produce
                    // evenly spaced particles regardless of the object speed, particle
                    // creation frequency, or game update rate.
                    float mu = currentTime / elapsedTime;

                    Vector3 position = Vector3.Lerp(previousPosition, newPosition, mu);

                    // Create the particle.
                    Vector3 finalpos = Vector3.Transform(offset, rotation);
                    finalpos += position;
                    particleSystem.AddParticle(finalpos + new Vector3(rand.Next(maxOffset * 2) - maxOffset, rand.Next(maxOffset * 2) - maxOffset, rand.Next(maxOffset * 2) - maxOffset), velocity);
                }

                // Store any time we didn't use, so it can be part of the next update.
                timeLeftOver = timeToSpend;
            }

            previousPosition = newPosition;
        }

        public void SetDeathTimer(double timerLength)
        {
            timeLeft = timerLength;
            Timed = true;
        }
    }
}
