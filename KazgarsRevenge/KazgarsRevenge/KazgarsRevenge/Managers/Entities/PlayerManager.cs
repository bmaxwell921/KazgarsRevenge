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
    class PlayerManager : EntityManager
    {
        List<GameEntity> players = new List<GameEntity>();
        public PlayerManager(MainGame game)
            : base(game)
        {

        }

        public void CreateDude()
        {
            GameEntity dude = new GameEntity("dude", "wtf");

            Model dudeModel = GetAnimatedModel("Models\\dude");
            AnimationPlayer dudeAnims = new AnimationPlayer(dudeModel.Tag as SkinningData);
            AnimatedModelComponent dudeGraphics = new AnimatedModelComponent(mainGame, new Box(new Vector3(200, 0, -210), 1, 1, 1), dudeModel, dudeAnims, new Vector3(1), Vector3.Zero, new Dictionary<string, AttachableModel>());
            modelManager.AddComponent(dudeGraphics);
            players.Add(dude);
        }

        public void CreateMainPlayer(Vector3 position)
        {
            GameEntity player = new GameEntity("localplayer", "good");

            //shared physical data (shared between AnimatedModelComponent, PhysicsComponent, and PlayerController
            Entity playerPhysicalData = new Cylinder(position, 37, 6, 2);
            //assigns a collision group to the physics
            playerPhysicalData.CollisionInformation.CollisionRules.Group = MainGame.PlayerCollisionGroup;

            //locking rotation on axis (so that bumping into something won't make the player tip over)
            playerPhysicalData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            //more accurate collision detection for the player
            playerPhysicalData.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;
            //need the collisioninformation's tag to be set, to get the entity in a collision handler
            playerPhysicalData.CollisionInformation.Tag = player;

            //giving a reference to the player's physical data to the camera, so it can follow the player aorund
            (Game.Services.GetService(typeof(CameraComponent)) as CameraComponent).AssignEntity(playerPhysicalData);

            Model playerModel = GetAnimatedModel("Models\\Player\\k_idle1");
            //shared animation data (need this to be in the player controller component as well as the graphics component, so that the controller can determine when to play animations)
            AnimationPlayer playerAnimations = new AnimationPlayer(playerModel.Tag as SkinningData);

            Dictionary<string, AttachableModel> attachables = new Dictionary<string, AttachableModel>();
            HealthData playerHealth = new HealthData(100);


            //the components that make up the player
            PhysicsComponent playerPhysics = new PhysicsComponent(mainGame, playerPhysicalData);
            AnimatedModelComponent playerGraphics = new AnimatedModelComponent(mainGame, playerPhysicalData, playerModel, playerAnimations, new Vector3(10f), Vector3.Down * 18, attachables);
            HealthHandlerComponent playerHealthHandler = new HealthHandlerComponent(mainGame, playerHealth, player);
            PlayerController playerController = new PlayerController(mainGame, player, playerPhysicalData, playerAnimations, attachables);

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

            players.Add(player);
        }
    }
}
