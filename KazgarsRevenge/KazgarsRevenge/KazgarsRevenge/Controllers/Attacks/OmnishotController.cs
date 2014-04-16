using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public class OmnishotController : AttackController
    {
        public OmnishotController(KazgarsRevengeGame game, GameEntity entity, int damage, FactionType factionToHit, AliveComponent creator)
            : base(game, entity, damage, factionToHit, creator, AttackType.Ranged)
        {
            this.lifeLength = 250;
        }

        bool penetrate = false;
        bool home = false;
        bool leech = false;
        bool bleed = false;

        public void AddEffects(bool penetrate, bool home, bool leech, bool bleed)
        {
            this.penetrate = penetrate;
            this.home = home;
            this.leech = leech;
            this.bleed = bleed;

            dieAfterContact = false;
        }
        protected override void HandleEntityCollision(GameEntity hitEntity)
        {
            if (hitEntity.Name == "room")
            {
                Entity.KillEntity();
            }
            base.HandleEntityCollision(hitEntity);
        }

        public override void End()
        {
            //create 20 arrows in 30 degree spread from current direction
            AttackManager attacks = Game.Services.GetService(typeof(AttackManager)) as AttackManager;

            float curYaw = GetBackwardsYaw(physicalData.OrientationMatrix.Forward);


            for (int i = 0; i < 20; ++i)
            {
                float yaw = (float)((i / 22.0f) * MathHelper.ToRadians(30) - MathHelper.ToRadians(15)) + curYaw;
                Vector3 dir = new Vector3((float)Math.Cos(yaw), 0, (float)Math.Sin(yaw));

                attacks.CreateArrow(physicalData.Position, dir, (int)damage, creator, home, penetrate, leech, bleed);
            }
        }
    }
}
