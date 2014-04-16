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
        public void CreateArrow(Vector3 position, Vector3 dir, int damage, AliveComponent creator, bool homing, bool penetrating, bool leeching, bool bleeding, bool multiShot)
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
                new UnanimatedModelComponent(mainGame, arrow, GetUnanimatedModel("Models\\Projectiles\\Arrow"),
                    new Vector3(10), Vector3.Backward * 6, 0, 0, 0);

            ProjectileController arrowAI = new ProjectileController(mainGame, arrow, damage, creator.Entity.Faction == FactionType.Players ? FactionType.Enemies : FactionType.Players, creator);
            if (homing)
            {
                arrowAI.Home();
                arrowGraphics.AddEmitter(typeof(HomingTrailParticleSystem), "trail", 80, 0, Vector3.Forward * 6);
            }
            else if(!multiShot)
            {
                ArrowVBillboard trail = new ArrowVBillboard(mainGame, arrow);
                arrow.AddComponent(typeof(ArrowVBillboard), trail);
                billboardManager.AddComponent(trail);
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

            if (!multiShot)
            {
                soundEffects.playRangedSound();
            }
        }



        public void CreateMeleeAttack(Vector3 position, int damage, AliveComponent creator, bool twohanded)
        {
            CreateMeleeAttack(position, damage, creator, 0, twohanded);
        }
        public void CreateMeleeAttack(Vector3 position, int damage, AliveComponent creator, float lifesteal, bool twohanded)
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
            AttackController attackAI = new AttackController(mainGame, newAttack, damage, creator.Entity.Faction == FactionType.Players ? FactionType.Enemies : FactionType.Players, creator, AttackType.Melee);
            if (lifesteal != 0)
            {
                attackAI.ReturnLife(lifesteal);
            }

            newAttack.AddComponent(typeof(PhysicsComponent), attackPhysics);
            genComponentManager.AddComponent(attackPhysics);

            newAttack.AddComponent(typeof(AttackController), attackAI);
            genComponentManager.AddComponent(attackAI);

            attacks.Add(newAttack);

            soundEffects.playMeleeMissSound();
            if (!twohanded)
            {
                soundEffects.playMeleeHitFloorSound();
            }
        }

        /// <summary>
        /// Called from a network message to create a Melee Attack
        /// </summary>
        public void CreateMeleeAttack(int creatorId, int attackId, FactionType assocFact, Vector3 position, int damage, bool twoHanded)
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
                (AliveComponent)creator.GetComponent(typeof(AliveComponent)), AttackType.Melee);

            newAttack.AddComponent(typeof(PhysicsComponent), attackPhysics);
            genComponentManager.AddComponent(attackPhysics);

            newAttack.AddComponent(typeof(AttackController), attackAI);
            genComponentManager.AddComponent(attackAI);

            attacks.Add(newAttack);
            soundEffects.playMeleeMissSound();
            if (!twoHanded)
            {
                soundEffects.playMeleeHitFloorSound();
            }

            SpawnWeaponSparks(position + Vector3.Down * 18);
        }

        public void CreateMagicAttack(Vector3 position, Vector3 dir, int damage, AliveComponent creator)
        {

            GameEntity magicks = new GameEntity("magic", creator.Entity.Faction, EntityType.Misc);
            position.Y = 40;
            Entity magicData = new Box(position, 17, 17, 15, .001f);
            magicData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            magicData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            magicData.LinearVelocity = dir * 450.0f;
            magicData.Orientation = Quaternion.CreateFromRotationMatrix(CreateRotationFromForward(dir));
            magicks.AddSharedData(typeof(Entity), magicData);

            PhysicsComponent magicPhysics = new PhysicsComponent(mainGame, magicks);
            UnanimatedModelComponent arrowGraphics =
                new UnanimatedModelComponent(mainGame, magicks);
            arrowGraphics.AddEmitter(typeof(MagicPrimaryTrail), "trail", 80, 0, Vector3.Forward * 6);

            ProjectileController magicController = new ProjectileController(mainGame, magicks, damage, GetHitFaction(creator), creator);
            magicController.KillOnFirstContact();

            magicks.AddComponent(typeof(PhysicsComponent), magicPhysics);
            genComponentManager.AddComponent(magicPhysics);

            magicks.AddComponent(typeof(UnanimatedModelComponent), arrowGraphics);
            modelManager.AddComponent(arrowGraphics);

            magicks.AddComponent(typeof(AttackController), magicController);
            genComponentManager.AddComponent(magicController);

            attacks.Add(magicks);

            soundEffects.playMagicSound();
        }
        #endregion

        #region Abilities
        //Ranged
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
            UnanimatedModelComponent arrowGraphics = new UnanimatedModelComponent(mainGame, arrow, GetUnanimatedModel("Models\\Projectiles\\Arrow"),
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
            UnanimatedModelComponent arrowGraphics = new UnanimatedModelComponent(mainGame, arrow, GetUnanimatedModel("Models\\Projectiles\\Arrow"),
                                                                                new Vector3(10), Vector3.Backward * 6, 0, 0, 0);
            ArrowVBillboard trailv = new ArrowVBillboard(mainGame, arrow);
            arrow.AddComponent(typeof(ArrowVBillboard), trailv);
            billboardManager.AddComponent(trailv);
            
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
            UnanimatedModelComponent arrowGraphics = new UnanimatedModelComponent(mainGame, arrow, GetUnanimatedModel("Models\\Projectiles\\Arrow"),
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

            Entity attackData = new Box(position, 120, 47, 120);
            attackData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            attackData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            attackData.LinearVelocity = Vector3.Zero;
            newAttack.AddSharedData(typeof(Entity), attackData);

            PhysicsComponent attackPhysics = new PhysicsComponent(mainGame, newAttack);
            AttackController attackAI = new AttackController(mainGame, newAttack, damage, creator.Entity.Faction == FactionType.Players ? FactionType.Enemies : FactionType.Players, creator, AttackType.None);
            attackAI.HitMultipleTargets();

            newAttack.AddComponent(typeof(PhysicsComponent), attackPhysics);
            genComponentManager.AddComponent(attackPhysics);

            newAttack.AddComponent(typeof(AttackController), attackAI);
            genComponentManager.AddComponent(attackAI);

            attacks.Add(newAttack);

            SpawnFireExplosionParticles(position, intensity);
        }

        public void CreateMakeItRain(Vector3 position, int damage, float radius, AliveComponent creator)
        {
            GameEntity newAttack = new GameEntity("aoe", creator.Entity.Faction, EntityType.Misc);

            Entity attackData = new Cylinder(position, 47, radius);
            attackData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            attackData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            attackData.LinearVelocity = Vector3.Zero;
            newAttack.AddSharedData(typeof(Entity), attackData);

            PhysicsComponent attackPhysics = new PhysicsComponent(mainGame, newAttack);
            AttackController attackAI = new AttackController(mainGame, newAttack, damage, creator.Entity.Faction == FactionType.Players ? FactionType.Enemies : FactionType.Players, creator, AttackType.Ranged);
            attackAI.HitMultipleTargets();

            newAttack.AddComponent(typeof(PhysicsComponent), attackPhysics);
            genComponentManager.AddComponent(attackPhysics);

            newAttack.AddComponent(typeof(AttackController), attackAI);
            genComponentManager.AddComponent(attackAI);

            attacks.Add(newAttack);


            for (int i = 0; i < radius / 2; ++i)
            {
                float randAngle = (float)rand.Next(628) / 100.0f;
                float randDist = rand.Next(5, (int)(radius));
                Vector3 randPos = new Vector3();
                randPos.X = (float)(Math.Cos(randAngle) * randDist) + position.X;
                randPos.Y = rand.Next(80, 300);
                randPos.Z = (float)(Math.Sin(randAngle) * randDist) + position.Z;
                CreateFallingArrow(randPos);
            }
        }

        public void CreateFlashBomb(Vector3 startPos, Vector3 targetPos, float radius, bool tar, AliveComponent creator)
        {
            GameEntity bomb = new GameEntity("arrow", FactionType.Neutral, EntityType.Misc);

            Entity bombData = new Box(startPos, 15, 50, 15, 1);
            bombData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;

            Vector3 vel = targetPos - startPos;
            vel.Y = 0;
            bombData.LinearVelocity = Vector3.Up * 400 + vel * 2;

            bomb.AddSharedData(typeof(Entity), bombData);

            PhysicsComponent bombPhysics = new PhysicsComponent(mainGame, bomb);
            UnanimatedModelComponent bombGraphics = new UnanimatedModelComponent(mainGame, bomb, GetUnanimatedModel("Models\\Projectiles\\grenade"), new Vector3(20), Vector3.Zero, 0, 0, 0);
            FlashBomb bombController = new FlashBomb(mainGame, bomb, targetPos, creator, radius, tar);

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
            bombData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;

            Vector3 vel = targetPos - startPos;
            vel.Y = 0;
            bombData.LinearVelocity = Vector3.Up * 400 + vel * 2;

            bomb.AddSharedData(typeof(Entity), bombData);

            PhysicsComponent bombPhysics = new PhysicsComponent(mainGame, bomb);
            UnanimatedModelComponent bombGraphics = new UnanimatedModelComponent(mainGame, bomb, GetUnanimatedModel("Models\\Projectiles\\Arrow"), new Vector3(20), Vector3.Zero, 0, 0, 0);
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
                GetUnanimatedModel("Models\\Projectiles\\Arrow"), new Vector3(20), Vector3.Zero, 0, 0, 0);
            RopeBillboard chainComponent = new RopeBillboard(mainGame, hook, creator.Entity.GetSharedData(typeof(Entity)) as Entity);
            GrapplingHookController hookController = new GrapplingHookController(mainGame, hook, creator);


            hook.AddComponent(typeof(PhysicsComponent), hookPhysics);
            genComponentManager.AddComponent(hookPhysics);

            hook.AddComponent(typeof(UnanimatedModelComponent), hookGraphics);
            modelManager.AddComponent(hookGraphics);

            hook.AddComponent(typeof(HorizontalStretchingBillboard), chainComponent);
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
                GetUnanimatedModel("Models\\Projectiles\\Arrow"), new Vector3(30), Vector3.Zero, 0, 0, 0);
            ChainBillboard chainComponent = new ChainBillboard(mainGame, hook, creator.Entity.GetSharedData(typeof(Entity)) as Entity);
            ChainSpearController hookController = new ChainSpearController(mainGame, hook, creator, forceful);


            hook.AddComponent(typeof(PhysicsComponent), hookPhysics);
            genComponentManager.AddComponent(hookPhysics);

            hook.AddComponent(typeof(UnanimatedModelComponent), hookGraphics);
            modelManager.AddComponent(hookGraphics);

            hook.AddComponent(typeof(HorizontalStretchingBillboard), chainComponent);
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
            UnanimatedModelComponent arrowGraphics = new UnanimatedModelComponent(mainGame, arrow, GetUnanimatedModel("Models\\Projectiles\\Arrow"),
                                                                                new Vector3(20), Vector3.Backward * 20, 0, 0, 0);
            arrowGraphics.AddEmitter(typeof(FireArrowParticleSystem), "trail", 50, 5, Vector3.Zero);

            ProjectileController arrowAI = new ProjectileController(mainGame, arrow, damage, creator.Entity.Faction == FactionType.Players ? FactionType.Enemies : FactionType.Players, creator);
            arrowAI.Penetrate();
            arrowAI.AddDebuff(DeBuff.Burning);

            arrow.AddComponent(typeof(PhysicsComponent), arrowPhysics);
            genComponentManager.AddComponent(arrowPhysics);

            arrow.AddComponent(typeof(UnanimatedModelComponent), arrowGraphics);
            modelManager.AddComponent(arrowGraphics);

            arrow.AddComponent(typeof(AttackController), arrowAI);
            genComponentManager.AddComponent(arrowAI);

            attacks.Add(arrow);

            soundEffects.playRangedSound();
        }


        //Melee
        public void CreateCharge(Vector3 pos, AliveComponent creator, double duration)
        {
            GameEntity entity = new GameEntity("attack", FactionType.Neutral, EntityType.None);

            Entity box = new Box(pos, 55, 20, 55);
            box.IsAffectedByGravity = false;
            box.LocalInertiaTensorInverse = new Matrix3X3();
            box.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            entity.AddSharedData(typeof(Entity), box);

            PhysicsComponent physics = new PhysicsComponent(mainGame, entity);
            entity.AddComponent(typeof(PhysicsComponent), physics);
            genComponentManager.AddComponent(physics);

            ChargeController controller = new ChargeController(mainGame, entity, duration, GetHitFaction(creator), creator);
            entity.AddComponent(typeof(ChargeController), controller);
            genComponentManager.AddComponent(controller);

            attacks.Add(entity);
        }

        public void CreateCleave(Vector3 position, int damage, AliveComponent creator, bool decap, bool invig)
        {
            position.Y = 20;
            GameEntity cleave = new GameEntity("att", FactionType.Neutral, EntityType.None);

            Entity physicalData = new Box(position, 100, 40, 100);
            physicalData.IsAffectedByGravity = false;
            physicalData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            cleave.AddSharedData(typeof(Entity), physicalData);

            PhysicsComponent physics = new PhysicsComponent(mainGame, cleave);
            cleave.AddComponent(typeof(PhysicsComponent), physics);
            genComponentManager.AddComponent(physics);

            CleaveController controller = new CleaveController(mainGame, cleave, damage, GetHitFaction(creator), creator, decap, invig);
            cleave.AddComponent(typeof(AttackController), controller);
            genComponentManager.AddComponent(controller);

            attacks.Add(cleave);
        }

        public void DevastingStrike(Vector3 position, Vector3 dir, int damage, AliveComponent creator, bool reach)
        {
            GameEntity newAttack = new GameEntity("melee", creator.Entity.Faction, EntityType.Misc);
            newAttack.id = IdentificationFactory.getId(EntityType.Misc, players.myId.id);

            if (reach)
            {
                position += dir * 50;
            }
            else
            {
                position += dir * 35;
            }

            Entity attackData = new Box(position, 35, 47, reach? 100 : 35);
            attackData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            attackData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            attackData.Orientation = Quaternion.CreateFromRotationMatrix(CreateRotationFromForward(dir));
            newAttack.AddSharedData(typeof(Entity), attackData);

            PhysicsComponent attackPhysics = new PhysicsComponent(mainGame, newAttack);
            AttackController attackAI = new AttackController(mainGame, newAttack, damage, creator.Entity.Faction == FactionType.Players ? FactionType.Enemies : FactionType.Players, creator, AttackType.Melee);
            if (reach)
            {
                attackAI.HitMultipleTargets();
            }

            newAttack.AddComponent(typeof(PhysicsComponent), attackPhysics);
            genComponentManager.AddComponent(attackPhysics);

            newAttack.AddComponent(typeof(AttackController), attackAI);
            genComponentManager.AddComponent(attackAI);

            attacks.Add(newAttack);


            position.Y = 2;
            if (reach)
            {
                camera.ShakeCamera(25);
                for (int i = -5; i < 5; ++i)
                {
                    SpawnWeaponSparks(position + dir * i * 8);
                }
            }
            else
            {
                camera.ShakeCamera(4);
                for (int i = -1; i < 1; ++i)
                {
                    SpawnWeaponSparks(position + dir * i * 5);
                }
            }

            soundEffects.PlayAbilitySound(AbilityName.DevastatingStrike);
        }

        public void CreateReflect(Vector3 position, float yaw, AliveComponent creator)
        {
            position.Y = 20;
            GameEntity cleave = new GameEntity("att", FactionType.Neutral, EntityType.None);

            Entity physicalData = new Box(position, 40, 40, 100);
            physicalData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            physicalData.IsAffectedByGravity = false;
            physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(yaw, 0, 0);
            cleave.AddSharedData(typeof(Entity), physicalData);

            PhysicsComponent physics = new PhysicsComponent(mainGame, cleave);
            cleave.AddComponent(typeof(PhysicsComponent), physics);
            genComponentManager.AddComponent(physics);

            ReflectController controller = new ReflectController(mainGame, cleave, GetHitFaction(creator), creator);
            cleave.AddComponent(typeof(AttackController), controller);
            genComponentManager.AddComponent(controller);

            attacks.Add(cleave);
        }

        public void CreateSwordnado(Vector3 position, double duration, int damage, AliveComponent creator)
        {
            position.Y = 20;
            GameEntity cleave = new GameEntity("att", FactionType.Neutral, EntityType.None);

            Entity physicalData = new Box(position, 100, 40, 100);
            physicalData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            physicalData.IsAffectedByGravity = false;
            cleave.AddSharedData(typeof(Entity), physicalData);

            PhysicsComponent physics = new PhysicsComponent(mainGame, cleave);
            cleave.AddComponent(typeof(PhysicsComponent), physics);
            genComponentManager.AddComponent(physics);

            SwordnadoController controller = new SwordnadoController(mainGame, cleave, damage, creator, duration, GetHitFaction(creator));
            cleave.AddComponent(typeof(AttackController), controller);
            genComponentManager.AddComponent(controller);

            attacks.Add(cleave);
        }
        
        public void CreateHeadbutt(Vector3 position, float yaw, AliveComponent creator)
        {
            position.Y = 20;
            GameEntity entity = new GameEntity("att", FactionType.Neutral, EntityType.None);

            Entity physicalData = new Box(position, 40, 40, 40);
            physicalData.IsAffectedByGravity = false;
            physicalData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            entity.AddSharedData(typeof(Entity), physicalData);

            PhysicsComponent physics = new PhysicsComponent(mainGame, entity);
            entity.AddComponent(typeof(PhysicsComponent), physics);
            genComponentManager.AddComponent(physics);

            AttackController controller = new AttackController(mainGame, entity, 0, GetHitFaction(creator), creator, AttackType.Melee);
            controller.AddDebuff(DeBuff.Headbutt);
            controller.HitMultipleTargets();
            entity.AddComponent(typeof(AttackController), controller);
            genComponentManager.AddComponent(controller);

            attacks.Add(entity);
        }
        
        public void CreateGarrote(Vector3 position, float yaw, AliveComponent creator, int damage, bool twist)
        {
            position.Y = 20;
            GameEntity entity = new GameEntity("att", FactionType.Neutral, EntityType.None);

            Entity physicalData = new Box(position, 40, 40, 40);
            physicalData.IsAffectedByGravity = false;
            physicalData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            entity.AddSharedData(typeof(Entity), physicalData);

            PhysicsComponent physics = new PhysicsComponent(mainGame, entity);
            entity.AddComponent(typeof(PhysicsComponent), physics);
            genComponentManager.AddComponent(physics);

            AttackController controller = new AttackController(mainGame, entity, damage, GetHitFaction(creator), creator, AttackType.Melee);
            if (twist)
            {
                controller.AddDebuff(DeBuff.Garrote);
            }

            entity.AddComponent(typeof(AttackController), controller);
            genComponentManager.AddComponent(controller);

            attacks.Add(entity);
        }

        public void CreateExecute(Vector3 position, int damage, AliveComponent creator)
        {
            position.Y = 20;
            GameEntity entity = new GameEntity("att", FactionType.Neutral, EntityType.None);

            Entity physicalData = new Box(position, 40, 40, 40);
            physicalData.IsAffectedByGravity = false;
            physicalData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            entity.AddSharedData(typeof(Entity), physicalData);

            PhysicsComponent physics = new PhysicsComponent(mainGame, entity);
            entity.AddComponent(typeof(PhysicsComponent), physics);
            genComponentManager.AddComponent(physics);

            AttackController controller = new AttackController(mainGame, entity, damage, GetHitFaction(creator), creator, AttackType.Melee);
            controller.AddDebuff(DeBuff.Execute);
            entity.AddComponent(typeof(AttackController), controller);
            genComponentManager.AddComponent(controller);

            attacks.Add(entity);
        }

        //Magic

        #endregion

        #region Enemy Abilities
        public void CreateFrostbolt(Vector3 position, Vector3 dir, int damage, AliveComponent creator)
        {
            GameEntity bolt = new GameEntity("arrow", creator.Entity.Faction, EntityType.Misc);
            position.Y = 40;
            Entity boltData = new Box(position, 10, 17, 32, .001f);
            boltData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            boltData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            boltData.LinearVelocity = dir * 400;
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

        public void CreateDragonCleave(Vector3 position, float yaw, int damage, AliveComponent creator)
        {
            position.Y = 20;
            GameEntity cleave = new GameEntity("att", FactionType.Neutral, EntityType.None);

            Entity physicalData = new Box(position, 40, 40, 100);
            physicalData.IsAffectedByGravity = false;
            physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(yaw, 0, 0);
            cleave.AddSharedData(typeof(Entity), physicalData);

            PhysicsComponent physics = new PhysicsComponent(mainGame, cleave);
            cleave.AddComponent(typeof(PhysicsComponent), physics);
            genComponentManager.AddComponent(physics);

            AttackController controller = new AttackController(mainGame, cleave, damage, GetHitFaction(creator), creator, AttackType.Melee);
            controller.HitMultipleTargets();
            cleave.AddComponent(typeof(AttackController), controller);
            genComponentManager.AddComponent(controller);

            attacks.Add(cleave);
        }

        public void CreateDragonFrostbolt(Vector3 position, Vector3 dir, int damage, AliveComponent creator)
        {
            GameEntity bolt = new GameEntity("bolt", creator.Entity.Faction, EntityType.Misc);
            position.Y = 40;
            Entity boltData = new Box(position, 32, 17, 32, .001f);
            boltData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            boltData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            boltData.LinearVelocity = dir * 450;
            boltData.Orientation = Quaternion.CreateFromRotationMatrix(CreateRotationFromForward(dir));
            bolt.AddSharedData(typeof(Entity), boltData);

            PhysicsComponent boltPhysics = new PhysicsComponent(mainGame, bolt);
            UnanimatedModelComponent boltGraphics = new UnanimatedModelComponent(mainGame, bolt, GetUnanimatedModel("Models\\Projectiles\\frost_bolt"),
                                                                                new Vector3(30), Vector3.Backward * 20, 0, 0, 0);
            boltGraphics.AddRollSpeed(.3f);
            boltGraphics.SetAlpha(.75f);
            boltGraphics.TurnOffOutline();

            boltGraphics.AddEmitter(typeof(FrostboltTrailParticleSystem), "trail", 30, 10, Vector3.Zero);
            boltGraphics.AddEmitter(typeof(FrostMistParticleSystem), "misttrail", 80, 10, Vector3.Zero);

            DragonFrostbolt boltAI = new DragonFrostbolt(mainGame, bolt, damage, creator);
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

        public void CreateDragonFirebolt(Vector3 position, Vector3 dir, int damage, AliveComponent creator)
        {
            GameEntity bolt = new GameEntity("bolt", creator.Entity.Faction, EntityType.Misc);
            position.Y = 40;
            Entity boltData = new Box(position, 32, 17, 32, .001f);
            boltData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            boltData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            boltData.LinearVelocity = dir * 500;
            boltData.Orientation = Quaternion.CreateFromRotationMatrix(CreateRotationFromForward(dir));
            bolt.AddSharedData(typeof(Entity), boltData);

            PhysicsComponent boltPhysics = new PhysicsComponent(mainGame, bolt);
            UnanimatedModelComponent boltGraphics = new UnanimatedModelComponent(mainGame, bolt, GetUnanimatedModel("Models\\Projectiles\\fireball"),
                                                                                new Vector3(20), Vector3.Backward * 20, 0, 0, 0);
            boltGraphics.AddRollSpeed(.3f);
            boltGraphics.TurnOffOutline();

            boltGraphics.AddEmitter(typeof(FireArrowParticleSystem), "trail", 75, 10, Vector3.Zero);
            boltGraphics.AddEmitter(typeof(SmokeTrailParticleSystem), "smoketrail", 30, 10, Vector3.Zero);

            DragonFirebolt boltAI = new DragonFirebolt(mainGame, bolt, damage, creator);
            boltAI.KillOnFirstContact();

            bolt.AddComponent(typeof(PhysicsComponent), boltPhysics);
            genComponentManager.AddComponent(boltPhysics);

            bolt.AddComponent(typeof(UnanimatedModelComponent), boltGraphics);
            modelManager.AddComponent(boltGraphics);

            bolt.AddComponent(typeof(AttackController), boltAI);
            genComponentManager.AddComponent(boltAI);

            attacks.Add(bolt);
        }

        public void CreateFireSpitBomb(Vector3 startPos, Vector3 targetPos, int damage, AliveComponent creator)
        {
            GameEntity bomb = new GameEntity("firespit", FactionType.Neutral, EntityType.Misc);

            Entity bombData = new Box(startPos, 15, 50, 15, 1);
            bombData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;

            Vector3 vel = targetPos - startPos;
            vel.Y = 0;
            bombData.LinearVelocity = Vector3.Up * 700 + vel * 1.05f;

            bomb.AddSharedData(typeof(Entity), bombData);

            PhysicsComponent bombPhysics = new PhysicsComponent(mainGame, bomb);
            UnanimatedModelComponent bombGraphics = new UnanimatedModelComponent(mainGame, bomb, GetUnanimatedModel("Models\\Projectiles\\fireball"), new Vector3(20), Vector3.Zero, 0, 0, 0);
            bombGraphics.AddRollSpeed(.1f);
            bombGraphics.AddEmitter(typeof(FireArrowParticleSystem), "trail", 80, 5, Vector3.Zero);
            bombGraphics.AddEmitter(typeof(SmokeTrailParticleSystem), "trailfire", 80, 0, Vector3.Zero);

            FireSpitBomb bombController = new FireSpitBomb(mainGame, bomb, targetPos, damage, creator);

            bomb.AddComponent(typeof(PhysicsComponent), bombPhysics);
            genComponentManager.AddComponent(bombPhysics);

            bomb.AddComponent(typeof(UnanimatedModelComponent), bombGraphics);
            modelManager.AddComponent(bombGraphics);

            bomb.AddComponent(typeof(FallingArrowController), bombController);
            genComponentManager.AddComponent(bombController);

            attacks.Add(bomb);
        }

        public void CreateFrostSpitBomb(Vector3 startPos, Vector3 targetPos, int damage, AliveComponent creator)
        {
            GameEntity bomb = new GameEntity("frostspit", FactionType.Neutral, EntityType.Misc);

            Entity bombData = new Box(startPos, 15, 50, 15, 1);
            bombData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;

            Vector3 vel = targetPos - startPos;
            vel.Y = 0;
            bombData.LinearVelocity = Vector3.Up * 700 + vel * 1.05f;

            bomb.AddSharedData(typeof(Entity), bombData);

            PhysicsComponent bombPhysics = new PhysicsComponent(mainGame, bomb);
            UnanimatedModelComponent bombGraphics = new UnanimatedModelComponent(mainGame, bomb, GetUnanimatedModel("Models\\Projectiles\\iceball"), new Vector3(25), Vector3.Zero, 0, MathHelper.Pi, 0);
            bombGraphics.AddRollSpeed(.1f);
            
            bombGraphics.AddEmitter(typeof(FrostboltTrailParticleSystem), "trail", 80, 5, Vector3.Zero);
            bombGraphics.AddEmitter(typeof(SmokeTrailParticleSystem), "trailfire", 35, 0, Vector3.Zero);

            FrostSpitBomb bombController = new FrostSpitBomb(mainGame, bomb, targetPos, damage, creator);

            bomb.AddComponent(typeof(PhysicsComponent), bombPhysics);
            genComponentManager.AddComponent(bombPhysics);

            bomb.AddComponent(typeof(UnanimatedModelComponent), bombGraphics);
            modelManager.AddComponent(bombGraphics);

            bomb.AddComponent(typeof(FallingArrowController), bombController);
            genComponentManager.AddComponent(bombController);

            attacks.Add(bomb);
        }

        public void CreateDragonFlamethrower(Vector3 position, AliveComponent creator, int damagePerTick, double duration)
        {
            GameEntity entity = new GameEntity("flamethrower", FactionType.Enemies, EntityType.None);

            Entity physicalData = new Box(position, 100, 40, 550);
            physicalData.IsAffectedByGravity = false;
            physicalData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            entity.AddSharedData(typeof(Entity), physicalData);

            PhysicsComponent physics = new PhysicsComponent(mainGame, entity);
            entity.AddComponent(typeof(PhysicsComponent), physics);
            genComponentManager.AddComponent(physics);

            FlamethrowerController AI = new FlamethrowerController(mainGame, entity, damagePerTick, creator, duration);
            entity.AddSharedData(typeof(FlamethrowerController), AI);
            genComponentManager.AddComponent(AI);

            attacks.Add(entity);
        }
        #endregion

        #region Misc
        public void CreateIceBlock(Vector3 position, double duration)
        {
            position.Y = 0;
            GameEntity entity = new GameEntity("", FactionType.Neutral, EntityType.None);

            Entity box = new Box(position, 1, 1, 1);
            entity.AddSharedData(typeof(Entity), box);

            UnanimatedModelComponent graphics = new UnanimatedModelComponent(mainGame, entity, GetUnanimatedModel("Models\\ice_block"), new Vector3(10), Vector3.Zero, 0, 0, 0);
            graphics.TurnOffOutline();
            graphics.AddEmitter(typeof(FrostAOEMistSystem), "frost", 10, 10, Vector3.Zero);
            entity.AddComponent(typeof(UnanimatedModelComponent), graphics);
            modelManager.AddComponent(graphics);

            DeathTimer controller = new DeathTimer(mainGame, entity, duration);
            entity.AddComponent(typeof(DeathTimer), controller);
            genComponentManager.AddComponent(controller);

            attacks.Add(entity);

            SpawnIceBlockPoof(position);
        }

        public void CreateLevelUpGraphics(Entity playerEntity)
        {
            GameEntity levelUp = new GameEntity("levelup", FactionType.Neutral, EntityType.None);

            Entity data = new Box(playerEntity.Position, 1, 1, 1);
            levelUp.AddSharedData(typeof(Entity), data);

            UnanimatedModelComponent graphics = new UnanimatedModelComponent(mainGame, levelUp, GetUnanimatedModel("Models\\sphere"), new Vector3(500), Vector3.Zero, 0, 0, 0);
            graphics.AddColorTint(Color.Gold);
            graphics.SetAlpha(.25f);
            graphics.TurnOffOutline();
            levelUp.AddComponent(typeof(UnanimatedModelComponent), graphics);
            modelManager.AddComponent(graphics);

            LevelUpBeamBillboard beam = new LevelUpBeamBillboard(mainGame, levelUp, playerEntity);
            levelUp.AddComponent(typeof(LevelUpBeamBillboard), beam);
            billboardManager.AddComponent(beam);

            LevelUpController controller = new LevelUpController(mainGame, levelUp, playerEntity);
            levelUp.AddComponent(typeof(LevelUpController), controller);
            genComponentManager.AddComponent(controller);


        }

        public void AddLevelUpCircleBillboard(GameEntity levelUp, Vector3 pos)
        {
            LevelUpBillboard circle = new LevelUpBillboard(mainGame, levelUp, pos);
            levelUp.AddComponent(typeof(LevelUpBillboard), circle);
            billboardManager.AddComponent(circle);
        }

        public void CreateFireSpitExplosion(Vector3 position, float radius, int damage, AliveComponent creator)
        {
            position.Y = 10;
            GameEntity newAttack = new GameEntity("explosion", creator.Entity.Faction, EntityType.Misc);

            Entity attackData = new Cylinder(position, 47, radius);
            attackData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            attackData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            attackData.LinearVelocity = Vector3.Zero;
            newAttack.AddSharedData(typeof(Entity), attackData);

            PhysicsComponent attackPhysics = new PhysicsComponent(mainGame, newAttack);
            AttackController attackAI = new AttackController(mainGame, newAttack, damage, GetHitFaction(creator), creator, AttackType.None);
            attackAI.HitMultipleTargets();
            

            newAttack.AddComponent(typeof(PhysicsComponent), attackPhysics);
            genComponentManager.AddComponent(attackPhysics);

            newAttack.AddComponent(typeof(AttackController), attackAI);
            genComponentManager.AddComponent(attackAI);

            attacks.Add(newAttack);

            SpawnFireSpitExplosionParticles(position);

            CreateFireHazard(position, damage / 2, creator);
        }

        public void CreateFireHazard(Vector3 position, int damagePerTick, AliveComponent creator)
        {
            position.Y = 10;
            int radius = 75;
            GameEntity hazard = new GameEntity("fire", FactionType.Neutral, EntityType.None);

            Entity hazardData = new Cylinder(position, 20, radius);
            hazardData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            hazard.AddSharedData(typeof(Entity), hazardData);

            PhysicsComponent physics = new PhysicsComponent(mainGame, hazard);
            hazard.AddComponent(typeof(PhysicsComponent), physics);
            genComponentManager.AddComponent(physics);

            UnanimatedModelComponent emitters = new UnanimatedModelComponent(mainGame, hazard);
            emitters.AddEmitter(typeof(FireAOESystem), "fire", 35, radius, 0, Vector3.Zero);
            emitters.AddEmitter(typeof(SmokeAOESystem), "smoke", 10, radius, 0, Vector3.Zero);
            hazard.AddComponent(typeof(UnanimatedModelComponent), emitters);
            modelManager.AddComponent(emitters);

            AOEController AI = new AOEController(mainGame, hazard, 500, damagePerTick, DeBuff.None, creator, 5000, GetHitFaction(creator));
            hazard.AddComponent(typeof(AOEController), AI);
            genComponentManager.AddComponent(AI);

            attacks.Add(hazard);
        }

        public void CreateFrostSpitExplosion(Vector3 position, float radius, int damage, AliveComponent creator)
        {
            position.Y = 10;
            GameEntity newAttack = new GameEntity("explosion", creator.Entity.Faction, EntityType.Misc);

            Entity attackData = new Cylinder(position, 47, radius);
            attackData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            attackData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            attackData.LinearVelocity = Vector3.Zero;
            newAttack.AddSharedData(typeof(Entity), attackData);

            PhysicsComponent attackPhysics = new PhysicsComponent(mainGame, newAttack);
            AttackController attackAI = new AttackController(mainGame, newAttack, damage, GetHitFaction(creator), creator, AttackType.None);
            attackAI.HitMultipleTargets();

            newAttack.AddComponent(typeof(PhysicsComponent), attackPhysics);
            genComponentManager.AddComponent(attackPhysics);

            newAttack.AddComponent(typeof(AttackController), attackAI);
            genComponentManager.AddComponent(attackAI);

            attacks.Add(newAttack);

            SpawnFrostSpitExplosionParticles(position);

            CreateFrostHazard(position, creator);
        }

        public void CreateFrostHazard(Vector3 position, AliveComponent creator)
        {
            position.Y = 10;
            int radius = 75;
            GameEntity hazard = new GameEntity("frost", FactionType.Neutral, EntityType.None);

            Entity hazardData = new Cylinder(position, 20, radius);
            hazardData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            hazard.AddSharedData(typeof(Entity), hazardData);

            PhysicsComponent physics = new PhysicsComponent(mainGame, hazard);
            hazard.AddComponent(typeof(PhysicsComponent), physics);
            genComponentManager.AddComponent(physics);

            UnanimatedModelComponent emitters = new UnanimatedModelComponent(mainGame, hazard);
            emitters.AddEmitter(typeof(FrostAOESystem), "frost", 35, radius, 0, Vector3.Zero);
            emitters.AddEmitter(typeof(FrostAOEMistSystem), "frostmist", 80, radius, 0, Vector3.Zero);
            hazard.AddComponent(typeof(UnanimatedModelComponent), emitters);
            modelManager.AddComponent(emitters);

            AOEController AI = new AOEController(mainGame, hazard, 250, 0, DeBuff.Frost, creator, 5000, GetHitFaction(creator));
            hazard.AddComponent(typeof(AOEController), AI);
            genComponentManager.AddComponent(AI);

            attacks.Add(hazard);
        }

        public void CreateFlashExplosion(Vector3 position, float radius, bool tar, AliveComponent creator)
        {
            if (tar)
            {
                CreateTarExplosion(position, radius, creator);
            }

            GameEntity newAttack = new GameEntity("explosion", creator.Entity.Faction, EntityType.Misc);

            Entity attackData = new Cylinder(position, 47, radius);
            attackData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            attackData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            attackData.LinearVelocity = Vector3.Zero;
            newAttack.AddSharedData(typeof(Entity), attackData);

            PhysicsComponent attackPhysics = new PhysicsComponent(mainGame, newAttack);
            AttackController attackAI = new AttackController(mainGame, newAttack, 0, GetHitFaction(creator), creator, AttackType.None);
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
            AttackController attackAI = new AttackController(mainGame, newAttack, 0, creator.Entity.Faction == FactionType.Players ? FactionType.Enemies : FactionType.Players, creator, AttackType.None);
            attackAI.HitMultipleTargets();
            attackAI.AddDebuff(DeBuff.Tar);

            newAttack.AddComponent(typeof(PhysicsComponent), attackPhysics);
            genComponentManager.AddComponent(attackPhysics);

            newAttack.AddComponent(typeof(AttackController), attackAI);
            genComponentManager.AddComponent(attackAI);

            attacks.Add(newAttack);

            SpawnTarParticles(position);
        }

        Quaternion downOrientation = Quaternion.CreateFromYawPitchRoll(0, -MathHelper.PiOver2, 0);
        public void CreateFallingArrow(Vector3 position)
        {
            GameEntity arrow = new GameEntity("arrow", FactionType.Neutral, EntityType.Misc);

            Entity arrowData = new Box(position, 15, 20, 15, 1);
            arrowData.IsAffectedByGravity = true;
            arrowData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;
            arrowData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            arrowData.LinearVelocity = Vector3.Down * 50;
            arrowData.Orientation = downOrientation;
            arrow.AddSharedData(typeof(Entity), arrowData);
            
            PhysicsComponent arrowPhysics = new PhysicsComponent(mainGame, arrow);
            UnanimatedModelComponent arrowGraphics = new UnanimatedModelComponent(mainGame, arrow, GetUnanimatedModel("Models\\Projectiles\\Arrow"), new Vector3(10), Vector3.Backward * 15, 0, 0, 0);
            arrowGraphics.AddColorTint(Color.Blue);

            FallingArrowController arrowController = new FallingArrowController(mainGame, arrow);

            position.Y = 3;
            ExpandingCircleBillboard impact = new ExpandingCircleBillboard(mainGame, arrow, position);
            arrow.AddComponent(typeof(ExpandingCircleBillboard), impact);

            arrow.AddComponent(typeof(PhysicsComponent), arrowPhysics);
            genComponentManager.AddComponent(arrowPhysics);

            arrow.AddComponent(typeof(UnanimatedModelComponent), arrowGraphics);
            modelManager.AddComponent(arrowGraphics);

            arrow.AddComponent(typeof(FallingArrowController), arrowController);
            genComponentManager.AddComponent(arrowController);

            attacks.Add(arrow);
        }

        public void CreateFireExplosionDebris(Vector3 position)
        {
            GameEntity debris = new GameEntity("debris", FactionType.Neutral, EntityType.None);

            float dir = (float)rand.Next(0, 628) / 100f;
            Entity physicalData = new Box(position, 1, 1, 1);
            physicalData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoBroadPhase;
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

        public void CreateFrostExplosionDebris(Vector3 position)
        {
            GameEntity debris = new GameEntity("debris", FactionType.Neutral, EntityType.None);

            float dir = (float)rand.Next(0, 628) / 100f;
            Entity physicalData = new Box(position, 1, 1, 1);
            physicalData.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoBroadPhase;
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
            graphics.AddEmitter(typeof(ToonFrostExplosionDebrisSystem), "trail", 20, 0, Vector3.Zero);
            graphics.AddEmitterSizeIncrement("trail", -.25f);
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

        public void SpawnIceBlockPoof(Vector3 position)
        {
            position.Y = 10;
            ParticleSystem poof = particles.GetSystem(typeof(ToonFrostExplosionPoofSystem));
            for (int i = 0; i < 15; ++i)
            {
                poof.AddParticle(position, Vector3.Zero);
            }
        }

        public void SpawnSpitSparks(Vector3 position)
        {
            ParticleSystem spit = particles.GetSystem(typeof(SpitSparks));
            for (int i = 0; i < 30; ++i)
            {
                spit.AddParticle(position, Vector3.Zero);
            }
        }

        public void SpawnWeaponSparks(Vector3 position)
        {
            ParticleSystem explosions = particles.GetSystem(typeof(WeaponSparksSystem));
            for (int i = 0; i < 10; ++i)
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

        public void SpawnFireDebuffPoof(Vector3 position)
        {
            ParticleSystem poof = particles.GetSystem(typeof(FireArrowParticleSystem));
            for (int i = 0; i < 10; ++i)
            {
                poof.AddParticle(position, Vector3.Zero);
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

        public void SpawnLevelUpParticles(Vector3 position)
        {
            ParticleSystem expl = particles.GetSystem(typeof(LevelUpExplosionSystem));
            for (int i = 0; i < 40; ++i)
            {
                expl.AddParticle(position, Vector3.Zero);
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

        public void SpawnFireExplosionParticles(Vector3 position, float intensity)
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
                CreateFireExplosionDebris(position);
            }
        }

        public void SpawnFireSpitExplosionParticles(Vector3 position)
        {
            SpawnFireExplosionParticles(position, 1.5f);
        }

        public void SpawnFrostExplosionParticles(Vector3 position, float intensity)
        {
            ParticleSystem boom = particles.GetSystem(typeof(ToonFrostExplosionMainSystem));
            for (int i = 0; i < (int)(19 * intensity + 1); ++i)
            {
                boom.AddParticle(position, Vector3.Zero);
            }

            boom = particles.GetSystem(typeof(ToonFrostExplosionPoofSystem));
            for (int i = 0; i < (int)(34 * intensity + 1); ++i)
            {
                boom.AddParticle(position, Vector3.Zero);
            }

            for (int i = 0; i < (int)(10 * intensity); ++i)
            {
                CreateFrostExplosionDebris(position);
            }
        }

        public void SpawnFrostSpitExplosionParticles(Vector3 position)
        {
            SpawnFrostExplosionParticles(position, 1.5f);
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
            for (int i = 0; i < 15; ++i)
            {
                boom.AddParticle(position, Vector3.Zero);
            }
        }
        #endregion
    }
}
