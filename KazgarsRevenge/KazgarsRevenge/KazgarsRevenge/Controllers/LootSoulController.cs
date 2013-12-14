using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.CollisionTests;
using BEPUphysics.Collidables;
using BEPUphysics.Collidables.MobileCollidables;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using SkinnedModelLib;

namespace KazgarsRevenge
{
    class LootSoulController : AIController
    {
        public enum LootSoulState
        {
            Wandering,
            Following,
            BeingLooted,
            Dying,
        }
        public LootSoulState soulState { get; private set; }

        public int totalSouls { get; private set; }
        Entity physicalData;
        AnimationPlayer animations;
        ParticleEmitter trailEmitter;
        private List<Item> loot;
        public List<Item> Loot
        {
            get
            {
                return soulState == LootSoulState.Dying ? new List<Item>() : loot;
            }
        }

        public LootSoulController(KazgarsRevengeGame game, GameEntity entity, int wanderSeed, List<Item> loot, int totalSouls, ParticleEmitter trailEmitter)
            : base(game, entity)
        {
            this.loot = loot;
            this.totalSouls = totalSouls;
            this.trailEmitter = trailEmitter;
            soulState = LootSoulState.Wandering;

            rand = new Random();//new Random(wanderSeed);
            physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
            physicalData.CollisionInformation.Events.InitialCollisionDetected += HandleSoulCollision;
            animations = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;
            animations.StartClip("soul_wander");
            

            deathLength = 70;//animations.skinningDataValue.AnimationClips["pig_attack"].Duration.TotalMilliseconds;
        }

        const float soulSensorSize = 400;
        GameEntity targetedSoul;
        Entity targetData;
        Random rand;
        double wanderCounter = 0;
        double wanderLength = 0;
        float groundSpeed = 15.0f;
        double deathCounter = 0;
        double deathLength;
        float newDir;
        float curDir;
        public override void Update(GameTime gameTime)
        {
            this.trailEmitter.Update(gameTime, physicalData.Position + Vector3.Up * 5);
            switch (soulState)
            {
                case LootSoulState.Wandering:
                    if (Loot.Count == 0)
                    {
                        soulState = LootSoulState.Dying;
                        physicalData.LinearVelocity = Vector3.Zero;
                        return;
                    }

                    //wandering
                    wanderCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (wanderCounter >= wanderLength)
                    {
                        wanderCounter = 0;
                        wanderLength = rand.Next(2000, 6000);

                        //wander towards player
                        GameEntity possPlayer = QueryNearest("localplayer", physicalData.Position, 10000);
                        if (possPlayer != null)
                        {
                            Vector3 dir = (possPlayer.GetSharedData(typeof(Entity)) as Entity).Position;
                            dir = dir - physicalData.Position;
                            dir.Y = 0;
                            newDir = GetPhysicsYaw(dir);
                            newDir += (rand.Next(60) - 30) / 100;
                        }
                        else
                        {
                            newDir = rand.Next(1, 627) / 100.0f;
                        }
                    }

                    CalcDir();

                    //looking for nearby souls to unite with
                    GameEntity possOtherSoul = QueryNearest("loot", physicalData.Position, soulSensorSize);
                    if (possOtherSoul != null)
                    {
                        targetedSoul = possOtherSoul;
                        targetData = targetedSoul.GetSharedData(typeof(Entity)) as Entity;
                        soulState = LootSoulState.Following;
                    }
                    break;
                case LootSoulState.Following:
                    if (Loot.Count == 0)
                    {
                        soulState = LootSoulState.Dying;
                        physicalData.LinearVelocity = Vector3.Zero;
                        return;
                    }
                    if (targetedSoul != null)
                    {
                        if (targetedSoul.Dead)
                        {
                            targetedSoul = null;
                            targetData = null;
                            soulState = LootSoulState.Wandering;
                        }
                        else
                        {
                            Vector3 move = targetData.Position - physicalData.Position;
                            move.Y = 0;
                            newDir = GetPhysicsYaw(move);
                            CalcDir();
                        }
                    }
                    break;
                case LootSoulState.BeingLooted:
                    break;
                case LootSoulState.Dying:
                    deathCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (deathCounter > deathLength)
                    {
                        entity.Kill();
                    }
                    break;
            }
        }

        private void CalcDir()
        {
            if (Math.Abs(curDir - newDir) > .06f)
            {
                float add = .05f;
                float diff = curDir - newDir;
                if (diff > 0 && diff < MathHelper.Pi || diff < 0 && -diff > MathHelper.Pi)
                {
                    add *= -1;
                }
                curDir += add;
                if (curDir > MathHelper.TwoPi)
                {
                    curDir -= MathHelper.TwoPi;
                }
                else if (curDir < 0)
                {
                    curDir += MathHelper.TwoPi;
                }
                Vector3 newVel = new Vector3((float)Math.Cos(curDir), 0, (float)Math.Sin(curDir));
                physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetGraphicsYaw(newVel), 0, 0);
                newVel *= groundSpeed;
                physicalData.LinearVelocity = newVel;
            }
        }

        public void OpenLoot()
        {
            soulState = LootSoulState.BeingLooted;
            animations.StartClip("soul_wander");
        }

        public void CloseLoot()
        {
            soulState = LootSoulState.Wandering;
            animations.StartClip("soul_wander");
        }

        protected void HandleSoulCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            if (soulState != LootSoulState.Dying)
            {
                GameEntity hitEntity = other.Tag as GameEntity;
                if (hitEntity != null)
                {
                    //if we hit another soul, merge with it
                    if (hitEntity.Name == "loot")
                    {
                        LootSoulController otherSoul = hitEntity.GetComponent(typeof(LootSoulController)) as LootSoulController;
                        if (otherSoul != null)
                        {
                            if (otherSoul.soulState != LootSoulState.Dying)
                            {
                                List<Item> toAdd = otherSoul.Unite();
                                for (int i = 0; i < toAdd.Count; ++i)
                                {
                                    loot.Add(toAdd[i]);
                                }
                                Vector3 newPos = physicalData.Position + (hitEntity.GetSharedData(typeof(Entity)) as Entity).Position;
                                newPos /= 2;

                                soulState = LootSoulState.Dying;
                                physicalData.LinearVelocity = Vector3.Zero;
                                (Game.Services.GetService(typeof(LootManager)) as LootManager).CreateLootSoul(newPos, loot, totalSouls + otherSoul.totalSouls);
                            }
                        }
                    }
                }
            }
        }

        public List<Item> Unite()
        {
            soulState = LootSoulState.Dying;
            physicalData.LinearVelocity = Vector3.Zero;
            return Loot;
        }
    }
}
