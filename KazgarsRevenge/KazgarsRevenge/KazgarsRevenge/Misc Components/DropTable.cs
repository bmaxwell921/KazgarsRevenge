using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    /// <summary>
    /// Class used to store drops and weights 
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

        /// <summary>
        /// Constructs a new empty drop table
        /// </summary>
        /// <param name="game"></param>
        /// <param name="entity"></param>
        public DropTable(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {
            gearDrops = new List<Droption<Equippable>>();
            consumableDrops = new List<Droption<Item>>();

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
        /// Don't bother setting the stats because they'll be overwritten
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

        /// <summary>
        /// Gets drops from this drop table.
        /// </summary>
        /// <param name="floor">The floor the owning entity was killed on</param>
        /// <param name="killingEntity">The entity that killed the owning entity</param>
        /// <returns></returns>
        public IList<Item> getDrops(FloorName floor, GameEntity killingEntity)
        {
            IList<Item> drops = new List<Item>();
            // TODO
            return drops;
        }

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
