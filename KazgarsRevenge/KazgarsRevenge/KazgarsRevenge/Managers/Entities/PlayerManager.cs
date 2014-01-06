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
    public class PlayerManager : EntityManager
    {
        public Identification myId
        {
            get;
            protected set;
        }

        public int numPlayers
        {
            get
            {
                return playerMap.Keys.Count;
            }
        }

        IDictionary<Identification, GameEntity> playerMap;
        public PlayerManager(KazgarsRevengeGame game)
            : base(game)
        {
            playerMap = new Dictionary<Identification, GameEntity>();
        }

        public void CreateMainPlayer(Vector3 position, Identification id)
        {
            GameEntity player = new GameEntity("localplayer", FactionType.Players);

            //shared physical data (shared between AnimatedModelComponent, PhysicsComponent, and PlayerController)
            Entity playerPhysicalData = new Cylinder(position, 37, 6, 2);
            //assigns a collision group to the physics
            playerPhysicalData.CollisionInformation.CollisionRules.Group = mainGame.PlayerCollisionGroup;

            //taking away friction
            playerPhysicalData.Material.KineticFriction = 0;
            playerPhysicalData.Material.StaticFriction = 0;

            //locking rotation on axis (so that bumping into something won't make the player tip over)
            playerPhysicalData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            //more accurate collision detection for the player
            playerPhysicalData.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;
            player.AddSharedData(typeof(Entity), playerPhysicalData);

            //giving a reference to the player's physical data to the camera, so it can follow the player around
            (Game.Services.GetService(typeof(CameraComponent)) as CameraComponent).AssignEntity(playerPhysicalData);

            Model playerModel = GetAnimatedModel("Models\\Player\\k_idle1");
            //shared animation data (need this to be in the player controller component as well as the graphics component, so that the controller can determine when to play animations)
            AnimationPlayer playerAnimations = new AnimationPlayer(playerModel.Tag as SkinningData);
            player.AddSharedData(typeof(AnimationPlayer), playerAnimations);

            Dictionary<string, AttachableModel> attachables = new Dictionary<string, AttachableModel>();
            player.AddSharedData(typeof(Dictionary<string, AttachableModel>), attachables);

            HealthData playerHealth = new HealthData(1000);
            player.AddSharedData(typeof(HealthData), playerHealth);

            //the components that make up the player
            PhysicsComponent playerPhysics = new PhysicsComponent(mainGame, player);
            AnimatedModelComponent playerGraphics = new AnimatedModelComponent(mainGame, player, playerModel, new Vector3(10f), Vector3.Down * 18);
            HealthHandlerComponent playerHealthHandler = new HealthHandlerComponent(mainGame, player);
            PlayerController playerController = new PlayerController(mainGame, player);
            NetMovementComponent nmc = new NetMovementComponent(mainGame, player);

            //adding the controllers to their respective managers 
            player.AddComponent(typeof(PhysicsComponent), playerPhysics);
            genComponentManager.AddComponent(playerPhysics);

            player.AddComponent(typeof(AnimatedModelComponent), playerGraphics);
            modelManager.AddComponent(playerGraphics);

            player.AddComponent(typeof(HealthHandlerComponent), playerHealthHandler);
            genComponentManager.AddComponent(playerHealthHandler);

            player.AddComponent(typeof(PlayerController), playerController);
            spriteManager.AddComponent(playerController);
            
            player.AddComponent(typeof(NetMovementComponent), nmc);
            nmm.AddComponent(nmc);

            playerMap[id] = player;
            myId = id;
        }

        public void CreateNetworkedPlayer(Vector3 position, Identification id)
        {
            // TODO ISSUE #9
            GameEntity player = new GameEntity("netplayer", FactionType.Players);

            Entity playerPhysicalData = new Box(position, 37, 6, 2);

            playerPhysicalData.IsAffectedByGravity = false;
            playerPhysicalData.CollisionInformation.CollisionRules.Group = mainGame.NetworkedPlayerCollisionGroup;
            playerPhysicalData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            playerPhysicalData.Material.KineticFriction = 0;
            playerPhysicalData.Material.StaticFriction = 0;
            player.AddSharedData(typeof(Entity), playerPhysicalData);

            Model playerModel = GetAnimatedModel("Models\\Player\\k_idle1");
            AnimationPlayer playerAnimations = new AnimationPlayer(playerModel.Tag as SkinningData);
            player.AddSharedData(typeof(AnimationPlayer), playerAnimations);

            Dictionary<string, AttachableModel> attachables = new Dictionary<string, AttachableModel>();
            player.AddSharedData(typeof(Dictionary<string, AttachableModel>), attachables);

            HealthData playerHealth = new HealthData(100);
            player.AddSharedData(typeof(HealthData), playerHealth);

            PhysicsComponent playerPhysics = new PhysicsComponent(mainGame, player);
            AnimatedModelComponent playerGraphics = new AnimatedModelComponent(mainGame, player, playerModel, new Vector3(10f), Vector3.Down * 18);
            HealthHandlerComponent playerHealthHandler = new HealthHandlerComponent(mainGame, player);
            NetworkPlayerController controller = new NetworkPlayerController(mainGame, player);

            player.AddComponent(typeof(PhysicsComponent), playerPhysics);
            genComponentManager.AddComponent(playerPhysics);

            player.AddComponent(typeof(AnimatedModelComponent), playerGraphics);
            modelManager.AddComponent(playerGraphics);

            player.AddComponent(typeof(HealthHandlerComponent), playerHealthHandler);
            genComponentManager.AddComponent(playerHealthHandler);

            player.AddComponent(typeof(NetworkPlayerController), controller);
            genComponentManager.AddComponent(controller);

            playerMap[id] = player;
        }

        public void SetPlayerLocation(Vector3 pos, Identification id)
        {
            if (playerMap.ContainsKey(id))
            {
                Entity sharedData = playerMap[id].GetSharedData(typeof(Entity)) as Entity;
                if (sharedData.Position != pos)
                {
                    //give new target to the controller to go to
                    sharedData.Position = new Vector3(pos.X, pos.Y, pos.Z);
                }
            }
            else
            {
                CreateNetworkedPlayer(pos, id);
            }
        }

        public void SetPlayerTarget(Vector3 pos, Identification id)
        {
            if (playerMap.ContainsKey(id))
            {
                NetworkPlayerController controller = playerMap[id].GetComponent(typeof(NetworkPlayerController)) as NetworkPlayerController;
                if (controller != null)
                {
                    //give new target to the controller to go to
                    controller.SetPosition(pos);
                }
            }
            else
            {
                CreateNetworkedPlayer(pos, id);
            }
        }

        public void DeletePlayer(Identification id)
        {
            // TODO Remove all components associated with the given player id

            // This line implemented so other clients don't read too much data from snapshots
            playerMap.Remove(id);
        }
    }
}
