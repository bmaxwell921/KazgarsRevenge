using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Class used to store drops and weights.
    /// 
    /// How to use this class:
    ///     1) Figure out the relative drop rate for each item for an enemy type. Example:
    ///         i) Sword - 10
    ///         ii) Shoulders - 5
    ///         iii) Legs - 3
    ///         iv) null - 2 <- null means no drop
    ///         Meaning the ratio of drops is 10:5:3:2. This doesn't have to add up to 100 cause it's all relative
    ///     2) Call the add method
    ///     3) When you want to get what's dropped, just call GetDrops
    /// </summary>
    public class DropTable : Component, ICloneable
    {
        // The actual drops
        private IDictionary<ItemType, IList<Droption>> drops;

        // The count of the weights
        private IDictionary<ItemType, int> itemTypeTots;

        // Function to calculate added stats for gear
        private Func<int> addedStatFunc;

        /// <summary>
        /// Constructs a new empty drop table
        /// </summary>
        /// <param name="game"></param>
        /// <param name="entity"></param>
        public DropTable(KazgarsRevengeGame game, GameEntity entity, Func<int> addedStatFunc)
            : base(game, entity)
        {
            drops = new Dictionary<ItemType, IList<Droption>>();
            itemTypeTots = new Dictionary<ItemType, int>();
            this.addedStatFunc = addedStatFunc;
        }

        /// <summary>
        /// Hopefully we'll use this in the future with a DropTableUtil or something that
        /// reads their values from a file
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            DropTable ret = new DropTable(this.Game as KazgarsRevengeGame, this.Entity, this.addedStatFunc);
            ret.drops = new Dictionary<ItemType, IList<Droption>>(this.drops);
            ret.itemTypeTots = new Dictionary<ItemType, int>(this.itemTypeTots);
            return ret;
        }

        #region Setup
        /// <summary>
        /// Adds a drop of the given type to this drop table.
        /// Pass in null for the item to represent no drop
        /// </summary>
        /// <param name="type"></param>
        /// <param name="item"></param>
        /// <param name="weight"></param>
        /// <param name="maxAmt">Optional param for the maximum amount of the item to drop. Default is 1</param>
        public void AddDrop(ItemType type, Item item, int weight, int maxAmt = 1)
        {
            if (!drops.ContainsKey(type))
            {
                drops[type] = new List<Droption>();
                itemTypeTots[type] = 0;
            }
            IList<Droption> dropList = drops[type];
            dropList.Add(new Droption(item, weight, maxAmt));
            itemTypeTots[type] = itemTypeTots[type] + weight;
        }
        #endregion

        #region Getting Drops
        /// <summary>
        /// Gets drops from this drop table.
        /// </summary>
        /// <param name="floor">The floor the owning entity was killed on</param>
        /// <param name="killingEntity">The entity that killed the owning entity</param>
        /// <returns></returns>
        public IList<Item> GetDrops(FloorName floor, GameEntity killingEntity)
        {
            IList<Item> drops = new List<Item>();
            GetGoldDrops(drops, floor);
            GetGearDrops(drops, floor, killingEntity);
            GetPotionDrops(drops, floor);
            GetEssenceDrops(drops, floor);
            GetRecipeDrops(drops, floor);
            return drops;
        }

        #region Gold
        // Adds the gold drop
        private void GetGoldDrops(IList<Item> drops, FloorName floor)
        {
            float TRIP_PERC = 0.15f;
            int amount = GetNormGoldAmt(floor);

            // 15% chance to get double gold
            if (RandSingleton.U_Instance.NextDouble() < TRIP_PERC)
            {
                amount *= 3;
            }
            
            drops.Add(new Item(ItemType.Gold, Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Coins.FEW), "gold", amount));
        }

        // Returns the normal gold amount for a given floor
        private int GetNormGoldAmt(FloorName floor)
        {
            int floorAmtSq = floor.ToInt() * floor.ToInt();
            // Equation: (5 * level^2) +/- rand(0, level^2)
            return 5 * floorAmtSq + RandSingleton.U_Instance.Next(-floorAmtSq, floorAmtSq);
        }

        #endregion

        #region Gear
        // Gets the gear dropped
        private void GetGearDrops(IList<Item> drops, FloorName floor, GameEntity killingEntity)
        {
            // No drops :(
            if (!this.drops.ContainsKey(ItemType.Equippable))
            {
                return;
            }
            Equippable dropped = (Equippable) GetDropped(this.drops[ItemType.Equippable], itemTypeTots[ItemType.Equippable]);
            if (dropped != null)
            {
                SetGearStats(dropped, floor, killingEntity);
                drops.Add(dropped);
            }
        }

        // Sets the dropped gear's stats
        private void SetGearStats(Equippable dropped, FloorName floor, GameEntity killingEntity)
        {
            int baseStat = (floor.ToInt() - 1) * 10;
            
            // Delegates!
            baseStat += this.addedStatFunc();

            // TODO do we need additional stats here?
            Dictionary<StatType, float> itemStats = new Dictionary<StatType, float>();
            itemStats[StatType.Strength] = baseStat;
        }
        #endregion

        #region Consumables
        // Gets the consumable drops
        private void GetPotionDrops(IList<Item> drops, FloorName floor)
        {
            if (!this.drops.ContainsKey(ItemType.Potion))
            {
                return;
            }
            Item dropped = GetDropped(this.drops[ItemType.Potion], itemTypeTots[ItemType.Potion]);
            if (dropped != null)
            {
                drops.Add(dropped);
            }
        }
        #endregion

        #region Essence
        private void GetEssenceDrops(IList<Item> drops, FloorName floor)
        {
            // TODO anything else? - Set the strength or whatever
            if (!this.drops.ContainsKey(ItemType.Essence))
            {
                return;
            }

            Item dropped = GetDropped(this.drops[ItemType.Essence], itemTypeTots[ItemType.Essence]);
            if (dropped != null)
            {
                drops.Add(dropped);
            }
        }
        #endregion

        #region Recipe
        private void GetRecipeDrops(IList<Item> drops, FloorName floor)
        {
            // TODO anything else?
            if (!this.drops.ContainsKey(ItemType.Recipe))
            {
                return;
            }

            Item dropped = GetDropped(this.drops[ItemType.Recipe], itemTypeTots[ItemType.Recipe]);
            if (dropped != null)
            {
                drops.Add(dropped);
            }
        }
        #endregion

        private static Item GetDropped(IList<Droption> list, int total)
        {
            if (list.Count <= 0)
            {
                return null;
            }
            int val = RandSingleton.U_Instance.Next(total);
            int min = 0; int max = 0;

            for (int i = 0; i < list.Count; ++i)
            {
                min = max;
                max += list[i].weight;

                if (min <= val && val < max)
                {
                    // Do a clone so nothing gets messed with
                    if (list[i].item == null)
                    {
                        return null;
                    }
                    Item clone = (Item) list[i].item.Clone();
                    clone.Quantity = RandSingleton.U_Instance.Next(list[i].maxAmt) + 1;
                    return clone;
                }
            }

            throw new Exception("Oh god something went horribly wrong while getting the dropped gear!");
        }

        #region Delegate Options
        /// <summary>
        /// Method that should be used to calculate the additional stats on an item
        /// </summary>
        /// <returns></returns>
        public static int GetNormalAddItemLevel()
        {
            return diceRoll(1, 6);
        }

        /// <summary>
        /// Method that should be used to calculate the additional stats on an item
        /// </summary>
        /// <returns></returns>
        public static int GetEliteAddItemLevel()
        {
            return 3 + diceRoll(1, 5);
        }

        /// <summary>
        /// Method that should be used to calculate the additional stats on an item
        /// </summary>
        /// <returns></returns>
        public static int GetBossAddItemLevel()
        {
            return 10;
        }
        #endregion

        /// <summary>
        /// returns (rolls)d(sides)
        /// diceRoll(3, 6) is the result of 3d6
        /// </summary>
        /// <param name="rolls">how many dice to roll</param>
        /// <param name="sides">how many sides the dice have</param>
        /// <returns>the combines result</returns>
        public static int diceRoll(int rolls, int sides)
        {
            int ret = 0;
            for (int i = 0; i < rolls; ++i)
            {
                ret += RandSingleton.U_Instance.Next(1, sides + 1);
            }
            return ret;
        }

        #endregion

        /// <summary>
        /// Class used to box up a drop option and it's weight.
        /// The weight is related to it's probability
        /// </summary>
        private class Droption
        {
            // THE ITEM!
            public Item item;

            // THE WEIGHT!
            public int weight;

            // The max amount of the item to drop
            public int maxAmt;

            /// <summary>
            /// Creates a new Drop option with the given values
            /// </summary>
            /// <param name="item"></param>
            /// <param name="weight"></param>
            public Droption(Item item, int weight, int maxAmt)
            {
                this.item = item;
                this.weight = weight;
                this.maxAmt = maxAmt;
            }
        }
    }
}
