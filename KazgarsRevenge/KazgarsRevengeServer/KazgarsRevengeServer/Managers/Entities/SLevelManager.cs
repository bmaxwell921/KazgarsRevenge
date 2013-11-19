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
        private readonly byte DUMMY_ID = 0;
        IList<GameEntity> rooms;

        SNetworkingMessageManager nmm;

        public SLevelManager(KazgarsRevengeGame game) : base(game)
        {
            rooms = new List<GameEntity>();
        }

        public override void Initialize()
        {
            base.Initialize();
            nmm = game.Services.GetService(typeof(SNetworkingMessageManager)) as SNetworkingMessageManager;
        }

        #region Level Creation
        public void CreateLevel(string modelPath, Vector3 position, float yaw)
        {
            if (nmm == null)
            {
                nmm = game.Services.GetService(typeof(SNetworkingMessageManager)) as SNetworkingMessageManager;
            }
            float roomScale = 10;

            GameEntity room = new GameEntity("room", FactionType.Neutral);

            Vector3[] verts;
            int[] indices;
            // Used to get the verts
            Model roomCollisionModel = Game.Content.Load<Model>(modelPath);
            TriangleMesh.GetVerticesAndIndicesFromModel(roomCollisionModel, out verts, out indices);
            StaticMesh roomMesh = new StaticMesh(verts, indices, new AffineTransform(new Vector3(roomScale), Quaternion.CreateFromYawPitchRoll(yaw, 0, 0), position));
            room.AddSharedData(typeof(StaticMesh), roomMesh);

            StaticMeshComponent roomPhysics = new StaticMeshComponent(game, room);

            //holds the position so the model is drawn correctly
            Entity roomLocation = new Box(position, 1, 1, 1);
            room.AddSharedData(typeof(Entity), roomLocation);

            room.AddComponent(typeof(StaticMeshComponent), roomPhysics);
            ((KazgarsRevengeGame)Game).genComponentManager.AddComponent(roomPhysics);

            rooms.Add(room);

            // ONCE DONE SEND MAP DATA
            nmm.SendLevel(this.GetLevelCSV());
        }

        public void CreateDemoLevel()
        {
            CreateLevel("Models\\Levels\\4x4Final02Co", new Vector3(200, -20, -200), 0);
        }

        public override void Update(GameTime gameTime)
        {
            if (game.gameState == GameState.GenerateMap)
            {
                // TODO change this to a real level creation
                CreateDemoLevel();
                game.gameState = GameState.Playing;
            }
            base.Update(gameTime);
        }

        // Returns the CSV for the level, using ids as Identifiers
        // TODO actually implement this
        public string GetLevelCSV()
        {
            /*
             * TODO do we need to send positions? I would think that if both the server
             * and the client knew what the common starting point was, there would be no problem
             * with the client just calculating positions of things
             */
            return DUMMY_ID.ToString();
        }
        #endregion
    }
}
