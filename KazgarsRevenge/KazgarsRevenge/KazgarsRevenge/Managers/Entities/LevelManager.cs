using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.MathExtensions;
using BEPUphysics.Collidables;
using BEPUphysics.DataStructures;
using SkinnedModelLib;
using Newtonsoft.Json;

namespace KazgarsRevenge
{
    class RoomData
    {
        public string filename;
        public float x;
        public float z;
        public float yaw;
        public RoomData(string filename, float x, float z, float yaw)
        {
            this.filename=filename;
            this.x=x;
            this.z=z;
            this.yaw=yaw;
        }
    }

    public enum FloorName
    {
        Dungeon=1,
        Library=2,
        TortureChamber=3,
        Lab=4,
        GrandHall=5,
    }

    public class LevelManager : EntityManager
    {
        #region LevelGeneration Stuff
        #endregion

        public FloorName CurrentFloor { get; private set; }

        List<GameEntity> rooms = new List<GameEntity>();
        public LevelManager(KazgarsRevengeGame game)
            : base(game)
        {
            CurrentFloor = FloorName.Dungeon;
            new LevelBuilder().BuildLevel(CurrentFloor);
        }

        public void DemoLevel()
        {
            CreateRoom("Models\\Levels\\tempChunk3", new Vector3(200, 0, -200), MathHelper.PiOver2);
            //CreateChunk("Dungeon1", new Vector3(120, 0, -200), 0);
        }

        public void CreateLevel(FloorName level)
        {
            switch (level)
            {
                default:
                case FloorName.Dungeon:

                    break;
            }
        }

