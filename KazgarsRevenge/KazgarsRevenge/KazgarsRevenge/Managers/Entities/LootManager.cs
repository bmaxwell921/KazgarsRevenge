using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    class LootManager : EntityManager
    {
        Dictionary<string, Texture2D> equippableIcons = new Dictionary<string, Texture2D>();
        string attachDir = "Models\\Attachables\\";
        public LootManager(KazgarsRevengeGame game)
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
                case "sword":
                    itemStats.Add(StatType.Strength, 5);
                    break;
                default:
                    itemStats.Add(StatType.Strength, 5);
                    break;
            }
            return itemStats;
        }

        /// <summary>
        /// generates new sword with random stats
        /// </summary>
        /// <returns></returns>
        public Equippable GenerateSword()
        {
            return GetSword(GetStats("sword"));
        }

        /// <summary>
        /// loads a saved sword with predefined stats
        /// </summary>
        /// <param name="swordStats"></param>
        /// <returns></returns>
        public Equippable GetSword(Dictionary<StatType, float> swordStats)
        {
            return new Weapon(GetIcon("sword"), "sword01", swordStats, GetUnanimatedModel(attachDir + "sword01"), AttackType.Melle, false);
        }

        public Equippable GenerateBow()
        {
            return GetBow(GetStats("bow"));
        }

        public Equippable GetBow(Dictionary<StatType, float> bowStats)
        {
            return new Weapon(GetIcon("bow"), "bow", bowStats, GetUnanimatedModel(attachDir + "bow01"), AttackType.Ranged, false);
        }
    }
}
