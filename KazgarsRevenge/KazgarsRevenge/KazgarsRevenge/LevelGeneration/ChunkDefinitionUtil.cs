using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace KazgarsRevenge
{
    /// <summary>
    /// Class that reads all the defined chunks and provides methods
    /// to help level generation
    /// </summary>
    public class ChunkDefinitionUtil
    {
        // Where all the ChunkDefinitions should be
        public static readonly string CHUNK_DEF_PATH = "./Chunks";
        public static readonly string CHUNK_EXT = "json";
        public static readonly char CHUNK_NAME_DELIMIT = ':';

        // Mapping from number of doors to ChunkInfo. Done like this for efficient look ups
        private IDictionary<int, IList<ChunkInfo>> chunkDefs;

        public ChunkDefinitionUtil()
        {
            chunkDefs = new Dictionary<int, IList<ChunkInfo>>();
            ReadChunkNames();
        }

        private void ReadChunkNames()
        {
            string[] chunkNames = Directory.GetFiles(CHUNK_DEF_PATH, "*." + CHUNK_EXT).Select(path => Path.GetFileName(path)).ToArray();
            foreach (string chunkName in chunkNames)
            {
                string message;
                IList<ChunkInfo> infos = parseName(chunkName, out message);
                if (infos == null)
                {
                    Console.WriteLine(message);
                }
                foreach (ChunkInfo ci in infos)
                {
                    AddChunkInfo(ci);
                }
            }
        }

        private void AddChunkInfo(ChunkInfo info)
        {
            // Classic map add with datastructure values
            if (!chunkDefs.ContainsKey(info.numDoors()))
            {
                IList<ChunkInfo> infos = new List<ChunkInfo>();
                infos.Add(info);
                chunkDefs[info.numDoors()] = infos;
                return;
            }

            chunkDefs[info.numDoors()].Add(info);
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
            string[] parts = chunkName.Split(CHUNK_NAME_DELIMIT);

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
                    dirs.Add(Extenders.DirectionFromChar(c));
                }

                return GetRotations(id, chunkType, dirs);
            }
            catch (Exception e)
            {
                message = e.Message;
                return null;
            }
        }

        // Gets all of the possible rotations for a given chunk
        private IList<ChunkInfo> GetRotations(int id, ChunkType chunkType, ISet<Direction> dirs)
        {
            IList<ChunkInfo> chunks = new List<ChunkInfo>();
            foreach (Rotation rotation in Enum.GetValues(typeof(Rotation)))
            {
                ISet<Direction> directions = new HashSet<Direction>();
                foreach (Direction dir in dirs)
                {
                    directions.Add(Extenders.RotateTo(dir, rotation));
                }
                chunks.Add(new ChunkInfo(id, rotation, chunkType, directions));
            }
            return chunks;
        }
    }
}
