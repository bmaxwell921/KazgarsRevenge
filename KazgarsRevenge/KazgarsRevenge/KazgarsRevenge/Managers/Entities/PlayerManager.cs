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

        // The account for the current player
        public Account currentAccount;

        IDictionary<Identification, GameEntity> playerMap;
        public PlayerManager(KazgarsRevengeGame game)
            : base(game)
        {
            playerMap = new Dictionary<Identification, GameEntity>();
        }

        public void CreateMainPlayerInLevel(Identification id)
        {
            this.CreateMainPlayer((Game.Services.GetService(typeof(LevelManager)) as LevelManager).GetPlayerSpawnLocation(), id);
        }

        public void ResetPlayerPosition()
        {
            SetPlayerLocation((Game.Services.GetService(typeof(LevelManager)) as LevelManager).GetPlayerSpawnLocation(), myId);
        }

        public void CreateMainPlayer(Vector3 position, Identification id)
        {
            // TODO use the currentAccount to set up the abilities and stuffs

            GameEntity player = new GameEntity("localplayer", FactionType.Players, EntityType.Player);
            player.id = id;
            //shared physical data (shared between AnimatedModelComponent, PhysicsComponent, and PlayerController)
            Entity playerPhysicalData = new Cylinder(position, 37, 6, 2);
            //assigns a collision group to the physics
            playerPhysicalData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;

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
            //shared animation data (need this to be in the player controller component as 
            // well as the graphics component, so that the controller can determine when to play animations)
            AnimationPlayer playerAnimations = new AnimationPlayer(playerModel.Tag as SkinningData);
            player.AddSharedData(typeof(AnimationPlayer), playerAnimations);

            Dictionary<string, AttachableModel> attachables = new Dictionary<string, AttachableModel>();
            player.AddSharedData(typeof(Dictionary<string, AttachableModel>), attachables);

            player.AddSharedData(typeof(Dictionary<string, Model>), new Dictionary<string, Model>());

            //the components that make up the player
            PhysicsComponent playerPhysics = new PhysicsComponent(mainGame, player);
            AnimatedModelComponent playerGraphics = new AnimatedModelComponent(mainGame, player, playerModel, 10f, Vector3.Down * 18);
            LocalPlayerController playerController = new LocalPlayerController(mainGame, player, currentAccount);
            BlobShadowDecal shadow = new BlobShadowDecal(mainGame, player, 15);
            AbilityTargetDecal groundIndicator = new AbilityTargetDecal(mainGame, player, 0);

            player.AddComponent(typeof(AbilityTargetDecal), groundIndicator);
            billboardManager.AddComponent(groundIndicator);

            //adding the controllers to their respective managers 
            player.AddComponent(typeof(PhysicsComponent), playerPhysics);
            genComponentManager.AddComponent(playerPhysics);

            player.AddComponent(typeof(AnimatedModelComponent), playerGraphics);
            modelManager.AddComponent(playerGraphics);

            player.AddComponent(typeof(AliveComponent), playerController);
            spriteManager.AddComponent(playerController);

            player.AddComponent(typeof(BlobShadowDecal), shadow);
            billboardManager.AddComponent(shadow);

            playerMap[id] = player;
            myId = id;
        }

        public void CreateNetworkedPlayer(Vector3 position, Identification id)
        {
            // TODO ISSUE #9
            GameEntity player = new GameEntity("netplayer", FactionType.Players, EntityType.Player);
            player.id = id;
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

            PhysicsComponent playerPhysics = new PhysicsComponent(mainGame, player);
            AnimatedModelComponent playerGraphics = new AnimatedModelComponent(mainGame, player, playerModel, 10f, Vector3.Down * 18);
            NetworkPlayerController controller = new NetworkPlayerController(mainGame, player, new Account("BLAH"));
            BlobShadowDecal shadow = new BlobShadowDecal(mainGame, player, 15);

            player.AddComponent(typeof(BlobShadowDecal), shadow);
            billboardManager.AddComponent(shadow);

            player.AddComponent(typeof(PhysicsComponent), playerPhysics);
            genComponentManager.AddComponent(playerPhysics);

            player.AddComponent(typeof(AnimatedModelComponent), playerGraphics);
            modelManager.AddComponent(playerGraphics);

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
            if(playerMap.ContainsKey(id))
                playerMap[id].KillEntity();
            // This line implemented so other clients don't read too much data from snapshots
            playerMap.Remove(id);
        }

        /// <summary>
        /// Gets a player from this manager with the associated id, or null if none exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public GameEntity getEntity(Identification id)
        {
            if (!playerMap.ContainsKey(id))
            {
                return null;
            }
            return playerMap[id];
        }

        public int GetHighestLevel()
        {
            int retLev = 1;
            foreach (KeyValuePair<Identification, GameEntity> k in playerMap)
            {
                int tempLevel = (k.Value.GetComponent(typeof(AliveComponent)) as AliveComponent).Level;
                if (tempLevel > retLev)
                {
                    retLev = tempLevel;
                }
            }
            return retLev;
        }

        public bool IsPlayerNear(Vector3 loc, float proximity)
        {
            foreach (KeyValuePair<Identification, GameEntity> k in playerMap)
            {
                Vector3 playerPos = (k.Value.GetSharedData(typeof(Entity)) as Entity).Position;
                if (Math.Abs(playerPos.X - loc.X) < proximity && Math.Abs(playerPos.Z - loc.Z) < proximity)
                {
                    return true;
                }
            }
            return false;
        }
        
        public string GetDebugString()
        {
            Vector3 ppos = (playerMap.Values.ToList()[0].GetSharedData(typeof(Entity)) as Entity).Position;
            return "X: " + ppos.X + "\nZ: " + ppos.Z;
        }
    }
}
