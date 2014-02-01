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
    public enum AbilityName
    {
        Snipe,

        HeartStrike,

        IceClawPrison,
    }
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

        #region Primaries
        const float arrowSpeed = 450.0f;
        public void CreateArrow(Vector3 position, Vector3 dir, int damage, AliveComponent creator)
        {
            GameEntity arrow = new GameEntity("arrow", creator.Entity.Faction, EntityType.Misc);
            position.Y += 20;
            Entity arrowData = new Box(position, 10, 17, 15, .001f);
            arrowData.CollisionInformation.CollisionRules.Group = creator.Entity.Faction == FactionType.Players ? mainGame.GoodProjectileCollisionGroup : mainGame.BadProjectileCollisionGroup;
            arrowData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            arrowData.LinearVelocity = dir * arrowSpeed;
            arrowData.Orientation = Quaternion.CreateFromRotationMatrix(CreateRotationFromForward(dir));
            arrow.AddSharedData(typeof(Entity), arrowData);

            PhysicsComponent arrowPhysics = new PhysicsComponent(mainGame, arrow);
            UnanimatedModelComponent arrowGraphics =
                new UnanimatedModelComponent(mainGame, arrow, GetUnanimatedModel("Models\\Attachables\\arrow"),
                    new Vector3(10), Vector3.Backward * 6, Matrix.Identity);

            ArrowController arrowAI = new ArrowController(mainGame, arrow, damage, creator.Entity.Faction == FactionType.Players ? FactionType.Enemies : FactionType.Players, creator);

            arrow.AddComponent(typeof(PhysicsComponent), arrowPhysics);
            genComponentManager.AddComponent(arrowPhysics);

            arrow.AddComponent(typeof(UnanimatedModelComponent), arrowGraphics);
            modelManager.AddComponent(arrowGraphics);

            arrow.AddComponent(typeof(AttackController), arrowAI);
            genComponentManager.AddComponent(arrowAI);

            attacks.Add(arrow);

            soundEffects.playRangedSound();
        }

        public void CreateMeleeAttack(Vector3 position, int damage, bool sparks, AliveComponent creator)
        {
            GameEntity newAttack = new GameEntity("arrow", creator.Entity.Faction, EntityType.Misc);
            //TODO give the newAttack an Id

            // Send it off to the server!
            //((MessageSender)Game.Services.GetService(typeof(MessageSender))).SendMeleeAttackMessage(creator.Entity.id.id, newAttack.id.id, creator.Entity.Faction, position, damage);
            
            Entity attackData = new Box(position, 35, 47, 35, .01f);
            attackData.CollisionInformation.CollisionRules.Group = creator.Entity.Faction == FactionType.Players ? mainGame.GoodProjectileCollisionGroup : mainGame.BadProjectileCollisionGroup;
            attackData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            attackData.LinearVelocity = Vector3.Zero;
            newAttack.AddSharedData(typeof(Entity), attackData);

            PhysicsComponent attackPhysics = new PhysicsComponent(mainGame, newAttack);

            AttackController attackAI = new AttackController(mainGame, newAttack, damage, creator.Entity.Faction == FactionType.Players ? FactionType.Enemies : FactionType.Players, creator);

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

        /// <summary>
        /// Called from a network message to create a Melee Attack
        /// </summary>
        public void CreateMeleeAttack(int creatorId, int attackId, FactionType assocFact, Vector3 position, int damage)
        {
            //GameEntity newAttack = new GameEntity("arrow", assocFact, EntityType.Misc);
            //newAttack.id = new Identification(attackId);

            //Entity attackData = new Box(position, 35, 47, 35, .01f);
            ////GameEntity entity = ;


            //attackData.CollisionInformation.CollisionRules.Group = assocFact == FactionType.Players ? mainGame.GoodProjectileCollisionGroup : mainGame.BadProjectileCollisionGroup;
            //attackData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            //attackData.LinearVelocity = Vector3.Zero;
            //newAttack.AddSharedData(typeof(Entity), attackData);

            //PhysicsComponent attackPhysics = new PhysicsComponent(mainGame, newAttack);

            //AttackController attackAI = new AttackController(mainGame, newAttack, attackData, damage, 300, assocFact == FactionType.Players ? FactionType.Enemies : FactionType.Players, (AliveComponent) entity.GetComponent(typeof(AliveComponent)));

            //newAttack.AddComponent(typeof(PhysicsComponent), attackPhysics);
            //genComponentManager.AddComponent(attackPhysics);

            //newAttack.AddComponent(typeof(AttackController), attackAI);
            //genComponentManager.AddComponent(attackAI);

            //attacks.Add(newAttack);
            //soundEffects.playMeleeSound();

            // SpawnWeaponSparks(position + Vector3.Down * 18);
        }

        public void CreateMagicAttack()
        {

            soundEffects.playMagicSound();
        }
        #endregion

        #region Abilities
        public void CreateSnipe(Vector3 position, Vector3 dir, int damage, AliveComponent creator)
        {
            GameEntity arrow = new GameEntity("arrow", creator.Entity.Faction, EntityType.Misc);
            position.Y += 20;
            Entity arrowData = new Box(position, 10, 17, 40, .001f);
            arrowData.CollisionInformation.CollisionRules.Group = creator.Entity.Faction == FactionType.Players ? mainGame.GoodProjectileCollisionGroup : mainGame.BadProjectileCollisionGroup;
            arrowData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            arrowData.LinearVelocity = dir * arrowSpeed;
            arrowData.Orientation = Quaternion.CreateFromRotationMatrix(CreateRotationFromForward(dir));
            arrow.AddSharedData(typeof(Entity), arrowData);

            PhysicsComponent arrowPhysics = new PhysicsComponent(mainGame, arrow);
            UnanimatedModelComponent arrowGraphics =
                new UnanimatedModelComponent(mainGame, arrow, GetUnanimatedModel("Models\\Attachables\\arrow"),
                    new Vector3(30), Vector3.Backward * 20, Matrix.Identity);

            ArrowController arrowAI = new ArrowController(mainGame, arrow, damage, creator.Entity.Faction == FactionType.Players ? FactionType.Enemies : FactionType.Players, creator);

            arrow.AddComponent(typeof(PhysicsComponent), arrowPhysics);
            genComponentManager.AddComponent(arrowPhysics);

            arrow.AddComponent(typeof(UnanimatedModelComponent), arrowGraphics);
            modelManager.AddComponent(arrowGraphics);

            arrow.AddComponent(typeof(AttackController), arrowAI);
            genComponentManager.AddComponent(arrowAI);

            attacks.Add(arrow);

            soundEffects.playRangedSound();
        }
        #endregion


        #region Misc
        public void CreateMouseSpikes(Vector3 position)
        {
            GameEntity spikes = new GameEntity("cursor", FactionType.Neutral, EntityType.Misc);

            Entity spikesPhysical = new Box(position, 1, 1, 1);
            spikes.AddSharedData(typeof(Entity), spikesPhysical);

            Model spikeModel = GetAnimatedModel("Models\\spikes");
            AnimationPlayer anims = new AnimationPlayer(spikeModel.Tag as SkinningData);
            spikes.AddSharedData(typeof(AnimationPlayer), anims);

            spikes.AddSharedData(typeof(Dictionary<string, AttachableModel>), new Dictionary<string, AttachableModel>());

            AnimatedModelComponent spikesGraphics = new AnimatedModelComponent(mainGame, spikes, spikeModel, 7, Vector3.Down * 20);
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
        #endregion

        #region Ability Definitions
        public Ability GetAbility(AbilityName ability)
        {
            switch (ability)
            {
                case AbilityName.Snipe:
                    return GetSnipe();
                case AbilityName.HeartStrike:
                    return GetHeartStrike();
                case AbilityName.IceClawPrison:
                    return GetIceClawPrison();
                default:
                    return null;
            }
        }

        public Ability GetSnipe()
        {
            return new Ability(1, Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\LW"), 1, AttackType.Ranged, "snipe", 1000);
        }

        public Ability GetHeartStrike()
        {
            return new Ability(1, Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\HS"), 6, AttackType.Ranged, "flip", 30);
        }

        public Ability GetIceClawPrison()
        {
            return new Ability(1, Game.Content.Load<Texture2D>("Textures\\whitePixel"), 6, AttackType.Ranged, "shoot", 1000);
        }
        #endregion

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
