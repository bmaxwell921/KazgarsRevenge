﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    class Potion : Item
    {
        public AbilityName PotionAbility = AbilityName.None;
        public Potion(Texture2D icon, int quantity, int id)
            : base(ItemType.Potion, icon, "potion", quantity, id)
        {
            switch (id)
            {
                case 1:
                    this.Name = "Health Potion";
                    this.PotionAbility = AbilityName.HealthPotion;
                    break;
                case 2:
                    this.Name = "Super Potion";
                    this.PotionAbility = AbilityName.SuperHealthPotion;
                    break;
                case 3:
                    this.Name = "Insta-Health";
                    this.PotionAbility = AbilityName.InstaHealthPotion;
                    break;
                case 4:
                    this.Name = "Potion of Luck";
                    this.PotionAbility = AbilityName.PotionOfLuck;
                    break;
                case 5:
                    this.Name = "Potion of Invisibility";
                    this.PotionAbility = AbilityName.None;
                    break;
                case 6:
                    this.Name = "Portal Potion";
                    this.PotionAbility = AbilityName.PortalPotion;
                    break;
            }

            SetTooltip(GetPotionTooltip(Name, GetPotionDescription()));
        }

        private string GetPotionDescription()
        {
            switch (this.ItemID)
            {
                case 1:
                    return "Heals for 20% health\nover 5 seconds";
                case 2:
                    return "Heals for 40% health\nover 5 seconds";
                case 3:
                    return "Heals for 30% health\ninstantly";
                case 4:
                    return "Increases the quality\nof loot\n"
                        + "dropped for 2 minutes";
                case 5:
                    return "Makes you undetectable by \n"
                    + "monsters for 20 seconds";
                case 6:
                    return "Teleports you either to the\n"
                           +"soulevator, or the last place\n"
                           +"you teleported from";
                default:
                    return "Default potion text!";
            }
        }

        private Tooltip GetPotionTooltip(string name, string description)
        {
            return new Tooltip(
                new List<TooltipLine>{
                    new TooltipLine(Color.White, name, .75f), 
                    new TooltipLine(Color.Gold, description, .4f),
                    new TooltipLine(Color.Gold, "\n\nSells for " + GoldCost + "g", .5f)});
        }

        public override object Clone()
        {
            return new Potion(this.Icon, this.Quantity, this.ItemID);
        }
    }
}