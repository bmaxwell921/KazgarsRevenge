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
        public SoulevatorController(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {
            (entity.GetSharedData(typeof(Entity)) as Entity).CollisionInformation.Events.DetectingInitialCollision += HandleCollision;
        }


        protected void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            GameEntity hitEntity = other.Tag as GameEntity;
            if (hitEntity != null && hitEntity.Name == "localplayer")
            {
                PlayerController controller = hitEntity.GetComponent(typeof(AliveComponent)) as PlayerController;
                if (controller != null)
                {
                    controller.EnterSoulevator();
                }
            }
        }
    }
}
