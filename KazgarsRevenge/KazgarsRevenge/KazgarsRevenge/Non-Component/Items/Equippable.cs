using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public enum GearQuality
    {
        Standard = 1,
        Good = 3,
        Epic = 6,
        Legendary = 16,
    }

    public class Equippable : Item
    {
        public Essence AppliedEssence { get; protected set; }
        public int ItemLevel { get; protected set; }
        public GearQuality Quality { get; protected set; }
        public Dictionary<StatType, float> StatEffects { get; protected set; }
        public Model GearModel { get; private set; }
        public GearSlot Slot { get; private set; }
        public GearSlot Slot2 { get; private set; }
        protected Dictionary<StatType, float> statAllocatements { get; set; }
        public Equippable(Texture2D icon, string name, Dictionary<StatType, float> statAllocatements, Model gearModel, GearSlot slot, GearSlot secondSlot, int id)
            : base(ItemType.Equippable, icon, name, 1, id)
        {
            this.statAllocatements = statAllocatements;
            this.StatEffects = new Dictionary<StatType, float>();
            this.GearModel = gearModel;
            this.Slot = slot;
            this.Slot2 = secondSlot;
            this.Quality = GearQuality.Standard;
            SetTooltip(new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "", 1) }));
        }

        /// <summary>
        /// must be called before gear is available to character (sets quality, and the tooltip based on that)
        /// should only be called on a cloned copy, also. don't alter the entry in AllItems or doom will ensue
        /// </summary>
        /// <param name="quality"></param>
        public void SetStats(GearQuality quality, int level)
        {
            this.Quality = quality;
            this.ItemLevel = level;

            float statAmount = level * (int)quality;

            StatEffects = new Dictionary<StatType, float>();
            foreach (KeyValuePair<StatType, float> k in statAllocatements)
            {
                if (k.Key == StatType.AttackSpeed || k.Key == StatType.CooldownReduction || k.Key == StatType.CritChance || k.Key == StatType.RunSpeed)
                {
                    StatEffects.Add(k.Key, k.Value);
                }
                else
                {
                    StatEffects.Add(k.Key, (float)Math.Ceiling(k.Value * statAmount));
                }
            }

            SetTooltip(GetEquippableTooltip());
        }

        public override object Clone()
        {
            // This is the only thing that needs to be deep copied.
            Dictionary<StatType, float> statsClone = null;
            if (this.statAllocatements != null)
            {
                statsClone = new Dictionary<StatType, float>(this.statAllocatements);
            }
            return new Equippable(this.Icon, this.Name, statsClone, this.GearModel, this.Slot, this.Slot2, this.ItemID);
        }

        protected virtual Tooltip GetEquippableTooltip()
        {
            List<TooltipLine> tiplines = new List<TooltipLine>
            {
                new TooltipLine(GetQualityColor(Quality), Name, .65f),
                new TooltipLine(Color.White, Slot.ToString(), .45f),
                new TooltipLine(Color.Gold, "Requires Level " + ItemLevel, .35f)
            };

            foreach (KeyValuePair<StatType, float> k in StatEffects)
            {
                if (k.Key == StatType.AttackSpeed || k.Key == StatType.CooldownReduction || k.Key == StatType.CritChance || k.Key == StatType.RunSpeed)
                {
                    tiplines.Add(new TooltipLine(Color.Green, "+" + Math.Ceiling(k.Value * 100) + "% " + k.Key.ToString(), .35f));
                }
                else
                {
                    tiplines.Add(new TooltipLine(Color.Green, "+" + k.Value + " " + k.Key.ToString(), .35f));
                }
            }

            return new Tooltip(tiplines);
        }

        /// <summary>
        /// Get the color associated with the quality
        /// </summary>
        public static Color GetQualityColor(GearQuality quality)
        {
            switch (quality)
            {
                case GearQuality.Good:
                    return Color.Blue;
                case GearQuality.Epic:
                    return Color.Purple;
                case GearQuality.Legendary:
                    return Color.Orange;
                default:
                    return Color.Green;
            }
        }
    }
}
