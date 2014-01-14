using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    /// <summary>
    /// contains all emitters for the entity
    /// </summary>
    class EmitterComponent : Component
    {
        Entity physicalData;

        Dictionary<Type, ParticleEmitter> emitters = new Dictionary<Type, ParticleEmitter>();

        public EmitterComponent(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {
            this.physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
        }

        public void AddEmitter(Type particleType, float particlesPerSecond, int maxOffset, Vector3 offsetFromCenter)
        {
            if (!emitters.ContainsKey(particleType))
            {
                emitters.Add(particleType, new ParticleEmitter((Game.Services.GetService(typeof(ParticleManager)) as ParticleManager).GetSystem(particleType), 
                    particlesPerSecond, physicalData.Position, maxOffset, offsetFromCenter));
            }

        }

        public void RemoveEmitter(Type particleType)
        {
            emitters.Remove(particleType);
        }

        public override void Update(GameTime gameTime)
        {
            foreach (KeyValuePair<Type, ParticleEmitter> k in emitters)
            {
                k.Value.Update(gameTime, physicalData.Position);
            }
        }
    }
}