        Dictionary<string, List<RoomData>> chunkDefinitions = new Dictionary<string, List<RoomData>>();
        string levelPath = "Models\\Levels\\";
        public void ReadFile(FloorName floor)
        {
            chunkDefinitions.Clear();

            string filetext;
            try
            {
                using (StreamReader sr = new StreamReader(floor.ToString() + ".json"))
                {
                    filetext = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't read the level file.\n" + e.Message);
            }



            /*
            string[] lines = filetext.Split(new char[] { '\n', '\r' });
            List<RoomData> chunkDef = new List<RoomData>();

            string chunkId = "";
            for (int i = 0; i < lines.Length; ++i)
            {
                string[] tokens = lines[i].Split(new char[] { ' ' });
                if (tokens[0] == "CHUNK")
                {
                    //done with last chunk, defining new chunk
                    if (chunkDef.Count > 0)
                    {
                        chunkDefinitions.Add(floorName + chunkId, chunkDef);
                    }
                    chunkId = tokens[1];
                    chunkDef = new List<RoomData>();
                }

                else if (tokens.Length == 4)
                {
                    chunkDef.Add(new RoomData(levelPath + tokens[0], Convert.ToSingle(tokens[1]) * 10, Convert.ToSingle(tokens[2]) * 10, -MathHelper.ToRadians(Convert.ToSingle(tokens[3]))));
                }
            }

            if (chunkDef.Count > 0)
            {
                chunkDefinitions.Add(floorName + chunkId, chunkDef);
            }*/
        }

        #region Level Creation
        public void CreateChunk(string chunkid, Vector3 chunkTranslation, float chunkRotation)
        {
            List<RoomData> chunkDef = chunkDefinitions[chunkid];
            foreach(RoomData room in chunkDef)
            {
                CreateRoom(room.filename, new Vector3(room.x + chunkTranslation.X, 0, room.z + chunkTranslation.Z), room.yaw);
            }
        }

        private void CreateRoom(string modelPath, Vector3 position, float yaw)
        {
            position.Y -= 18.5f;
            float roomScale = 10;

            GameEntity room = new GameEntity("room", FactionType.Neutral, EntityType.Misc);


            Model roomModel = GetUnanimatedModel(modelPath);

            Model roomCollisionModel = Game.Content.Load<Model>(modelPath);// + "Co");
            Vector3[] verts;
            int[] indices;
            TriangleMesh.GetVerticesAndIndicesFromModel(roomCollisionModel, out verts, out indices);
            StaticMesh roomMesh = new StaticMesh(verts, indices, new AffineTransform(new Vector3(roomScale), Quaternion.CreateFromYawPitchRoll(yaw, 0, 0), position));
            roomMesh.CollisionRules.Group = mainGame.LevelCollisionGroup;
            room.AddSharedData(typeof(StaticMesh), roomMesh);

            StaticMeshComponent roomPhysics = new StaticMeshComponent(mainGame, room);

            //holds the position so the model is drawn correctly
            Entity roomLocation = new Box(position, 1, 1, 1);
            room.AddSharedData(typeof(Entity), roomLocation);

            UnanimatedModelComponent roomGraphics = new UnanimatedModelComponent(mainGame, room, roomModel, new Vector3(roomScale), Vector3.Zero, Matrix.CreateFromYawPitchRoll(yaw, 0, 0));

            room.AddComponent(typeof(StaticMeshComponent), roomPhysics);
            genComponentManager.AddComponent(roomPhysics);

            room.AddComponent(typeof(UnanimatedModelComponent), roomGraphics);
            levelModelManager.AddComponent(roomGraphics);

            rooms.Add(room);
        }
        #endregion

        // Reads the map from the given array of bytes, representing chunk types
        public void CreateMapFrom(int[] map)
        {
            // TODO actually implement                     
            DemoLevel();
            mainGame.gameState = GameState.Playing;
        }

        // Translates the given room id into the path to that respective model and yaw (rotation) for a room
        private string TranslateIdToPathYaw(int id, out float yaw)
        {
            yaw = 0;
            return "Models\\Levels\\tempChunk";
        }

        /// <summary>
        /// Class responsible for actually building the chunk
        /// </summary>
        private class LevelBuilder
        {
            private int levelWidth;
            private int levelHeight;

            // Default uses the level width and height as defined in the Constants file
            public LevelBuilder()
                : this(Constants.LEVEL_WIDTH, Constants.LEVEL_HEIGHT)
            {
                
            }

            // Specific constructor for more cool stuff
            public LevelBuilder(int levelWidth, int levelHeight)
            {
                this.SetLevelBounds(levelWidth, levelHeight);
            }

            // Sets the level bounds for level generation
            public void SetLevelBounds(int levelWidth, int levelHeight)
            {
                this.levelWidth = levelWidth;
                this.levelHeight = levelHeight;
            }

            /// <summary>
            /// Builds a Level based on the given floorName and returns a list of all the rooms 
            /// in the level
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public Chunk[,] BuildLevel(FloorName name)
            {
                ChunkInfo[,] chunks = new ChunkInfo[levelWidth, levelHeight];
                // First we gotta figure out what chunks to place
                ChooseChunks(name, chunks);
                //return ReadChunks(name, chunks);
                return null;
            }

            #region Choosing Chunks
            private void ChooseChunks(FloorName name, ChunkInfo[,] chunks)
            {
                PlaceSoulevator(name, chunks);
                Vector2 bossLoc = PlaceBoss(name, chunks);
                PlaceKey(name, chunks, bossLoc);
                PlaceTheRest(name, chunks);
            }

            // Places a home chunk in the middle of the level
            private void PlaceSoulevator(FloorName name, ChunkInfo[,] chunks)
            {
                int midX = levelWidth / 2;
                int midY = levelHeight / 2;

                chunks[midX, midY] = ChunkUtil.Instance.GetSoulevatorChunk(name);
            }

            // Places the boss in a random location
            private Vector2 PlaceBoss(FloorName name, ChunkInfo[,] chunks)
            {
                // Do nothing for now
                // TODO choose random spot for the boss to go, return the location
                return Vector2.Zero;
            }

            private void PlaceKey(FloorName name, ChunkInfo[,] chunks, Vector2 bossLoc)
            {
                // Do nothing for now
                // TODO Place the key 'far enough' away from the boss
            }

            private void PlaceTheRest(FloorName name, ChunkInfo[,] chunks)
            {
                // Goes one by one thru the chunks array and chooses a random chunk to place there
                for (int i = 0; i < levelWidth; ++i)
                {
                    for (int j = 0; j < levelHeight; ++j)
                    {
                        // If there's nothing there, place something
                        if (chunks[i, j] == null)
                        {
                            ISet<Direction> reqDirs = GetRequiredDirections(name, chunks, i, j);
                            chunks[i, j] = ChunkUtil.Instance.GetSatisfyingChunk(name, ChunkType.NORMAL, reqDirs);
                        }
                        // Otherwise it's the boss/soulevator/key
                    }
                }
            }

            // Gets where we need doors at
            private ISet<Direction> GetRequiredDirections(FloorName name, ChunkInfo[,] chunks, int i, int j)
            {
                // Just gotta check what's to the left, what's to the right, what's up, and what's (goin) down
                ISet<Direction> reqDirs = new HashSet<Direction>();

                // Left chunk. It it has a door, then we need a door to the east
                if (HasDoorAt(chunks, i - 1, j, Direction.EAST))
                {
                    reqDirs.Add(Direction.WEST);
                }
                // Right chunk. It it has a door, then we need a door to the west
                if (HasDoorAt(chunks, i + 1, j, Direction.WEST))
                {
                    reqDirs.Add(Direction.EAST);
                }
                // Top chunk. It it has a door, then we need a door to the north
                if (HasDoorAt(chunks, i, j - 1, Direction.SOUTH))
                {
                    reqDirs.Add(Direction.NORTH);
                }
                // Bottom chunk. It it has a door, then we need a door to the south
                if (HasDoorAt(chunks, i, j + 1, Direction.NORTH))
                {
                    reqDirs.Add(Direction.SOUTH);
                }

                return reqDirs;
            }

            // Checks whether the chunk at chunks[i,j] has a chunk and if it needs a door at reqDir
            private bool HasDoorAt(ChunkInfo[,] chunks, int i, int j, Direction reqDir)
            {
                if (i < 0 || i >= levelWidth || j < 0 || j >= levelHeight)
                {
                    return false;
                }
                if (chunks[i, j] == null)
                {
                    // TODO here is where we can change it to make less doors. Right now by returning true on an unplaced chunk we are 
                    // forcing the maximum amount of doors
                    return true;
                }

                return chunks[i, j].hasDoorAt(reqDir);
            }

            #endregion

            //private Chunk[,] ReadChunks(FloorName name, ChunkInfo[,])
            //{
                // TODO right here
            //}
            //#region Creating Rooms
            //// Creates a list of all the rooms making up this chunk
            //private IList<GameEntity> CreateRooms(FloorName name, ChunkInfo[,] chunks)
            //{
            //    // DON'T FORGET TO ROTATE!
            //}
            //#endregion
        }

        
    }
}
