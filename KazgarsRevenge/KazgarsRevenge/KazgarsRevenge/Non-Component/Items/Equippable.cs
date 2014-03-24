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
        Undecided,
        Standard,
        Good,
        Epic,
        Legendary,
    }

    public class Equippable : Item
    {
        public GearQuality Quality { get; protected set; }
        public Dictionary<StatType, float> StatEffects { get; protected set; }
        public Model GearModel { get; private set; }
        public GearSlot Slot { get; private set; }
        public GearSlot Slot2 { get; private set; }
        public Equippable(Texture2D icon, string name, Dictionary<StatType, float> statEffects, Model gearModel, GearSlot slot, GearSlot secondSlot, int id)
            : base(ItemType.Equippable, icon, name, 1, id)
        {
            this.StatEffects = statEffects;
            this.GearModel = gearModel;
            this.Slot = slot;
            this.Slot2 = secondSlot;
            this.Quality = GearQuality.Undecided;
            SetTooltip(new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "default tooltip", 1) }));
        }

        /// <summary>
        /// must be called before gear is available to character (sets quality, and the tooltip based on that)
        /// </summary>
        /// <param name="quality"></param>
        public void SetQuality(GearQuality quality)
        {
            this.Quality = quality;
            SetTooltip(GetEquippableTooltip());
        }

        public override object Clone()
        {
            // This is the only thing that needs to be deep copied.
            Dictionary<StatType, float> statsClone = null;
            if (this.StatEffects != null)
            {
                statsClone = new Dictionary<StatType, float>(this.StatEffects);
            }
            return new Equippable(this.Icon, this.Name, statsClone, this.GearModel, this.Slot, this.Slot2, this.ItemID);
        }

        private Tooltip GetEquippableTooltip()
        {
            return new Tooltip(
                new List<TooltipLine>{
                new TooltipLine(GetQualityColor(Quality), Name, 1),
                //new TooltipLine(Color.White,
                });
        }

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
