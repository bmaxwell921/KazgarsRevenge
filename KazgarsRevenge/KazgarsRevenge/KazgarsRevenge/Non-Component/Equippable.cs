using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public class Equippable : Item
    {
        public Dictionary<StatType, float> StatEffects { get; protected set; }
        public Model GearModel { get; private set; }
        public GearSlot Slot { get; private set; }
        public GearSlot Slot2 { get; private set; }
        public Equippable(Texture2D icon, string name, Dictionary<StatType, float> statEffects, Model gearModel, GearSlot slot, GearSlot secondSlot)
            : base(ItemType.Equippable, icon, name, 1)
        {
            this.StatEffects = statEffects;
            this.GearModel = gearModel;
            this.Slot = slot;
            this.Slot2 = secondSlot;
        }

        public override object Clone()
        {
            // This is the only thing that needs to be deep copied.
            Dictionary<StatType, float> statsClone = null;
            if (this.StatEffects != null)
            {
                statsClone = new Dictionary<StatType, float>(this.StatEffects);
            }
            return new Equippable(this.Icon, this.Name, statsClone, this.GearModel, this.Slot, this.Slot2);
        }
    }
}
