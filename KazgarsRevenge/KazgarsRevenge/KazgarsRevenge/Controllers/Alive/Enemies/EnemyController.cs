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

        AnimationPlayer animations;
        LootManager lewts;

        protected EnemyControllerSettings settings;

        CameraComponent camera;
        LevelManager levels;

        public EnemyController(KazgarsRevengeGame game, GameEntity entity, int level)
            : base(game, entity, level)
        {
            this.animations = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;
            settings.aniPrefix = "pig_";
            settings.attackAniName = "attack";
            settings.idleAniName = "idle";
            settings.moveAniName = "walk";
            settings.hitAniName = "hit";
            settings.deathAniName = "death";
            settings.attackRange = 30;
            settings.noticePlayerRange = 150;
            settings.stopChasingRange = 400;
            settings.walkSpeed = GetStat(StatType.RunSpeed) / 2;


            lewts = (LootManager)game.Services.GetService(typeof(LootManager));
            levels = (LevelManager)game.Services.GetService(typeof(LevelManager));
            idleUpdateFunction = new AIUpdateFunction(AIWanderingHostile);
            currentUpdateFunction = idleUpdateFunction;

            attackingUpdateFunction = new AIUpdateFunction(AIAutoAttackingTarget);
            camera = game.Services.GetService(typeof(CameraComponent)) as CameraComponent;
        }

        public override void Start()
        {
            PlayAnimation(settings.aniPrefix + settings.idleAniName);
            attackLength = animations.GetAniMillis(settings.aniPrefix + settings.attackAniName);
            attackCheckLength = attackLength / 10;

            base.Start();
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

            currentUpdateFunction = attackingUpdateFunction;
            targetData = threatLevels[0].Entity.GetSharedData(typeof(Entity)) as Entity;
            targetHealth = threatLevels[0].Entity.GetComponent(typeof(AliveComponent)) as AliveComponent;
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

        string currentAniName;
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

        //change this to adjust what the AI falls back on when there is nothing around to kill
        protected AIUpdateFunction idleUpdateFunction;

        protected AIUpdateFunction attackingUpdateFunction;

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
                    currentUpdateFunction = attackingUpdateFunction;
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

        protected double attackCheckCounter;
        protected double attackCheckLength;
        protected int numChecks = 0;

        protected double attackCreateCounter;
        protected double attackCounter = double.MaxValue;
        protected double attackLength = 1000;
        protected bool startedAttack = false;
        protected virtual void AIAutoAttackingTarget(double millis)
        {
            Vector3 diff = new Vector3(targetData.Position.X - physicalData.Position.X, 0, targetData.Position.Z - physicalData.Position.Z);
            if (startedAttack)
            {
                physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetGraphicsYaw(diff), 0, 0);
                attackCreateCounter += millis;
                attackCounter += millis;
                attackCheckCounter += millis;
                if (attackCheckCounter >= attackCheckLength)
                {
                    DuringAttack(numChecks);
                    ++numChecks;
                    attackCheckCounter = 0;
                }
                if (attackCreateCounter >= attackLength / 2)
                {
                    CreateAttack();
                    attackCreateCounter = 0;
                    startedAttack = false;
                }
            }
            else if (targetHealth != null && targetData != null && !targetHealth.Dead)
            {
                attackCounter += millis;
                if (attackCounter >= attackLength)
                {
                    physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetGraphicsYaw(diff), 0, 0);
                    attackCounter = 0;
                    //if the player is within attack radius, swing
                    if (Math.Abs(diff.X) < settings.attackRange && Math.Abs(diff.Z) < settings.attackRange)
                    {
                        startedAttack = true;
                        attackCounter = 0;
                        attackCreateCounter = 0;
                        numChecks = 0;
                        attackCheckCounter = double.MaxValue;
                        PlayAnimation(settings.aniPrefix + settings.attackAniName);
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

        Vector3 currentPathPoint = Vector3.Zero;
        List<Vector3> currentPath;
        const float PATH_RADIUS_SATISFACTION = 10.0f;
        double maxPathRequestCounter;
        double maxPathRequestLength = 4000;
        double minChaseCounter;
        double minChaseLength = 7000;
        protected virtual void AIRunningToTarget(double millis)
        {
            minChaseCounter += millis;
            maxPathRequestCounter += millis;
            if (targetHealth != null && targetData != null && !targetHealth.Dead)
            {
                Vector3 diff = new Vector3(targetData.Position.X - physicalData.Position.X, 0, targetData.Position.Z - physicalData.Position.Z);

                //if the target is within attack radius,
                //     (TODO: and nothing is between this and the target, e.g. raycast returns nothing but target?)
                //go to attack state
                bool nothingBetween = true;
                if (Math.Abs(diff.X) < settings.attackRange && Math.Abs(diff.Z) < settings.attackRange && nothingBetween)
                {
                    currentUpdateFunction = attackingUpdateFunction;
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
                    if (nothingBetween || Math.Abs(diff.X) > LevelManager.BLOCK_SIZE || Math.Abs(diff.Z) > LevelManager.BLOCK_SIZE)
                    {
                        //if we don't have a path, request one from levels
                        if ((currentPath == null || currentPath.Count == 0) && currentPathPoint == Vector3.Zero)
                        {
                            GetNewPath();
                        }

                        if (currentPath != null && currentPath.Count > 0)
                        {
                            //request new path if target is a block or more away from the last path
                            Vector3 diffTargetPath = new Vector3(targetData.Position.X - currentPath[currentPath.Count - 1].X, 0, targetData.Position.Z - currentPath[currentPath.Count - 1].Z);
                            if (Math.Abs(diffTargetPath.X) > LevelManager.BLOCK_SIZE && Math.Abs(diffTargetPath.Z) > LevelManager.BLOCK_SIZE)
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
                animations.PauseAnimation();

                lewts.CreateLootSoul(physicalData.Position, Entity.Type);
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

        protected void SwitchToWandering()
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

        protected virtual void CreateAttack()
        {
            attacks.CreateMeleeAttack(physicalData.Position + physicalData.OrientationMatrix.Forward * 25, GeneratePrimaryDamage(StatType.Strength), false, this);
        }

        protected virtual void DuringAttack(int i)
        {

        }

        protected override void KillAlive()
        {
            state = EnemyState.Dying;
            timerLength = animations.GetAniMillis(settings.aniPrefix + settings.deathAniName) - 100;
            timerCounter = 0;
            PlayAnimation(settings.aniPrefix + settings.deathAniName);
            animations.StopMixing();
            Entity.GetComponent(typeof(PhysicsComponent)).KillComponent();
            
            base.KillAlive();
        }

        public override void HandleStun()
        {
            PlayAnimation(settings.aniPrefix + settings.idleAniName);
            attackCreateCounter = 0;
            startedAttack = false;
            attackCounter = attackLength;
            minChaseCounter = 0;
            physicalData.LinearVelocity = Vector3.Zero;
        }

        public override void StopPull()
        {
            attackCounter = attackLength;
            base.StopPull();
        }

        List<int> armBoneIndices = new List<int>() { 10, 11, 12, 13, 14, 15, 16, 17 };
        List<EntityIntPair> threatLevels = new List<EntityIntPair>();
        protected override void TakeDamage(int d, GameEntity from)
        {
            if (state != EnemyState.Dying && state != EnemyState.Decaying)
            {
                PlayAnimation(settings.aniPrefix + settings.hitAniName, MixType.MixOnce);
                animations.SetNonMixedBones(armBoneIndices);
                if (d > 0)
                {
                    SpawnHitParticles();
                }
                CalculateThreat(d, from);
                minChaseCounter = 0;
            }
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

        protected void GetNewPath()
        {
            if (maxPathRequestCounter >= maxPathRequestLength)
            {
                Vector3 src = physicalData.Position;
                src.Y = 0;
                Vector3 dest = targetData.Position;
                dest.Y = 0;
                currentPath = levels.GetPath(src, dest) as List<Vector3>;
                maxPathRequestCounter = 0;
                GetNextPathPoint();
            }
        }

        protected void GetNextPathPoint()
        {
            if (currentPath == null || currentPath.Count == 0)
            {
                currentPathPoint = Vector3.Zero;
            }
            else
            {
                currentPathPoint = currentPath[0];
                currentPath.RemoveAt(0);
            }
        }

    }
}
