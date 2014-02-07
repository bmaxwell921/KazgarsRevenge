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
        /// <summary>
        /// Class used to hold information about the current level
        /// </summary>
        private class LevelInfo
        {
            // The floor for the level
            public FloorName currentFloor;

            // The chunk objects that make up this Level
            public Chunk[,] chunks;

            // The information about the chunks in this level TODO might be unneeded
            public ChunkInfo[,] chunkInfos;

            // TODO add this
            // public Graph pathGraph;

            public LevelInfo(Chunk[,] chunks, ChunkInfo[,] chunkInfos)
            {
                this.chunks = chunks;
                this.chunkInfos = chunkInfos;
            }
        }

        // In BLOCKS, how far chunks are away from each other
        public static readonly int CHUNK_SIZE = 24;

        // How large a block is in 3D space
        public static readonly int BLOCK_SIZE = 100;

        // The y component of the room location
        public static readonly float LEVEL_Y = -18.5f;

        public static readonly string ROOM_PATH = @"Models\Rooms\";

        public FloorName CurrentFloor = FloorName.Dungeon;

        // The information about the currentLevel
        private LevelInfo currentLevel;

        // Rooms making up this level
        private List<GameEntity> rooms;

        public LevelManager(KazgarsRevengeGame game)
            : base(game)
        {
            rooms = new List<GameEntity>();
        }

        public void DemoLevel()
        {
            rooms.Add(CreateRoom("Models\\Levels\\tempChunk3", new Vector3(200, 0, -200), MathHelper.PiOver2));
            //CreateChunk("Dungeon1", new Vector3(120, 0, -200), 0);
        }

        /// <summary>
        /// Generates a new level to play with the default width and height in chunks (3x3)
        /// </summary>
        /// <param name="name"></param>
        public void CreateLevel(FloorName name)
        {
            // TODO test this
            //this.CreateLevel(name, Constants.LEVEL_WIDTH, Constants.LEVEL_HEIGHT);
            this.CreateLevel(name, 1, 1);
        }

        /// <summary>
        /// Generates a new level to play with the given width and height in chunks
        /// </summary>
        /// <param name="name"></param>
        /// <param name="levelWidth"></param>
        /// <param name="levelHeight"></param>
        public void CreateLevel(FloorName name, int levelWidth, int levelHeight)
        {
            this.rooms = new List<GameEntity>();
            LevelBuilder lBuilder = new LevelBuilder(levelWidth, levelHeight);
            this.currentLevel = lBuilder.BuildLevel(name);

            for (int i = 0; i < levelWidth; ++i)
            {
                for (int j = 0; j < levelHeight; ++j)
                {
                    this.rooms.AddRange(CreateChunkRooms(currentLevel.chunks[i, j], currentLevel.chunkInfos[i, j], i , j));
                }
            }
        }

        // Returns a list of all the rooms that belong to this chunk, in their proper locations
        private IList<GameEntity> CreateChunkRooms(Chunk chunk, ChunkInfo chunkInfo, int i, int j)
        {
            IList<GameEntity> rooms = new List<GameEntity>();
            Vector3 chunkLocation = new Vector3(i * CHUNK_SIZE * BLOCK_SIZE, LEVEL_Y, j * CHUNK_SIZE * BLOCK_SIZE);
            foreach (Room room in chunk.rooms)
            {
                rooms.Add(CreateRoom(room, chunkInfo.rotation, chunkLocation));
            }

            return rooms;
        }

        // Creates a gameEntity for the given information
        private GameEntity CreateRoom(Room room, Rotation chunkRotation, Vector3 chunkLocation)
        {
            // TODO remove - trying to get placement right
            chunkRotation = Rotation.ZERO;

            // Room.location is the room's location relative to the chunk's location, so we add the values to get its proper location
            Vector3 roomLocation = chunkLocation + new Vector3(room.location.x * BLOCK_SIZE, 0, room.location.y * BLOCK_SIZE);

            Vector3 chunkCenter = (chunkLocation + new Vector3(CHUNK_SIZE, 0, CHUNK_SIZE)) / 2;
            // Rotate it
            Vector3 newLocation;
            float newRotation;
            GetRotatedInfo(roomLocation, room.rotation, chunkRotation, chunkCenter, out newLocation, out newRotation);

            GameEntity roomGE = CreateRoom(ROOM_PATH + room.name, newLocation, newRotation);

            // TODO need to send in the center of the room, not newLocation
            //AddSpawners(roomGE, room, newLocation, room.rotation, chunkCenter, chunkRotation);
            
            return roomGE;
        }

        // Initializes newLocation and newRotation by applying the chunk's rotation to the room's location and rotation
        private void GetRotatedInfo(Vector3 roomLoc, Rotation roomRotation, Rotation chunkRotation, Vector3 chunkCenter, out Vector3 newLocation, out float newRotation)
        {
            // Rotate around the chunk's center
            newLocation = RotatedLocation(roomLoc, chunkCenter, chunkRotation);
            newRotation = roomRotation.ToRadians() + chunkRotation.ToRadians();
        }

        // Gets the location rotated around point by the given amount
        private Vector3 RotatedLocation(Vector3 location, Vector3 point, Rotation chunkRotation)
        {
            /*
             * We need to rotate around the center of the chunk. First we have to translate to (0,0,0) since that's where
             * the point rotate rotates around, then we do the rotate, then we translate back.
             */ 
            Matrix rotater = Matrix.CreateTranslation(point) * Matrix.CreateRotationY(chunkRotation.ToRadians()) * Matrix.CreateTranslation(-point);
            return Vector3.Transform(location, rotater);
        }

        private GameEntity CreateRoom(string modelPath, Vector3 position, float yaw)
        {
            position.Y = LEVEL_Y;
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

            return room;
        }

        // Players need to be within 60 units for the thing to spawn. Based on the fact that chunks are 240x240
        private static readonly float PROXIMITY = 60;

        // Spawn every 3 seconds
        private static readonly float DELAY = 3000;

        // Adds the necessary spawning components to the roomGE
        private void AddSpawners(GameEntity roomGE, Room room, Vector3 roomCenter, Rotation roomRotation, Vector3 chunkCenter, Rotation chunkRotation)
        {
            IList<RoomBlock> enemySpawners = room.GetEnemySpawners();
            ISet<Vector3> spawnLocs = new HashSet<Vector3>();
            foreach (RoomBlock spawner in enemySpawners)
            {
                // TODO this is probably wrong too. The rotations are hurting my brain
                // Need to rotate the location by the room rotation, then by the chunk rotation
                Vector3 blockLoc = new Vector3(spawner.location.x, LEVEL_Y, spawner.location.y);
                Vector3 rotatedLoc = RotatedLocation(blockLoc, roomCenter, roomRotation);
                rotatedLoc = RotatedLocation(rotatedLoc, chunkCenter, chunkRotation);
                spawnLocs.Add(rotatedLoc);
            }

            roomGE.AddComponent(typeof(EnemyProximitySpawner), new EnemyProximitySpawner((KazgarsRevengeGame)Game, roomGE, EntityType.NormalEnemy, spawnLocs, PROXIMITY, DELAY));
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
            public LevelInfo BuildLevel(FloorName name)
            {
                ChunkInfo[,] chunkInfos = new ChunkInfo[levelWidth, levelHeight];
                // First we gotta figure out what chunks to place
                ChooseChunks(name, chunkInfos);

                Chunk[,] chunks = ReadChunks(name, chunkInfos);
                
                // Graph mvGraph = CreateMovementGraph(name, chunks);
                return new LevelInfo(chunks, chunkInfos);
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

            // Reads each ChunkInfo from JSON into an actual object
            private Chunk[,] ReadChunks(FloorName name, ChunkInfo[,] chunks)
            {
                Chunk[,] ret = new Chunk[levelWidth, levelHeight];
                for (int i = 0; i < levelWidth; ++i)
                {
                    for (int j = 0; j < levelHeight; ++j)
                    {
                        Chunk c = ChunkUtil.Instance.ReadChunk(chunks[i, j]);
                        // Set the rotation properly, all the JSONs have the rotation as ZERO
                        c.rotation = chunks[i, j].rotation;
                        ret[i, j] = c;
                    }
                }
                return ret;
            }

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
