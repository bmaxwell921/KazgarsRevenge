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

        public string idleAniName;
        public string attackAniName;
        public string walkAniName;
        public string runAniName;
        public string hitAniName;
        public string deathAniName;

        public float walkSpeed;
        public float runSpeed;

        public float attackRange;
        public float noticePlayerRange;
        public float stopChasingRange;
        
        public double attackLength;
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
        AttackManager attacks;

        Random rand;
        EnemyControllerSettings settings;

        public EnemyController(KazgarsRevengeGame game, GameEntity entity, EnemyControllerSettings settings)
            : base(game, entity, settings.level)
        {
            this.animations = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;
            this.settings = settings;
            lewts = game.Services.GetService(typeof(LootManager)) as LootManager;
            attacks = game.Services.GetService(typeof(AttackManager)) as AttackManager;

            PlayAnimation(settings.attackAniName);

            rand = new Random();

            idleUpdateFunction = new AIUpdateFunction(AIWanderingHostile);
            currentUpdateFunction = idleUpdateFunction;

            attackingUpdateFunction = new AIUpdateFunction(AIAutoAttackingTarget);
        }

        List<int> armBoneIndices = new List<int>() { 10, 11, 12, 13, 14, 15, 16, 17 };
        List<EntityIntPair> threatLevels = new List<EntityIntPair>();
        protected override void TakeDamage(int d, GameEntity from)
        {
            if (state != EnemyState.Dying && state != EnemyState.Decaying)
            {
                animations.MixClipOnce(settings.hitAniName, armBoneIndices);
                attacks.SpawnBloodSpurt(physicalData.Position, physicalData.OrientationMatrix.Forward);
                CalculateThreat(d, from);
                minChaseCounter = 0;
            }
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
            animations.StartClip(animationName);
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
                timerLength = animations.GetAniMillis(settings.deathAniName) - 100;
                timerCounter = 0;
                PlayAnimation(settings.deathAniName);
                animations.StopMixing();
                AIDeath();
                Entity.GetComponent(typeof(PhysicsComponent)).Kill();
                return;
            }
            switch (state)
            {
                case EnemyState.Normal:
                    currentUpdateFunction(gameTime.ElapsedGameTime.TotalMilliseconds);
                    break;
                case EnemyState.Dying:
                    AIDying(gameTime.ElapsedGameTime.TotalMilliseconds);
                    break;
                case EnemyState.Decaying:
                    AIDecaying(gameTime.ElapsedGameTime.TotalMilliseconds);
                    break;
            }

        }


        #region State Definitions

        bool chillin = false;
        protected virtual void AIWanderingHostile(double millis)
        {
            threatLevels.Clear();

            GameEntity possTargetPlayer = QueryNearEntity("localplayer", physicalData.Position, 0, 100);
            if (possTargetPlayer != null)
            {
                targetHealth = possTargetPlayer.GetComponent(typeof(AliveComponent)) as AliveComponent;
                if (targetHealth != null && !targetHealth.Dead)
                {
                    targetData = possTargetPlayer.GetSharedData(typeof(Entity)) as Entity;
                    currentUpdateFunction = new AIUpdateFunction(AIAutoAttackingTarget);
                    animations.StartClip(settings.runAniName);
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
                    physicalData.LinearVelocity = curVel;

                    PlayAnimation(settings.walkAniName);
                    chillin = false;
                }
            }
            else
            {
                if (physicalData.LinearVelocity.Length() < .1f)
                {
                    PlayAnimation(settings.idleAniName);
                    chillin = true;
                }
                physicalData.LinearVelocity = curVel;
                //wandering
                if (timerCounter >= timerLength)
                {
                    //once it's wandered a little ways, sit still for a bit
                    timerCounter = 0;
                    timerLength = rand.Next(1000, 5000);
                    physicalData.LinearVelocity = Vector3.Zero;

                    PlayAnimation(settings.idleAniName);
                    chillin = true;
                }
            }
        }

        double swingCounter;
        double attackCounter = double.MaxValue;
        bool swinging = false;
        protected virtual void AIAutoAttackingTarget(double millis)
        {
            if (swinging)
            {
                swingCounter += millis;
                attackCounter += millis;
                if (swingCounter >= settings.attackLength / 2)
                {
                    attacks.CreateMelleAttack(physicalData.Position + physicalData.OrientationMatrix.Forward * 8, settings.attackDamage, FactionType.Enemies, false, this);
                    swingCounter = 0;
                    swinging = false;
                }
            }
            else if (targetHealth != null && targetData != null && !targetHealth.Dead)
            {
                attackCounter += millis;
                if (attackCounter >= settings.attackLength)
                {
                    attackCounter = 0;
                    //if the player is within attack radius, swing
                    Vector3 diff = new Vector3(targetData.Position.X - physicalData.Position.X, 0, targetData.Position.Z - physicalData.Position.Z);

                    if (Math.Abs(diff.X) < settings.attackRange && Math.Abs(diff.Z) < settings.attackRange)
                    {
                        attackCounter = 0;
                        PlayAnimation(settings.attackAniName);
                        swinging = true;
                        swingCounter = 0;
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
                    currentUpdateFunction = new AIUpdateFunction(AIAutoAttackingTarget);
                    physicalData.LinearVelocity = Vector3.Zero;
                }
                else if (minChaseCounter > minChaseLength && (Math.Abs(diff.X) > settings.stopChasingRange || Math.Abs(diff.Z) > settings.stopChasingRange))
                {
                    //target out of range, wander again
                    currentUpdateFunction = idleUpdateFunction;
                    chillin = false;
                    PlayAnimation(settings.walkAniName);
                    timerCounter = 0;
                }
                else
                {//otherwise, run towards it
                    if (diff != Vector3.Zero)
                    {
                        diff.Normalize();
                    }
                    physicalData.LinearVelocity = diff * settings.runSpeed;
                    physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetGraphicsYaw(diff), 0, 0);
                    if (currentAniName != settings.runAniName)
                    {
                        PlayAnimation(settings.runAniName);
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
                modelParams = Entity.GetSharedData(typeof(SharedEffectParams)) as SharedEffectParams;

                lewts.CreateLootSoul(physicalData.Position, Entity.Type);
            }
        }

        SharedEffectParams modelParams;
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

        #endregion


        protected virtual void AIDeath()
        {

        }
    }
}
