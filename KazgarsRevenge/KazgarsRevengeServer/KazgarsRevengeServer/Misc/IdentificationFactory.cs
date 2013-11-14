using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KazgarsRevenge;

namespace KazgarsRevengeServer
{
    /// <summary>
    /// Convenience class to generate new Ids
    /// </summary>
    class IdentificationFactory
    {
        static byte lastId = 0;

        public static Identification GenerateNextId()
        {
            return new Identification(lastId++);
        }
    }
}
