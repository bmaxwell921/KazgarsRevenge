using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.Collidables;
using SkinnedModelLib;

namespace KazgarsRevenge
{
    public class EntityIntPair
    {
        public GameEntity Entity;
        public int amount;
        public EntityIntPair(GameEntity entity, int amount)
        {
            this.Entity = entity;
            this.amount = amount;
        }

        public void AddAmount(int d)
        {
            amount += d;
        }
    }
    public struct EnemyControllerSettings
    {
        public string aniPrefix;
        public string attackAniName;
        public string moveAniName;
        public string idleAniName;
        public string hitAniName;
        public string deathAniName;

        public double attackLength;
        public double attackCreateMillis;
        public float attackRange;
        public float noticePlayerRange;
        public float stopChasingRange;
        public float walkSpeed;
    }
    public class EnemyController : AliveComponent
    {
        enum EnemyState
        {
            Normal,
            Dying,
            Decaying,
        }
        EnemyState state = EnemyState.Normal;
        protected EnemyControllerSettings settings;

        CameraComponent camera;
        LevelManager levels;
        LootManager lewts;
        Space physics;

        public EnemyController(KazgarsRevengeGame game, GameEntity entity, int level)
            : base(game, entity, level)
        {
            settings.aniPrefix = "pig_";
            settings.attackAniName = "attack";
            settings.idleAniName = "idle";
            settings.moveAniName = "walk";
            settings.hitAniName = "hit";
            settings.deathAniName = "death";
            settings.attackRange = 30;
            settings.noticePlayerRange = 150;
            settings.stopChasingRange = 400;
            settings.attackLength = 1417;
            settings.attackCreateMillis = settings.attackLength / 2;
            settings.walkSpeed = GetStat(StatType.RunSpeed) / 2;


            lewts = (LootManager)game.Services.GetService(typeof(LootManager));
            levels = (LevelManager)game.Services.GetService(typeof(LevelManager));
            physics = (Space)game.Services.GetService(typeof(Space));
            idleUpdateFunction = new AIUpdateFunction(AIWanderingHostile);
            currentUpdateFunction = idleUpdateFunction;

            camera = game.Services.GetService(typeof(CameraComponent)) as CameraComponent;

            rayCastFilter = RayCastFilter;
        }


        protected string currentAniName { get; private set; }
        public void PlayAnimation(string animationName)
        {
            animations.StartClip(animationName, MixType.None);
            currentAniName = animationName;
        }

        public void PlayAnimation(string animationName, MixType mix)
        {
            animations.StartClip(animationName, mix);
            currentAniName = animationName;
        }

        protected AliveComponent targetHealth;
        protected Entity targetData;

        protected double timerCounter;
        protected double timerLength;
        Vector3 curVel = Vector3.Zero;

        protected delegate void AIUpdateFunction(double millis);
        protected AIUpdateFunction idleUpdateFunction;
        //change this to switch states
        protected AIUpdateFunction currentUpdateFunction;
        public override void Update(GameTime gameTime)
        {
            switch (state)
            {
                case EnemyState.Normal:
                    if (!activeDebuffs.ContainsKey(DeBuff.Stunned) && InsideCameraBox(camera.CameraBox))
                    {
                        currentUpdateFunction(gameTime.ElapsedGameTime.TotalMilliseconds);
                    }
                    break;
                case EnemyState.Dying:
                    AIDying(gameTime.ElapsedGameTime.TotalMilliseconds);
                    break;
                case EnemyState.Decaying:
                    AIDecaying(gameTime.ElapsedGameTime.TotalMilliseconds);
                    break;
            }

            base.Update(gameTime);
        }


        #region State Definitions

        bool chillin = false;
        protected virtual void AIWanderingHostile(double millis)
        {
            threatLevels.Clear();

            GameEntity possTargetPlayer = QueryNearEntityFaction(FactionType.Players, physicalData.Position, 0, settings.noticePlayerRange, false);
            if (possTargetPlayer != null)
            {
                targetHealth = possTargetPlayer.GetComponent(typeof(AliveComponent)) as AliveComponent;
                if (targetHealth != null && !targetHealth.Dead)
                {
                    targetData = possTargetPlayer.GetSharedData(typeof(Entity)) as Entity;
                    SwitchToAttacking();
                    PlayAnimation(settings.aniPrefix + settings.moveAniName);
                    return;
                }
            }

            AIKinematicWander(millis);
        }

