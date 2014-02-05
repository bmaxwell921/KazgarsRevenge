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
            Identification newId = IdentificationFactory.getId(EntityType.Player, 0);
            newId.client = newId.id;
            players[newId] = null;
            return newId;
        }

        public void SetUpPlayer(Vector3 position, Identification id)
        {
            if (gcm == null)
            {
                gcm = game.Services.GetService(typeof(GeneralComponentManager)) as GeneralComponentManager;
            }
            GameEntity player = new GameEntity("netPlayer" + id.id, FactionType.Players, EntityType.Player);
            
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

            // components to make up player
            PhysicsComponent playerPhysics = new PhysicsComponent(game, player);
            AliveComponent playerHealthHandler = new AliveComponent(game, player, 1);

            // TODO make controller for players? - is this going to be the same as the client?

            player.AddComponent(typeof(PhysicsComponent), playerPhysics);
            gcm.AddComponent(playerPhysics);

            player.AddComponent(typeof(AliveComponent), playerHealthHandler);
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

            base.Update(gameTime);
        }

        private void SetPlayerVel(Identification id, Vector3 vel)
        {
            if (!players.ContainsKey(id))
            {
                //((LoggerManager)Game.Services.GetService(typeof(LoggerManager))).Log(Level.DEBUG, String.Format("Unknown player with id: {0}", id));
                return;
            }
            (players[id].GetSharedData(typeof(Entity)) as Entity).LinearVelocity = vel;
        }

        public Vector3 GetPlayerPosition(Identification id)
        {
            if (!players.ContainsKey(id))
            {
                // hack
                return Vector3.Zero;
            }
            return (players[id].GetSharedData(typeof(Entity)) as Entity).Position;
        }

        public void SetPlayerLocation(Vector3 pos, Identification id)
        {
            if (!players.ContainsKey(id))
            {
                return;
            }
            Entity sharedData = players[id].GetSharedData(typeof(Entity)) as Entity;
            sharedData.Position = pos;
        }

        public void Reset()
        {
            players.Clear();
        }

        public void DisconnectPlayer(int id)
        {
            players.Remove(new Identification(id, id));            
        }

        // Used for getting the new host lol
        public Identification GetLowestId()
        {
            Identification min = null;

            foreach (Identification curId in players.Keys)
            {
                if (min == null || min.id > curId.id)
                {
                    min = curId;
                }
            }

            return min;
        }
    }
}
