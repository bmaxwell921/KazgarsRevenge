using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge.Non_Component
{
    public enum PotionType
    {
        HP,
        SuperHP,
        InstantHP,
        Luck,
        Invis,
    }
    class Potion : Item
    {
        public Potion(ItemType type, Texture2D icon, PotionType potionType, int quantity)
            : base(type, icon, "potion", quantity)
        {
            switch (potionType)
            {
                case PotionType.HP:
                    this.Name = "Health Potion";
                    break;
                case PotionType.SuperHP:
                    this.Name = "Healthier Potion";
                    break;
                case PotionType.InstantHP:
                    this.Name = "Instanta-Health";
                    break;
                case PotionType.Luck:
                    this.Name = "Potion of Luck";
                    break;
                case PotionType.Invis:
                    this.Name = "Potion of Invisibility";
                    break;
            }
        }
    }
}
