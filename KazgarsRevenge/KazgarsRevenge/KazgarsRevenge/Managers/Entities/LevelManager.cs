﻿using System;
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

            // The information about the chunks in this level TODO might be unneeded
            public ChunkInfo[,] chunkInfos;

            // The graph representing enemy pathing possibilities. this is gonna be connected as fuck
             public AdjacencyGraph<Vector3, Edge<Vector3>> pathGraph;

            public LevelInfo(FloorName currentFloor, Chunk[,] chunks, ChunkInfo[,] chunkInfos)
            {
                this.currentFloor = currentFloor;
                this.chunks = chunks;
                this.chunkInfos = chunkInfos;
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

            // Go thru each chunkInfo and add the rooms
            for (int i = 0; i < levelWidth; ++i)
            {
                for (int j = 0; j < levelHeight; ++j)
                {
                    this.rooms.AddRange(CreateChunkRooms(currentLevel.chunks[i, j], currentLevel.chunkInfos[i, j], i , j));
                }
            }

            // Build the pathGraph - done here so it's decoupled for multithreading.
            // TODO this could be done with outside of the KR program if it's too slow
            for (int i = 0; i < levelWidth; ++i)
            {
                for (int j = 0; j < levelHeight; ++j)
                {
                    buildAndAddPathing(currentLevel.chunks[i, j], currentLevel.chunkInfos[i, j], i , j);
                }
            }
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

            //CreatePointLight(position);

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

            //holds the position so the model is drawn correctly (not added to physics)
            //size is related to how far away this will start being rendered
            Entity roomLocation = new Box(position, 1100, 1100, 1100);
            room.AddSharedData(typeof(Entity), roomLocation);

            UnanimatedModelComponent roomGraphics = new UnanimatedModelComponent(mainGame, room, roomModel, new Vector3(roomScale), Vector3.Zero, Matrix.CreateFromYawPitchRoll(yaw, 0, 0));

            room.AddComponent(typeof(StaticMeshComponent), roomPhysics);
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
            EnemyProximitySpawner eps = new EnemyProximitySpawner((KazgarsRevengeGame)Game, roomGE, EntityType.NormalEnemy, spawnLocs, PROXIMITY, DELAY, 10);
            roomGE.AddComponent(typeof(EnemyProximitySpawner), eps);
            genComponentManager.AddComponent(eps);
        }
        #endregion

        #region Graph Building
        private void buildAndAddPathing(Chunk chunk, ChunkInfo chunkInfo, int i, int j)
        {
            Vector3 chunkLocation = new Vector3(i * CHUNK_SIZE, 0, j * CHUNK_SIZE);

            foreach (Room room in chunk.rooms)
            {
                buildRoomGraph(room, chunkLocation, chunk.rotation);
            }
        }

        private void buildRoomGraph(Room room, Vector3 chunkLocation, Rotation chunkRotation)
        {
            // Holds the center of each block
            ISet<Vector3> blockCenters = new HashSet<Vector3>();
            Vector3 roomCenter = new Vector3(room.location.x, LEVEL_Y, room.location.y) + new Vector3(room.Width, LEVEL_Y, room.Height) / 2 + chunkLocation;
            Vector3 chunkCenter = chunkLocation + new Vector3(CHUNK_SIZE, LEVEL_Y, CHUNK_SIZE) / 2;
            Vector3 roomTopLeft = new Vector3(room.location.x, LEVEL_Y, room.location.y) + chunkLocation;

            foreach (RoomBlock block in room.blocks)
            {
                // The location of a block is relative to the room and chunk's top left corner
                Vector3 blockCenter = chunkLocation + roomTopLeft + new Vector3(block.location.x, LEVEL_Y, block.location.y) + new Vector3(RoomBlock.SIZE, LEVEL_Y, RoomBlock.SIZE) / 2;
                // We need to rotate this location to be correct in the room
                blockCenter = GetRotatedLocation(blockCenter, roomCenter, room.rotation);
                // Then we need to rotate it to the correct location in the chunk
                blockCenter = GetRotatedLocation(blockCenter, chunkCenter, chunkRotation);

                // Set their location a bit above the ground so they don't fall thru
                blockCenter = new Vector3(blockCenter.X, LEVEL_Y, blockCenter.Z);
                
                // These have not been transformed by BLOCK_SIZE yet
                blockCenters.Add(blockCenter);
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
                        Vector3 testBlock = new Vector3(blockCenter.X + i, LEVEL_Y, blockCenter.Z + j);
                        // If a neighbor is one of the blocks in the room, the connect them
                        if (!testBlock.Equals(blockCenter) && blockCenters.Contains(testBlock))
                        {
                            // Transform them by the BLOCK_SIZE
                            Vector3 transBlockCenter = new Vector3(blockCenter.X * BLOCK_SIZE, LEVEL_Y, blockCenter.Z * BLOCK_SIZE);
                            Vector3 transTestBlock = new Vector3(testBlock.X * BLOCK_SIZE, LEVEL_Y, testBlock.Z * BLOCK_SIZE);

                            /* 
                            * AddEdge requires that both nodes are in the graph already,
                             * so add the current block as needed here
                             */
                            if (!currentLevel.pathGraph.ContainsVertex(transBlockCenter))
                            {
                                currentLevel.pathGraph.AddVertex(transBlockCenter);
                            }
                            if (!currentLevel.pathGraph.ContainsVertex(transTestBlock))
                            {
                                currentLevel.pathGraph.AddVertex(transTestBlock);
                            }
                            Console.WriteLine("Adding edge between: {0} and {1}", transBlockCenter, transTestBlock);
                            currentLevel.pathGraph.AddEdge(new Edge<Vector3>(transBlockCenter, transTestBlock));
                        }
                    }
                }
            }

        }
        #endregion

        /// <summary>
        /// Returns a Path from the source location to the destination location.
        /// First element is the given src, last element is the  given dest
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

            // Actually do the computation - TODO hopefully this return correctly...
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

        #region Building Level
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
        }
        #endregion
    }
}
