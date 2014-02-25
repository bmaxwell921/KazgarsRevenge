using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    public class RandSingleton
    {
        // Teh instance!
        private static Random instance;

        public static Random Instance
        {
            get
            {
                if (instance == null)
                {
                    // TODO make it so the seed can be set elsewhere? Or just remove it so they're not always the same
                    instance = new Random(42);
                }

                return instance;
            }
        }
    }
}
