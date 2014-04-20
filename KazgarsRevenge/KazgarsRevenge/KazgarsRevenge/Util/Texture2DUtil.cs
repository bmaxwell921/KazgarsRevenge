using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
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

        /// <summary>
        /// Gets the requested texture rotated by the specified multiple of 90 degrees.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="rot"></param>
        /// <param name="graphics"></param>
        /// <returns></returns>
        public Texture2D GetRotatedTexture(string name, Rotation rot, GraphicsDevice graphics)
        {
            if (rot == Rotation.ZERO)
            {
                return GetTexture(name);
            }

            //check for cached version of rotated texture
            if (!textures.ContainsKey(name + rot.ToDegrees()))
            {
                //do a thread-safe load of the rotated image
                Texture2D originalTex = GetTexture(name);
                mutex.WaitOne();
                try
                {
                    textures[name + rot.ToDegrees()] = RotateTexture(originalTex, rot, graphics);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }

            }
            return textures[name + rot.ToDegrees()];
        }

        /// <summary>
        /// assumes the image is an even number of pixels wide/tall, rotate multiple of 90 degrees COUNTERclockwise
        /// </summary>
        private Texture2D RotateTexture(Texture2D originalTex, Rotation rot, GraphicsDevice graphics)
        {
            Color[] colors = new Color[originalTex.Width * originalTex.Height];
            originalTex.GetData(colors);

            int w = originalTex.Width;
            int h = originalTex.Height;

            int xul = 0;
            int yul = 0;

            int xur = w - 1;
            int yur = 0;

            int xbr = w - 1;
            int ybr = h - 1;

            int xbl = 0;
            int ybl = h - 1;


            for (int x = 0; x < w / 2; ++x)
            {
                for (int y = 0; y < h / 2; ++y)
                {
                    ++xul;
                    ++yur;
                    --xbr;
                    --ybl;

                    Color tmpur;
                    Color tmpbr;
                    Color tmpbl;
                    switch (rot)
                    {
                        case Rotation.NINETY:
                            //save current bottom left and bottom right pixels
                            tmpbl = colors[xbl + ybl * w];
                            tmpbr = colors[xbr + ybr * w];
                            //set bottom left to upper left pixel
                            colors[xbl + ybl * w] = colors[xul + yul * w];
                            //set upper left to upper right pixel
                            colors[xul + yul * w] = colors[xur + yur * w];
                            //set upper right to saved bottom right pixel
                            colors[xur + yur * w] = tmpbr;
                            //set bottom right to saved bottom left pixel
                            colors[xbr + ybr * w] = tmpbl;
                            break;
                        case Rotation.ONE_EIGHTY:
                            //save current bottom right and bottom left pixels
                            tmpbr = colors[xbr + ybr * w];
                            tmpbl = colors[xbl + ybl * w];
                            //set bottom right to upper left pixel
                            colors[xbr + ybr * w] = colors[xul + yul * w];
                            //set bottom left to upper right pixel
                            colors[xbl + ybl * w] = colors[xur + yur * w];
                            //set upper left to saved bottom right pixel's old color
                            colors[xul + yul * w] = tmpbr;
                            //set upper right to saved bottom left pixel's old color
                            colors[xur + yur * w] = tmpbl;
                            break;
                        case Rotation.TWO_SEVENTY:
                            //save current upper right and bottom left pixels
                            tmpur = colors[xur + yur * w];
                            tmpbl = colors[xbl + ybl * w];
                            //set upper right to upper left pixel
                            colors[xur + yur * w] = colors[xul + yul * w];
                            //set bottom left to bottom right pixel
                            colors[xbl + ybl * w] = colors[xbr + ybr * w];
                            //set upper left to saved bottom left pixel's old color
                            colors[xul + yul * w] = tmpbl;
                            //set bottom right to saved upper right pixel's old color
                            colors[xbr + ybr * w] = tmpur;
                            break;
                    }
                }
                ++yul;
                --xur;
                --ybr;
                ++xbl;
                xul = 0;
                yur = 0;
                xbr = w - 1;
                ybl = h - 1;
            }

            Texture2D retTex = new Texture2D(graphics, originalTex.Width, originalTex.Height);
            retTex.SetData(colors);
            return retTex;
        }

    }
}
