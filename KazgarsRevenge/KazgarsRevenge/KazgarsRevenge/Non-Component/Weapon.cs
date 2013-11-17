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
        public WeaponType Type { get; private set; }
        public Weapon(Texture2D icon, string name, Dictionary<StatType, float> statEffects, Model gearModel, WeaponType type, bool twoHanded)
            : base(icon, name, statEffects, gearModel, GearSlot.Righthand, GearSlot.Lefthand)
        {
            this.Type = type;
            this.TwoHanded = twoHanded;
        }
    }
}