        protected virtual void AIKinematicWander(double millis)
        {
            timerCounter += millis;
            if (chillin)
            {
                if (timerCounter >= timerLength)
                {
                    timerCounter = 0;
                    timerLength = rand.Next(4000, 8000);
                    float newDir = rand.Next(1, 627) / 10.0f;
                    Vector3 newVel = new Vector3((float)Math.Cos(newDir), 0, (float)Math.Sin(newDir));
                    physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetGraphicsYaw(newVel), 0, 0);
                    newVel *= settings.walkSpeed;
                    curVel = newVel;
                    ChangeVelocity(curVel);

                    PlayAnimation(settings.aniPrefix + settings.moveAniName);
                    chillin = false;
                }
            }
            else
            {
                //if it's already sitting still, play idle animation (just in case)
                if (physicalData.LinearVelocity.Length() < .1f)
                {
                    PlayAnimation(settings.aniPrefix + settings.idleAniName);
                    chillin = true;
                }
                ChangeVelocity(curVel);
                //wandering
                if (timerCounter >= timerLength)
                {
                    //once it's wandered a little ways, sit still for a bit
                    timerCounter = 0;
                    timerLength = rand.Next(5000, 8500);
                    ChangeVelocity(Vector3.Zero);

                    PlayAnimation(settings.aniPrefix + settings.idleAniName);
                    chillin = true;
                }
            }
        }

