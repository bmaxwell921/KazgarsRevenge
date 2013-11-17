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
        public Dictionary<StatType, float> StatEffects { get; private set; }
        public Model GearModel { get; private set; }
        public GearSlot Slot { get; private set; }
        public GearSlot Slot2 { get; private set; }
        public Equippable(Texture2D icon, string name, Dictionary<StatType, float> statEffects, Model gearModel, GearSlot slot, GearSlot secondSlot)
            : base(icon, name)
        {
            this.StatEffects = statEffects;
            this.GearModel = gearModel;
            this.Slot = slot;
            this.Slot2 = secondSlot;
        }
    }
}
