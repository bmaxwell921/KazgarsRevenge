using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.CollisionTests;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.Collidables;
using BEPUphysics.Collidables.MobileCollidables;

namespace KazgarsRevenge
{
    public class KeyController : Component
    {
        Entity physicalData;
        public KeyController(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {

            this.physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
            physicalData.IsAffectedByGravity = false;
            physicalData.CollisionInformation.Events.DetectingInitialCollision += HandleCollision;
        }

        private object lockObj = new Object();
        protected void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            lock (lockObj)
            {
                if (!Entity.Dead)
                {
                    GameEntity hitEntity = other.Tag as GameEntity;
                    if (hitEntity != null && hitEntity.Type == EntityType.Player)
                    {
                        (Game.Services.GetService(typeof(LevelManager)) as LevelManager).UnlockDoors();
                        (Game.Services.GetService(typeof(SoundEffectLibrary)) as SoundEffectLibrary).playUnlockSound();
                        ParticleSystem p = (Game.Services.GetService(typeof(ParticleManager)) as ParticleManager).GetSystem(typeof(KeyUnlockSystem));
                        for (int i = 0; i < 50; ++i)
                        {
                            p.AddParticle(physicalData.Position, Vector3.Zero);
                        }
                        Entity.KillEntity();
                    }
                }
            }
        }

    }
}
