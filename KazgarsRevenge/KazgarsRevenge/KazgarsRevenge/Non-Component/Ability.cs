using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    public delegate void AbilityCallback(Vector3 position, Vector3 orientation, AliveComponent creator);
    public class Ability
    {
        public string AniName { get; private set; }
        public Texture2D icon { get; private set; }
        public AttackType Type { get; private set; }
        public AbilityCallback AbilityCreator;

        int abilityLevel;
        public float cooldownSeconds;
        float lastUsed = -300f; //-5 minutes
        public bool onCooldown = false;
        public float timeRemaining;
        String tooltip = "N/A";

        public Ability(int abilityLevelIn, Texture2D iconIn, float cooldownSecondsIn, AttackType typeIn, string aniName, AbilityCallback toCall)
        {   
            abilityLevel = abilityLevelIn;
            icon = iconIn;
            cooldownSeconds = cooldownSecondsIn;
            Type = typeIn;
            timeRemaining = cooldownSeconds;
            this.AniName = aniName;
        }

        public void setToolTip(String toolTipString)
        {
            tooltip = toolTipString;
        }

        public bool tryUse(float currentTime)
        {
            if (currentTime >= lastUsed + cooldownSeconds)
            {
                lastUsed = currentTime;
                timeRemaining = cooldownSeconds;
                onCooldown = true;
                return true;
            }
            return false;
        }

        public void update(float currentTime)
        {
            if (currentTime < lastUsed + cooldownSeconds)
            {
                timeRemaining = (lastUsed + cooldownSeconds) - currentTime;
            }
            else
            {
                onCooldown = false;
            }
        }
    }
}

