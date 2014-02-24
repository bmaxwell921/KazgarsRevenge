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

        public float attackRange;
        public float noticePlayerRange;
        public float stopChasingRange;
        
        public int attackDamage;

        public int level;
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

        Random rand;
        protected EnemyControllerSettings settings;

        CameraComponent camera;

        public EnemyController(KazgarsRevengeGame game, GameEntity entity, EnemyControllerSettings settings)
            : base(game, entity, settings.level)
        {
            this.animations = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;
            this.settings = settings;
            lewts = game.Services.GetService(typeof(LootManager)) as LootManager;

            PlayAnimation(settings.aniPrefix + "idle");

            rand = game.rand;

            attackLength = animations.GetAniMillis(settings.aniPrefix + settings.attackAniName);

            idleUpdateFunction = new AIUpdateFunction(AIWanderingHostile);
            currentUpdateFunction = idleUpdateFunction;

            attackingUpdateFunction = new AIUpdateFunction(AIAutoAttackingTarget);

            camera = game.Services.GetService(typeof(CameraComponent)) as CameraComponent;
        }


        List<int> armBoneIndices = new List<int>() { 10, 11, 12, 13, 14, 15, 16, 17 };
        List<EntityIntPair> threatLevels = new List<EntityIntPair>();
        protected override void TakeDamage(int d, GameEntity from)
        {
            if (state != EnemyState.Dying && state != EnemyState.Decaying)
            {
                PlayAnimation(settings.aniPrefix + "hit", MixType.MixOnce);
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
            attacks.SpawnBloodSpurt(physicalData.Position, physicalData.OrientationMatrix.Forward);
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

        AliveComponent targetHealth;
        Entity targetData;

        double timerCounter;
        double timerLength;
        Vector3 curVel = Vector3.Zero;

        protected delegate void AIUpdateFunction(double millis);

        //change this to adjust what the AI falls back on when there is nothing around to kill
        AIUpdateFunction idleUpdateFunction;

        AIUpdateFunction attackingUpdateFunction;

        //change this to switch states
        AIUpdateFunction currentUpdateFunction;
        public override void Update(GameTime gameTime)
        {
            if (state != EnemyState.Dying && state != EnemyState.Decaying && Dead)
            {
                state = EnemyState.Dying;
                timerLength = animations.GetAniMillis(settings.aniPrefix + "death") - 100;
                timerCounter = 0;
                PlayAnimation(settings.aniPrefix + "death");
                animations.StopMixing();
                AIDeath();
                Entity.GetComponent(typeof(PhysicsComponent)).Kill();
                return;
            }
            switch (state)
            {
                case EnemyState.Normal:
                    if (!HasDeBuff(DeBuff.Stunned) && InsideCameraBox(camera.CameraBox))
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
                    PlayAnimation(settings.aniPrefix + "walk");
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
                    newVel *= stats[StatType.RunSpeed] / 2;
                    curVel = newVel;
                    ChangeVelocity(curVel);

                    PlayAnimation(settings.aniPrefix + "walk");
                    chillin = false;
                }
            }
            else
            {
                if (physicalData.LinearVelocity.Length() < .1f)
                {
                    PlayAnimation(settings.aniPrefix + "idle");
                    chillin = true;
                }
                ChangeVelocity(curVel);
                //wandering
                if (timerCounter >= timerLength)
                {
                    //once it's wandered a little ways, sit still for a bit
                    timerCounter = 0;
                    timerLength = rand.Next(1000, 5000);
                    ChangeVelocity(Vector3.Zero);

                    PlayAnimation(settings.aniPrefix + "idle");
                    chillin = true;
                }
            }
        }

        protected double attackCreateCounter;
        protected double attackCounter = double.MaxValue;
        protected double attackLength = 1000;
        bool startingAttack = false;
        protected virtual void AIAutoAttackingTarget(double millis)
        {
            Vector3 diff = new Vector3(targetData.Position.X - physicalData.Position.X, 0, targetData.Position.Z - physicalData.Position.Z);
            if (startingAttack)
            {
                physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetGraphicsYaw(diff), 0, 0);
                attackCreateCounter += millis;
                attackCounter += millis;
                if (attackCreateCounter >= attackLength / 2)
                {
                    CreateAttack();
                    attackCreateCounter = 0;
                    startingAttack = false;
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
                        StartNormalAttack();
                        startingAttack = true;
                        attackCounter = 0;
                        attackCreateCounter = 0;
                        PlayAnimation(settings.aniPrefix + settings.attackAniName);
                    }
                    else
                    {
                        //otherwise, go to run state
                        currentUpdateFunction = new AIUpdateFunction(AIRunningToTarget);
                        attackCounter = double.MaxValue;
                    }
                }
            }
            else
            {
                targetHealth = null;
                targetData = null;
                currentUpdateFunction = idleUpdateFunction;
            }
        }

        double minChaseCounter;
        double minChaseLength = 7000;
        protected virtual void AIRunningToTarget(double millis)
        {
            minChaseCounter += millis;
            if (targetHealth != null && targetData != null && !targetHealth.Dead)
            {
                //if the target is within attack radius, go to attack state
                Vector3 diff = new Vector3(targetData.Position.X - physicalData.Position.X, 0, targetData.Position.Z - physicalData.Position.Z);

                if (Math.Abs(diff.X) < settings.attackRange && Math.Abs(diff.Z) < settings.attackRange)
                {
                    currentUpdateFunction = attackingUpdateFunction;
                    ChangeVelocity(Vector3.Zero);
                }
                else if (minChaseCounter > minChaseLength && (Math.Abs(diff.X) > settings.stopChasingRange || Math.Abs(diff.Z) > settings.stopChasingRange))
                {
                    //target out of range, wander again
                    currentUpdateFunction = idleUpdateFunction;
                    chillin = false;
                    PlayAnimation(settings.aniPrefix + "walk");
                    timerCounter = 0;
                }
                else
                {//otherwise, run towards it
                    if (diff != Vector3.Zero)
                    {
                        diff.Normalize();
                    }
                    ChangeVelocity(diff * stats[StatType.RunSpeed]);
                    physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetGraphicsYaw(diff), 0, 0);
                    if (currentAniName != settings.aniPrefix + "walk")
                    {
                        PlayAnimation(settings.aniPrefix + "walk");
                    }
                }
            }
            else
            {
                targetHealth = null;
                targetData = null;
                currentUpdateFunction = idleUpdateFunction;
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
                modelParams = Entity.GetSharedData(typeof(SharedGraphicsParams)) as SharedGraphicsParams;

                lewts.CreateLootSoul(physicalData.Position, Entity.Type);
            }
        }

        SharedGraphicsParams modelParams;
        protected void AIDecaying(double millis)
        {
            //fade out model until completely transparent, then kill the entity
            timerCounter += millis;
            if(timerCounter >= timerLength)
            {
                Entity.Kill();
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
        #endregion

        protected virtual void CreateAttack()
        {
            attacks.CreateMeleeAttack(physicalData.Position + physicalData.OrientationMatrix.Forward * 8, settings.attackDamage, false, this);
        }

        protected virtual void StartNormalAttack()
        {

        }

        protected virtual void AIDeath()
        {

        }

        public override void HandleStun()
        {
            PlayAnimation(settings.aniPrefix + "idle");
            attackCreateCounter = 0;
            startingAttack = false;
            attackCounter = attackLength;
            minChaseCounter = 0;
            physicalData.LinearVelocity = Vector3.Zero;
        }

        public override void StopPull()
        {
            attackCounter = attackLength;
            base.StopPull();
        }

        protected bool InsideCameraBox(BoundingBox cameraBox)
        {
            Vector3 pos = physicalData.Position;
            return !(pos.X < cameraBox.Min.X
                || pos.X > cameraBox.Max.X
                || pos.Z < cameraBox.Min.Z
                || pos.Z > cameraBox.Max.Z);
        }

    }
}
