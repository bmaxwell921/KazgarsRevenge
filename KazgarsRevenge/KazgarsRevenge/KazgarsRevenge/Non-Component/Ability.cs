using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    class Ability
    {
        public string AniName { get; private set; }


        int abilityLevel;
        int levelLearned;
        public Texture2D icon { get; private set; }
        public float cooldownSeconds;
        float lastUsed = -300f; //-5 minutes
        public bool onCooldown = false;
        public float timeRemaining;
        String tooltip = "N/A";
        AttackType type;

        public Ability(int abilityLevelIn, int levelLearnedIn, Texture2D iconIn, float cooldownSecondsIn, AttackType typeIn, string aniName)
        {   
            abilityLevel = abilityLevelIn;
            levelLearned = levelLearnedIn;
            icon = iconIn;
            cooldownSeconds = cooldownSecondsIn;
            type = typeIn;
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

