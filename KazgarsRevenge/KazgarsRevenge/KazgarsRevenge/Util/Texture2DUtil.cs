using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Threading;

namespace KazgarsRevenge
{
    /// <summary>
    /// Utility responsible for handling loading and 
    /// distributing Texture2D assets. Singleton cause it's the best.
    /// 
    /// TODO could implement this as LRU if we need to worry about memory
    /// </summary>
    public class Texture2DUtil
    {

        #region Singleton Stuff
        private static Texture2DUtil instance;

        public static Texture2DUtil Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Texture2DUtil();
                }
                return instance;
            }
        }

        private Texture2DUtil()
        {
            this.textures = new Dictionary<string, Texture2D>();
            mutex = new Mutex();
        }
        #endregion

        // All the loaded textures
        private IDictionary<string, Texture2D> textures;

        private ContentManager cm;

        // Just in case we need to multi-thread it
        private Mutex mutex;

        /// <summary>
        /// Sets the content manager for the Util to use when loading
        /// </summary>
        /// <param name="content"></param>
        public void SetContent(ContentManager content)
        {
            cm = content;
        }
        /// <summary>
        /// Loads textures that are most likely to be used - abilities, coins, potions
        /// </summary>
        public void PreemptiveLoad()
        {
            PerformLoad(TextureStrings.UI.Items.Coins.FEW);
            PerformLoad(TextureStrings.UI.Items.Coins.SOME);
            PerformLoad(TextureStrings.UI.Items.Coins.LOTS);
            PerformLoad(TextureStrings.UI.Items.Potions.HEALTH);
        }
        
        // Performs a thread safe load
        private void PerformLoad(string toLoad)
        {
            mutex.WaitOne();
            try
            {
                textures[toLoad] = cm.Load<Texture2D>(toLoad);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Gets the requested texture. The name should come from the TextureStrings class
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Texture2D GetTexture(string name)
        {
            if (!textures.ContainsKey(name))
            {
                PerformLoad(name);
            }
            return textures[name];
        }
    }
}
