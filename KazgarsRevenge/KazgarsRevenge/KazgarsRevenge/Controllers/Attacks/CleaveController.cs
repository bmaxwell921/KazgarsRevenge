using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    public class CleaveController : AttackController
    {
        bool decapitation = false;
        public CleaveController(KazgarsRevengeGame game, GameEntity entity, int damage, FactionType factionToHit, AliveComponent creator, bool decap, bool invig)
            : base(game, entity, damage, factionToHit, creator, AttackType.Melee)
        {
            HitMultipleTargets();
            this.decapitation = decap;
            if (invig)
            {
                ReturnLife(.2f);
            }
        }

        protected override int GetDamage()
        {
            if (decapitation && RandSingleton.U_Instance.Next(10) < 3)
            {       
                return (int)damage * 4;
            }
            return (int)damage;
        }
    }
}
