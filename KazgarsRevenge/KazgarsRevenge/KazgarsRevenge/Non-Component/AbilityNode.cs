using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge.Non_Component
{
    class AbilityNode
    {
        public AbilityName name { get; private set; }
        public AbilityName[] parents { get; private set; }
        public bool unlockable { get; private set; }
        public int cost { get; private set; }
        public Dictionary<AbilityName, bool> abilityLearnedFlags { get; private set; }

        public AbilityNode(AbilityName nameIn, AbilityName[] parentsIn, bool unlockableIn, int costIn, Dictionary<AbilityName, bool> abilityLearnedFlagsIn)
        {
            name = nameIn;
            parents = parentsIn;
            unlockable = unlockableIn;
            cost = costIn;
            abilityLearnedFlags = abilityLearnedFlagsIn;
        }

        public bool canUnlock()
        {
            if (unlockable) return true;
            else
            {
                foreach(AbilityName names in parents){
                    if (abilityLearnedFlags[names])
                    {
                        unlockable = true;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
