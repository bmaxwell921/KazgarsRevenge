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
    public class SoulevatorController : Component
    {
        SharedGraphicsParams modelParams;
        Entity physicalData;
        public SoulevatorController(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {
            physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
            physicalData.CollisionInformation.Events.DetectingInitialCollision += HandleCollision;

            modelParams = entity.GetSharedData(typeof(SharedGraphicsParams)) as SharedGraphicsParams;
        }

        float alpha = 1;
        bool increasing = false;
        public override void Update(GameTime gameTime)
        {
            if (increasing)
            {
                alpha += .005f;
                if (alpha >= .6f)
                {
                    increasing = false;
                }
            }
            else
            {
                alpha -= .005f;
                if (alpha <= .2f)
                {
                    increasing = true;
                }
            }

            modelParams.alpha = alpha;

            base.Update(gameTime);
        }

        protected void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            GameEntity hitEntity = other.Tag as GameEntity;
            if (hitEntity != null && hitEntity.Name == "localplayer")
            {
                LocalPlayerController controller = hitEntity.GetComponent(typeof(AliveComponent)) as LocalPlayerController;
                if (controller != null)
                {
                    controller.EnterSoulevator(physicalData.Position);
                }
            }
        }
    }
}
