using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    class LevelManager : EntityManager
    {
        List<GameEntity> rooms = new List<GameEntity>();
        public LevelManager(MainGame game)
            : base(game)
        {
            
        }

        public void DemoLevel()
        {
            CreateLevel("Models\\Levels\\4x4Final02", new Vector3(200, -20, -200), 0);
        }

        #region Level Creation
        public void CreateLevel(string modelPath, Vector3 position, float yaw)
        {
            float roomScale = 10;

            GameEntity room = new GameEntity("room", "neutral");


            Model roomModel = GetUnanimatedModel(modelPath);

            Model roomCollisionModel = Game.Content.Load<Model>(modelPath + "Co");
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
        }
        #endregion
    }
}
