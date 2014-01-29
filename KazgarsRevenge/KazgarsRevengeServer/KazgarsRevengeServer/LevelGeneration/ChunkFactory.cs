using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KazgarsRevenge;

namespace KazgarsRevengeServer.LevelGeneration
{
    /// <summary>
    /// Class responsible for getting instances of ChunkInfos. Uses Factory pattern...and Singleton pattern...cause i like it
    /// </summary>
    public class ChunkFactory
    {
        private LoggerManager lm;

        // Singleton instance
        private static ChunkFactory instance;

        #region Singleton methods
        private ChunkFactory()
        {
            allChunks = new Dictionary<ChunkType, ISet<ChunkInfo>>();
        }

        public static ChunkFactory getInstance()
        {
            if (instance == null)
            {
                instance = new ChunkFactory();
            }
            return instance;
        }

        #endregion

        private IDictionary<ChunkType, ISet<ChunkInfo>> allChunks;

        /// <summary>
        /// Adds all of the given chunkInfos to this factory. All of the given infos must 
        /// correspond to the given type
        /// </summary>
        /// <param name="type">The type of chunk that all the chunkInfos represent</param>
        /// <param name="chunkInfos"></param>
        public void addChunksOfType(ChunkType type, params ChunkInfo[] chunkInfos)
        {
            ISet<ChunkInfo> chunks;
            // Stupid c#

            // Add to the infos if any exist for this type
            allChunks.TryGetValue(type, out chunks);
            if (chunks == null)
            {
                // create a new one if this is the first type
                chunks = new HashSet<ChunkInfo>();
            }

            foreach (ChunkInfo chunkInfo in chunkInfos)
            {
                chunks.Add(chunkInfo);
            }

            allChunks[type] = chunks;
        }

        /// <summary>
        /// Add a logger for this Factory
        /// </summary>
        /// <param name="lm"></param>
        public void registerLogger(LoggerManager lm)
        {
            this.lm = lm;
        }

        /// <summary>
        /// Returns a list of all known Chunks that have doors in the necessary locations
        /// </summary>
        /// <param name="type"></param>
        /// <param name="reqDirs">The required door locations</param>
        /// <returns></returns>
        public IList<ChunkInfo> getPossibleChunks(ChunkType type, ISet<Direction> reqDirs)
        {
            IList<ChunkInfo> possibilities = new List<ChunkInfo>();

            if (!allChunks.ContainsKey(type))
            {
                lm.Log(Level.DEBUG, String.Format("No known ChunkInfos for type: {0}", type));
                return possibilities;
            }

            foreach (ChunkInfo info in allChunks[type])
            {
                if (info.satisfiesDoorRequirement(reqDirs))
                {
                    possibilities.Add(info);
                }
            }

            return possibilities;
        }
    }
}
