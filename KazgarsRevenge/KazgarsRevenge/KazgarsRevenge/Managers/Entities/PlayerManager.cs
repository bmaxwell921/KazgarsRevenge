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

        IDictionary<Identification, GameEntity> playerMap;
        public PlayerManager(KazgarsRevengeGame game)
            : base(game)
        {
            playerMap = new Dictionary<Identification, GameEntity>();
        }

        public void Reset()
        {
            //(playerMap[myId].GetSharedData(typeof(Entity)) as Entity).Position = new Vector3(200, 0, -200);
        }

        public void CreateMainPlayer(Vector3 position, Identification id)
        {
            GameEntity player = new GameEntity("localplayer", FactionType.Players);

            //shared physical data (shared between AnimatedModelComponent, PhysicsComponent, and PlayerController
            Entity playerPhysicalData = new Cylinder(position, 37, 6, 2);
            //assigns a collision group to the physics
            playerPhysicalData.CollisionInformation.CollisionRules.Group = mainGame.PlayerCollisionGroup;

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

            HealthData playerHealth = new HealthData(100);
            player.AddSharedData(typeof(HealthData), playerHealth);


            //the components that make up the player
            PhysicsComponent playerPhysics = new PhysicsComponent(mainGame, player);
            AnimatedModelComponent playerGraphics = new AnimatedModelComponent(mainGame, player, playerModel, new Vector3(10f), Vector3.Down * 18);
            HealthHandlerComponent playerHealthHandler = new HealthHandlerComponent(mainGame, player);
            PlayerController playerController = new PlayerController(mainGame, player);

            //adding the controllers to their respective managers 
            //(need to decide what kinds of components need their own managers; currently 
            //it's just a 2d renderer, a 3d renderer, and a general manager)
            player.AddComponent(typeof(PhysicsComponent), playerPhysics);
            genComponentManager.AddComponent(playerPhysics);

            player.AddComponent(typeof(AnimatedModelComponent), playerGraphics);
            modelManager.AddComponent(playerGraphics);

            player.AddComponent(typeof(HealthHandlerComponent), playerHealthHandler);
            genComponentManager.AddComponent(playerHealthHandler);

            player.AddComponent(typeof(PlayerController), playerController);

            spriteManager.AddComponent(playerController);

            playerMap[id] = player;
            myId = id;
        }

        public void CreateNetworkedPlayer(Vector3 position, Identification id)
        {
            // TODO ISSUE #9
            GameEntity player = new GameEntity("netplayer", FactionType.Players);

            //shared physical data (shared between AnimatedModelComponent, PhysicsComponent, and PlayerController
            Entity playerPhysicalData = new Cylinder(position, 37, 6, 2);
            //assigns a collision group to the physics
            playerPhysicalData.CollisionInformation.CollisionRules.Group = mainGame.PlayerCollisionGroup;

            //locking rotation on axis (so that bumping into something won't make the player tip over)
            playerPhysicalData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            //more accurate collision detection for the player
            playerPhysicalData.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;
            player.AddSharedData(typeof(Entity), playerPhysicalData);

            // DON'T MAKE THE CAMERA FOLLOW EVERYONE
            //(Game.Services.GetService(typeof(CameraComponent)) as CameraComponent).AssignEntity(playerPhysicalData);

            Model playerModel = GetAnimatedModel("Models\\Player\\k_idle1");
            //shared animation data (need this to be in the player controller component as well as the graphics component, so that the controller can determine when to play animations)
            AnimationPlayer playerAnimations = new AnimationPlayer(playerModel.Tag as SkinningData);
            player.AddSharedData(typeof(AnimationPlayer), playerAnimations);

            Dictionary<string, AttachableModel> attachables = new Dictionary<string, AttachableModel>();
            player.AddSharedData(typeof(Dictionary<string, AttachableModel>), attachables);

            HealthData playerHealth = new HealthData(100);
            player.AddSharedData(typeof(HealthData), playerHealth);


            //the components that make up the player
            PhysicsComponent playerPhysics = new PhysicsComponent(mainGame, player);
            AnimatedModelComponent playerGraphics = new AnimatedModelComponent(mainGame, player, playerModel, new Vector3(10f), Vector3.Down * 18);
            HealthHandlerComponent playerHealthHandler = new HealthHandlerComponent(mainGame, player);
            
            // NETWORKED PLAYERS ARE NOT CONTROLLED BY THIS CLIENT 
            // TODO ISSUE #10
            //PlayerController playerController = new PlayerController(mainGame, player);

            //adding the controllers to their respective managers 
            //(need to decide what kinds of components need their own managers; currently 
            //it's just a 2d renderer, a 3d renderer, and a general manager)
            player.AddComponent(typeof(PhysicsComponent), playerPhysics);
            genComponentManager.AddComponent(playerPhysics);

            player.AddComponent(typeof(AnimatedModelComponent), playerGraphics);
            modelManager.AddComponent(playerGraphics);

            player.AddComponent(typeof(HealthHandlerComponent), playerHealthHandler);
            genComponentManager.AddComponent(playerHealthHandler);

            //player.AddComponent(typeof(PlayerController), playerController);

            //spriteManager.AddComponent(playerController);

            playerMap[id] = player;
        }
    }
}
