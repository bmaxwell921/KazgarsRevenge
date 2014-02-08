using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    public abstract class DrawableComponent3D : Component
    {
        protected Entity physicalData;
        public DrawableComponent3D(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {

            this.physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
        }

        protected Dictionary<Type, ParticleEmitter> emitters = new Dictionary<Type, ParticleEmitter>();
        public void AddEmitter(Type particleType, float particlesPerSecond, int maxOffset, Vector3 offsetFromCenter)
        {
            if (!emitters.ContainsKey(particleType))
            {
                emitters.Add(particleType, new ParticleEmitter((Game.Services.GetService(typeof(ParticleManager)) as ParticleManager).GetSystem(particleType),
                    particlesPerSecond, physicalData.Position, maxOffset, offsetFromCenter));
            }
        }

        public void AddParticleTimer(Type t, double length)
        {
            if (emitters.ContainsKey(t))
            {
                emitters[t].SetDeathTimer(length);
            }
        }

        public void RemoveEmitter(Type particleType)
        {
            emitters.Remove(particleType);
        }

        abstract public void Draw(GameTime gameTime, Matrix view, Matrix projection, bool edgeDetection);
    }
}
