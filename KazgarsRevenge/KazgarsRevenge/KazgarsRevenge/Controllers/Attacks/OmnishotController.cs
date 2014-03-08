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
            : base(game, entity, damage, factionToHit, creator)
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
            AttackManager attacks = Game.Services.GetService(typeof(AttackManager)) as AttackManager;
            for (int i = 0; i < 20; ++i)
            {
                float yaw = (float)((i / 22.0f) * Math.PI * 2);
                Vector3 dir = new Vector3((float)Math.Cos(yaw), 0, (float)Math.Sin(yaw));

                attacks.CreateArrow(physicalData.Position, dir, (int)damage, creator, home, penetrate, leech, bleed);
            }
        }
    }
}
