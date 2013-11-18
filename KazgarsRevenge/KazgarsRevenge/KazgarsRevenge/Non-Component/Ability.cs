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
        int abilityLevel;
        int levelLearned;
        public Texture2D icon { get; private set; }
        float cooldownSeconds;
        String tooltip = "N/A";
        AttackType type;

        public Ability(int abilityLevelIn, int levelLearnedIn, Texture2D iconIn, float cooldownSecondsIn, AttackType typeIn)
        {
            abilityLevel = abilityLevelIn;
            levelLearned = levelLearnedIn;
            icon = iconIn;
            cooldownSeconds = cooldownSecondsIn;
            type = typeIn;
        }

        public void setToolTip(String toolTipString)
        {
            tooltip = toolTipString;
        }
    }
}

