using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    class GearGenerator : ModelCreator
    {
        Dictionary<string, Texture2D> equippableIcons = new Dictionary<string, Texture2D>();
        string attachDir = "Models\\Attachables\\";
        public GearGenerator(KazgarsRevengeGame game)
            : base(game)
        {

        }

        private Texture2D GetIcon(string gearName)
        {
            string texName = "Textures\\";
            switch (gearName)
            {
                case "sword":
                    texName += "whitePixel";
                    break;
                default:
                    texName += "whitePixel";
                    break;
            }

            Texture2D retTex;
            if (!equippableIcons.TryGetValue(texName, out retTex))
            {
                retTex = Game.Content.Load<Texture2D>(texName);
                equippableIcons.Add(texName, retTex);
            }
            return retTex;
        }

        private Dictionary<StatType, float> GetStats(string itemName)
        {
            Dictionary<StatType, float> itemStats = new Dictionary<StatType, float>();
            switch (itemName)
            {
                case "sword"://+5 str. heck yeah.
                    itemStats.Add(StatType.Strength, 5);
                    break;
                default:
                    itemStats.Add(StatType.Strength, 5);
                    break;
            }
            return itemStats;
        }

        public Equippable GetSword()
        {
            return new Equippable(GetIcon("sword"), "sword", GetStats("sword"), GetUnanimatedModel(attachDir + "sword01"));
        }

        public Equippable GetBow()
        {
            return new Equippable(GetIcon("bow"), "bow", GetStats("bow"), GetUnanimatedModel(attachDir + "bow01"));
        }
    }
}
