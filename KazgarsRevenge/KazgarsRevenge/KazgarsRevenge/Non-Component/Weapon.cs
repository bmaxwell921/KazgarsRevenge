using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public class Weapon : Equippable
    {
        public bool TwoHanded { get; private set; }
        public AttackType PrimaryAttackType { get; private set; }
        public Weapon(Texture2D icon, string name, Dictionary<StatType, float> statEffects, Model gearModel, AttackType type, bool twoHanded)
            : base(icon, name, statEffects, gearModel, GearSlot.Righthand, GearSlot.Lefthand)
        {
            this.PrimaryAttackType = type;
            this.TwoHanded = twoHanded;
        }

        public override object Clone()
        {
            Dictionary<StatType, float> statsClone = null;
            if (this.StatEffects != null)
            {
                statsClone = new Dictionary<StatType, float>(this.StatEffects);
            }
            return new Weapon(this.Icon, this.Name, statsClone, this.GearModel, this.PrimaryAttackType, this.TwoHanded);
        }
    }
}
