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
        public Dictionary<StatType, float> statEffects { get; private set; }
        public Model gearModel { get; private set; }
        public Equippable(Texture2D icon, string name, Dictionary<StatType, float> statEffects, Model gearModel)
            : base(icon, name)
        {
            this.statEffects = statEffects;
            this.gearModel = gearModel;
        }
    }
}
