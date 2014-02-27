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

using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Algorithms.ShortestPath;
using QuickGraph.Algorithms.Observers;

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

            // The information about the chunks in this level
            public ChunkInfo[,] chunkInfos;

            public IList<Vector3> spawnLocs;

            // The graph representing enemy pathing possibilities. this is gonna be connected as fuck
             public AdjacencyGraph<Vector3, Edge<Vector3>> pathGraph;

            public LevelInfo(FloorName currentFloor, Chunk[,] chunks, ChunkInfo[,] chunkInfos)
            {
                this.currentFloor = currentFloor;
                this.chunks = chunks;
                this.chunkInfos = chunkInfos;
                this.spawnLocs = new List<Vector3>();
                pathGraph = new AdjacencyGraph<Vector3, Edge<Vector3>>();
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
            this.CreateLevel(name, Constants.LEVEL_WIDTH, Constants.LEVEL_HEIGHT);
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
            LevelBuilder lBuilder = new LevelBuilder(Game.Services.GetService(typeof(LoggerManager)) as LoggerManager, levelWidth, levelHeight);
            this.currentLevel = lBuilder.BuildLevel(name);

            // Go thru each chunkInfo and add the rooms
            for (int i = 0; i < levelWidth; ++i)
            {
                for (int j = 0; j < levelHeight; ++j)
                {
                    this.rooms.AddRange(CreateChunkRooms(currentLevel.chunks[i, j], currentLevel.chunkInfos[i, j], i , j));
                }
            }

            /*
             * This is a set of all the locations of doors in the entire map. After we're done connecting
             * each room's internal blocks together we'll go thru all these door locations and add edges
             * between doors that are adjacent
             */ 
            ISet<Vector3> doors = new HashSet<Vector3>();
            for (int i = 0; i < levelWidth; ++i)
            {
                for (int j = 0; j < levelHeight; ++j)
                {
                    BuildAndAddPathing(currentLevel.chunks[i, j], currentLevel.chunkInfos[i, j], i , j, doors);
                }
            }

            ConnectDoors(doors);
        }

        /// <summary>
        /// Returns a location to spawn the player at
        /// </summary>
        /// <returns></returns>
        public Vector3 GetPlayerSpawnLocation()
        {
            Vector3 spawnLoc = currentLevel.spawnLocs[RandSingleton.Instance.Next(currentLevel.spawnLocs.Count)];
            return spawnLoc;
        }

        #region Room Creation
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
            Vector3 roomCenter = new Vector3(room.location.x, LEVEL_Y, room.location.y) + new Vector3(room.Width, LEVEL_Y, room.Height) / 2;
            // Center is the location plus half the width and height
            Vector3 chunkCenter = chunkLocation + new Vector3(CHUNK_SIZE, LEVEL_Y, CHUNK_SIZE) / 2;

            // Rotates the center of the room as needed by the chunk rotation
            Vector3 rotatedCenter = GetRotatedLocation(roomCenter + chunkLocation, chunkCenter, chunkRotation);
            // The yaw for a room should just be the room's rotation plus the chunk's rotation
            float yaw = room.rotation.ToRadians() + chunkRotation.ToRadians();

            // Create the actual entity
            GameEntity roomGE = CreateRoom(ROOM_PATH + room.name, rotatedCenter * BLOCK_SIZE, yaw);
            
            // Here for convenience
            Vector3 roomTopLeft = new Vector3(room.location.x, LEVEL_Y, room.location.y);
            // Add all the spawners
            AddSpawners(roomGE, room.GetEnemySpawners(), roomTopLeft, room.rotation, chunkLocation,  chunkRotation, room.UnRotWidth, room.UnRotHeight);
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
        private void AddSpawners(GameEntity roomGE, IList<RoomBlock> enemySpawners, Vector3 roomTopLeft, Rotation roomRotation, Vector3 chunkTopLeft, Rotation chunkRotation, int roomWidth, int roomHeight)
        {
            ISet<Vector3> spawnLocs = new HashSet<Vector3>();
            foreach (RoomBlock spawner in enemySpawners)
            {
                Vector3 spawnCenter = GetRotatedBlock(chunkTopLeft, roomTopLeft, new Vector3(spawner.location.x, LEVEL_Y, spawner.location.y), chunkRotation, roomRotation, roomWidth, roomHeight);
                spawnLocs.Add(spawnCenter);
            }
            EnemyProximitySpawner eps = new EnemyProximitySpawner((KazgarsRevengeGame)Game, roomGE, EntityType.NormalEnemy, spawnLocs, PROXIMITY, DELAY, 1);
            roomGE.AddComponent(typeof(EnemyProximitySpawner), eps);
            genComponentManager.AddComponent(eps);
        }

        /// <summary>
        /// Returns a Vector3 located at the center of the block at the given location basically.
        /// </summary>
        /// <param name="chunkTopLeft">Chunk's top left corner location</param>
        /// <param name="roomTopLeft">Room's top left corner, RELATIVE TO THE CHUNK - IT SHOULD NOT INCLUDE THE CHUNK LOCATION</param>
        /// <param name="blockLoc"></param>
        /// <param name="chunkRotation"></param>
        /// <param name="roomRotation"></param>
        /// <param name="roomWidth"></param>
        /// <param name="roomHeight"></param>
        /// <returns></returns>
        private Vector3 GetRotatedBlock(Vector3 chunkTopLeft, Vector3 roomTopLeft, Vector3 blockLoc, Rotation chunkRotation, Rotation roomRotation, int roomWidth, int roomHeight)
        {
            // Rotate it about the room
            int numRoomRotations = (int)(roomRotation.ToDegrees() / 90f);
            int swapWidth = roomWidth;
            // We start by rotating about the room so leave this position as relative to the room topLeft
            Vector3 spawnTopLeft = blockLoc;
            for (int i = 0; i < numRoomRotations; ++i)
            {
                spawnTopLeft = rotateBlockTo(spawnTopLeft, swapWidth);
                // Update the swapWidth to reflect the change from rotating
                swapWidth = (swapWidth == roomWidth) ? roomHeight : roomWidth;
            }

            // Rotate it about the chunk
            int numChunkRotations = (int)(chunkRotation.ToDegrees() / 90f);

            // Now that it's been rotated in the room, add the room position for rotating it about the chunk
            spawnTopLeft += roomTopLeft;

            for (int i = 0; i < numChunkRotations; ++i)
            {
                // Don't need to change the rotate width because chunks are square
                spawnTopLeft = rotateBlockTo(spawnTopLeft, CHUNK_SIZE);
            }

            // Now that it's been totally rotated, we can add in the chunkLocation
            spawnTopLeft += chunkTopLeft;

            // Center it, scale it up, and return
            return new Vector3((spawnTopLeft.X + RoomBlock.SIZE / 2f) * BLOCK_SIZE, MOB_SPAWN_Y, (spawnTopLeft.Z + RoomBlock.SIZE / 2f) * BLOCK_SIZE);
        }

        // Rotates the given location by 90 degrees in the room with the given width and height
        private Vector3 rotateBlockTo(Vector3 blockLoc, int width)
        {
            return new Vector3(blockLoc.Z, blockLoc.Y, width - blockLoc.X - 1);
        }

        #endregion

        #region Graph Building
        private void BuildAndAddPathing(Chunk chunk, ChunkInfo chunkInfo, int i, int j, ISet<Vector3> doors)
        {
            Vector3 chunkLocation = new Vector3(i * CHUNK_SIZE, 0, j * CHUNK_SIZE);
            foreach (Room room in chunk.rooms)
            {
                BuildRoomGraph(room, chunkLocation, chunk.rotation, doors);
            }
        }

        // Builds the graph for a single room, adding any doorBlocks to the doors param
        private void BuildRoomGraph(Room room, Vector3 chunkLocation, Rotation chunkRotation, ISet<Vector3> doors)
        {
            // Holds the center of each block
            ISet<Vector3> blockCenters = new HashSet<Vector3>();
            Vector3 roomCenter = new Vector3(room.location.x, LEVEL_Y, room.location.y) + new Vector3(room.Width, LEVEL_Y, room.Height) / 2;
            Vector3 chunkCenter = chunkLocation + new Vector3(CHUNK_SIZE, LEVEL_Y, CHUNK_SIZE) / 2;
            Vector3 roomTopLeft = new Vector3(room.location.x, LEVEL_Y, room.location.y);

            foreach (RoomBlock block in room.blocks)
            {
                Vector3 blockCenter = GetRotatedBlock(chunkLocation, roomTopLeft, new Vector3(block.location.x, LEVEL_Y, block.location.y), chunkRotation, room.rotation, room.UnRotWidth, room.UnRotHeight);
                // For the graph the y should probs be 0, I'll make it ignore the Y when checking for closeness tho
                blockCenter.Y = LEVEL_Y;
                // These have not been transformed by BLOCK_SIZE yet
                blockCenters.Add(blockCenter);

                // If this block is a door we need to hold onto it so we can attach it to the other doors
                if (block.IsDoor())
                {
                    doors.Add(blockCenter);
                }

                // If it's a player spawn, we need to know that we can spawn players here
                if (block.IsPlayerSpawn())
                {
                    // Set him a little higher
                    blockCenter.Y = MOB_SPAWN_Y;
                    currentLevel.spawnLocs.Add(blockCenter);
                }
            }

            /*
             * Now that we have the center of each block, we can put them in the graph.
             *  For each block, check if any of it's adjacent neighbors are in the set, if so
             *  then add an edge
             */
            foreach (Vector3 blockCenter in blockCenters)
            {
                // Look at all of its adjacent neighbors
                for (int i = -1; i <= 1; ++i)
                {
                    for (int j = -1; j <= 1; ++j)
                    {
                        Vector3 testBlock = new Vector3(blockCenter.X + i, blockCenter.Y, blockCenter.Z + j);
                        // If a neighbor is one of the blocks in the room, the connect them
                        if (!testBlock.Equals(blockCenter) && blockCenters.Contains(testBlock))
                        {
                            this.AddEdge(new Edge<Vector3>(blockCenter, testBlock));
                        }
                    }
                }
            }

        }

        // Connects all adjacent doors to each other
        private void ConnectDoors(ISet<Vector3> doors)
        {
            /*
             * If the door we're looking at is x, then possible doors to connect to are the z locations
             *  [ ][z][ ]
             *  [z][x][z]
             *  [ ][z][ ]
             */ 
            Vector3[] adjs = {new Vector3(-1, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 0, -1), new Vector3(0, 0, 1)};
            /*
             * Same idea as connecting all roomBlocks, iterate over all the doors and then iterate over the
             * 4 non-diagonal adjacent blocks. If a block is in the doors set, then add an edge
             */ 
            foreach (Vector3 door in doors)
            {
                foreach (Vector3 adj in adjs)
                {
                    Vector3 testDoor = door + adj;
                    // If the adjacent is one of the doors, add the edge
                    if (doors.Contains(testDoor))
                    {
                        // Doors next to each other in a room have already been connected
                        this.AddEdge(new Edge<Vector3>(door, testDoor));
                    }
                }
            }
        }

        // Method used to safely add an edge to the pathGraph
        private void AddEdge(Edge<Vector3> edge)
        {
            if (!currentLevel.pathGraph.ContainsVertex(edge.Source))
            {
                currentLevel.pathGraph.AddVertex(edge.Source);
            }
            if (!currentLevel.pathGraph.ContainsVertex(edge.Target))
            {
                currentLevel.pathGraph.AddVertex(edge.Target);
            }
            if (!currentLevel.pathGraph.ContainsEdge(edge))
            {
                currentLevel.pathGraph.AddEdge(edge);
            }
        }
        #endregion

        #region Path Finding
        /// <summary>
        /// Returns a Path from the source location to the destination location.
        /// First element is the given src, last element is the  given dest.
        /// 
        /// NOTE: BE AWARE OF THE Y VALUE. ALONG THE PATH THE Y'S WILL ALWAYS BE 0
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <returns>The path from src to dest, or null if no path exists</returns>
        public IList<Vector3> GetPath(Vector3 src, Vector3 dest)
        {
            // First we gotta find the closest node in the graph to src
            // and the closest node to dest
            Vector3 srcNode = FindClosestNodeTo(src);
            Vector3 destNode = FindClosestNodeTo(dest);

            /* 
             * If they're too far then that means we couldn't actually find the
             * locations...not sure if this is actually possible, but it happened
             * when testing
             */ 
            if (TooFar(src, srcNode) || TooFar(dest, destNode))
            {
                return null;
            }

            // Let the library do all the hard work
            // Delegate to calculate the cost from one node to another
            Func<Edge<Vector3>, double> cost = edge =>
            {
                return Vector3.Distance(edge.Source, edge.Target);
            };

            // Delegate to calculate the heuristic for a node
            Func<Vector3, double> heuristic = vect =>
            {
                return Vector3.Distance(vect, dest);
            };
            AStarShortestPathAlgorithm<Vector3, Edge<Vector3>> aStar = new AStarShortestPathAlgorithm<Vector3, Edge<Vector3>>(currentLevel.pathGraph, cost, heuristic);

            // Observer used to get the path
            VertexPredecessorRecorderObserver<Vector3, Edge<Vector3>> predObs = new VertexPredecessorRecorderObserver<Vector3, Edge<Vector3>>();
            predObs.Attach(aStar);

            // Actually do the computation
            aStar.ComputeDistanceBetween(srcNode, destNode);
            IEnumerable<Edge<Vector3>> path;
            bool validPath = predObs.TryGetPath(destNode, out path);

            if (!validPath)
            {
                return null;
            }

            IList<Vector3> pathList = new List<Vector3>();
            pathList.Add(src);
            // Fill in the path as a nice, usable list
            foreach (Edge<Vector3> pathEle in path)
            {
                pathList.Add(pathEle.Source);
            }
            // The way i'm adding stuff to the path list, we'll miss the final destination, so add it here
            pathList.Add(destNode);
            pathList.Add(dest);
            return pathList;
            
        }

        /*
         * Checks whether the given realLocation is too far from the graphNode, ie calculating the path
         * will just give us silly answers
         */ 

        private bool TooFar(Vector3 realLoc, Vector3 graphNode)
        {
            // Set they Y to be the same so it doesn't mess with anything
            Vector3 compReal = new Vector3(realLoc.X, LEVEL_Y, realLoc.Z);
            Vector3 compGraph = new Vector3(graphNode.X, LEVEL_Y, graphNode.Z);
            /*
             * Trig:
             * Blocks are 100x100. The farthest anything can be from the middle of the center is
             * the length of the line from the center to a corner. From Pythagorean Theorem the length
             * is sqrt((BLOCK_SIZE / 2) ^ 2 + (BLOCK_SIZE / 2) ^ 2) 
             */
            // Add a buffer cause why not
            double buffer = 5;
            double halfBlockSize = BLOCK_SIZE / 2.0;
            double maxDist = Math.Sqrt(halfBlockSize * halfBlockSize + halfBlockSize * halfBlockSize) + buffer;
            return Vector3.Distance(compReal, compGraph) > maxDist;
        }

        // Returns the Vector3 node in the pathGraph closest to the given location
        private Vector3 FindClosestNodeTo(Vector3 location)
        {
            Vector3 min = Vector3.Zero;
            // Set this as null for the first time around
            double? minDist = null;

            foreach (Vector3 node in currentLevel.pathGraph.Vertices)
            {
                double thisDist = Vector3.Distance(location, node);
                if (minDist == null || thisDist < minDist)
                {
                    min = node;
                    minDist = thisDist;
                }
            }
            return min;
        }
        #endregion

        #region Building Level
        /// <summary>
        /// Class responsible for actually building the chunk
        /// </summary>
        private class LevelBuilder
        {
            private int levelWidth;
            private int levelHeight;

            private LoggerManager lm;

            // Default uses the level width and height as defined in the Constants file
            public LevelBuilder(LoggerManager lm)
                : this(lm, Constants.LEVEL_WIDTH, Constants.LEVEL_HEIGHT)
            {
                
            }

            // Specific constructor for more cool stuff
            public LevelBuilder(LoggerManager lm, int levelWidth, int levelHeight)
            {
                this.lm = lm;
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
                return new LevelInfo(name, chunks, chunkInfos);
            }

            #region Choosing Chunks
            private void ChooseChunks(FloorName name, ChunkInfo[,] chunks)
            {
                // TODO re-add these for full implementation
                PlaceSoulevator(name, chunks);
                //Vector2 bossLoc = PlaceBoss(name, chunks);
                //PlaceKey(name, chunks, bossLoc);
                PlaceTheRest(name, chunks);
            }

            private void LogChunkChoice(int i, int j, ChunkInfo ci)
            {
                lm.Log(Level.DEBUG, String.Format("LevelGeneration chose chunk:\n\t\t\t{0}\n\t\t\tLocation:({1},{2})\n\t\t\tFileName:{3}", ci.ToString(), i, j, ci.FileName));
            }

            // Places a home chunk in the middle of the level
            private void PlaceSoulevator(FloorName name, ChunkInfo[,] chunks)
            {
                int midX = levelWidth / 2;
                int midY = levelHeight / 2;

                chunks[midX, midY] = ChunkUtil.Instance.GetSoulevatorChunk(name);
                this.LogChunkChoice(midX, midY, chunks[midX, midY]);
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
                            this.LogChunkChoice(i, j, chunks[i, j]);
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
                    // forcing the maximum amount of doors. Maybe have a density value that is generated?
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
                        Chunk c = ChunkUtil.Instance.ReadChunk(chunks[i, j], name);
                        // Set the rotation properly, all the JSONs have the rotation as ZERO
                        c.rotation = chunks[i, j].rotation;
                        ret[i, j] = c;
                    }
                }
                return ret;
            }
        }
        #endregion
    }
}
