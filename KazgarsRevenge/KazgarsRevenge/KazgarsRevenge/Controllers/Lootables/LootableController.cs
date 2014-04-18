using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;


namespace KazgarsRevenge
{
    abstract public class LootableController : AIComponent
    {
        protected List<Item> loot;
        public List<Item> Loot
        {
            get
            {
                return loot;
            }
        }
        public LootableController(KazgarsRevengeGame game, GameEntity entity, List<Item> loot)
            : base(game, entity)
        {
            this.loot = loot;
        }

        public abstract void OpenLoot(Vector3 position, Quaternion q);
        public abstract void CloseLoot();
        public abstract void StartSpin();
        public abstract bool CanLoot();

        public Item GetLoot(int lootIndex)
        {
            if (loot.Count - 1 < lootIndex)
            {
                //that loot doesn't exist
                return null;
            }

            return loot[lootIndex];
        }
        public void RemoveLoot(int lootIndex)
        {
            if (loot.Count - 1 >= lootIndex)
            {
                loot.RemoveAt(lootIndex);
            }
        }


        public bool RequestLootFromServer(int i)
        {
            return true;
        }
    }
}
