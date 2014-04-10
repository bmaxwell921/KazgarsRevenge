using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public class Essence : Item
    {
        public StatType BoostedStat{get; private set;}
        public float StatIncrease{get; private set;}

        public Essence(Texture2D icon, int id)
            : base(ItemType.Essence, icon, "Essence", 1, id)
        {
            string essencePrefix = "";
            string essenceSuffix = "";
            #region essence definitions
            switch(id){
                case 901:
                    BoostedStat = StatType.Strength;
                    essencePrefix = "Lesser";
                    essenceSuffix = "Warrior";
                    StatIncrease = increase1;
                    break;
                case 902:
                    BoostedStat = StatType.Agility;
                    essencePrefix = "Lesser";
                    essenceSuffix = "Ranger";
                    StatIncrease = increase1;
                    break;
                case 903:
                    BoostedStat = StatType.Intellect;
                    essencePrefix = "Lesser";
                    essenceSuffix = "Warlock";
                    StatIncrease = increase1;
                    break;
                case 904:
                    BoostedStat = StatType.Strength;
                    essenceSuffix = "Warrior";
                    StatIncrease = increase2;
                    break;
                case 905:
                    BoostedStat = StatType.Agility;
                    essenceSuffix = "Ranger";
                    StatIncrease = increase2;
                    break;
                case 906:
                    BoostedStat = StatType.Intellect;
                    essenceSuffix = "Warlock";
                    StatIncrease = increase2;
                    break;
                case 907:
                    BoostedStat = StatType.Strength;
                    essencePrefix = "Greater";
                    essenceSuffix = "Warrior";
                    StatIncrease = increase3;
                    break;
                case 908:
                    BoostedStat = StatType.Agility;
                    essencePrefix = "Greater";
                    essenceSuffix = "Ranger";
                    StatIncrease = increase3;
                    break;
                case 909:
                    BoostedStat = StatType.Intellect;
                    essencePrefix = "Greater";
                    essenceSuffix = "Warlock";
                    StatIncrease = increase3;
                    break;
                case 910:
                    BoostedStat = StatType.Strength;
                    essencePrefix = "Potent";
                    essenceSuffix = "Warrior";
                    StatIncrease = increase4;
                    break;
                case 911:
                    BoostedStat = StatType.Agility;
                    essencePrefix = "Potent";
                    essenceSuffix = "of the Ranger";
                    StatIncrease = increase4;
                    break;
                case 912:
                    BoostedStat = StatType.Intellect;
                    essencePrefix = "Potent";
                    essenceSuffix = "Warlock";
                    StatIncrease = increase4;
                    break;
                case 913:
                    BoostedStat = StatType.Strength;
                    essencePrefix = "Illustrious";
                    essenceSuffix = "Warrior";
                    StatIncrease = increase5;
                    break;
                case 914:
                    BoostedStat = StatType.Agility;
                    essencePrefix = "Illustrious";
                    essenceSuffix = "Ranger";
                    StatIncrease = increase5;
                    break;
                case 915:
                    BoostedStat = StatType.Intellect;
                    essencePrefix = "Illustrious";
                    essenceSuffix = "Warlock";
                    StatIncrease = increase5;
                    break;
            }
            #endregion

            SetTooltip(new Tooltip(
                new List<TooltipLine> {
                new TooltipLine(Color.Blue, essencePrefix + " Essence", .65f),
                new TooltipLine(Color.Blue, "of the " + essenceSuffix, .65f),
                new TooltipLine(Color.Gold, "Use: increases the selected gear's", .4f),
                new TooltipLine(Color.Gold, "stats by +" + StatIncrease + " " + BoostedStat.ToString() , .4f)
                }));
        }


        public readonly static int increase1 = 5;
        public readonly static int increase2 = 10;
        public readonly static int increase3 = 25;
        public readonly static int increase4 = 50;
        public readonly static int increase5 = 100;
    }
}
