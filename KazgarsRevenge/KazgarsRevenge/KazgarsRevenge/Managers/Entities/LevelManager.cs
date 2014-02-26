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

            public LevelInfo(FloorName currentFloor, Chunk[,] chunks, ChunkInfo[,] chunkInfos)
            {
                this.currentFloor = currentFloor;
                this.chunks = chunks;
                this.chunkInfos = chunkInfos;
            }
        }

        // In BLOCKS, how far chunks are away from each other
        public static readonly int CHUNK_SIZE = 24;

        // How large a block is in 3D space
        public static readonly int BLOCK_SIZE = 100;

        // The y component of the room location
        public static readonly float LEVEL_Y = 0;

        // Put mobs a little bit above the ground so they don't sink down
        public static readonly float MOB_SPAWN_Y = 20;

        public static readonly string ROOM_PATH = @"Models\Rooms\";

        public FloorName CurrentFloor = FloorName.Dungeon;

        // The information about the currentLevel
        private LevelInfo currentLevel;

        // Rooms making up this level
        private List<GameEntity> rooms;
        private List<GameEntity> lights = new List<GameEntity>();

        public LevelManager(KazgarsRevengeGame game)
            : base(game)
        {
            rooms = new List<GameEntity>();
        }

        public void DemoLevel()
        {
            for (int i = 0; i < 10; ++i)
            {
                for (int j = 0; j < 10; ++j)
                {
                    CreatePointLight(new Vector3(-700 + i * 700, 0, -700 + j * 700));
                }
            }
            rooms.Add(CreateRoom("Models\\Levels\\tempChunk3", new Vector3(200, 0, -200), MathHelper.PiOver2));
            //CreateChunk("Dungeon1", new Vector3(120, 0, -200), 0);
        }

        /// <summary>
        /// Generates a new level to play with the default width and height in chunks (3x3)
        /// </summary>
        /// <param name="name"></param>
        public void CreateLevel(FloorName name)
        {
            //this.CreateLevel(name, Constants.LEVEL_WIDTH, Constants.LEVEL_HEIGHT);
            this.CreateLevel(name, 2, 1);
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

            // Go thru each chunkInfo and add the rooms
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
            // We only scale things properly after we're done placing them
            IList<GameEntity> rooms = new List<GameEntity>();

            // Multiply the i and j by CHUNK_SIZE because chunk (1,0) starts at (24, 0)
            Vector3 chunkLocation = new Vector3(i * CHUNK_SIZE, 0, j * CHUNK_SIZE);

            foreach (Room room in chunk.rooms)
            {
                rooms.Add(CreateRoom(room, chunkInfo.rotation, chunkLocation));
            }

            return rooms;
        }

        // Creates a gameEntity for the given information
        private GameEntity CreateRoom(Room room, Rotation chunkRotation, Vector3 chunkLocation)
        {
            // Room.location is the top left so we move it to the center by adding half the width and height, then we move it by the chunklocation
            Vector3 roomCenter = new Vector3(room.location.x, LEVEL_Y, room.location.y) + new Vector3(room.Width, LEVEL_Y, room.Height) / 2 + chunkLocation;
            // Center is the location plus half the width and height
            Vector3 chunkCenter = chunkLocation + new Vector3(CHUNK_SIZE, LEVEL_Y, CHUNK_SIZE) / 2;

            // Rotates the center of the room as needed by the chunk rotation
            Vector3 rotatedCenter = GetRotatedLocation(roomCenter, chunkCenter, chunkRotation);
            // The yaw for a room should just be the room's rotation plus the chunk's rotation
            float yaw = room.rotation.ToRadians() + chunkRotation.ToRadians();

            // Create the actual entity
            GameEntity roomGE = CreateRoom(ROOM_PATH + room.name, rotatedCenter * BLOCK_SIZE, yaw);
            
            // Here for convenience
            Vector3 roomTopLeft = new Vector3(room.location.x, LEVEL_Y, room.location.y) + chunkLocation;
            // Add all the spawners
            AddSpawners(roomGE, room.GetEnemySpawners(), roomTopLeft, roomCenter, room.rotation, chunkLocation, chunkCenter, chunkRotation);

            //TODO player spawners

            return roomGE;
        }

        private Vector3 GetRotatedLocation(Vector3 location, Vector3 rotationPoint, Rotation rotationAmt)
        {
            // I can't believe this code from the internet worked!
            return Vector3.Transform(location - rotationPoint, Matrix.CreateRotationY(rotationAmt.ToRadians())) + rotationPoint;
        }

        private GameEntity CreateRoom(string modelPath, Vector3 position, float yaw)
        {
            position.Y = LEVEL_Y;
            float roomScale = 10;

            GameEntity room = new GameEntity("room", FactionType.Neutral, EntityType.Misc);

            Model roomModel = GetUnanimatedModel(modelPath);

            List<StaticMesh> levelCollidables = new List<StaticMesh>();
            LevelTagData levelInfo = roomModel.Tag as LevelTagData;
            if (levelInfo != null)
            {
                List<Vector3> lightPositions = levelInfo.lightLocations;
                if (lightPositions != null)
                {
                    foreach (Vector3 v in lightPositions)
                    {
                        CreatePointLight(v);
                    }
                }
                
                List<Vector3[]> allVertices = levelInfo.physicsVertices;
                List<int[]> allIndices = levelInfo.physicsIndices;
                List<Matrix> allTransforms = levelInfo.physicsTransforms;

                if (allVertices.Count > 0)
                {
                    for (int i = 0; i < allVertices.Count; ++i)
                    {
                        Matrix m = allTransforms[i] * Matrix.CreateScale(roomScale) * Matrix.CreateFromYawPitchRoll(yaw, 0, 0);
                        Vector3 scale;
                        Quaternion rot;
                        Vector3 trans;
                        m.Decompose(out scale, out rot, out trans);
                        AffineTransform a = new AffineTransform(scale, rot, trans + position);
                        StaticMesh roomMesh = new StaticMesh(allVertices[i], allIndices[i], a);
                        roomMesh.CollisionRules.Group = mainGame.LevelCollisionGroup;
                        levelCollidables.Add(roomMesh);
                    }
                }
                else
                {
                    Vector3[] vertices;
                    int[] indices;
                    TriangleMesh.GetVerticesAndIndicesFromModel(roomModel, out vertices, out indices);
                    StaticMesh roomMesh = new StaticMesh(vertices, indices, new AffineTransform(new Vector3(roomScale), Quaternion.CreateFromYawPitchRoll(yaw, 0, 0), position));
                    roomMesh.CollisionRules.Group = mainGame.LevelCollisionGroup;
                    levelCollidables.Add(roomMesh);
                }
            }
            else
            {
                Vector3[] vertices;
                int[] indices;
                TriangleMesh.GetVerticesAndIndicesFromModel(roomModel, out vertices, out indices);
                StaticMesh roomMesh = new StaticMesh(vertices, indices, new AffineTransform(new Vector3(roomScale), Quaternion.CreateFromYawPitchRoll(yaw, 0, 0), position));
                levelCollidables.Add(roomMesh);
            }
            

            room.AddSharedData(typeof(List<StaticMesh>), levelCollidables);

            StaticMeshesComponent roomPhysics = new StaticMeshesComponent(mainGame, room);

            //holds the position so the model is drawn correctly (not added to physics)
            //size is related to how far away this will start being rendered
            Entity roomLocation = new Box(position, 1100, 1100, 1100);
            room.AddSharedData(typeof(Entity), roomLocation);

            UnanimatedModelComponent roomGraphics = new UnanimatedModelComponent(mainGame, room, roomModel, new Vector3(roomScale), Vector3.Zero, yaw, 0, 0);

            room.AddComponent(typeof(StaticMeshesComponent), roomPhysics);
            genComponentManager.AddComponent(roomPhysics);

            room.AddComponent(typeof(UnanimatedModelComponent), roomGraphics);
            levelModelManager.AddComponent(roomGraphics);


            return room;
        }

        private void CreatePointLight(Vector3 position)
        {
            GameEntity light = new GameEntity("light", FactionType.Neutral, EntityType.None);

            position.Y = 40;
            Entity physicalData = new Box(position, .1f, .1f, .1f);
            physicalData.IsAffectedByGravity = false;
            physicalData.LocalInertiaTensorInverse = new Matrix3X3();
            physicalData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoBroadPhase;
            light.AddSharedData(typeof(Entity), physicalData);

            light.AddSharedData(typeof(Color), Color.White);

            PhysicsComponent lightPhysics = new PhysicsComponent(mainGame, light);

            light.AddSharedData(typeof(PhysicsComponent), lightPhysics);
            genComponentManager.AddComponent(lightPhysics);

            lights.Add(light);
        }

        // Players need to be within 60 units for the thing to spawn. Based on the fact that chunks are 240x240
        // TODO Set these based on a difficulty level???
        private static readonly float PROXIMITY = 60;
        // Spawn every 3 seconds
        private static readonly float DELAY = 3000;

        // Adds the necessary spawning components to the roomGE
        private void AddSpawners(GameEntity roomGE, IList<RoomBlock> enemySpawners, Vector3 roomTopLeft, Vector3 roomCenter, Rotation roomRotation, Vector3 chunkTopLeft, Vector3 chunkCenter, Rotation chunkRotation)
        {
            ISet<Vector3> spawnLocs = new HashSet<Vector3>();
            foreach (RoomBlock spawner in enemySpawners)
            {
                // Need to rotate the location by the room rotation, then by the chunk rotation
                //Vector3 blockLoc = new Vector3(spawner.location.x, LEVEL_Y, spawner.location.y);
                //Vector3 rotatedLoc = RotatedLocation(blockLoc, roomCenter, Vector3.Zero, roomRotation);
                //rotatedLoc = RotatedLocation(rotatedLoc, Vector3.Zero, chunkCenter, chunkRotation);
                //spawnLocs.Add(rotatedLoc);

                // The location of a block is relative to the room and chunk's top left corner
                Vector3 spawnCenter = chunkTopLeft +roomTopLeft + new Vector3(spawner.location.x, LEVEL_Y, spawner.location.y) + new Vector3(RoomBlock.SIZE, LEVEL_Y, RoomBlock.SIZE) / 2;
                // We need to rotate this location to be correct in the room
                spawnCenter = GetRotatedLocation(spawnCenter, roomCenter, roomRotation);
                // Then we need to rotate it to the correct location in the chunk
                spawnCenter = GetRotatedLocation(spawnCenter, chunkCenter, chunkRotation);

                // Set their location a bit above the ground so they don't fall thru
                spawnCenter = new Vector3(spawnCenter.X * BLOCK_SIZE, MOB_SPAWN_Y, spawnCenter.Z * BLOCK_SIZE);

                spawnLocs.Add(spawnCenter);
            }
            EnemyProximitySpawner eps = new EnemyProximitySpawner((KazgarsRevengeGame)Game, roomGE, EntityType.NormalEnemy, spawnLocs, PROXIMITY, DELAY);
            roomGE.AddComponent(typeof(EnemyProximitySpawner), eps);
            genComponentManager.AddComponent(eps);
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
                return new LevelInfo(name, chunks, chunkInfos);
            }

            #region Choosing Chunks
            private void ChooseChunks(FloorName name, ChunkInfo[,] chunks)
            {
                // TODO re-add these for full implementation
                //PlaceSoulevator(name, chunks);
                //Vector2 bossLoc = PlaceBoss(name, chunks);
                //PlaceKey(name, chunks, bossLoc);
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
