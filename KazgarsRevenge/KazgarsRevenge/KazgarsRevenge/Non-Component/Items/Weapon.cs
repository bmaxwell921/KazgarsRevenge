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
        public Weapon(Texture2D icon, string name, Dictionary<StatType, float> statEffects, Model gearModel, AttackType type, bool twoHanded, int id)
            : base(icon, name, statEffects, gearModel, GearSlot.Righthand, GearSlot.Lefthand, id)
        {
            this.PrimaryAttackType = type;
            this.TwoHanded = twoHanded;
        }

        public override object Clone()
        {
            Dictionary<StatType, float> statsClone = null;
            if (this.statAllocatements != null)
            {
                statsClone = new Dictionary<StatType, float>(this.statAllocatements);
            }
            return new Weapon(this.Icon, this.Name, statsClone, this.GearModel, this.PrimaryAttackType, this.TwoHanded, this.ItemID);
        }

        protected override Tooltip GetEquippableTooltip()
        {
            List<TooltipLine> tiplines = new List<TooltipLine>
            {
                new TooltipLine(GetQualityColor(Quality), Name, .65f),
                new TooltipLine(Color.White, (TwoHanded? "Twohand " : "Onehand ") + PrimaryAttackType.ToString()+" Weapon", .45f),
                new TooltipLine(Color.Gold, "Requires Level " + ItemLevel, .35f)
            };

            foreach (KeyValuePair<StatType, float> k in StatEffects)
            {
                tiplines.Add(new TooltipLine(Color.Green, "+" + k.Value + " " + k.Key.ToString(), .35f));
            }

            tiplines.Add(new TooltipLine(Color.Gold, "\nSells for " + GoldCost + "g", .5f));
            return new Tooltip(tiplines);
        }
    }
}
