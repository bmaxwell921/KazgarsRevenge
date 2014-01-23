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
using KazgarsRevenge.Libraries;

namespace KazgarsRevenge
{
    public class AttackManager : EntityManager
    {
        List<GameEntity> attacks = new List<GameEntity>();
        SoundEffectLibrary soundEffects;

        public AttackManager(KazgarsRevengeGame game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            soundEffects = Game.Services.GetService(typeof(SoundEffectLibrary)) as SoundEffectLibrary;

            particles = Game.Services.GetService(typeof(ParticleManager)) as ParticleManager;
        }


        Matrix arrowGraphicRot = Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2, 0, 0);
        public void CreateArrow(Vector3 position, Vector3 initialTrajectory, int damage, FactionType arrowFaction, GameEntity creator)
        {
            GameEntity arrow = new GameEntity("arrow", arrowFaction, EntityType.Misc);
            position.Y += 20;
            Entity arrowData = new Box(position, 10, 17, 10, .001f);
            arrowData.CollisionInformation.CollisionRules.Group = arrowFaction == FactionType.Players ? mainGame.GoodProjectileCollisionGroup : mainGame.BadProjectileCollisionGroup;
            arrowData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            arrowData.LinearVelocity = initialTrajectory;
            arrowData.Orientation = Quaternion.CreateFromRotationMatrix(CreateRotationFromForward(initialTrajectory));
            arrow.AddSharedData(typeof(Entity), arrowData);

            PhysicsComponent arrowPhysics = new PhysicsComponent(mainGame, arrow);
            UnanimatedModelComponent arrowGraphics =
                new UnanimatedModelComponent(mainGame, arrow, GetUnanimatedModel("Models\\Attachables\\arrow"),
                    new Vector3(10), Vector3.Zero, arrowGraphicRot);

            AttackController arrowAI = new AttackController(mainGame, arrow, arrowData, damage, 3000, arrowFaction == FactionType.Players ? FactionType.Enemies : FactionType.Players, creator);

            arrow.AddComponent(typeof(PhysicsComponent), arrowPhysics);
            genComponentManager.AddComponent(arrowPhysics);

            arrow.AddComponent(typeof(UnanimatedModelComponent), arrowGraphics);
            modelManager.AddComponent(arrowGraphics);

            arrow.AddComponent(typeof(AttackController), arrowAI);
            genComponentManager.AddComponent(arrowAI);

            attacks.Add(arrow);

            soundEffects.playRangedSound();
        }

        public void CreateMelleAttack(Vector3 position, int damage, FactionType faction, bool sparks, GameEntity creator)
        {
            GameEntity newAttack = new GameEntity("arrow", faction, EntityType.Misc);

            Entity attackData = new Box(position, 35, 47, 35, .01f);
            attackData.CollisionInformation.CollisionRules.Group = faction == FactionType.Players ? mainGame.GoodProjectileCollisionGroup : mainGame.BadProjectileCollisionGroup;
            attackData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            attackData.LinearVelocity = Vector3.Zero;
            newAttack.AddSharedData(typeof(Entity), attackData);

            PhysicsComponent attackPhysics = new PhysicsComponent(mainGame, newAttack);

            AttackController attackAI = new AttackController(mainGame, newAttack, attackData, damage, 300, faction == FactionType.Players? FactionType.Enemies : FactionType.Players, creator);

            newAttack.AddComponent(typeof(PhysicsComponent), attackPhysics);
            genComponentManager.AddComponent(attackPhysics);

            newAttack.AddComponent(typeof(AttackController), attackAI);
            genComponentManager.AddComponent(attackAI);

            attacks.Add(newAttack);
            soundEffects.playMeleeSound();

            if (sparks)
            {
                SpawnWeaponSparks(position + Vector3.Down * 18);
            }
        }

        public void CreateMagicAttack()
        {

            soundEffects.playMagicSound();
        }

        public void CreateMouseSpikes(Vector3 position)
        {
            GameEntity spikes = new GameEntity("cursor", FactionType.Neutral, EntityType.Misc);

            Entity spikesPhysical = new Box(position, 1, 1, 1);
            spikes.AddSharedData(typeof(Entity), spikesPhysical);

            Model spikeModel = GetAnimatedModel("Models\\spikes");
            AnimationPlayer anims = new AnimationPlayer(spikeModel.Tag as SkinningData);
            spikes.AddSharedData(typeof(AnimationPlayer), anims);

            spikes.AddSharedData(typeof(Dictionary<string, AttachableModel>), new Dictionary<string, AttachableModel>());

            AnimatedModelComponent spikesGraphics = new AnimatedModelComponent(mainGame, spikes, spikeModel, new Vector3(7), Vector3.Down * 20);
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


        #region Particles
        ParticleManager particles;
        public void SpawnWeaponSparks(Vector3 position)
        {
            ParticleSystem explosions = particles.GetSystem(typeof(WeaponSparksSystem));
            for (int i = 0; i < 20; ++i)
            {
                explosions.AddParticle(position, Vector3.Zero);
            }
        }

        public void SpawnBloodSpurt(Vector3 position, Vector3 forward)
        {
            ParticleSystem blood = particles.GetSystem(typeof(BloodParticleSystem));
            forward *= 25;
            for (int i = 0; i < 10; ++i)
            {
                blood.AddParticle(position, forward);
            }
        }
        #endregion
    }
}
