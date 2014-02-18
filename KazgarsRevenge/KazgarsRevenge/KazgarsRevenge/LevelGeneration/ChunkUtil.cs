using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace KazgarsRevenge
{
    /// <summary>
    /// Class that reads all the defined chunks and provides methods
    /// to help level generation
    /// </summary>
    public class ChunkUtil
    {
        #region Singleton Stuff
        private static ChunkUtil instance;

        public static ChunkUtil Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ChunkUtil();
                }
                return instance;
            }
        }
        #endregion

        // Where all the ChunkDefinitions should be
        public static readonly string CHUNK_DEF_PATH = "./Chunks";

        // Mapping from number of doors to ChunkInfo. Done like this for 'efficient' look ups
        private IDictionary<ChunkType, IDictionary<int, IList<ChunkInfo>>> chunkDefs;

        // A cache of chunks so we don't always have to read chunks
        private IDictionary<string, Chunk> chunkCache;

        private ChunkUtil()
        {
            chunkDefs = new Dictionary<ChunkType, IDictionary<int, IList<ChunkInfo>>>();
            chunkCache = new Dictionary<string, Chunk>();
            ReadChunkNames();
        }

        #region ChunkName Reading
        // Reads and parses all the known chunks
        public void ReadChunkNames()
        {
            string[] chunkNames = Directory.GetFiles(CHUNK_DEF_PATH, "*." + ChunkInfo.CHUNK_EXT).Select(path => Path.GetFileName(path)).ToArray();
            foreach (string chunkName in chunkNames)
            {
                string extensionLessName = Path.GetFileNameWithoutExtension(chunkName);
                string message;
                IList<ChunkInfo> infos = parseName(extensionLessName, out message);
                if (infos == null)
                {
                    Console.WriteLine(message);
                    continue;
                }
                foreach (ChunkInfo ci in infos)
                {
                    AddChunkInfo(ci);
                }
            }
        }

        // Adds the given ChunkInfo to this Utility
        private void AddChunkInfo(ChunkInfo info)
        {
            // first, get the right dictionary for the type
            IDictionary<int, IList<ChunkInfo>> typeDic;
            if (!chunkDefs.TryGetValue(info.chunkType, out typeDic))
            {
                // creating a new one if we need to
                typeDic = new Dictionary<int, IList<ChunkInfo>>();
            }

            // then get the list of chunkInfos for the new chunkInfo's number of doors
            IList<ChunkInfo> infos;
            if (!typeDic.TryGetValue(info.numDoors(), out infos))
            {
                // again, creating a new list of we need to
                infos = new List<ChunkInfo>();
            }

            // add 'em  back in
            infos.Add(info);
            typeDic[info.numDoors()] = infos;
            chunkDefs[info.chunkType] = typeDic;
        }

        /* Converts the given chunkName into its ChunkInfo equivalent
         * ChunkNames have the form:
         *      <id>:<type>:<doorLocations>
         * Example:
         *      0:H:NSEW
         */ 
            
        private IList<ChunkInfo> parseName(string chunkName, out string message)
        {
            int VALID_COUNT = 3;
            // parse it into its 3 parts
            string[] parts = chunkName.Split(ChunkInfo.CHUNK_NAME_DELIMIT);

            if (parts.Count() != VALID_COUNT)
            {
                message = "Invalid count for the chunkName";
                return null;
            }

            try
            {
                int id = Convert.ToInt32(parts[0]);
                if (parts[1].Count() > 1)
                {
                    message = "Invalid count for chunkName type";
                    return null;
                }

                ChunkType chunkType = Extenders.ChunkTypeFromChar(parts[1].ToCharArray()[0]);

                message = "";
                ISet<Direction> dirs = new HashSet<Direction>();
                foreach (char c in parts[2])
                {
                    dirs.Add(Extenders.FromChar(c));
                }

                return GetRotations(chunkName, id, chunkType, dirs);
            }
            catch (Exception e)
            {
                message = e.Message;
                return null;
            }
        }

        // Gets all of the possible rotations for a given chunk
        private IList<ChunkInfo> GetRotations(string chunkName, int id, ChunkType chunkType, ISet<Direction> dirs)
        {
            IList<ChunkInfo> chunks = new List<ChunkInfo>();
            foreach (Rotation rotation in Enum.GetValues(typeof(Rotation)))
            {
                ISet<Direction> directions = new HashSet<Direction>();
                foreach (Direction dir in dirs)
                {
                    directions.Add(Extenders.RotateTo(dir, rotation));
                }
                chunks.Add(new ChunkInfo(chunkName, id, rotation, chunkType, directions));
            }
            return chunks;
        }
        #endregion

        #region Level Generation Methods
        /// <summary>
        /// Gets a random ChunkInfo that satisfies the provided requirements
        /// </summary>
        /// <param name="level">The 'level' which the Chunk will be placed in</param>
        /// <param name="type">The type of chunk to get</param>
        /// <param name="reqDoors">Where doors need to be</param>
        /// <returns></returns>
        public ChunkInfo GetSatisfyingChunk(FloorName floor, ChunkType type, ISet<Direction> reqDoors)
        {
            // TODO incorporate the floor somehow
            IList<ChunkInfo> rightDoorCount = chunkDefs[type][reqDoors.Count()];
            IList<ChunkInfo> possibleChunks = new List<ChunkInfo>();

            foreach (ChunkInfo ci in rightDoorCount)
            {
                // Allow only those who satisfy the requirements
                if (ci.satisfiesDoorRequirement(reqDoors))
                {
                    possibleChunks.Add(ci);
                }
            }
            return possibleChunks[RandSingleton.Instance.Next(possibleChunks.Count)];
        }

        // Home chunks must have doors on all sides
        public ChunkInfo GetSoulevatorChunk(FloorName name)
        {
            ISet<Direction> allDirs = new HashSet<Direction>();
            allDirs.Add(Direction.NORTH);
            allDirs.Add(Direction.SOUTH);
            allDirs.Add(Direction.EAST);
            allDirs.Add(Direction.WEST);
            return GetSatisfyingChunk(name, ChunkType.SOULEVATOR, allDirs);
        }

        /// <summary>
        /// Reads the given chunkInfo from file and returns the concrete object
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public Chunk ReadChunk(ChunkInfo chunk)
        {
            if (chunkCache.ContainsKey(chunk.GetFileName()))
            {
                return (Chunk) chunkCache[chunk.GetFileName()].Clone();
            }
            try
            {
                String chunkJson;
                using (StreamReader sr = new StreamReader(Path.Combine(CHUNK_DEF_PATH, chunk.GetFileName())))
                {
                    chunkJson = sr.ReadToEnd();
                }
                Chunk deser = JsonConvert.DeserializeObject<Chunk>(chunkJson);
                // So we can place things properly
                deser.CalcRoomWidthHeight();
                chunkCache[chunk.GetFileName()] = deser;
                return deser;
            }
            catch (Exception e)
            {
                Console.WriteLine("Can't read file: {0}", e.Message);
                return null;
            }
        }

        #endregion


    }
}
