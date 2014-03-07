using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    public class RandSingleton
    {
        // The seeded instance
        private static Random s_instance;

        /// <summary>
        /// I put this distinction between a seeded instance and a non seeded instance.
        /// If we allow users to put in a seed it should only affect the level generation;
        /// loot and enemy levels shouldn't be affected by the seed so you can still have 
        /// a unique play through.
        /// </summary>
        public static Random S_Instance
        {
            get
            {
                if (s_instance == null)
                {
                    // TODO make it so the seed can be set elsewhere? Or just remove it so they're not always the same
                    s_instance = new Random(42);
                }

                return s_instance;
            }
        }

        // The unseeded instance
        private static Random u_instance;

        /// <summary>
        /// This is the random instance that should be used for loot and enemy levels
        /// and stuff that shouldn't be the same when a seed is given
        /// </summary>
        public static Random U_Instance
        {
            get
            {
                if (u_instance == null)
                {
                    u_instance = new Random();
                }
                return u_instance;
            }
        }
    }
}
