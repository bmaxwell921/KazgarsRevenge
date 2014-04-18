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
        Ground = 0,
        Dungeon = 1,
        Library = 2,
        TortureChamber = 3,
        Lab = 4,
        GrandHall = 5,
    }

    public class LevelManager : EntityManager
    {
        /// <summary>
        /// Class used to hold information about the current level
        /// </summary>
        public class LevelInfo
        {
            // The floor for the level
            public FloorName currentFloor;

            // The chunk objects that make up this Level
            public Chunk[,] chunks;

            // The information about the chunks in this level
            public ChunkInfo[,] chunkInfos;

            public IList<Vector3> spawnLocs;

            // Keep track of which chunks the player has been in
            public ISet<Vector2> visitedChunks;
            // If they go to a new chunk, then the LocalPlayerController needs to know to update the minimap
            public bool updatedVisited;

            // Convenient accessors for the height and width of the level
            public int width;
            public int height;

            // The graph representing enemy pathing possibilities.
             public AdjacencyGraph<Vector3, Edge<Vector3>> pathGraph;

            public LevelInfo(FloorName currentFloor, Chunk[,] chunks, ChunkInfo[,] chunkInfos, int width, int height)
            {
                this.currentFloor = currentFloor;
                this.chunks = chunks;
                this.chunkInfos = chunkInfos;
                this.spawnLocs = new List<Vector3>();
                this.width = width;
                this.height = height;
                this.visitedChunks = new HashSet<Vector2>();
                this.updatedVisited = false;
                this.pathGraph = new AdjacencyGraph<Vector3, Edge<Vector3>>();
            }
        }

        // In BLOCKS, how far chunks are away from each other
        public static readonly int CHUNK_SIZE = 24;

        // How large a block is in 3D space
        public static readonly int BLOCK_SIZE = 100;
        Vector3 roomScale = new Vector3(10);

        // The y component of the room location
        public static readonly float LEVEL_Y = 0;

        // Put mobs a little bit above the ground so they don't sink down
        public static readonly float MOB_SPAWN_Y = 20;

        public static readonly string ROOM_PATH = @"Models\Levels\Rooms\";

        //public FloorName CurrentFloor = FloorName.Dungeon;

        // The information about the currentLevel
        public LevelInfo currentLevel;

        // Rooms making up this level
        private List<GameEntity> rooms;
        private List<GameEntity> lights = new List<GameEntity>();

        private IDictionary<char, Vector3> pathConnections;

        public LevelManager(KazgarsRevengeGame game)
            : base(game)
        {
            rooms = new List<GameEntity>();
            SetUpPathConnections();
        }

        private void SetUpPathConnections()
        {
            pathConnections = new Dictionary<char, Vector3>();
            pathConnections['N'] = new Vector3(0, 0, -0.5f);
            pathConnections['S'] = new Vector3(0, 0, 0.5f);
            pathConnections['E'] = new Vector3(0.5f, 0, 0);
            pathConnections['W'] = new Vector3(-0.5f, 0, 0);
        }

        public void CreateGround()
        {
            this.rooms = new List<GameEntity>();
            LevelBuilder lb = new LevelBuilder(Game.Services.GetService(typeof(LoggerManager)) as LoggerManager, 1, 1);
            this.currentLevel = lb.BuildGround();
            this.rooms.AddRange(CreateChunkRooms(currentLevel.chunks[0, 0], currentLevel.chunkInfos[0, 0], 0, 0));
            Texture2DUtil.Instance.GetTexture(@"Textures\UI\MegaMap\" + currentLevel.chunkInfos[0, 0].ChunkName);
        }

        /// <summary>
        /// Generates a new level to play with the default width and height in chunks (3x3)
        /// </summary>
        /// <param name="name"></param>
        public void CreateLevel(FloorName name)
        {
            camera.StartLevel(name);
            // Just a special case for the game
            if (name == FloorName.Ground)
            {
                CreateGround();
                return;
            }
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
            // Just a special case for the ground
            if (name == FloorName.Ground)
            {
                CreateGround();
                return;
            }
            this.rooms = new List<GameEntity>();
            LevelBuilder lBuilder = new LevelBuilder(Game.Services.GetService(typeof(LoggerManager)) as LoggerManager, levelWidth, levelHeight);
            this.currentLevel = lBuilder.BuildLevel(name);

            // Go thru each chunkInfo and add the rooms
            for (int i = 0; i < levelWidth; ++i)
            {
                for (int j = 0; j < levelHeight; ++j)
                {
                    this.rooms.AddRange(CreateChunkRooms(currentLevel.chunks[i, j], currentLevel.chunkInfos[i, j], i , j));
                    
                    // Load the map images here so it doesn't lag in game
                    Texture2DUtil.Instance.GetTexture(currentLevel.chunkInfos[i, j].miniMapImgName(true));
                    Texture2DUtil.Instance.GetTexture(currentLevel.chunkInfos[i, j].miniMapImgName(false));
                    Texture2DUtil.Instance.GetTexture(@"Textures\UI\MegaMap\" + currentLevel.chunkInfos[i, j].ChunkName);
                }
            }
        }

        /// <summary>
        /// Returns a location to spawn the player at
        /// </summary>
        /// <returns></returns>
        public Vector3 GetPlayerSpawnLocation()
        {
            Vector3 spawnLoc = currentLevel.spawnLocs[RandSingleton.S_Instance.Next(currentLevel.spawnLocs.Count)];
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

            ProcessObjectMap(chunkInfo.ChunkName + "-objMap", chunkInfo.rotation.ToRadians(), new Vector3(CHUNK_SIZE, LEVEL_Y, CHUNK_SIZE) * BLOCK_SIZE / 2 + chunkLocation * BLOCK_SIZE);

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
            AddRoomGraph(room, roomCenter, chunkLocation, chunkCenter, chunkRotation);
            
            // Here for convenience
            Vector3 roomTopLeft = new Vector3(room.location.x, LEVEL_Y, room.location.y); 
            return roomGE;
        }

        // Adds this room's stuff to the path graph.
        // roomCenter hasn't been transformed to the chunkLocation or rotation
        private void AddRoomGraph(Room room, Vector3 roomCenter, Vector3 chunkLocation, Vector3 chunkCenter, Rotation chunkRotation)
        {
            IList<Vector3> nodes = new List<Vector3>();
            Vector3 rotatedCenter = GetRotatedLocation(roomCenter + chunkLocation, chunkCenter, chunkRotation);

            IList<char> connections = ReadConnections(room.name);
            if (connections == null)
            {
                return;
            }
            foreach (char connection in connections)
            {
                nodes.Add(GetRotatedLocation(roomCenter + chunkLocation + pathConnections[connection], chunkCenter, chunkRotation));
            }

            foreach (Vector3 node in nodes)
            {
                this.AddEdge(new Edge<Vector3>(rotatedCenter * BLOCK_SIZE, node * BLOCK_SIZE));
            }
        }

        private IList<char> ReadConnections(string roomName)
        {
            /*
             * Format:
             *  <ID>-<CONNECTION_LOCS>-<NAME>
             * Example:
             *  0-NSEW-Soulevator
             *  
             * A 0 for the <CONNECTION_LOCS> means there are no connections
             */ 
            char delim = '-';
            string[] splat = roomName.Split(delim);

            if (splat.Count() != 3)
            {
                return null;
            }

            IList<char> connections = new List<char>();
            try
            {
                for (int i = 0; i < splat[1].Count(); ++i)
                {
                    char cur = splat[1][i];
                    if (cur.Equals('0'))
                    {
                        return null;
                    }
                    if (pathConnections.ContainsKey(cur))
                    {
                        connections.Add(cur);
                    }
                }

                return connections;
            }
            catch (Exception)
            {
                (Game.Services.GetService(typeof(LoggerManager)) as LoggerManager).Log(Level.DEBUG, String.Format("Problem parsing roomName: {0}", roomName));
                return null;
            }
        }

        private Vector3 GetRotatedLocation(Vector3 location, Vector3 rotationPoint, Rotation rotationAmt)
        {
            // I can't believe this code from the internet worked!
            return Vector3.Transform(location - rotationPoint, Matrix.CreateRotationY(rotationAmt.ToRadians())) + rotationPoint;
        }

        private GameEntity CreateRoom(string modelPath, Vector3 position, float yaw)
        {
            position.Y = LEVEL_Y;

            GameEntity room = new GameEntity("room", FactionType.Neutral, EntityType.Misc);

            Model roomModel = GetUnanimatedModel(modelPath);

            Vector3[] vertices;
            int[] indices;
            TriangleMesh.GetVerticesAndIndicesFromModel(roomModel, out vertices, out indices);
            StaticMesh roomMesh = new StaticMesh(vertices, indices, new AffineTransform(roomScale, Quaternion.CreateFromYawPitchRoll(yaw, 0, 0), position));
            room.AddSharedData(typeof(StaticMesh), roomMesh);

            StaticMeshComponent roomPhysics = new StaticMeshComponent(mainGame, room);

            //holds the position so the model is drawn correctly (not added to physics)
            //size is related to how far away this will start being rendered
            Entity roomLocation = new Box(position, BLOCK_SIZE, 2, BLOCK_SIZE);
            room.AddSharedData(typeof(Entity), roomLocation);

            UnanimatedModelComponent roomGraphics = new UnanimatedModelComponent(mainGame, room, roomModel, roomScale, Vector3.Zero, yaw, 0, 0);

            room.AddComponent(typeof(StaticMeshComponent), roomPhysics);
            genComponentManager.AddComponent(roomPhysics);

            room.AddComponent(typeof(UnanimatedModelComponent), roomGraphics);
            levelModelManager.AddComponent(roomGraphics);


            return room;
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

        #region Object Map Processing

        GameEntity key;
        List<GameEntity> lockedDoors = new List<GameEntity>();
        List<GameEntity> props = new List<GameEntity>();

        string objectMapDir = "Models\\Levels\\RoomObjectMaps\\";
        private void ProcessObjectMap(string objectMapName, float yaw, Vector3 position)
        {
            Matrix chunkTransform = Matrix.CreateScale(roomScale)
                * Matrix.CreateFromYawPitchRoll(yaw, 0, 0)
                * Matrix.CreateTranslation(position);
            Model objectMapModel = Game.Content.Load<Model>(objectMapDir + objectMapName);

            LevelTagData levelInfo = objectMapModel.Tag as LevelTagData;
            if (levelInfo != null)
            {
                List<Vector3> lightPositions = levelInfo.lightLocations;
                List<Color> lightColors = levelInfo.lightColors;
                for (int i = 0; i < lightPositions.Count; ++i)
                {
                    Color c = Color.White;
                    if (i < lightColors.Count)
                    {
                        c = lightColors[i];
                    }
                    CreatePointLight(Vector3.Transform(lightPositions[i], chunkTransform), c);
                }

                List<Vector3> soulpos = levelInfo.soulLocations;
                if (soulpos.Count > 0)
                {
                    CreateSoulevator(Vector3.Transform(soulpos[0], chunkTransform));
                }

                List<Vector3> ewLockedDoors = levelInfo.ewLockedDoorLocations;
                foreach (Vector3 v in ewLockedDoors)
                {
                    CreateLockedDoor(Vector3.Transform(v, chunkTransform), yaw + MathHelper.PiOver2);
                }

                List<Vector3> nsLockedDoors = levelInfo.nsLockedDoorLocations;
                foreach (Vector3 v in nsLockedDoors)
                {
                    CreateLockedDoor(Vector3.Transform(v, chunkTransform), yaw);
                }

                List<Vector3> ewOpenDoors = levelInfo.ewOpenDoorLocations;
                foreach (Vector3 v in ewLockedDoors)
                {
                    CreateOpenDoor(Vector3.Transform(v, chunkTransform), yaw + MathHelper.PiOver2);
                }

                List<Vector3> nsOpenDoors = levelInfo.nsOpenDoorLocations;
                foreach (Vector3 v in nsLockedDoors)
                {
                    CreateOpenDoor(Vector3.Transform(v, chunkTransform), yaw);
                }

                List<Vector3> hangingLightProps = levelInfo.hangingLightLocations;
                foreach (Vector3 v in hangingLightProps)
                {
                    CreateHangingLightProp(Vector3.Transform(v, chunkTransform));
                }

                List<Vector3> smallObjects = levelInfo.groundPropLocations;
                foreach (Vector3 v in smallObjects)
                {
                    CreateProp(Vector3.Transform(v, chunkTransform));
                }

                List<Vector3> mobSpawnLocations = levelInfo.mobSpawnLocations;
                foreach (Vector3 v in mobSpawnLocations)
                {
                    CreateMobSpawner(Vector3.Transform(v, chunkTransform));
                }

                List<Vector3> playerspawns = levelInfo.playerSpawnLocations;
                foreach (Vector3 v in playerspawns)
                {
                    AddPlayerSpawn(Vector3.Transform(v, chunkTransform));
                }

                List<Vector3> bossSpawns = levelInfo.bossSpawnLocations;
                if (bossSpawns.Count > 0)
                {
                    (Game.Services.GetService(typeof(EnemyManager)) as EnemyManager).CreateBoss(IdentificationFactory.getId(EntityType.Boss, Identification.NO_CLIENT), Vector3.Transform(bossSpawns[RandSingleton.U_Instance.Next(bossSpawns.Count)], chunkTransform));
                }

                List<Vector3> keyLocations = levelInfo.keyLocations;
                if (keyLocations.Count > 0)
                {
                    CreateKey(Vector3.Transform(keyLocations[RandSingleton.U_Instance.Next(keyLocations.Count)], chunkTransform));
                }

                List<Vector3> emitterLocations = levelInfo.emitterLocations;
                foreach (Vector3 v in emitterLocations)
                {
                    CreateTorchEmitter(Vector3.Transform(v, chunkTransform));
                }

            }
        }

        private void CreatePointLight(Vector3 position, Color color)
        {
            GameEntity light = new GameEntity("light", FactionType.Neutral, EntityType.None);

            position.Y = 40;
            Entity physicalData = new Box(position, .1f, .1f, .1f);
            physicalData.IsAffectedByGravity = false;
            physicalData.LocalInertiaTensorInverse = new Matrix3X3();
            physicalData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoBroadPhase;
            light.AddSharedData(typeof(Entity), physicalData);

            light.AddSharedData(typeof(Color), color);

            PhysicsComponent lightPhysics = new PhysicsComponent(mainGame, light);

            light.AddSharedData(typeof(PhysicsComponent), lightPhysics);
            genComponentManager.AddComponent(lightPhysics);

            lights.Add(light);
        }

        private void CreateKey(Vector3 pos)
        {
            pos.Y = 40;

            GameEntity entity = new GameEntity("key", FactionType.Neutral, EntityType.Misc);

            Entity physicalData = new Box(pos, 40, 40, 40);
            physicalData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            entity.AddSharedData(typeof(Entity), physicalData);

            PhysicsComponent physics = new PhysicsComponent(mainGame, entity);
            entity.AddComponent(typeof(PhysicsComponent), physics);
            genComponentManager.AddComponent(physics);

            UnanimatedModelComponent graphics = new UnanimatedModelComponent(mainGame, entity, GetUnanimatedModel("Models\\key_1"), new Vector3(25), Vector3.Zero, 0, 0, 0);
            graphics.AddYawSpeed(.05f);
            graphics.AddEmitter(typeof(KeyGlowSystem), "glow", 25, 1, Vector3.Up * 20);
            entity.AddComponent(typeof(UnanimatedModelComponent), graphics);
            modelManager.AddComponent(graphics);

            KeyController controller = new KeyController(mainGame, entity);
            entity.AddComponent(typeof(KeyController), controller);
            genComponentManager.AddComponent(controller);

            key = entity;
        }

        private void CreateSoulevator(Vector3 pos)
        {
            GameEntity prop = new GameEntity("prop", FactionType.Neutral, EntityType.None);

            Entity physicalData = new Box(pos, 60, 5, 60);
            physicalData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            prop.AddSharedData(typeof(Entity), physicalData);

            PhysicsComponent physics = new PhysicsComponent(mainGame, prop);
            prop.AddComponent(typeof(PhysicsComponent), physics);
            genComponentManager.AddComponent(physics);

            UnanimatedModelComponent graphics = new UnanimatedModelComponent(mainGame, prop, GetUnanimatedModel("Models\\Levels\\Props\\soulevator"), new Vector3(10, 100 ,10), Vector3.Down * 60, 0, 0, 0);
            graphics.TurnOffOutline();
            graphics.SetAlpha(.5f);
            graphics.AddYawSpeed(.01f);
            graphics.AddEmitter(typeof(SoulevatorMistSystem), "mist", 65, 85, 0, Vector3.Zero);
            prop.AddComponent(typeof(UnanimatedModelComponent), graphics);
            levelModelManager.AddComponent(graphics);

            SoulevatorController controller = new SoulevatorController(mainGame, prop);
            prop.AddComponent(typeof(SoulevatorController), controller);
            genComponentManager.AddComponent(controller);

            rooms.Add(prop);
        }

        private void CreateOpenDoor(Vector3 pos, float yaw)
        {
            pos.Y += 20;
            GameEntity door = new GameEntity("room", FactionType.Neutral, EntityType.None);

            Entity physicalData = new Box(pos, BLOCK_SIZE * .4f, 30, 5);
            physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(yaw, 0, 0);
            door.AddSharedData(typeof(Entity), physicalData);

            UnanimatedModelComponent doorGraphics = new UnanimatedModelComponent(mainGame, door, GetUnanimatedModel("Models\\Levels\\Props\\01-opendoor"), roomScale, Vector3.Down * 20, 0, 0, 0);
            door.AddComponent(typeof(UnanimatedModelComponent), doorGraphics);
            modelManager.AddComponent(doorGraphics);

            props.Add(door);
        }

        private void CreateLockedDoor(Vector3 pos, float yaw)
        {
            pos.Y += 20;
            GameEntity door = new GameEntity("room", FactionType.Neutral, EntityType.None);

            Entity physicalData = new Box(pos, BLOCK_SIZE * .4f, 30, 5);
            physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(yaw, 0, 0);
            door.AddSharedData(typeof(Entity), physicalData);

            PhysicsComponent doorPhysics = new PhysicsComponent(mainGame, door);
            door.AddComponent(typeof(PhysicsComponent), doorPhysics);
            genComponentManager.AddComponent(doorPhysics);

            UnanimatedModelComponent doorGraphics = new UnanimatedModelComponent(mainGame, door, GetUnanimatedModel("Models\\Levels\\Props\\01-closedDoor"), roomScale, Vector3.Down * 20, 0, 0, 0);
            door.AddComponent(typeof(UnanimatedModelComponent), doorGraphics);
            modelManager.AddComponent(doorGraphics);

            lockedDoors.Add(door);
        }

        private void CreateHangingLightProp(Vector3 pos)
        {
            GameEntity lightProp = new GameEntity("prop", FactionType.Neutral, EntityType.None);

            Entity physicalData = new Box(pos, 1, 1, 1);
            lightProp.AddSharedData(typeof(Entity), physicalData);

            UnanimatedModelComponent graphics = new UnanimatedModelComponent(mainGame, lightProp, GetUnanimatedModel("Models\\Levels\\Props\\01-lightFixture"), new Vector3(10), Vector3.Zero, 0, 0, 0);
            graphics.TurnOffOutline();
            lightProp.AddComponent(typeof(UnanimatedModelComponent), graphics);
            modelManager.AddComponent(graphics);

            props.Add(lightProp);
        }

        private void CreateProp(Vector3 pos)
        {
            if (RandSingleton.U_Instance.Next(100) < 5)
            {
                GameEntity prop = new GameEntity("prop", FactionType.Neutral, EntityType.None);

                Entity physicalData = new Box(pos, 1, 1, 1);
                prop.AddSharedData(typeof(Entity), physicalData);

                UnanimatedModelComponent graphics = new UnanimatedModelComponent(mainGame, prop, GetUnanimatedModel("Models\\Levels\\Props\\01-chair"), new Vector3(10), Vector3.Zero, 0, 0, 0);
                prop.AddComponent(typeof(UnanimatedModelComponent), graphics);
                modelManager.AddComponent(graphics);

                props.Add(prop);
            }
        }

        // Players need to be within 60 units for the thing to spawn. Based on the fact that chunks are 240x240
        // TODO Set these based on a difficulty level???
        private static readonly float PROXIMITY = 60;
        // Spawn every 3 seconds
        private static readonly float DELAY = 4000;

        private void CreateMobSpawner(Vector3 pos)
        {
            if (RandSingleton.U_Instance.Next(1000) < 20)
            {
                GameEntity spawner = new GameEntity("spawner", FactionType.Neutral, EntityType.None);

                List<Vector3> spawnLocs = new List<Vector3>();
                spawnLocs.Add(pos);

                EntityType type = EntityType.NormalEnemy;
                int num = 1;
                int r = RandSingleton.U_Instance.Next(100);
                if (r < 50)
                {//normal cluster
                    num = RandSingleton.U_Instance.Next(3, 7);
                }
                else if (r < 60)
                {//elite single
                    type = EntityType.EliteEnemy;
                }
                else if (r < 67)
                {//elite cluster
                    type = EntityType.EliteEnemy;
                    num = RandSingleton.U_Instance.Next(2, 5);
                }

                //add some randomness to the delay to get an even distribution of spawn times
                float delayAdd = RandSingleton.U_Instance.Next(1500) - 750;
                EnemyProximitySpawner eps = new EnemyProximitySpawner(mainGame, spawner, type, spawnLocs, PROXIMITY * CHUNK_SIZE, DELAY + delayAdd, num);
                spawner.AddComponent(typeof(EnemyProximitySpawner), eps);
                genComponentManager.AddComponent(eps);

                rooms.Add(spawner);
            }
        }

        private void AddPlayerSpawn(Vector3 pos)
        {
            pos.Y = MOB_SPAWN_Y;
            currentLevel.spawnLocs.Add(pos);
        }

        private void CreateTorchEmitter(Vector3 pos)
        {
            GameEntity torch = new GameEntity("", FactionType.Neutral, EntityType.None);

            Entity physicalData = new Box(pos, 1, 1, 1);
            torch.AddSharedData(typeof(Entity), physicalData);

            UnanimatedModelComponent graphics = new UnanimatedModelComponent(mainGame, torch);
            graphics.AddEmitter(typeof(TorchSystem), "", 1, 2, Vector3.Zero);
            torch.AddComponent(typeof(UnanimatedModelComponent), graphics);
            modelManager.AddComponent(graphics);
            
            props.Add(torch);
        }
        #endregion

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
            Edge<Vector3> reverse = new Edge<Vector3>(edge.Target, edge.Source);
            if (!currentLevel.pathGraph.ContainsEdge(reverse))
            {
                currentLevel.pathGraph.AddEdge(reverse);
            }
        }

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
            Vector3 destNode = FindClosestNodeTo(dest, dest);
            Vector3 srcNode = FindClosestNodeTo(src, destNode);

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
                return Vector3.Distance(vect, destNode);
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

        // Returns the Vector3 node in the pathGraph closest to the given location. Taking into account the final location we're trying to get to
        private Vector3 FindClosestNodeTo(Vector3 location, Vector3 dest)
        {
            Vector3 min = Vector3.Zero;
            // Set this as null for the first time around
            double? minDist = null;

            foreach (Vector3 node in currentLevel.pathGraph.Vertices)
            {
                double thisDist = Vector3.Distance(location, node);
                
                // If it's not broke, don't fix it.
                double heuristic = 0;
                if (minDist == null || thisDist + heuristic < minDist)
                {
                    min = node;
                    minDist = thisDist + heuristic;
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

            /*
             * Individual number of each chunk:
             *  [0][1][2]
             *  [3][4][5]
             *  [6][7][8]
             */ 
            private IList<int> chunkNums;

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
            private void SetLevelBounds(int levelWidth, int levelHeight)
            {
                this.levelWidth = levelWidth;
                this.levelHeight = levelHeight;
            }

            private void fillChunkNums()
            {
                chunkNums = new List<int>();
                for (int i = 0; i < levelWidth * levelHeight; ++i)
                {
                    // Middle is reserved for the soulevator
                    if (i == levelWidth * levelHeight / 2)
                    {
                        continue;
                    }
                    chunkNums.Add(i);
                }
            }

            /// <summary>
            /// Builds the Level 0 floor
            /// </summary>
            /// <returns></returns>
            public LevelInfo BuildGround()
            {
                ChunkInfo[,] chunkInfos = new ChunkInfo[1, 1];
                chunkInfos[0, 0] = ChunkUtil.Instance.GetGround();
                Chunk[,] chunks = ReadChunks(FloorName.Ground, chunkInfos);
                return new LevelInfo(FloorName.Ground, chunks, chunkInfos, 1, 1);
            }

            /// <summary>
            /// Builds a Level based on the given floorName and returns a list of all the rooms 
            /// in the level
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public LevelInfo BuildLevel(FloorName name)
            {
                fillChunkNums();
                ChunkInfo[,] chunkInfos = new ChunkInfo[levelWidth, levelHeight];
                // First we gotta figure out what chunks to place
                ChooseChunks(name, chunkInfos);

                Chunk[,] chunks = ReadChunks(name, chunkInfos);
                return new LevelInfo(name, chunks, chunkInfos, levelWidth, levelHeight);
            }

            #region Choosing Chunks
            private void ChooseChunks(FloorName name, ChunkInfo[,] chunks)
            {
                PlaceSoulevator(name, chunks);
                Vector2 bossLoc = PlaceBoss(name, chunks);
                PlaceKey(name, chunks, bossLoc);
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
                int chunkNum = chunkNums[RandSingleton.S_Instance.Next(0, chunkNums.Count)];
                
                // Convert from 1D to 2D
                int xCoord = chunkNum % levelWidth;
                int yCoord = chunkNum / levelWidth;

                ISet<Direction> reqDirs = GetRequiredDirections(name, chunks, xCoord, yCoord);
                chunks[xCoord, yCoord] = ChunkUtil.Instance.GetSatisfyingChunk(name, ChunkType.BOSS, reqDirs);
                this.LogChunkChoice(xCoord, yCoord, chunks[xCoord, yCoord]);
                return new Vector2(xCoord, yCoord);
            }

            private void PlaceKey(FloorName name, ChunkInfo[,] chunks, Vector2 bossLoc)
            {
                /*
                 * Ok here's the idea: we've chose the bossLoc from the chunkNums array:
                 *      [0, 1, 2, 3, 5, 6, 7, 8] - for a normal level. 
                 * Let's say 8 was chosen. In this case we want the key to be pretty far from the boss
                 * the farthest chunks from 8 are [0, 3]. So basically we figure out where the bossLoc
                 * is in the chunkNums array, then choose a key location from the other half
                 *  
                 */ 

                // Convert bossLoc back to 1D
                int bossNum = (int)bossLoc.X + levelWidth * (int)bossLoc.Y;

                int index = chunkNums.IndexOf(bossNum);

                int min = 0;
                int max = 0;

                if (index >= chunkNums.Count / 2)
                {
                    max = chunkNums.Count / 2;
                }
                else
                {
                    min = chunkNums.Count / 2;
                    max = chunkNums.Count;
                }

                int keyNum = chunkNums[RandSingleton.S_Instance.Next(min, max)];
                int xCoord = keyNum % levelWidth;
                int yCoord = keyNum / levelWidth;

                ISet<Direction> reqDirs = GetRequiredDirections(name, chunks, xCoord, yCoord);
                chunks[xCoord, yCoord] = ChunkUtil.Instance.GetSatisfyingChunk(name, ChunkType.KEY, reqDirs);
                this.LogChunkChoice(xCoord, yCoord, chunks[xCoord, yCoord]);
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

        #region Level Script Stuff
        public GameEntity CreateDragonFirePillar(AliveComponent dragon)
        {
            Entity data = dragon.Entity.GetSharedData(typeof(Entity)) as Entity;
            Vector3 position = data.Position + Vector3.Right * 200 + Vector3.Up * 20;
            GameEntity pillar = new GameEntity("firepillar", FactionType.Enemies, EntityType.Misc);

            Entity physicalData = new Box(position, 50, 50, 50);
            physicalData.LocalInertiaTensorInverse = new Matrix3X3();
            pillar.AddSharedData(typeof(Entity), physicalData);

            PhysicsComponent physics = new PhysicsComponent(mainGame, pillar);
            pillar.AddComponent(typeof(PhysicsComponent), physics);
            genComponentManager.AddComponent(physics);

            UnanimatedModelComponent graphics = new UnanimatedModelComponent(mainGame, pillar, GetUnanimatedModel("Models\\Levels\\Props\\fire_column"), new Vector3(10), Vector3.Down * 20, 0, 0, 0);
            graphics.SlideAnimateTexture(.1f);
            graphics.AddEmitter(typeof(FirePillarMistSystem), "firemist", 40, 25, Vector3.Down * 17);
            graphics.AddEmitter(typeof(FirePillarSystem), "fire", 10, 25, Vector3.Down * 25);
            pillar.AddComponent(typeof(UnanimatedModelComponent), graphics);
            modelManager.AddComponent(graphics);

            PillarBeamBillboard beam = new PillarBeamBillboard(mainGame, pillar, dragon.Entity.GetSharedData(typeof(Entity)) as Entity, false);
            pillar.AddComponent(typeof(PillarBeamBillboard), beam);
            billboardManager.AddComponent(beam);

            PillarController AI = new PillarController(mainGame, pillar);
            pillar.AddComponent(typeof(PillarController), AI);
            genComponentManager.AddComponent(AI);

            misc.Add(pillar);

            return pillar;
        }

        public GameEntity CreateDragonFrostPillar(AliveComponent dragon)
        {
            Entity data = dragon.Entity.GetSharedData(typeof(Entity)) as Entity;
            Vector3 position = data.Position + Vector3.Left * 200 + Vector3.Up * 20;
            GameEntity pillar = new GameEntity("frostpillar", FactionType.Enemies, EntityType.Misc);

            Entity physicalData = new Box(position, 50, 50, 50);
            physicalData.LocalInertiaTensorInverse = new Matrix3X3();
            pillar.AddSharedData(typeof(Entity), physicalData);

            PhysicsComponent physics = new PhysicsComponent(mainGame, pillar);
            pillar.AddComponent(typeof(PhysicsComponent), physics);
            genComponentManager.AddComponent(physics);

            UnanimatedModelComponent graphics = new UnanimatedModelComponent(mainGame, pillar, GetUnanimatedModel("Models\\Levels\\Props\\ice_column"), new Vector3(10), Vector3.Down * 20, 0, 0, 0);
            pillar.AddComponent(typeof(UnanimatedModelComponent), graphics);
            modelManager.AddComponent(graphics);
            graphics.AddEmitter(typeof(FrostAOEMistSystem), "mist", 40, 25, Vector3.Down * 17);

            PillarBeamBillboard beam = new PillarBeamBillboard(mainGame, pillar, dragon.Entity.GetSharedData(typeof(Entity)) as Entity, true);
            pillar.AddComponent(typeof(PillarBeamBillboard), beam);
            billboardManager.AddComponent(beam);

            PillarController AI = new PillarController(mainGame, pillar);
            pillar.AddComponent(typeof(PillarController), AI);
            genComponentManager.AddComponent(AI);

            misc.Add(pillar);

            return pillar;
        }

        public void UnlockDoors()
        {
            for (int i = 0; i < lockedDoors.Count; ++i)
            {
                lockedDoors[i].KillEntity();
            }
            lockedDoors.Clear();
        }
        #endregion

        public override void Update(GameTime gameTime)
        {
            for (int i = misc.Count - 1; i >= 0; --i)
            {
                if (misc[i].Dead)
                {
                    misc.RemoveAt(i);
                }
            }
            base.Update(gameTime);
        }

        List<GameEntity> misc = new List<GameEntity>();
        public GameEntity AddPathMarker(Vector3 position, string name)
        {
            GameEntity marker = new GameEntity("marker", FactionType.Neutral, EntityType.None);

            Entity physicalData = new Box(position, 1, 1, 1);
            marker.AddSharedData(typeof(Entity), physicalData);

            string modelPath;
            switch (name)
            {
                case "Animated Armor":
                    modelPath = "Models\\Weapons\\axe";
                    break;
                case "Skeleton":
                    modelPath = "Models\\Weapons\\bow01";
                    break;
                default:
                    modelPath = "Models\\Weapons\\sword01";
                    break;
            }
            UnanimatedModelComponent markerGraphics = new UnanimatedModelComponent(mainGame, marker, GetUnanimatedModel(modelPath), new Vector3(10), Vector3.Zero, 0, 0, 0);
            marker.AddComponent(typeof(UnanimatedModelComponent), markerGraphics);
            modelManager.AddComponent(markerGraphics);

            misc.Add(marker);

            return marker;
        }

        public IDictionary<string, string> getMiniImageMap()
        {
            // Go thru all the current level's chunk infos and get the correct minimap image
            IDictionary<string, string> map = new Dictionary<string, string>();
            if (currentLevel == null)
            {
                throw new Exception("Tried to get miniMap before the level was generated!");
            }
            string keyPre = "Loc";
            for (int i = 0; i < currentLevel.width; ++i)
            {
                for (int j = 0; j < currentLevel.height; ++j)
                {
                    int num = i + j * currentLevel.width;

                    // If the chunk has been visited then show it, otherwise just a grey square
                    if (currentLevel.visitedChunks.Contains(new Vector2(i, j)))
                    {
                        map[keyPre + num] = currentLevel.chunkInfos[i, j].miniMapImgName(true);
                    }
                    else
                    {
                        map[keyPre + num] = TextureStrings.UI.MiniMap.UNKNOWN;
                    }
                }
            }

            return map;
        }

        /// <summary>
        /// Called by LocalPlayerController to see if it needs to update
        /// the images for the miniMap
        /// </summary>
        /// <returns></returns>
        public bool NeedsMiniMapUpdate()
        {
            bool ret = currentLevel.updatedVisited;
            // Reset it so we aren't constantly updating the minimap
            currentLevel.updatedVisited = false;
            return ret;
        }

        // Returns the chunk number that the player is currently in
        public int GetCurrentChunk(Vector3 location)
        {
            int xCoord = (int)location.X / (CHUNK_SIZE * BLOCK_SIZE);
            int yCoord = (int)location.Z / (CHUNK_SIZE * BLOCK_SIZE);

            // This method is always called (for drawing the minimap), so we only need to update the visited chunks here
            Vector2 visited = new Vector2(xCoord, yCoord);
            if (!currentLevel.visitedChunks.Contains(visited))
            {
                currentLevel.visitedChunks.Add(visited);
                currentLevel.updatedVisited = true;
            }
            return xCoord + yCoord * currentLevel.width;
        }

        // Returns the image name of the chunk the player is currently in
        public string GetCurrentChunkImgName(Vector3 location)
        {
            int xCoord = (int)location.X / (CHUNK_SIZE * BLOCK_SIZE);
            int yCoord = (int)location.Z / (CHUNK_SIZE * BLOCK_SIZE);
            return @"Textures\UI\MegaMap\" + currentLevel.chunkInfos[xCoord, yCoord].ChunkName;
        }

        // Gets rotation of the chunk the player is currently in
        public Rotation GetCurrentChunkRotation(Vector3 location)
        {
            int xCoord = (int)location.X / (CHUNK_SIZE * BLOCK_SIZE);
            int yCoord = (int)location.Z / (CHUNK_SIZE * BLOCK_SIZE);
            return currentLevel.chunkInfos[xCoord, yCoord].rotation;
        }
    }
}
