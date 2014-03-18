using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    class Potion : Item
    {
        public Potion(ItemType type, Texture2D icon, int quantity, int id)
            : base(type, icon, "potion", quantity, id)
        {
            switch (id)
            {
                case 1:
                    this.Name = "Health Potion";
                    break;
                case 2:
                    this.Name = "Healthier Potion";
                    break;
                case 3:
                    this.Name = "Instanta-Health";
                    break;
                case 4:
                    this.Name = "Potion of Luck";
                    break;
                case 5:
                    this.Name = "Potion of Invisibility";
                    break;
            }
        }
    }
}
