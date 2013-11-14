using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    class Item
    {
        public Texture2D Icon { get; private set; }
        public string Name { get; private set; }
        public Item(Texture2D icon, string name)
        {
            this.Icon = icon;
            this.Name = name;
        }
    }
}
