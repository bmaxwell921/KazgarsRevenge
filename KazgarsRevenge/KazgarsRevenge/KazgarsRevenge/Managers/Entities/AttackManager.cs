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
        Random rand;

        public AttackManager(KazgarsRevengeGame game)
            : base(game)
        {
            rand = mainGame.rand;
        }

        public override void Initialize()
        {
            base.Initialize();

            soundEffects = Game.Services.GetService(typeof(SoundEffectLibrary)) as SoundEffectLibrary;

            particles = Game.Services.GetService(typeof(ParticleManager)) as ParticleManager;
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = attacks.Count - 1; i >= 0; --i)
            {
                if (attacks[i].Dead)
                {
                    attacks.RemoveAt(i);
                }
            }
            base.Update(gameTime);
        }

        #region Primaries
        public void CreateArrow(Vector3 position, Vector3 dir, int damage, AliveComponent creator, bool homing, bool penetrating, bool leeching, bool bleeding)
        {
            GameEntity arrow = new GameEntity("arrow", creator.Entity.Faction, EntityType.Misc);
            position.Y = 40;
            Entity arrowData = new Box(position, 17, 17, 15, .001f);
            arrowData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            arrowData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            arrowData.LinearVelocity = dir * 450.0f;
            arrowData.Orientation = Quaternion.CreateFromRotationMatrix(CreateRotationFromForward(dir));
            arrow.AddSharedData(typeof(Entity), arrowData);

            PhysicsComponent arrowPhysics = new PhysicsComponent(mainGame, arrow);
            UnanimatedModelComponent arrowGraphics =
                new UnanimatedModelComponent(mainGame, arrow, GetUnanimatedModel("Models\\Attachables\\arrow"),
                    new Vector3(10), Vector3.Backward * 6, 0, 0, 0);

            ProjectileController arrowAI = new ProjectileController(mainGame, arrow, damage, creator.Entity.Faction == FactionType.Players ? FactionType.Enemies : FactionType.Players, creator);
            if (homing)
            {
                arrowAI.Home();
                arrowGraphics.AddEmitter(typeof(HomingTrailParticleSystem), "trail", 80, 0, Vector3.Forward * 6);
            }
            if (penetrating)
            {
                arrowAI.Penetrate();
            }
            if (leeching)
            {
                arrowAI.ReturnLife(.1f);
            }
            if (bleeding)
            {
                arrowAI.Bleed();
            }
            

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
            GameEntity newAttack = new GameEntity("melee", creator.Entity.Faction, EntityType.Misc);
            newAttack.id = IdentificationFactory.getId(EntityType.Misc, players.myId.id);

            // Send it off to the server! TODO
            //((MessageSender)Game.Services.GetService(typeof(MessageSender))).SendMeleeAttackMessage(creator.Entity.id.id, newAttack.id.id, creator.Entity.Faction, position, damage);
            
            Entity attackData = new Box(position, 35, 47, 35, .01f);
            attackData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
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
            GameEntity newAttack = new GameEntity("melee", assocFact, EntityType.Misc);
            newAttack.id = new Identification(attackId, players.myId.id);

            EnemyManager em = (EnemyManager) Game.Services.GetService(typeof(EnemyManager));
            GameEntity creator = (assocFact == FactionType.Players) ? players.getEntity(new Identification(creatorId, players.myId.id)) : em.getEntity(new Identification(creatorId, players.myId.id));

            Entity attackData = new Box(position, 35, 47, 35, .01f);
            attackData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            attackData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            attackData.LinearVelocity = Vector3.Zero;
            newAttack.AddSharedData(typeof(Entity), attackData);

            PhysicsComponent attackPhysics = new PhysicsComponent(mainGame, newAttack);

            AttackController attackAI = new AttackController(mainGame, newAttack, damage, assocFact == FactionType.Players ? FactionType.Enemies : FactionType.Players, 
                (AliveComponent)creator.GetComponent(typeof(AliveComponent)));

            newAttack.AddComponent(typeof(PhysicsComponent), attackPhysics);
            genComponentManager.AddComponent(attackPhysics);

            newAttack.AddComponent(typeof(AttackController), attackAI);
            genComponentManager.AddComponent(attackAI);

            attacks.Add(newAttack);
            soundEffects.playMeleeSound();

            SpawnWeaponSparks(position + Vector3.Down * 18);
        }

        public void CreateMagicAttack()
        {

            soundEffects.playMagicSound();
        }
        #endregion

        #region Abilities
        public void CreateSnipe(Vector3 position, Vector3 dir, int damage, AliveComponent creator, bool magnet)
        {
            GameEntity arrow = new GameEntity("arrow", creator.Entity.Faction, EntityType.Misc);
            position.Y = 40;
            Entity arrowData = new Box(position, 17, 17, 32, .001f);
            arrowData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            arrowData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            arrowData.LinearVelocity = dir * 700;
            arrowData.Orientation = Quaternion.CreateFromRotationMatrix(CreateRotationFromForward(dir));
            arrow.AddSharedData(typeof(Entity), arrowData);

            PhysicsComponent arrowPhysics = new PhysicsComponent(mainGame, arrow);
            UnanimatedModelComponent arrowGraphics = new UnanimatedModelComponent(mainGame, arrow, GetUnanimatedModel("Models\\Attachables\\arrow"),
                                                                                new Vector3(20), Vector3.Backward * 20, 0, 0, 0);
            arrowGraphics.AddEmitter(typeof(SnipeTrailParticleSystem), "trail", 80, 0, Vector3.Forward * 8);

            SnipeController arrowAI = new SnipeController(mainGame, arrow, damage, creator.Entity.Faction == FactionType.Players ? FactionType.Enemies : FactionType.Players, creator, magnet);
            
            arrow.AddComponent(typeof(PhysicsComponent), arrowPhysics);
            genComponentManager.AddComponent(arrowPhysics);

            arrow.AddComponent(typeof(UnanimatedModelComponent), arrowGraphics);
            modelManager.AddComponent(arrowGraphics);

            arrow.AddComponent(typeof(AttackController), arrowAI);
            genComponentManager.AddComponent(arrowAI);

            attacks.Add(arrow);

            soundEffects.playRangedSound();
        }
        
        public void CreateOmnishot(Vector3 position, Vector3 dir, int damage, AliveComponent creator, bool homing, bool penetrating, bool leeching, bool bleeding)
        {
            GameEntity arrow = new GameEntity("arrow", creator.Entity.Faction, EntityType.Misc);
            position.Y = 40;
            Entity arrowData = new Box(position, 10, 17, 32, .001f);
            arrowData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            arrowData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            arrowData.LinearVelocity = dir * 400.0f;
            arrowData.Orientation = Quaternion.CreateFromRotationMatrix(CreateRotationFromForward(dir));
            arrow.AddSharedData(typeof(Entity), arrowData);

            PhysicsComponent arrowPhysics = new PhysicsComponent(mainGame, arrow);
            UnanimatedModelComponent arrowGraphics = new UnanimatedModelComponent(mainGame, arrow, GetUnanimatedModel("Models\\Attachables\\arrow"),
                                                                                new Vector3(10), Vector3.Backward * 6, 0, 0, 0);
            //arrowGraphics.AddEmitter(typeof(SoulTrailParticleSystem), 50, 2, Vector3.Zero);
            
            OmnishotController arrowAI = new OmnishotController(mainGame, arrow, damage, creator.Entity.Faction == FactionType.Players ? FactionType.Enemies : FactionType.Players, creator);

            arrowAI.AddEffects(penetrating, homing, leeching, bleeding);

            arrow.AddComponent(typeof(PhysicsComponent), arrowPhysics);
            genComponentManager.AddComponent(arrowPhysics);

            arrow.AddComponent(typeof(UnanimatedModelComponent), arrowGraphics);
            modelManager.AddComponent(arrowGraphics);

            arrow.AddComponent(typeof(AttackController), arrowAI);
            genComponentManager.AddComponent(arrowAI);

            attacks.Add(arrow);

            soundEffects.playRangedSound();
        }

        public void CreateLooseCannon(Vector3 position, Vector3 dir, int damage, AliveComponent creator, float percentCharged)
        {
            GameEntity arrow = new GameEntity("arrow", creator.Entity.Faction, EntityType.Misc);
            position.Y = 40;
            Entity arrowData = new Box(position, 10, 17, 32, .001f);
            arrowData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            arrowData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            arrowData.LinearVelocity = dir * (150 + 1000 * percentCharged);
            arrowData.Orientation = Quaternion.CreateFromRotationMatrix(CreateRotationFromForward(dir));
            arrow.AddSharedData(typeof(Entity), arrowData);

            PhysicsComponent arrowPhysics = new PhysicsComponent(mainGame, arrow);
            UnanimatedModelComponent arrowGraphics = new UnanimatedModelComponent(mainGame, arrow, GetUnanimatedModel("Models\\Attachables\\arrow"),
                                                                                new Vector3(10), Vector3.Backward * 6, 0, 0, 0);
            arrowGraphics.AddEmitter(typeof(SmokeTrailParticleSystem), "trail", 80, 0, Vector3.Forward * 13);

            LooseCannonController arrowAI = new LooseCannonController(mainGame, arrow, damage, creator.Entity.Faction == FactionType.Players ? FactionType.Enemies : FactionType.Players, creator, percentCharged);

            arrow.AddComponent(typeof(PhysicsComponent), arrowPhysics);
            genComponentManager.AddComponent(arrowPhysics);

            arrow.AddComponent(typeof(UnanimatedModelComponent), arrowGraphics);
            modelManager.AddComponent(arrowGraphics);

            arrow.AddComponent(typeof(AttackController), arrowAI);
            genComponentManager.AddComponent(arrowAI);

            attacks.Add(arrow);

            soundEffects.playRangedSound();
        }

        public void CreateExplosion(Vector3 position, int damage, AliveComponent creator, float intensity)
        {
            GameEntity newAttack = new GameEntity("explosion", creator.Entity.Faction, EntityType.Misc);

            Entity attackData = new Box(position, 120, 47, 120, .01f);
            attackData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            attackData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            attackData.LinearVelocity = Vector3.Zero;
            newAttack.AddSharedData(typeof(Entity), attackData);

            PhysicsComponent attackPhysics = new PhysicsComponent(mainGame, newAttack);
            AttackController attackAI = new AttackController(mainGame, newAttack, damage, creator.Entity.Faction == FactionType.Players ? FactionType.Enemies : FactionType.Players, creator);
            attackAI.HitMultipleTargets();

            newAttack.AddComponent(typeof(PhysicsComponent), attackPhysics);
            genComponentManager.AddComponent(attackPhysics);

            newAttack.AddComponent(typeof(AttackController), attackAI);
            genComponentManager.AddComponent(attackAI);

            attacks.Add(newAttack);

            SpawnExplosionParticles(position, intensity);
        }

        public void CreateMakeItRain(Vector3 position, int damage, float radius, AliveComponent creator)
        {
            GameEntity newAttack = new GameEntity("aoe", creator.Entity.Faction, EntityType.Misc);

            Entity attackData = new Cylinder(position, 47, radius);//new Box(position, radius * 2, 47, radius * 2, .01f);
            attackData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            attackData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            attackData.LinearVelocity = Vector3.Zero;
            newAttack.AddSharedData(typeof(Entity), attackData);

            PhysicsComponent attackPhysics = new PhysicsComponent(mainGame, newAttack);
            AttackController attackAI = new AttackController(mainGame, newAttack, damage, creator.Entity.Faction == FactionType.Players ? FactionType.Enemies : FactionType.Players, creator);
            attackAI.HitMultipleTargets();

            newAttack.AddComponent(typeof(PhysicsComponent), attackPhysics);
            genComponentManager.AddComponent(attackPhysics);

            newAttack.AddComponent(typeof(AttackController), attackAI);
            genComponentManager.AddComponent(attackAI);

            attacks.Add(newAttack);


            for (int i = 0; i < 50; ++i)
            {
                Vector3 randPos = new Vector3();
                randPos.X = rand.Next((int)(position.X - radius), (int)(position.X + radius));
                randPos.Z = rand.Next((int)(position.Z - radius), (int)(position.Z + radius));
                randPos.Y = rand.Next(80, 300);
                CreateFallingArrow(randPos);
            }
        }

        public void CreateFlashBomb(Vector3 startPos, Vector3 targetPos, float radius, AliveComponent creator)
        {
            GameEntity bomb = new GameEntity("arrow", FactionType.Neutral, EntityType.Misc);

            Entity bombData = new Box(startPos, 15, 50, 15, 1);
            bombData.IsAffectedByGravity = true;
            bombData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            bombData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            bombData.Orientation = Quaternion.CreateFromYawPitchRoll(0, 0, 0);

            Vector3 vel = targetPos - startPos;
            vel.Y = 0;
            bombData.LinearVelocity = Vector3.Up * 400 + vel * 2;

            bomb.AddSharedData(typeof(Entity), bombData);

            PhysicsComponent bombPhysics = new PhysicsComponent(mainGame, bomb);
            UnanimatedModelComponent bombGraphics = new UnanimatedModelComponent(mainGame, bomb, GetUnanimatedModel("Models\\Attachables\\Arrow"), new Vector3(20), Vector3.Zero, 0, 0, 0);
            FlashBomb bombController = new FlashBomb(mainGame, bomb, targetPos, creator, radius);

            bomb.AddComponent(typeof(PhysicsComponent), bombPhysics);
            genComponentManager.AddComponent(bombPhysics);

            bomb.AddComponent(typeof(UnanimatedModelComponent), bombGraphics);
            modelManager.AddComponent(bombGraphics);

            bomb.AddComponent(typeof(FallingArrowController), bombController);
            genComponentManager.AddComponent(bombController);

            attacks.Add(bomb);
        }

        public void CreateTarBomb(Vector3 startPos, Vector3 targetPos, float radius, AliveComponent creator)
        {
            GameEntity bomb = new GameEntity("arrow", FactionType.Neutral, EntityType.Misc);

            Entity bombData = new Box(startPos, 15, 50, 15, 1);
            bombData.IsAffectedByGravity = true;
            bombData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            bombData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            bombData.Orientation = Quaternion.CreateFromYawPitchRoll(0, 0, 0);

            Vector3 vel = targetPos - startPos;
            vel.Y = 0;
            bombData.LinearVelocity = Vector3.Up * 400 + vel * 2;

            bomb.AddSharedData(typeof(Entity), bombData);

            PhysicsComponent bombPhysics = new PhysicsComponent(mainGame, bomb);
            UnanimatedModelComponent bombGraphics = new UnanimatedModelComponent(mainGame, bomb, GetUnanimatedModel("Models\\Attachables\\Arrow"), new Vector3(20), Vector3.Zero, 0, 0, 0);
            TarBomb bombController = new TarBomb(mainGame, bomb, targetPos, creator, radius);

            bomb.AddComponent(typeof(PhysicsComponent), bombPhysics);
            genComponentManager.AddComponent(bombPhysics);

            bomb.AddComponent(typeof(UnanimatedModelComponent), bombGraphics);
            modelManager.AddComponent(bombGraphics);

            bomb.AddComponent(typeof(FallingArrowController), bombController);
            genComponentManager.AddComponent(bombController);

            attacks.Add(bomb);
        }

        public void CreateGrapplingHook(Vector3 position, Vector3 dir, AliveComponent creator, float speed)
        {
            GameEntity hook = new GameEntity("hook", FactionType.Neutral, EntityType.Misc);

            Entity hookData = new Box(position, 25, 25, 25);
            hookData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            hookData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            hookData.LinearVelocity = dir * speed;
            hookData.Orientation = Quaternion.CreateFromRotationMatrix(CreateRotationFromForward(dir));
            hook.AddSharedData(typeof(Entity), hookData);

            PhysicsComponent hookPhysics = new PhysicsComponent(mainGame, hook);
            UnanimatedModelComponent hookGraphics = new UnanimatedModelComponent(mainGame, hook,
                GetUnanimatedModel("Models\\Attachables\\arrow"), new Vector3(20), Vector3.Zero, 0, 0, 0);
            RopeBillboard chainComponent = new RopeBillboard(mainGame, hook, creator);
            GrapplingHookController hookController = new GrapplingHookController(mainGame, hook, creator);


            hook.AddComponent(typeof(PhysicsComponent), hookPhysics);
            genComponentManager.AddComponent(hookPhysics);

            hook.AddComponent(typeof(UnanimatedModelComponent), hookGraphics);
            modelManager.AddComponent(hookGraphics);

            hook.AddComponent(typeof(StretchingBillboard), chainComponent);
            billboardManager.AddComponent(chainComponent);

            hook.AddComponent(typeof(GrapplingHookController), hookController);
            genComponentManager.AddComponent(hookController);

            attacks.Add(hook);
        }

        public void CreateChainSpear(Vector3 position, Vector3 dir, AliveComponent creator, float speed, bool forceful)
        {
            GameEntity hook = new GameEntity("hook", FactionType.Neutral, EntityType.Misc);

            Entity hookData = new Box(position, 25, 25, 25);
            hookData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            hookData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            hookData.LinearVelocity = dir * speed;
            hookData.Orientation = Quaternion.CreateFromRotationMatrix(CreateRotationFromForward(dir));
            hook.AddSharedData(typeof(Entity), hookData);

            PhysicsComponent hookPhysics = new PhysicsComponent(mainGame, hook);
            UnanimatedModelComponent hookGraphics = new UnanimatedModelComponent(mainGame, hook,
                GetUnanimatedModel("Models\\Attachables\\arrow"), new Vector3(50), Vector3.Zero, 0, 0, 0);
            ChainBillboard chainComponent = new ChainBillboard(mainGame, hook, creator);
            ChainSpearController hookController = new ChainSpearController(mainGame, hook, creator, forceful);


            hook.AddComponent(typeof(PhysicsComponent), hookPhysics);
            genComponentManager.AddComponent(hookPhysics);

            hook.AddComponent(typeof(UnanimatedModelComponent), hookGraphics);
            modelManager.AddComponent(hookGraphics);

            hook.AddComponent(typeof(StretchingBillboard), chainComponent);
            billboardManager.AddComponent(chainComponent);

            hook.AddComponent(typeof(ChainSpearController), hookController);
            genComponentManager.AddComponent(hookController);

            attacks.Add(hook);
        }

        public void CreateMoltenBolt(Vector3 position, Vector3 dir, int damage, AliveComponent creator)
        {
            GameEntity arrow = new GameEntity("bolt", creator.Entity.Faction, EntityType.Misc);
            position.Y = 40;
            Entity arrowData = new Box(position, 10, 17, 32, .001f);
            arrowData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            arrowData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            arrowData.LinearVelocity = dir * 450;
            arrowData.Orientation = Quaternion.CreateFromRotationMatrix(CreateRotationFromForward(dir));
            arrow.AddSharedData(typeof(Entity), arrowData);

            PhysicsComponent arrowPhysics = new PhysicsComponent(mainGame, arrow);
            UnanimatedModelComponent arrowGraphics = new UnanimatedModelComponent(mainGame, arrow, GetUnanimatedModel("Models\\Attachables\\arrow"),
                                                                                new Vector3(20), Vector3.Backward * 20, 0, 0, 0);
            arrowGraphics.AddEmitter(typeof(FireArrowParticleSystem), "trail", 50, 5, Vector3.Zero);

            ProjectileController arrowAI = new ProjectileController(mainGame, arrow, damage, creator.Entity.Faction == FactionType.Players ? FactionType.Enemies : FactionType.Players, creator);
            arrowAI.Penetrate();
            arrowAI.AddDebuff(DeBuff.Igniting);

            arrow.AddComponent(typeof(PhysicsComponent), arrowPhysics);
            genComponentManager.AddComponent(arrowPhysics);

            arrow.AddComponent(typeof(UnanimatedModelComponent), arrowGraphics);
            modelManager.AddComponent(arrowGraphics);

            arrow.AddComponent(typeof(AttackController), arrowAI);
            genComponentManager.AddComponent(arrowAI);

            attacks.Add(arrow);

            soundEffects.playRangedSound();
        }

        public void CreateFrostbolt(Vector3 position, Vector3 dir, int damage, AliveComponent creator)
        {
            GameEntity bolt = new GameEntity("arrow", creator.Entity.Faction, EntityType.Misc);
            position.Y = 40;
            Entity boltData = new Box(position, 10, 17, 32, .001f);
            boltData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            boltData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            boltData.LinearVelocity = dir * 450;
            boltData.Orientation = Quaternion.CreateFromRotationMatrix(CreateRotationFromForward(dir));
            bolt.AddSharedData(typeof(Entity), boltData);

            PhysicsComponent boltPhysics = new PhysicsComponent(mainGame, bolt);
            UnanimatedModelComponent boltGraphics = new UnanimatedModelComponent(mainGame, bolt, GetUnanimatedModel("Models\\Projectiles\\frost_bolt"),
                                                                                new Vector3(20), Vector3.Backward * 20, 0, 0, 0);
            boltGraphics.AddRollSpeed(.3f);
            boltGraphics.SetAlpha(.75f);
            boltGraphics.TurnOffOutline();

            boltGraphics.AddEmitter(typeof(FrostboltTrailParticleSystem), "trail", 20, 10, Vector3.Zero);
            boltGraphics.AddEmitter(typeof(FrostMistParticleSystem), "misttrail", 80, 10, Vector3.Zero);

            ProjectileController boltAI = new ProjectileController(mainGame, bolt, damage, creator.Entity.Faction == FactionType.Players ? FactionType.Enemies : FactionType.Players, creator);
            boltAI.KillOnFirstContact();
            boltAI.AddDebuff(DeBuff.Frost);

            bolt.AddComponent(typeof(PhysicsComponent), boltPhysics);
            genComponentManager.AddComponent(boltPhysics);

            bolt.AddComponent(typeof(UnanimatedModelComponent), boltGraphics);
            modelManager.AddComponent(boltGraphics);

            bolt.AddComponent(typeof(AttackController), boltAI);
            genComponentManager.AddComponent(boltAI);

            attacks.Add(bolt);
        }

        public void CreateFirebolt(Vector3 position, Vector3 dir, int damage, AliveComponent creator)
        {
            GameEntity bolt = new GameEntity("bolt", creator.Entity.Faction, EntityType.Misc);
            position.Y = 40;
            Entity boltData = new Box(position, 10, 17, 32, .001f);
            boltData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            boltData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            boltData.LinearVelocity = dir * 450;
            boltData.Orientation = Quaternion.CreateFromRotationMatrix(CreateRotationFromForward(dir));
            bolt.AddSharedData(typeof(Entity), boltData);

            PhysicsComponent boltPhysics = new PhysicsComponent(mainGame, bolt);
            UnanimatedModelComponent boltGraphics = new UnanimatedModelComponent(mainGame, bolt, GetUnanimatedModel("Models\\Projectiles\\frost_bolt"),
                                                                                new Vector3(20), Vector3.Backward * 20, 0, 0, 0);
            boltGraphics.AddRollSpeed(.3f);
            boltGraphics.TurnOffOutline();

            boltGraphics.AddEmitter(typeof(FireArrowParticleSystem), "trail", 50, 10, Vector3.Zero);
            //boltGraphics.AddEmitter(typeof(FireMistTrailSystem), "mist", 80, 10, Vector3.Zero);

            ProjectileController boltAI = new ProjectileController(mainGame, bolt, damage, creator.Entity.Faction == FactionType.Players ? FactionType.Enemies : FactionType.Players, creator);
            boltAI.KillOnFirstContact();

            bolt.AddComponent(typeof(PhysicsComponent), boltPhysics);
            genComponentManager.AddComponent(boltPhysics);

            bolt.AddComponent(typeof(UnanimatedModelComponent), boltGraphics);
            modelManager.AddComponent(boltGraphics);

            bolt.AddComponent(typeof(AttackController), boltAI);
            genComponentManager.AddComponent(boltAI);

            attacks.Add(bolt);
        }

        public void CreateFireSpitBomb(Vector3 startPos, Vector3 targetPos, float radius, AliveComponent creator)
        {
            GameEntity bomb = new GameEntity("firespit", FactionType.Neutral, EntityType.Misc);

            Entity bombData = new Box(startPos, 15, 50, 15, 1);
            bombData.IsAffectedByGravity = true;
            bombData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            bombData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            bombData.Orientation = Quaternion.CreateFromYawPitchRoll(0, 0, 0);

            Vector3 vel = targetPos - startPos;
            vel.Y = 0;
            bombData.LinearVelocity = Vector3.Up * 400 + vel * 2;

            bomb.AddSharedData(typeof(Entity), bombData);

            PhysicsComponent bombPhysics = new PhysicsComponent(mainGame, bomb);
            UnanimatedModelComponent bombGraphics = new UnanimatedModelComponent(mainGame, bomb, GetUnanimatedModel("Models\\Attachables\\Arrow"), new Vector3(20), Vector3.Zero, 0, 0, 0);
            bombGraphics.AddEmitter(typeof(FireArrowParticleSystem), "trail", 50, 5, Vector3.Zero);

            FireSpitBomb bombController = new FireSpitBomb(mainGame, bomb, targetPos, creator, radius);

            bomb.AddComponent(typeof(PhysicsComponent), bombPhysics);
            genComponentManager.AddComponent(bombPhysics);

            bomb.AddComponent(typeof(UnanimatedModelComponent), bombGraphics);
            modelManager.AddComponent(bombGraphics);

            bomb.AddComponent(typeof(FallingArrowController), bombController);
            genComponentManager.AddComponent(bombController);

            attacks.Add(bomb);
        }
        #endregion

        #region Misc
        public void CreateFireSpitExplosion(Vector3 position, float radius, AliveComponent creator)
        {
            GameEntity newAttack = new GameEntity("explosion", creator.Entity.Faction, EntityType.Misc);

            Entity attackData = new Cylinder(position, 47, radius);
            attackData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            attackData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            attackData.LinearVelocity = Vector3.Zero;
            newAttack.AddSharedData(typeof(Entity), attackData);

            PhysicsComponent attackPhysics = new PhysicsComponent(mainGame, newAttack);
            AttackController attackAI = new AttackController(mainGame, newAttack, 0, GetHitFaction(creator), creator);
            attackAI.HitMultipleTargets();

            newAttack.AddComponent(typeof(PhysicsComponent), attackPhysics);
            genComponentManager.AddComponent(attackPhysics);

            newAttack.AddComponent(typeof(AttackController), attackAI);
            genComponentManager.AddComponent(attackAI);

            attacks.Add(newAttack);

            SpawnFireSpitExplosionParticles(position);
        }

        public void CreateFlashExplosion(Vector3 position, float radius, AliveComponent creator)
        {
            GameEntity newAttack = new GameEntity("explosion", creator.Entity.Faction, EntityType.Misc);

            Entity attackData = new Cylinder(position, 47, radius);
            attackData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            attackData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            attackData.LinearVelocity = Vector3.Zero;
            newAttack.AddSharedData(typeof(Entity), attackData);

            PhysicsComponent attackPhysics = new PhysicsComponent(mainGame, newAttack);
            AttackController attackAI = new AttackController(mainGame, newAttack, 0, GetHitFaction(creator), creator);
            attackAI.HitMultipleTargets();
            attackAI.AddDebuff(DeBuff.FlashBomb);

            newAttack.AddComponent(typeof(PhysicsComponent), attackPhysics);
            genComponentManager.AddComponent(attackPhysics);

            newAttack.AddComponent(typeof(AttackController), attackAI);
            genComponentManager.AddComponent(attackAI);

            attacks.Add(newAttack);

            SpawnFlashParticles(position);
        }

        public void CreateTarExplosion(Vector3 position, float radius, AliveComponent creator)
        {
            GameEntity newAttack = new GameEntity("explosion", creator.Entity.Faction, EntityType.Misc);

            Entity attackData = new Cylinder(position, 47, radius);
            attackData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            attackData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            attackData.LinearVelocity = Vector3.Zero;
            newAttack.AddSharedData(typeof(Entity), attackData);

            PhysicsComponent attackPhysics = new PhysicsComponent(mainGame, newAttack);
            AttackController attackAI = new AttackController(mainGame, newAttack, 0, creator.Entity.Faction == FactionType.Players ? FactionType.Enemies : FactionType.Players, creator);
            attackAI.HitMultipleTargets();
            attackAI.AddDebuff(DeBuff.Tar);

            newAttack.AddComponent(typeof(PhysicsComponent), attackPhysics);
            genComponentManager.AddComponent(attackPhysics);

            newAttack.AddComponent(typeof(AttackController), attackAI);
            genComponentManager.AddComponent(attackAI);

            attacks.Add(newAttack);

            SpawnTarParticles(position);
        }

        public void CreateFallingArrow(Vector3 position)
        {
            GameEntity arrow = new GameEntity("arrow", FactionType.Neutral, EntityType.Misc);

            Entity arrowData = new Box(position, 15, 50, 15, 1);
            arrowData.IsAffectedByGravity = true;
            arrowData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            arrowData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            arrowData.LinearVelocity = Vector3.Down * 50;
            arrowData.Orientation = Quaternion.CreateFromYawPitchRoll(0, MathHelper.PiOver2, 0);
            arrow.AddSharedData(typeof(Entity), arrowData);
            
            PhysicsComponent arrowPhysics = new PhysicsComponent(mainGame, arrow);
            UnanimatedModelComponent arrowGraphics = new UnanimatedModelComponent(mainGame, arrow, GetUnanimatedModel("Models\\Attachables\\Arrow"), new Vector3(20), Vector3.Zero, 0, 0, 0);
            FallingArrowController arrowController = new FallingArrowController(mainGame, arrow);
            arrowGraphics.AddEmitter(typeof(HomingTrailParticleSystem), "trail", 10, 2, Vector3.Zero);

            arrow.AddComponent(typeof(PhysicsComponent), arrowPhysics);
            genComponentManager.AddComponent(arrowPhysics);

            arrow.AddComponent(typeof(UnanimatedModelComponent), arrowGraphics);
            modelManager.AddComponent(arrowGraphics);

            arrow.AddComponent(typeof(FallingArrowController), arrowController);
            genComponentManager.AddComponent(arrowController);

            attacks.Add(arrow);
        }

        public void CreateExplosionDebris(Vector3 position)
        {
            GameEntity debris = new GameEntity("debris", FactionType.Neutral, EntityType.None);

            float dir = (float)rand.Next(0, 628) / 100f;
            Entity physicalData = new Box(position, 1, 1, 50);
            physicalData.IsAffectedByGravity = false;
            physicalData.LinearVelocity = new Vector3((float)Math.Cos(dir) * 100, 150, (float)Math.Sin(dir) * 100);
            debris.AddSharedData(typeof(Entity), physicalData);

            PhysicsComponent physics = new PhysicsComponent(mainGame, debris);
            debris.AddComponent(typeof(PhysicsComponent), physics);
            genComponentManager.AddComponent(physics);

            DebrisController controller = new DebrisController(mainGame, debris);
            debris.AddComponent(typeof(DebrisController), controller);
            genComponentManager.AddComponent(controller);

            UnanimatedModelComponent graphics = new UnanimatedModelComponent(mainGame, debris);
            graphics.AddEmitter(typeof(ToonExplosionDebrisSystem), "trail", 20, 0, Vector3.Zero);
            graphics.AddEmitterSizeIncrement("trail", -.75f);
            debris.AddComponent(typeof(UnanimatedModelComponent), graphics);
            modelManager.AddComponent(graphics);

            attacks.Add(debris);
        }

        public void CreateHomingHeal(Vector3 position, AliveComponent target, int healAmount)
        {
            GameEntity heal = new GameEntity("heal", FactionType.Neutral, EntityType.Misc);
            position.Y = 20;
            Entity healPhysicalData = new Box(position, 1, 47, 1, .001f);
            healPhysicalData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            healPhysicalData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            healPhysicalData.LinearVelocity = Vector3.Forward * 250.0f;
            healPhysicalData.Orientation = Quaternion.CreateFromRotationMatrix(CreateRotationFromForward(Vector3.Forward));
            heal.AddSharedData(typeof(Entity), healPhysicalData);

            PhysicsComponent healPhysics = new PhysicsComponent(mainGame, heal);
            UnanimatedModelComponent healGraphics = new UnanimatedModelComponent(mainGame, heal);
            healGraphics.AddEmitter(typeof(HealTrailParticleSystem), "trail", 75, 5, Vector3.Zero);

            HomingHealController healAI = new HomingHealController(mainGame, heal, target, healAmount);

            heal.AddComponent(typeof(PhysicsComponent), healPhysics);
            genComponentManager.AddComponent(healPhysics);

            heal.AddComponent(typeof(UnanimatedModelComponent), healGraphics);
            modelManager.AddComponent(healGraphics);

            heal.AddComponent(typeof(AttackController), healAI);
            genComponentManager.AddComponent(healAI);

            attacks.Add(heal);
        }
        
        public void CreateMouseSpikes(Vector3 position)
        {
            position.Y = 0;
            GameEntity spikes = new GameEntity("cursor", FactionType.Neutral, EntityType.Misc);

            Entity spikesPhysical = new Box(position, 1, 1, 1);
            spikes.AddSharedData(typeof(Entity), spikesPhysical);

            Model spikeModel = GetAnimatedModel("Models\\spikes");
            AnimationPlayer anims = new AnimationPlayer(spikeModel.Tag as SkinningData);
            spikes.AddSharedData(typeof(AnimationPlayer), anims);

            spikes.AddSharedData(typeof(Dictionary<string, AttachableModel>), new Dictionary<string, AttachableModel>());

            AnimatedModelComponent spikesGraphics = new AnimatedModelComponent(mainGame, spikes, spikeModel, 7, Vector3.Zero);
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

        private FactionType GetHitFaction(AliveComponent creator)
        {
            return creator.Entity.Faction == FactionType.Players ? FactionType.Enemies : FactionType.Players;
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

        public void SpawnHitBlood(Vector3 position)
        {
            ParticleSystem blood = particles.GetSystem(typeof(BloodParticleSystem));
            for (int i = 0; i < 10; ++i)
            {
                blood.AddParticle(position, Vector3.Zero);
            }
        }

        public void SpawnHitSparks(Vector3 position, Vector3 forward)
        {
            ParticleSystem sparks = particles.GetSystem(typeof(HitSparksSystem));
            forward *= 25;
            for (int i = 0; i < 10; ++i)
            {
                sparks.AddParticle(position, forward);
            }
        }

        public void SpawnLittleBloodSpurt(Vector3 position)
        {
            ParticleSystem blood = particles.GetSystem(typeof(BloodParticleSystem));
            for (int i = 0; i < 10; ++i)
            {
                blood.AddParticle(position, Vector3.Zero);
            }
        }

        public void SpawnBuffParticles(Vector3 position)
        {
            ParticleSystem poof = particles.GetSystem(typeof(Buff1ParticleSystem));
            for (int i = 0; i < 20; ++i)
            {
                poof.AddParticle(position, Vector3.Zero);
            }
        }

        public void SpawnExplosionParticles(Vector3 position, float intensity)
        {
            ParticleSystem boom = particles.GetSystem(typeof(ToonExplosionMainSystem));
            for (int i = 0; i < (int)(19 * intensity + 1); ++i)
            {
                boom.AddParticle(position, Vector3.Zero);
            }

            boom = particles.GetSystem(typeof(ToonExplosionPoofSystem));
            for (int i = 0; i < (int)(34 * intensity + 1); ++i)
            {
                boom.AddParticle(position, Vector3.Zero);
            }

            for (int i = 0; i < (int)(10 * intensity); ++i)
            {
                CreateExplosionDebris(position);
            }
        }

        public void SpawnFireSpitExplosionParticles(Vector3 position)
        {
            SpawnExplosionParticles(position, 1.5f);
        }

        public void SpawnFlashParticles(Vector3 position)
        {
            ParticleSystem boom = particles.GetSystem(typeof(FlashExplosionSmokeBig));
            for (int i = 0; i < 55; ++i)
            {
                boom.AddParticle(position, Vector3.Zero);
            }
        }

        public void SpawnTarParticles(Vector3 position)
        {
            ParticleSystem boom = particles.GetSystem(typeof(TarExplosionParticleSystem));
            for (int i = 0; i < 30; ++i)
            {
                boom.AddParticle(position, Vector3.Zero);
            }
        }
        #endregion
    }
}
