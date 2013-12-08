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

    public enum FloorNames
    {
        Dungeon,
        Library,
        TortureChamber,
        Lab,
        GrandHall,
    }

    public class LevelManager : EntityManager
    {
        List<GameEntity> rooms = new List<GameEntity>();
        public LevelManager(KazgarsRevengeGame game)
            : base(game)
        {
            ReadFile("Dungeon");
        }

        public void DemoLevel()
        {
            //CreateRoom("Models\\Levels\\tempChunk", new Vector3(200, -20, -200), MathHelper.PiOver2);
            CreateChunk("Dungeon1", new Vector3(120, 0, -200), 0);
        }

        Dictionary<string, List<RoomData>> chunkDefinitions = new Dictionary<string, List<RoomData>>();
        string levelPath = "Models\\Levels\\";
        public void ReadFile(string floorName)
        {
            string filetext;
            try
            {
                using (StreamReader sr = new StreamReader(floorName + ".txt"))
                {
                    filetext = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't read the level file. " + e.Message);
            }

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
            }
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
            position.Y -= 20;
            float roomScale = 10;

            GameEntity room = new GameEntity("room", FactionType.Players);


            Model roomModel = GetUnanimatedModel(modelPath);

            Model roomCollisionModel = Game.Content.Load<Model>(modelPath);// + "Co");
            Vector3[] verts;
            int[] indices;
            TriangleMesh.GetVerticesAndIndicesFromModel(roomCollisionModel, out verts, out indices);
            StaticMesh roomMesh = new StaticMesh(verts, indices, new AffineTransform(new Vector3(roomScale), Quaternion.CreateFromYawPitchRoll(yaw, 0, 0), position));
            room.AddSharedData(typeof(StaticMesh), roomMesh);

            StaticMeshComponent roomPhysics = new StaticMeshComponent(mainGame, room);

            //holds the position so the model is drawn correctly
            Entity roomLocation = new Box(position, 1, 1, 1);
            room.AddSharedData(typeof(Entity), roomLocation);

            UnanimatedModelComponent roomGraphics = new UnanimatedModelComponent(mainGame, room, roomModel, new Vector3(10), Vector3.Zero, Matrix.CreateFromYawPitchRoll(yaw, 0, 0));

            room.AddComponent(typeof(StaticMeshComponent), roomPhysics);
            genComponentManager.AddComponent(roomPhysics);

            room.AddComponent(typeof(UnanimatedModelComponent), roomGraphics);
            modelManager.AddComponent(roomGraphics);


            rooms.Add(room);

            players.Reset();
        }
        #endregion

        // Reads the map data from a CSV string and builds the level, moves gameState into playing
        public void CreateMapFrom(string map)
        {
            // TODO actually implement
            float yaw;
            string modelPath = this.TranslateIdToPathYaw(0, out yaw);
            CreateRoom(modelPath, new Vector3(200, -20, -200), yaw);

            mainGame.gameState = GameState.Playing;
        }

        // Translates the given room id into the path to that respective model and yaw (rotation) for a room
        private string TranslateIdToPathYaw(int id, out float yaw)
        {
            yaw = 0;
            return "Models\\Levels\\tempChunk";
        }
    }
}
