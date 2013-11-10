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
    class AttackManager : EntityManager
    {
        List<GameEntity> attacks = new List<GameEntity>();
        public AttackManager(MainGame game)
            : base(game)
        {

        }


        Matrix arrowGraphicRot = Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2, 0, 0);
        public void CreateArrow(Vector3 position, Vector3 initialTrajectory, int damage, string factionToHit)
        {
            GameEntity newArrow = new GameEntity("arrow", "good");
            position.Y += 20;
            Entity arrowData = new Box(position, 10, 17, 10, .001f);
            arrowData.CollisionInformation.CollisionRules.Group = factionToHit == "good" ? MainGame.GoodProjectileCollisionGroup : MainGame.BadProjectileCollisionGroup;
            arrowData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            arrowData.LinearVelocity = initialTrajectory;
            arrowData.Orientation = Quaternion.CreateFromRotationMatrix(CreateRotationFromForward(initialTrajectory));

            PhysicsComponent arrowPhysics = new PhysicsComponent(mainGame, arrowData);
            UnanimatedModelComponent arrowGraphics =
                new UnanimatedModelComponent(mainGame, GetUnanimatedModel("Models\\Attachables\\arrow"),//, "Models\\Attachables\\sword"),
                    arrowData, new Vector3(10), Vector3.Zero, arrowGraphicRot);

            AttackController arrowAI = new AttackController(mainGame, newArrow, arrowData, damage, 3000, factionToHit);

            newArrow.AddComponent(typeof(PhysicsComponent), arrowPhysics);
            genComponentManager.AddComponent(arrowPhysics);

            newArrow.AddComponent(typeof(UnanimatedModelComponent), arrowGraphics);
            modelManager.AddComponent(arrowGraphics);

            newArrow.AddComponent(typeof(AttackController), arrowAI);
            genComponentManager.AddComponent(arrowAI);

            attacks.Add(newArrow);
        }

        public void CreateMelleAttack(Vector3 position, int damage, string factionToHit)
        {
            GameEntity newAttack = new GameEntity("arrow", "good");

            Entity attackData = new Box(position, 35, 47, 35, .01f);
            attackData.CollisionInformation.CollisionRules.Group = factionToHit == "good" ? MainGame.GoodProjectileCollisionGroup : MainGame.BadProjectileCollisionGroup;
            attackData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            attackData.LinearVelocity = Vector3.Zero;

            PhysicsComponent attackPhysics = new PhysicsComponent(mainGame, attackData);

            AttackController attackAI = new AttackController(mainGame, newAttack, attackData, damage, 300, factionToHit);

            newAttack.AddComponent(typeof(PhysicsComponent), attackPhysics);
            genComponentManager.AddComponent(attackPhysics);

            newAttack.AddComponent(typeof(AttackController), attackAI);
            genComponentManager.AddComponent(attackAI);

            attacks.Add(newAttack);
        }

        public void CreateMouseSpikes(Vector3 position)
        {
            GameEntity spikes = new GameEntity("cursor", "neutral");

            Entity spikesPhysical = new Box(position, 1, 1, 1);
            Model spikeModel = GetAnimatedModel("Models\\spikes");
            AnimationPlayer anims = new AnimationPlayer(spikeModel.Tag as SkinningData);

            AnimatedModelComponent spikesGraphics = new AnimatedModelComponent(mainGame, spikesPhysical, spikeModel, anims, new Vector3(5), Vector3.Down * 20, new Dictionary<string, AttachableModel>());
            CursorSpikeController spikesController = new CursorSpikeController(mainGame, anims, spikes);

            spikes.AddComponent(typeof(AnimatedModelComponent), spikesGraphics);
            modelManager.AddComponent(spikesGraphics);

            spikes.AddComponent(typeof(CursorSpikeController), spikesController);
            genComponentManager.AddComponent(spikesController);

            attacks.Add(spikes);
        }

        private Matrix CreateRotationFromForward(Vector3 forward)
        {
            Matrix rotation = Matrix.Identity;
            rotation.Forward = Vector3.Normalize(forward);
            rotation.Right = Vector3.Normalize(Vector3.Cross(rotation.Forward, Vector3.Up));
            rotation.Up = Vector3.Up;
            return rotation;
        }
    }
}
