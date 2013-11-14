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
        public AttackManager(KazgarsRevengeGame game)
            : base(game)
        {

        }


        Matrix arrowGraphicRot = Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2, 0, 0);
        public void CreateArrow(Vector3 position, Vector3 initialTrajectory, int damage, string factionToHit)
        {
            GameEntity arrow = new GameEntity("arrow", "good");
            position.Y += 20;
            Entity arrowData = new Box(position, 10, 17, 10, .001f);
            arrowData.CollisionInformation.CollisionRules.Group = factionToHit == "good" ? MainGame.GoodProjectileCollisionGroup : MainGame.BadProjectileCollisionGroup;
            arrowData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            arrowData.LinearVelocity = initialTrajectory;
            arrowData.Orientation = Quaternion.CreateFromRotationMatrix(CreateRotationFromForward(initialTrajectory));
            arrow.AddSharedData(typeof(Entity), arrowData);

            PhysicsComponent arrowPhysics = new PhysicsComponent(mainGame, arrow);
            UnanimatedModelComponent arrowGraphics =
                new UnanimatedModelComponent(mainGame, arrow, GetUnanimatedModel("Models\\Attachables\\arrow"),
                    new Vector3(10), Vector3.Zero, arrowGraphicRot);

            AttackController arrowAI = new AttackController(mainGame, arrow, arrowData, damage, 3000, factionToHit);

            arrow.AddComponent(typeof(PhysicsComponent), arrowPhysics);
            genComponentManager.AddComponent(arrowPhysics);

            arrow.AddComponent(typeof(UnanimatedModelComponent), arrowGraphics);
            modelManager.AddComponent(arrowGraphics);

            arrow.AddComponent(typeof(AttackController), arrowAI);
            genComponentManager.AddComponent(arrowAI);

            attacks.Add(arrow);
        }

        public void CreateMelleAttack(Vector3 position, int damage, string factionToHit)
        {
            GameEntity newAttack = new GameEntity("arrow", "good");

            Entity attackData = new Box(position, 35, 47, 35, .01f);
            attackData.CollisionInformation.CollisionRules.Group = factionToHit == "good" ? MainGame.GoodProjectileCollisionGroup : MainGame.BadProjectileCollisionGroup;
            attackData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            attackData.LinearVelocity = Vector3.Zero;
            newAttack.AddSharedData(typeof(Entity), attackData);

            PhysicsComponent attackPhysics = new PhysicsComponent(mainGame, newAttack);

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
            spikes.AddSharedData(typeof(Entity), spikesPhysical);

            Model spikeModel = GetAnimatedModel("Models\\spikes");
            AnimationPlayer anims = new AnimationPlayer(spikeModel.Tag as SkinningData);
            spikes.AddSharedData(typeof(AnimationPlayer), anims);

            spikes.AddSharedData(typeof(Dictionary<string, AttachableModel>), new Dictionary<string, AttachableModel>());

            AnimatedModelComponent spikesGraphics = new AnimatedModelComponent(mainGame, spikes, spikeModel, new Vector3(5), Vector3.Down * 20);
            CursorSpikeController spikesController = new CursorSpikeController(mainGame, spikes);

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
