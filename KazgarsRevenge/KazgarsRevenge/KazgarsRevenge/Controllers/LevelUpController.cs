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
    public class LevelUpController : Component
    {
        Entity myData;
        Entity followData;

        SharedGraphicsParams modelParams;
        public LevelUpController(KazgarsRevengeGame game, GameEntity entity, Entity followData)
            :base(game, entity)
        {
            this.followData = followData;
            myData = entity.GetSharedData(typeof(Entity)) as Entity;
            modelParams = entity.GetSharedData(typeof(SharedGraphicsParams)) as SharedGraphicsParams;
        }

        bool exploding = false;
        float increase = 0;
        float rate = 1500;
        public override void Update(GameTime gameTime)
        {
            myData.Position = followData.Position;

            if (!exploding)
            {
                increase += 5;
                rate += increase;
                modelParams.size.X -= (float)((gameTime.ElapsedGameTime.TotalMilliseconds / 1500) * (rate));
                modelParams.size.Y = modelParams.size.X;
                modelParams.size.Z = modelParams.size.X;
                if (modelParams.size.X <= 0)
                {
                    (Game.Services.GetService(typeof(AttackManager)) as AttackManager).SpawnBuffParticles(myData.Position);
                    (Entity.GetComponent(typeof(UnanimatedModelComponent)) as UnanimatedModelComponent).AddColorTint(Color.White);
                    exploding = true;
                }
            }
            else
            {
                modelParams.size.X += (float)((gameTime.ElapsedGameTime.TotalMilliseconds / 1500) * 2000);
                modelParams.size.Y = modelParams.size.X;
                modelParams.size.Z = modelParams.size.X;

                if (modelParams.size.X >= 500)
                {
                    Entity.KillEntity();
                }
            }
        }
    }
}
