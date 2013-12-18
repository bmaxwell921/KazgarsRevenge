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
    }
    public class EnemyController : AIController
    {
        enum EnemyState
        {
            Wandering,
            RunningToPlayer,
            AttackingPlayer,
            Dying,
        }
        EnemyState state = EnemyState.Wandering;

        HealthData health;
        Entity physicalData;
        AnimationPlayer animations;
        LootManager lewts;
        AttackManager attacks;

        EnemyControllerSettings settings;

        public EnemyController(KazgarsRevengeGame game, GameEntity entity, EnemyControllerSettings settings)
            : base(game, entity)
        {
            this.health = entity.GetSharedData(typeof(HealthData)) as HealthData;
            this.physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
            this.animations = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;
            this.settings = settings;
            lewts = game.Services.GetService(typeof(LootManager)) as LootManager;
            attacks = game.Services.GetService(typeof(AttackManager)) as AttackManager;

            PlayAnimation(settings.attackAniName);

            rand = new Random();
        }

        List<int> armBoneIndices = new List<int>() { 10, 11, 12, 13, 14, 15, 16, 17 };
        public override void PlayHit()
        {
            if (state != EnemyState.Dying)
            {
                animations.MixClipOnce(settings.hitAniName, armBoneIndices);
                attacks.SpawnBloodSpurt(physicalData.Position, physicalData.OrientationMatrix.Forward);
            }
        }

        string currentAniName;
        public void PlayAnimation(string animationName)
        {
            animations.StartClip(animationName);
            currentAniName = animationName;
        }

        HealthData targetHealth;
        Entity targetedPlayerData;
        double wanderCounter;
        double wanderLength;
        double deathCounter;
        double deathLength;
        double swingCounter;
        bool chillin = false;
        Random rand;
        double attackCounter = double.MaxValue;
        bool swinging = false;
        Vector3 curVel = Vector3.Zero;
        public override void Update(GameTime gameTime)
        {
            if (state != EnemyState.Dying && health.Dead)
            {
                state = EnemyState.Dying;
                deathLength = animations.GetAniMillis(settings.deathAniName);
                deathCounter = 0;
                PlayAnimation(settings.deathAniName);
                animations.StopMixing();
                return;
            }
            switch (state)
            {
                case EnemyState.Wandering:
                    GameEntity possTargetPlayer = QueryNearEntity("localplayer", physicalData.Position, 0, 100);
                    if (possTargetPlayer != null)
                    {
                        targetHealth = possTargetPlayer.GetSharedData(typeof(HealthData)) as HealthData;
                        if (targetHealth != null && !targetHealth.Dead)
                        {
                            targetedPlayerData = possTargetPlayer.GetSharedData(typeof(Entity)) as Entity;
                            state = EnemyState.RunningToPlayer;
                            animations.StartClip(settings.runAniName);
                            return;
                        }
                    }
                    wanderCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (chillin)
                    {
                        if (wanderCounter >= wanderLength)
                        {
                            wanderCounter = 0;
                            wanderLength = rand.Next(4000, 8000);
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
                        if (wanderCounter >= wanderLength)
                        {
                            //once it's wandered a little ways, sit still for a bit
                            wanderCounter = 0;
                            wanderLength = rand.Next(1000, 5000);
                            physicalData.LinearVelocity = Vector3.Zero;

                            PlayAnimation(settings.idleAniName);
                            chillin = true;
                        }
                    }
                    break;
                case EnemyState.RunningToPlayer:
                    if (targetHealth != null && targetedPlayerData != null && !targetHealth.Dead)
                    {
                        //if the player is within attack radius, go to attack state
                        Vector3 diff = new Vector3(targetedPlayerData.Position.X - physicalData.Position.X, 0, targetedPlayerData.Position.Z - physicalData.Position.Z);

                        if (Math.Abs(diff.X) < settings.attackRange && Math.Abs(diff.Z) < settings.attackRange)
                        {
                            state = EnemyState.AttackingPlayer;
                            physicalData.LinearVelocity = Vector3.Zero;
                        }
                        else if (Math.Abs(diff.X) > settings.stopChasingRange && Math.Abs(diff.Z) > settings.stopChasingRange)
                        {
                            //player out of range, wander again
                            state = EnemyState.Wandering;
                            chillin = false;
                            PlayAnimation(settings.walkAniName);
                            wanderCounter = 0;
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
                        targetedPlayerData = null;
                        state = EnemyState.Wandering;
                    }
                    break;
                case EnemyState.AttackingPlayer:
                    if (swinging)
                    {
                        swingCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
                        attackCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
                        if (swingCounter >= settings.attackLength / 2)
                        {
                            attacks.CreateMelleAttack(physicalData.Position + physicalData.OrientationMatrix.Forward * 8, settings.attackDamage, FactionType.Enemies, false);
                            swingCounter = 0;
                            swinging = false;
                        }
                    }
                    else if (targetHealth != null && targetedPlayerData != null && !targetHealth.Dead)
                    {
                        attackCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
                        if (attackCounter >= settings.attackLength)
                        {
                            attackCounter = 0;
                            //if the player is within attack radius, swing
                            Vector3 diff = new Vector3(targetedPlayerData.Position.X - physicalData.Position.X, 0, targetedPlayerData.Position.Z - physicalData.Position.Z);

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
                                state = EnemyState.RunningToPlayer;
                                attackCounter = double.MaxValue;
                            }
                        }
                    }
                    else
                    {
                        targetHealth = null;
                        targetedPlayerData = null;
                        state = EnemyState.Wandering;
                    }
                    break;
                case EnemyState.Dying:
                    deathCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (deathCounter >= deathLength)
                    {
                        entity.Kill();
                    }
                    break;
            }

        }

        public override void End()
        {
            Vector3 pos = physicalData.Position;
            pos.Y = 10;
            lewts.CreateLootSoul(pos, new List<Item>() { lewts.GenerateSword() });
        }
    }
}
