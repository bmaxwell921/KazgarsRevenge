using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public enum AbilityType
    {
        Instant,
        Charge,
        GroundTarget,
        Passive,
    }
    public class Ability
    {
        public string ActionName { get; private set; }
        public Texture2D icon { get; private set; }
        public AttackType PrimaryType { get; private set; }
        public AbilityType AbilityType { get; private set; }
        public AbilityName AbilityName { get; private set; }
        public int PowerCost { get; private set; }
        public Tooltip Tooltip { get; private set; }

        public double cooldownMillisLength;
        public double cooldownMillisRemaining;
        public bool onCooldown { get; private set; }

        public Ability(AbilityName abilityName, Texture2D iconIn, float cooldownMillis, AttackType typeIn, string actionName, AbilityType abilityType, int powerCost, string descriptionText)
        {   
            icon = iconIn;
            cooldownMillisLength = cooldownMillis;
            PrimaryType = typeIn;
            cooldownMillisRemaining = cooldownMillisLength;
            this.ActionName = actionName;
            onCooldown = true;
            this.AbilityType = abilityType;
            this.AbilityName = abilityName;
            this.PowerCost = powerCost;

            if (abilityName != KazgarsRevenge.AbilityName.None)
            {
                List<TooltipLine> ttlines = new List<TooltipLine>();
                string n = abilityName.ToString();
                for (int i = 0; i < n.Length; ++i)
                {
                    if (i > 0 && char.IsUpper(n[i]))
                    {
                        n = n.Insert(i, " ");
                        ++i;
                    }
                }
                ttlines.Add(new TooltipLine(Color.White, n, .75f));
                if (PowerCost > 0)
                {
                    ttlines.Add(new TooltipLine(Color.White, PowerCost + " Power", .5f));
                }
                if (cooldownMillisLength > 0)
                {
                    ttlines.Add(new TooltipLine(Color.White, Math.Round(cooldownMillisLength / 1000.0f, 2) + " sec. cooldown", .4f));
                }
                if (PrimaryType != AttackType.None)
                {
                    ttlines.Add(new TooltipLine(Color.White, "Requires " + PrimaryType.ToString() + " Weapon", .4f));
                }
                ttlines.Add(new TooltipLine(Color.Gold, descriptionText, .4f));

                this.Tooltip = new Tooltip(ttlines);
            }
        }

        public void ResetCooldown()
        {
            onCooldown = false;
        }

        public void Use()
        {
            cooldownMillisRemaining = cooldownMillisLength;
            onCooldown = true;
        }

        public void update(double elapsed)
        {
            if (onCooldown)
            {
                cooldownMillisRemaining -= elapsed;
                if (cooldownMillisRemaining <= 0)
                {
                    onCooldown = false;
                }
            }
        }
    }
}

