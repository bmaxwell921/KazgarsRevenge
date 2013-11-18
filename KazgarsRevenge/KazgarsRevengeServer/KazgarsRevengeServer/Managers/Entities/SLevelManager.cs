using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KazgarsRevenge;
using Microsoft.Xna.Framework;
using BEPUphysics.DataStructures;
using BEPUphysics.Collidables;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics.MathExtensions;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Entities;

namespace KazgarsRevengeServer
{
    public class SLevelManager : SEntityManager
    {
        IList<GameEntity> rooms;

        public SLevelManager(KazgarsRevengeGame game) : base(game)
        {
            rooms = new List<GameEntity>();
        }

        #region Level Creation
        public void CreateLevel(string modelPath, Vector3 position, float yaw)
        {
            float roomScale = 10;

            GameEntity room = new GameEntity("room", "neutral");

            Vector3[] verts;
            int[] indices;
            // Used to get the verts
            Model roomCollisionModel = Game.Content.Load<Model>(modelPath + "Co");
            TriangleMesh.GetVerticesAndIndicesFromModel(roomCollisionModel, out verts, out indices);
            StaticMesh roomMesh = new StaticMesh(verts, indices, new AffineTransform(new Vector3(roomScale), Quaternion.CreateFromYawPitchRoll(yaw, 0, 0), position));
            room.AddSharedData(typeof(StaticMesh), roomMesh);

            StaticMeshComponent roomPhysics = new StaticMeshComponent(game, room);

            //holds the position so the model is drawn correctly
            Entity roomLocation = new Box(position, 1, 1, 1);
            room.AddSharedData(typeof(Entity), roomLocation);

            room.AddComponent(typeof(StaticMeshComponent), roomPhysics);
            gcm.AddComponent(roomPhysics);

            rooms.Add(room);
        }
        #endregion
    }
}
