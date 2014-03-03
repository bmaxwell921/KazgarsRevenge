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
    ///         Meaning the ratio of drops is 10:5:3. This doesn't have to add up to 100 cause it's all relative
    ///     2) Call the add method
    ///     3) When you want to get what's dropped, just call GetDrops
    /// </summary>
    public class DropTable : Component
    {
        // Separate list for gear drops
        private IList<Droption<Equippable>> gearDrops;

        // Separate list for consumable drops
        private IList<Droption<Item>> consumableDrops;

        // Since users just pass in weights for the item drop we need to hold onto the total for our randoms
        private int consumableTotal;

        // Same idea as above
        private int gearTotal;

        public delegate int AddedStatFunc();

        private AddedStatFunc addedStatFunc;

        /// <summary>
        /// Constructs a new empty drop table
        /// </summary>
        /// <param name="game"></param>
        /// <param name="entity"></param>
        public DropTable(KazgarsRevengeGame game, GameEntity entity, AddedStatFunc addedStatFunc)
            : base(game, entity)
        {
            gearDrops = new List<Droption<Equippable>>();
            consumableDrops = new List<Droption<Item>>();
            this.addedStatFunc = addedStatFunc;
            consumableTotal = 0;
            gearTotal = 0;
        }

        #region Setup
        /// <summary>
        /// Adds the given item to this drop table. Don't bother setting any Item stats because
        /// they'll be overwritten
        /// </summary>
        /// <param name="item"></param>
        /// <param name="weight"></param>
        public void AddConsumableDrop(Item item, int weight)
        {
            consumableDrops.Add(new Droption<Item>(item, weight));

            // Update the total so it's in sync
            consumableTotal += weight;
        }

        /// <summary>
        /// Adds all of the given items and weights to this drop table.
        /// items and weights should have the same length
        /// </summary>
        /// <param name="items"></param>
        /// <param name="weights"></param>
        public void AddAllConsumableDrops(IList<Item> items, IList<int> weights)
        {
            if (items.Count != weights.Count)
            {
                throw new ArgumentException("Tried to add items and weights to a drop table, but the lengths weren't the same.");
            }

            for (int i = 0; i < items.Count; ++i)
            {
                AddConsumableDrop(items[i], weights[i]);
            }
        }

        /// <summary>
        /// Adds a new gear drop to this drop table.
        /// Don't bother setting the stats because they'll be overwritten, EVERYTHING ELSE SHOULD BE INITIALIZED
        /// </summary>
        /// <param name="item"></param>
        /// <param name="weight"></param>
        public void AddGearDrop(Equippable item, int weight)
        {
            gearDrops.Add(new Droption<Equippable>(item, weight));
            gearTotal += weight;
        }

        /// <summary>
        /// Same as AddAllConsumableDrops
        /// </summary>
        /// <param name="items"></param>
        /// <param name="weights"></param>
        public void AddAllGearDrops(IList<Equippable> items, IList<int> weights)
        {
            if (items.Count != weights.Count)
            {
                throw new ArgumentException("Tried to add equippables and weights to a drop table, but the lengths weren't the same.");
            }

            for (int i = 0; i < items.Count; ++i)
            {
                AddGearDrop(items[i], weights[i]);
            }
        }
        #endregion
        // TODO remember to set the weapon levels when returning drops

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
            // TODO
            GetGoldDrops(drops, floor);
            GetGearDrops(drops, floor, killingEntity);
            return drops;
        }

        // Adds the gold drop
        private void GetGoldDrops(IList<Item> drops, FloorName floor)
        {
            float TRIP_PERC = 0.15f;
            int amount = GetNormGoldAmt(floor);

            // 15% chance to get double gold
            if (RandSingleton.U_instance.NextDouble() < TRIP_PERC)
            {
                amount *= 3;
            }
            
            /*
             * TODO THIS LOAD SHOULD ABSOLUTELY NOT HAPPEN HERE. Images like this should be preloaded once and then passed around
             * to anyone who needs it
             */ 
            drops.Add(new Item(ItemType.Gold, Game.Content.Load<Texture2D>(@"Textures\UI\Items\gold1"), "gold", amount));
        }

        // Returns the normal gold amount for a given floor
        private int GetNormGoldAmt(FloorName floor)
        {
            int floorAmtSq = floor.ToInt() * floor.ToInt();
            // Equation: (5 * level^2) +/- rand(0, level^2)
            return 5 * floorAmtSq + RandSingleton.U_instance.Next(-floorAmtSq, floorAmtSq);
        }

        // Gets the gear dropped
        private void GetGearDrops(IList<Item> drops, FloorName floor, GameEntity killingEntity)
        {
            Equippable dropped = GetDroppedGear();
            SetGearStats(dropped, floor, killingEntity);
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
            //dropped.StatEffects = itemStats;

        }

        /// <summary>
        /// Method that should be used to calculate the additional stats on an item
        /// </summary>
        /// <returns></returns>
        public static int GetMinionAddItemLevel()
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
                ret += RandSingleton.U_instance.Next(1, sides + 1);
            }
            return ret;
        }

        private Equippable GetDroppedGear()
        {
            int val = RandSingleton.U_instance.Next(gearTotal);
            int min = 0; int max = 0;

            bool done = false;
            for (int i = 0; !done; ++i)
            {
                min = max;
                max += gearDrops[i].weight;

                if (min <= val && val < max)
                {
                    return gearDrops[i].item;
                }
            }

            throw new Exception("Oh god something went horribly wrong while getting the dropped gear!");
        }

        #endregion

        /// <summary>
        /// Class used to box up a drop option and it's weight.
        /// The weight is related to it's probability
        /// </summary>
        private class Droption<T> where T : Item
        {
            // THE ITEM!
            public T item;

            // THE WEIGHT!
            public int weight;

            /// <summary>
            /// Creates a new Drop option with the given values
            /// </summary>
            /// <param name="item"></param>
            /// <param name="weight"></param>
            public Droption(T item, int weight)
            {
                this.item = item;
                this.weight = weight;
            }
        }
    }
}
