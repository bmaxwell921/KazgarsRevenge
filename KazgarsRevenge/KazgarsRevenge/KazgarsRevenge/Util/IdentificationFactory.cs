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
        // Static ints used to generate ids
        private static IDictionary<EntityType, int> ids = new Dictionary<EntityType, int>();

        private static readonly int START_ID = 0;

        // Used to get a new unique Id for an EntityType
        public static Identification getId(EntityType type)
        {
            if (!ids.ContainsKey(type))
            {
                ids.Add(type, START_ID);
            }
            return new Identification(ids[type]++);
        }

        // Gets the value of the id to be returned by the next call to getId
        public static int getCurrentVal(EntityType type)
        {
            if (!ids.ContainsKey(type))
            {
                return START_ID;
            }
            return ids[type];
        }

        // Updates the next id value for use for the given type. This method should be used if a different client has generated a higher id for an entity type
        public static void updateId(EntityType type, int nextVal)
        {
            ids[type] = nextVal;
        }
    }
}
