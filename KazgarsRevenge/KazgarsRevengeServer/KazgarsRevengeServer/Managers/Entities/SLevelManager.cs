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
        SPlayerManager pm;

        public SLevelManager(KazgarsRevengeGame game) : base(game)
        {
            rooms = new List<GameEntity>();
        }

        public override void Initialize()
        {
            base.Initialize();
            nmm = game.Services.GetService(typeof(SNetworkingMessageManager)) as SNetworkingMessageManager;
            pm = game.Services.GetService(typeof(SPlayerManager)) as SPlayerManager;
        }

        #region Level Creation

        public void DemoLevel()
        {
            CreateRoom("Models\\Levels\\tempChunk3", new Vector3(200, 0, -200), MathHelper.PiOver2);

            IDictionary<Identification, Vector3> playerPos = PlacePlayers();
            // ONCE DONE SEND MAP DATA
            ((SMessageSender)game.Services.GetService(typeof(SMessageSender))).SendMapData(this.GetLevelIds(), playerPos);

            game.gameState = GameState.Playing;
        }

        // Creates the collidable stuff for the given room
        // This is only used for demo levels
        public void CreateRoom(string modelPath, Vector3 position, float yaw)
        {
            if (nmm == null)
            {
                nmm = game.Services.GetService(typeof(SNetworkingMessageManager)) as SNetworkingMessageManager;
            }

            if (pm == null)
            {
                pm = game.Services.GetService(typeof(SPlayerManager)) as SPlayerManager;
            }

            if (gcm == null)
            {
                gcm = game.Services.GetService(typeof(GeneralComponentManager)) as GeneralComponentManager;
            }

            position.Y -= 18.5f;
            float roomScale = 10;

            GameEntity room = new GameEntity("room", FactionType.Neutral, EntityType.Misc);

            Model roomCollisionModel = Game.Content.Load<Model>(modelPath);
            Vector3[] verts;
            int[] indices;
            TriangleMesh.GetVerticesAndIndicesFromModel(roomCollisionModel, out verts, out indices);
            StaticMesh roomMesh = new StaticMesh(verts, indices, new AffineTransform(new Vector3(roomScale), Quaternion.CreateFromYawPitchRoll(yaw, 0, 0), position));
            roomMesh.CollisionRules.Group = game.LevelCollisionGroup;
            room.AddSharedData(typeof(StaticMesh), roomMesh);

            StaticMeshComponent roomPhysics = new StaticMeshComponent(game, room);

            // used for drawing...remove?
            Entity roomLocation = new Box(position, 1, 1, 1);
            room.AddSharedData(typeof(Entity), roomLocation);

            room.AddComponent(typeof(StaticMeshComponent), roomPhysics);
            gcm.AddComponent(roomPhysics);

            // Add the Id to be received later
            room.AddSharedData(typeof(Identification), new Identification(DUMMY_ID));

            rooms.Add(room);
        }

        /*
         * Returns the byte array representing the created level
         * If a level is generated as such:
         *      [ 5 | 3 | 2 ]
         *      [ 2 | 1 | 3 ]
         *      [ 4 | 1 | 4 ]
         * Where the width and height are 3 and the numbers in each cell represents the chunk
         * id for that chunk, then the resultant byte[] would be
         *      {5, 3, 2, 2, 1, 3, 4, 1, 4}
         * Since we read the level from left to right, top to bottom     
         * 
         * TODO this will need to be updated once level generation actually works
         */
        private byte[] GetLevelIds()
        {
            // If this breaks, make sure rooms has the right amount of stuff
            byte[] ret = new byte[game.levelWidth * game.levelHeight];

            int i = 0;
            foreach (GameEntity room in rooms)
            {
                ret[i++] = (room.GetSharedData(typeof(Identification)) as Identification).id;
            }

            return ret;
        }

        /*
         * Places players in an already generated level, and updates the playerManager
         * so it knows where everyone is. The returned value is a dictionary mapping player
         * identifications to that player's position
         */ 
        private IDictionary<Identification, Vector3> PlacePlayers()
        {
            Vector3 dummyStartingPos = new Vector3(200, 0, -200);
            int translate = 100;
            IDictionary<Identification, Vector3> ret = new Dictionary<Identification, Vector3>();

            /* Since we allow a max of 5 players, the 'best' arrangement would be 
             * like this
             *      x   x   x
             *      x   x   
             * where the x's are players     
             */ 
            int cols = 3;
            int rows = 2;

            // How many players we've already placed
            int numPlaced = 0;
            for (int i = 0; i < cols; ++i)
            {
                for (int j = 0; j < rows; ++j)
                {
                    // done, we've placed everyone
                    if (numPlaced >= pm.NumPlayers)
                    {
                        break;
                    }

                    // Actual place to put him
                    Vector3 playerLoc = dummyStartingPos + new Vector3(i * translate, 0, j * translate);
                    ret[new Identification((byte)numPlaced)] = playerLoc;

                    // Let the player manager know
                    pm.SetUpPlayer(playerLoc, new Identification((byte)numPlaced));
                    ++numPlaced;
                }
            }

            return ret;
        }

        public override void Update(GameTime gameTime)
        {
            if (game.gameState == GameState.GenerateMap)
            {
                // TODO change this to a real level creation
                DemoLevel();
                game.gameState = GameState.Playing;
            }
            base.Update(gameTime);
        }
        #endregion

        public void Reset()
        {
            rooms.Clear();
        }
    }
}
