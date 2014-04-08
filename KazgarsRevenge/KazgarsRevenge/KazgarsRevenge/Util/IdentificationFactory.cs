using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    /// <summary>
    /// Class used to get Ids for entities
    /// </summary>
    public class IdentificationFactory
    {
        // TODO the enemy manager doesn't like it that different enemy types can have the same id

        // Static ints used to generate ids
        private static IDictionary<EntityType, int> ids = new Dictionary<EntityType, int>();

        private static int id = 0;

        private static readonly int START_ID = 0;

        // Used to get a new unique Id for an EntityType
        public static Identification getId(EntityType type, int clientId)
        {
            //if (!ids.ContainsKey(type))
            //{
            //    ids.Add(type, START_ID);
            //}
            //return new Identification(ids[type]++, clientId);
            return new Identification(id++, clientId);
        }

        // Gets the value of the id to be returned by the next call to getId
        public static int getCurrentVal(EntityType type)
        {
            //if (!ids.ContainsKey(type))
            //{
            //    return START_ID;
            //}
            //return ids[type];
            return id;
        }

        // Updates the next id value for use for the given type. This method should be used if a different client has generated a higher id for an entity type
        public static void updateId(EntityType type, int nextVal)
        {
            //ids[type] = nextVal;
            id = nextVal;
        }

        /// <summary>
        /// Resets all ids
        /// </summary>
        public static void Reset()
        {
            //ids.Clear();
            id = START_ID;
        }
    }
}
