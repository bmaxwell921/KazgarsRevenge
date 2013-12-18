using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KazgarsRevenge;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;

namespace KazgarsRevengeServer
{
    public class SPlayerManager : SEntityManager
    {
        // Map of Identifications to players
        // HOST IS ALWAYS IDENTIFICATION 0 
        public Dictionary<Identification, GameEntity> players
        {
            get;
            protected set;
        }

        public int NumPlayers
        {
            get
            {
                return players.Keys.Count;
            }
        }

        public SPlayerManager(KazgarsRevengeGame game)
            : base(game)
        {
            players = new Dictionary<Identification, GameEntity>();
        }

        /// <summary>
        /// Creates and adds a new player to the game, returning the new player's identification
        /// </summary>
        /// <returns></returns>
        public Identification GetId()
        {
            Identification newId = IdentificationFactory.GenerateNextId();
            players[newId] = null;
            return newId;
        }

        public void SetUpPlayer(Vector3 position, Identification id)
        {
            if (gcm == null)
            {
                gcm = game.Services.GetService(typeof(GeneralComponentManager)) as GeneralComponentManager;
            }
            GameEntity player = new GameEntity("netPlayer" + id.id, FactionType.Players);
            
            // shared physical data
            Entity playerPhysicalData = new Cylinder(position, 37, 6, 2);
            // assigning collision group to the physics
            playerPhysicalData.CollisionInformation.CollisionRules.Group = game.PlayerCollisionGroup;

            //no friction
            playerPhysicalData.Material.KineticFriction = 0;
            playerPhysicalData.Material.StaticFriction = 0;

            // lock rotation go bumping into other players doesn't cause it to tip over
            playerPhysicalData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            // more accurate collision detection for player
            playerPhysicalData.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;
            player.AddSharedData(typeof(Entity), playerPhysicalData);

            HealthData playerHealth = new HealthData(100);
            player.AddSharedData(typeof(HealthData), playerHealth);

            // components to make up player
            PhysicsComponent playerPhysics = new PhysicsComponent(game, player);
            HealthHandlerComponent playerHealthHandler = new HealthHandlerComponent(game, player);

            // TODO make controller for players? - is this going to be the same as the client?

            player.AddComponent(typeof(PhysicsComponent), playerPhysics);
            gcm.AddComponent(playerPhysics);

            player.AddComponent(typeof(HealthHandlerComponent), playerHealthHandler);
            gcm.AddComponent(playerHealthHandler);

            players[id] = player;
        }

        public override void Update(GameTime gameTime)
        {
            MessageQueue mq = game.Services.GetService(typeof(MessageQueue)) as MessageQueue;
            IList<BaseMessage> msgs = mq.GetAllOf(typeof(VelocityMessage));

            foreach (BaseMessage bm in msgs)
            {
                VelocityMessage vm = (VelocityMessage) bm;
                SetPlayerVel(vm.id, vm.vel);
            }

            foreach (KeyValuePair<Identification, GameEntity> k in players)
            {
                if (k.Value != null && playerVels.ContainsKey(k.Key))
                {
                    (k.Value.GetSharedData(typeof(Entity)) as Entity).LinearVelocity = playerVels[k.Key];
                }
            }
            base.Update(gameTime);
        }

        Dictionary<Identification, Vector3> playerVels = new Dictionary<Identification, Vector3>();
        private void SetPlayerVel(Identification id, Vector3 vel)
        {
            //(players[id].GetSharedData(typeof(Entity)) as Entity).LinearVelocity = vel;
            if (!playerVels.ContainsKey(id))
            {
                playerVels.Add(id, vel);
            }
            else
            {
                playerVels[id] = vel;
            }
        }

        public Vector3 GetPlayerPosition(Identification id)
        {
            return (players[id].GetSharedData(typeof(Entity)) as Entity).Position;
        }

        public void SetPlayerLocation(Vector3 pos, Identification id)
        {
            Entity sharedData = players[id].GetSharedData(typeof(Entity)) as Entity;
            sharedData.Position = pos;
        }
    }
}
