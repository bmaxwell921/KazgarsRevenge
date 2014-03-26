using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;

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

        protected Dictionary<string, ParticleEmitter> emitters = new Dictionary<string, ParticleEmitter>();

        public void AddEmitter(Type particleType, string systemName, float particlesPerSecond, int maxOffset, Vector3 offsetFromCenter)
        {
            AddEmitter(particleType, systemName, particlesPerSecond, maxOffset, maxOffset, offsetFromCenter);
        }

        public void AddEmitter(Type particleType, string systemName, float particlesPerSecond, int maxHorizontalOffset, int maxVerticalOffset, Vector3 offsetFromCenter)
        {
            if (!emitters.ContainsKey(systemName))
            {
                ParticleEmitter toAdd = new ParticleEmitter((Game.Services.GetService(typeof(ParticleManager)) as ParticleManager).GetSystem(particleType),
                    particlesPerSecond, physicalData.Position, offsetFromCenter);
                toAdd.SetHorizontalOffset(maxHorizontalOffset);
                toAdd.SetVerticalOffset(maxVerticalOffset);

                emitters.Add(systemName, toAdd);
            }
        }

        public void AddParticleTimer(string systemName, double length)
        {
            if (emitters.ContainsKey(systemName))
            {
                emitters[systemName].SetDeathTimer(length);
            }
        }

        public void RemoveEmitter(string systemName)
        {
            if (emitters.ContainsKey(systemName))
            {
                emitters.Remove(systemName);
            }
        }

        public void KillAllEmitters()
        {
            emitters.Clear();
        }

        public void AddEmitterSizeIncrementExponential(string emitterKey, float incrementPerSecond, float incrementIncrementPerSecond)
        {
            if (emitters.ContainsKey(emitterKey))
            {
                emitters[emitterKey].IncreaseSizePerSecondExponential(incrementPerSecond, incrementIncrementPerSecond);
            }
        }

        public void AddEmitterSizeIncrement(string emitterKey, float incrementPerSecond)
        {
            if (emitters.ContainsKey(emitterKey))
            {
                emitters[emitterKey].IncreaseSizePerSecond(incrementPerSecond);
            }
        }

        protected bool InsideCameraBox(BoundingBox cameraBox)
        {
            Box boxData = physicalData as Box;
            if (boxData == null)
            {
                return true;
            }
            Vector3 pos = boxData.Position;
            float minx = pos.X - boxData.HalfWidth;
            float minz = pos.Z - boxData.HalfWidth;
            float maxx = pos.X + boxData.HalfWidth;
            float maxz = pos.Z + boxData.HalfWidth;
            return !(minx > cameraBox.Max.X
                    || minz > cameraBox.Max.Z
                    || maxx < cameraBox.Min.X
                    || maxz < cameraBox.Min.Z);
        }

        abstract public void Draw(GameTime gameTime, CameraComponent camera, bool edgeDetection);
    }
}
