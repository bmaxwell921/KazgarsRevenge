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

        public Ability(AbilityName abilityName, Texture2D iconIn, float cooldownMillis, AttackType typeIn, string actionName, AbilityType abilityType, int powerCost)
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

            this.Tooltip = new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "default ability tooltip", 1) });
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

