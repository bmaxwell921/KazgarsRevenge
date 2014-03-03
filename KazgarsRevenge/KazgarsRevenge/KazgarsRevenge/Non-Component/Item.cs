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
        Potion,
        Gold,
        Essence,
        Equippable,
        Recipe,
    }
    public class Item : ICloneable
    {
        public bool Stackable { get; private set; }
        public int Quantity { get; protected set; }
        public ItemType Type { get; protected set; }
        public Texture2D Icon { get; protected set; }
        public string Name { get; protected set; }

        /// <summary>
        /// creates a stackable item
        /// </summary>
        public Item(ItemType type, Texture2D icon, string name, int quantity)
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
        }

        /// <summary>
        /// use this to stack the same type of item with itself
        /// </summary>
        /// <param name="quantity"></param>
        public void AddQuantity(int quantity)
        {
            this.Quantity += quantity;
            this.Stackable = true;
        }

        /// <summary>
        /// Returns a 'deep' copy of this item. Everything is either a value type
        /// or immutable so yeah
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            return new Item(this.Type, this.Icon, this.Name, this.Quantity);
        }
    }
}