        protected double attackCreateCounter;
        protected double attackCounter = double.MaxValue;
        protected bool startedAttack = false;
        protected double attackAniLength = 0;
        protected double attackAniCounter = 0;
        protected bool endingAttack = false;
        protected bool raycastCheckTarget = true;
        protected virtual void AIAutoAttackingTarget(double millis)
        {
            attackAniCounter += millis;
            if (endingAttack && attackAniCounter >= attackAniLength)
            {
                PlayAnimation(settings.aniPrefix + settings.idleAniName);
                endingAttack = false;
            }
            Vector3 diff = new Vector3(targetData.Position.X - physicalData.Position.X, 0, targetData.Position.Z - physicalData.Position.Z);
            if (startedAttack)
            {
                physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetGraphicsYaw(diff), 0, 0);
                attackCreateCounter += millis;
                attackCounter += millis;
                if (attackCreateCounter >= settings.attackCreateMillis)
                {
                    CreateAttack();
                    attackCreateCounter = 0;
                    startedAttack = false;
                    endingAttack = true;
                }
            }
            else if (targetHealth != null && targetData != null && !targetHealth.Dead)
            {
                attackCounter += millis;
                if (attackCounter >= settings.attackLength)
                {
                    physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetGraphicsYaw(diff), 0, 0);
                    attackCounter = 0;

                    //if the player is within attack radius, begin attack
                    if (Math.Abs(diff.X) < settings.attackRange && Math.Abs(diff.Z) < settings.attackRange)
                    {
                        //when about to begin next attack, check if there is anything in between us and the target first
                        if (raycastCheckTarget && settings.attackRange > LevelManager.BLOCK_SIZE && !RayCastCheckForRoom(diff))
                        {
                            currentUpdateFunction = new AIUpdateFunction(AIRunningToTarget);
                            maxPathRequestCounter = 10000;
                            attackCounter = double.MaxValue;
                            return;
                        }
                        StartAttack();
                    }
                    else
                    {
                        //otherwise, go to run state
                        maxPathRequestCounter = 10000;
                        currentUpdateFunction = new AIUpdateFunction(AIRunningToTarget);
                        attackCounter = double.MaxValue;
                    }
                }
            }
            else
            {
                SwitchToWandering();
            }
        }

        protected bool usesPath = true;
        Vector3 currentPathPoint = Vector3.Zero;
        List<Vector3> currentPath;
        const float PATH_RADIUS_SATISFACTION = 10.0f;
        double maxPathRequestCounter;
        double maxPathRequestLength = 1500;
        double minChaseCounter;
        double minChaseLength = 7000;
        protected virtual void AIRunningToTarget(double millis)
        {
            minChaseCounter += millis;
            maxPathRequestCounter += millis;
            if (targetHealth != null && targetData != null && !targetHealth.Dead)
            {
                Vector3 diff = new Vector3(targetData.Position.X - physicalData.Position.X, 0, targetData.Position.Z - physicalData.Position.Z);
                bool nothingBetween = true;
                //if we have a longer range on our attack, raycast to check if a wall is in the way
                if (settings.attackRange > LevelManager.BLOCK_SIZE)
                {
                    nothingBetween = RayCastCheckForRoom(diff);
                }

                //if the target is within attack radius,
                //     (TODO: and nothing is between this and the target, e.g. raycast returns nothing but target?)
                //go to attack state
                if (Math.Abs(diff.X) < settings.attackRange && Math.Abs(diff.Z) < settings.attackRange && (nothingBetween || !usesPath))
                {
                    SwitchToAttacking();
                    ChangeVelocity(Vector3.Zero);
                }
                else if (minChaseCounter > minChaseLength && (Math.Abs(diff.X) > settings.stopChasingRange || Math.Abs(diff.Z) > settings.stopChasingRange))
                {
                    //target out of range, wander again
                    SwitchToWandering();
                }
                else
                {//otherwise, run towards it

                    //if we're not in the same block as the target, run to next path point. otherwise, run straight towards it
                    if (!usesPath)
                    {

                    }
                    else if (nothingBetween || Math.Abs(diff.X) > LevelManager.BLOCK_SIZE / 2 || Math.Abs(diff.Z) > LevelManager.BLOCK_SIZE / 2)
                    {
                        //if we don't have a path, request one from levels
                        if ((currentPath == null || currentPath.Count == 0))
                        {
                            GetNewPath();
                        }

                        if (currentPath != null && currentPath.Count > 0)
                        {
                            //request new path if target is far enough away from the last path's endpoint
                            Vector3 diffTargetPath = new Vector3(targetData.Position.X - currentPath[currentPath.Count - 1].X, 0, targetData.Position.Z - currentPath[currentPath.Count - 1].Z);
                            if (Math.Abs(diffTargetPath.X) > LevelManager.BLOCK_SIZE / 2 && Math.Abs(diffTargetPath.Z) > LevelManager.BLOCK_SIZE / 2)
                            {
                                GetNewPath();
                            }
                        }

                        //if we're close enough to the current targeted point, get next point in the path
                        diff = new Vector3(currentPathPoint.X - physicalData.Position.X, 0, currentPathPoint.Z - physicalData.Position.Z);
                        if (Math.Abs(diff.X) < PATH_RADIUS_SATISFACTION && Math.Abs(diff.Z) < PATH_RADIUS_SATISFACTION)
                        {
                            GetNextPathPoint();
                            diff = new Vector3(currentPathPoint.X - physicalData.Position.X, 0, currentPathPoint.Z - physicalData.Position.Z);
                        }
                    }
                    else
                    {
                        //if we're not in the same block and just running at the target, get rid of old path (so that the next point won't be backwards)
                        if (currentPath != null)
                        {
                            currentPath.Clear();
                            UpdatePathMarkers();
                        }

                    }

                    //run to the next point in the path
                    if (diff != Vector3.Zero)
                    {
                        diff.Normalize();
                    }

                    ChangeVelocity(diff * GetStat(StatType.RunSpeed));
                    physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetGraphicsYaw(diff), 0, 0);
                    if (currentAniName != settings.aniPrefix + settings.moveAniName)
                    {
                        PlayAnimation(settings.aniPrefix + settings.moveAniName);
                    }
                }
            }
            else
            {
                SwitchToWandering();
            }
        }

        protected void AIDying(double millis)
        {
            timerCounter += millis;
            if (timerCounter>= timerLength)
            {
                timerCounter = 0;
                timerLength = 3000;
                state = EnemyState.Decaying;

                //lewts.CreateLootSoul(physicalData.Position, Entity.Type);
                lewts.CreateLootSoul(physicalData.Position, this.Entity);
            }
        }

        protected void AIDecaying(double millis)
        {
            //fade out model until completely transparent, then kill the entity
            timerCounter += millis;
            if(timerCounter >= timerLength)
            {
                Entity.KillEntity();
            }

            float percent = (float)(1 - timerCounter / timerLength);
            modelParams.alpha = percent;
            modelParams.lineIntensity = percent;
        }

        private void ChangeVelocity(Vector3 vel)
        {
            if (!pulling)
            {
                physicalData.LinearVelocity = vel;
            }
        }

        protected virtual void SwitchToAttacking()
        {
            if (currentPath != null)
            {
                currentPath.Clear();
                UpdatePathMarkers();
            }
            currentUpdateFunction = new AIUpdateFunction(AIAutoAttackingTarget);
        }

        protected virtual void SwitchToWandering()
        {
            targetHealth = null;
            targetData = null;
            chillin = true;
            physicalData.LinearVelocity = Vector3.Zero;
            timerCounter = 0;
            PlayAnimation(settings.aniPrefix + settings.idleAniName);
            currentUpdateFunction = idleUpdateFunction;
        }
        #endregion

        #region attack helpers
        protected virtual void CreateAttack()
        {
            attacks.CreateMeleeAttack(physicalData.Position + physicalData.OrientationMatrix.Forward * 25, GeneratePrimaryDamage(StatType.Strength), false, this);
        }

        protected virtual void StartAttack()
        {
            attackAniCounter = 0;
            startedAttack = true;
            attackCounter = 0;
            attackCreateCounter = 0;
            PlayAnimation(settings.aniPrefix + settings.attackAniName);
        }

        protected List<string> chargingBoneNames = new List<string>();
        protected virtual void AddChargeParticles(Type particleType)
        {
            foreach (string s in chargingBoneNames)
            {
                model.AddEmitter(particleType, "charge" + s, 50, 0, Vector3.Zero, s);
                model.AddEmitterSizeIncrementExponential("charge" + s, 15, 2);
                model.AddParticleTimer("charge" + s, settings.attackCreateMillis);
            }
        }

        List<int> armBoneIndices = new List<int>() { 10, 11, 12, 13, 14, 15, 16, 17 };
        List<EntityIntPair> threatLevels = new List<EntityIntPair>();
        protected override void TakeDamage(int d, GameEntity from)
        {
            if (state != EnemyState.Dying && state != EnemyState.Decaying)
            {
                if (d > 0)
                {
                    DoDamagedGraphics();
                }
                CalculateThreat(d, from);
                minChaseCounter = 0;
            }
        }
        protected virtual void DoDamagedGraphics()
        {
            PlayAnimation(settings.aniPrefix + settings.hitAniName, MixType.MixOnce);
            animations.SetNonMixedBones(armBoneIndices);
            SpawnHitParticles();
        }
        #endregion

        #region overrides
        protected override void KillAlive()
        {
            state = EnemyState.Dying;
            timerLength = animations.GetAniMillis(settings.aniPrefix + settings.deathAniName) - 100;
            timerCounter = 0;
            PlayAnimation(settings.aniPrefix + settings.deathAniName, MixType.PauseAtEnd);
            animations.StopMixing();
            Entity.GetComponent(typeof(PhysicsComponent)).KillComponent();

            model.RemoveEmitter("chargeleft");
            model.RemoveEmitter("chargeright");

            if (debuggingPaths)
            {
                if (currentPath != null)
                {
                    currentPath.Clear();
                }
                UpdatePathMarkers();
            }

            base.KillAlive();
        }

        public override void HandleStun()
        {
            PlayAnimation(settings.aniPrefix + settings.idleAniName);
            attackCreateCounter = 0;
            startedAttack = false;
            attackCounter = settings.attackLength;
            minChaseCounter = 0;
            physicalData.LinearVelocity = Vector3.Zero;
        }

        public override void StopPull()
        {
            attackCounter = settings.attackLength;
            base.StopPull();
        }

        public override void Start()
        {
            PlayAnimation(settings.aniPrefix + settings.idleAniName);

            attackAniLength = animations.GetAniMillis(settings.aniPrefix + settings.attackAniName);

            base.Start();
        }
        #endregion

        #region helpers
        /// <summary>
        /// raycasts to see if any physics objects part of an entity named "room" are in the way
        /// </summary>
        private bool RayCastCheckForRoom(Vector3 toTarget)
        {
            bool nothingBetween = true;

            Vector3 castOrigin = physicalData.Position;
            Vector3 castDirection = toTarget;
            if (castDirection != Vector3.Zero)
            {
                castDirection.Normalize();
            }
            Ray r = new Ray(castOrigin, castDirection);
            List<RayCastResult> results = new List<RayCastResult>();
            physics.RayCast(r, toTarget.Length(), rayCastFilter, results);

            foreach (RayCastResult result in results)
            {
                if (result.HitObject != null)
                {
                    GameEntity ent = result.HitObject.Tag as GameEntity;
                    if (ent != null && ent.Name == "room")
                    {
                        nothingBetween = false;
                        break;
                    }
                }
            }

            return nothingBetween;
        }

        Func<BroadPhaseEntry, bool> rayCastFilter;
        bool RayCastFilter(BroadPhaseEntry entry)
        {
            return entry != physicalData.CollisionInformation
                && entry.CollisionRules.Personal <= CollisionRule.Normal;
        }

        /// <summary>
        /// calculate threat and switch to attacker if it is now the highest
        /// </summary>
        protected void CalculateThreat(int d, GameEntity from)
        {

            //find entity
            int i;
            for (i = 0; i < threatLevels.Count; ++i)
            {
                if (threatLevels[i].Entity == from)
                {
                    break;
                }
            }
            //entity needs to be added to threat list
            if (i == threatLevels.Count)
            {
                threatLevels.Add(new EntityIntPair(from, d));
            }

            //add threat
            threatLevels[i].AddAmount(d);

            //if bigger than next in list, take out and reinsert
            if (i > 0 && threatLevels[i - 1].amount > threatLevels[i].amount)
            {
                EntityIntPair tempEnt = threatLevels[i];
                threatLevels.RemoveAt(i);
                InsertThreatEntity(tempEnt);
            }

            targetData = threatLevels[0].Entity.GetSharedData(typeof(Entity)) as Entity;
            targetHealth = threatLevels[0].Entity.GetComponent(typeof(AliveComponent)) as AliveComponent;
            SwitchToAttacking();
        }

        private void InsertThreatEntity(EntityIntPair ent)
        {
            for (int i = 0; i < threatLevels.Count; ++i)
            {
                if (threatLevels[i].amount < ent.amount)
                {
                    threatLevels.Insert(i, ent);
                    return;
                }
            }
            threatLevels.Add(ent);
        }
        protected virtual void SpawnHitParticles()
        {
            attacks.SpawnHitBlood(physicalData.Position);
        }

        protected bool InsideCameraBox(BoundingBox cameraBox)
        {
            Vector3 pos = physicalData.Position;
            return !(pos.X < cameraBox.Min.X
                || pos.X > cameraBox.Max.X
                || pos.Z < cameraBox.Min.Z
                || pos.Z > cameraBox.Max.Z);
        }

        bool debuggingPaths = false;
        List<GameEntity> currentPathMarkers = new List<GameEntity>();
        protected void GetNewPath()
        {
            if (maxPathRequestCounter >= maxPathRequestLength)
            {
                Vector3 src = physicalData.Position;
                src.Y = 0;
                Vector3 dest = targetData.Position;
                dest.Y = 0;
                currentPath = levels.GetPath(src, dest) as List<Vector3>;
                if (debuggingPaths)
                {
                    UpdatePathMarkers();
                }

                maxPathRequestCounter = 0;
                GetNextPathPoint();
            }
        }

        private void UpdatePathMarkers()
        {
            for (int i = 0; i < currentPathMarkers.Count; ++i)
            {
                currentPathMarkers[i].KillEntity();
            }

            currentPathMarkers.Clear();
            if (currentPath != null)
            {
                for (int i = 0; i < currentPath.Count; ++i)
                {
                    currentPathMarkers.Add(levels.AddPathMarker(currentPath[i], Entity.Name));
                }
            }
        }

        protected void GetNextPathPoint()
        {
            if (currentPath == null || currentPath.Count == 0)
            {
                //if no path, run straight at the target
                currentPathPoint = targetData.Position;
            }
            else
            {
                currentPathPoint = currentPath[0];
                currentPath.RemoveAt(0);
            }
        }

        #endregion

    }
}
