using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public enum ItemType
    {
        Gold,//id of 0
        Potion,//ids from 1 to 1000
        Recipe,//ids from 1001 to 2000
        Essence,//ids from 2001 to 3000
        Equippable,//ids from 3001 to 10,000
    }
    public class Item : ICloneable
    {
        public bool Stackable { get; private set; }
        public int Quantity { get; set; }
        public ItemType Type { get; protected set; }
        public Texture2D Icon { get; protected set; }
        public string Name { get; protected set; }
        public int ItemID { get; private set; }
        public Tooltip Tooltip { get; private set; }

        /// <summary>
        /// creates a stackable item
        /// </summary>
        public Item(ItemType type, Texture2D icon, string name, int quantity, int id)
        {
            this.Icon = icon;
            this.Name = name;
            this.Type = type;
            if (type == ItemType.Gold || type == ItemType.Essence || type == ItemType.Potion)
            {
                this.Stackable = true;
                this.Quantity = quantity;
            }
            else
            {
                this.Stackable = false;
                this.Quantity = 1;
            }

            this.ItemID = id;

            if (this.ItemID == 0)
            {
                SetTooltip(GetGoldTooltip());
            }
        }

        /// <summary>
        /// creates a stackable item
        /// </summary>
        public Item(ItemType type, Texture2D icon, string name, int quantity, int id, Tooltip tooltip)
        {
            this.Icon = icon;
            this.Name = name;
            this.Type = type;
            if (type == ItemType.Gold || type == ItemType.Essence || type == ItemType.Potion)
            {
                this.Stackable = true;
                this.Quantity = quantity;
            }
            else
            {
                this.Stackable = false;
                this.Quantity = 1;
            }

            this.ItemID = id;
            this.Tooltip = tooltip;
        }

        protected void SetTooltip(Tooltip t)
        {
            this.Tooltip = t;
        }

        private Tooltip GetGoldTooltip()
        {
            return new Tooltip(
                new List<TooltipLine>
                {
                    new TooltipLine(Color.White, "Gold", 1),
                    new TooltipLine(Color.Gold, "" + Quantity, .75f)
                });
        }

        /// <summary>
        /// use this to stack the same type of item with itself
        /// </summary>
        /// <param name="quantity"></param>
        public void AddQuantity(int quantity)
        {
            this.Quantity += quantity;
            this.Stackable = true;

            if (this.ItemID == 0)
            {
                SetTooltip(GetGoldTooltip());
            }
        }

        /// <summary>
        /// Returns a 'deep' copy of this item. Everything is either a value type
        /// or immutable so yeah
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            return new Item(this.Type, this.Icon, this.Name, this.Quantity, this.ItemID, this.Tooltip);
        }
    }
}
