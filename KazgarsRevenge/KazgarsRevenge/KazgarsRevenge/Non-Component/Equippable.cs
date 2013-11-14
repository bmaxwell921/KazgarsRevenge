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
        Dictionary<PlayerController.StatType, float> statEffects;
        public Equippable(Texture2D icon, string name, Dictionary<PlayerController.StatType, float> statEffects)
            : base(icon, name)
        {
            this.statEffects = statEffects;
        }


    }
}
