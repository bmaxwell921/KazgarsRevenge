﻿using System;
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
    class LootSoulController : AIComponent
    {
        public enum LootSoulState
        {
            Dying,
            Scared,
            Wandering,
            Following,
            BeingLooted,
        }
        public LootSoulState soulState { get; private set; }

        public int totalSouls { get; private set; }
        AnimationPlayer animations;
        private List<Item> loot;
        public List<Item> Loot
        {
            get
            {
                return loot;
            }
        }

        public LootSoulController(KazgarsRevengeGame game, GameEntity entity, int wanderSeed, List<Item> loot, int totalSouls)
            : base(game, entity)
        {
            this.loot = loot;
            this.totalSouls = totalSouls;
            soulState = LootSoulState.Wandering;

            rand = new Random();
            physicalData.CollisionInformation.Events.InitialCollisionDetected += HandleSoulCollision;
            animations = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;
            animations.StartClip("soul_wander");
        }

        const float soulSensorSize = 400;
        GameEntity targetedSoul;
        Entity targetData;
        Random rand;
        double timerCounter = 0;
        double timerLength = 0;
        float groundSpeed = 25.0f;
        float scaredSpeed = 50.0f;
        public override void Update(GameTime gameTime)
        {
            timerCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
            switch (soulState)
            {
                case LootSoulState.Wandering:
                    if (Loot.Count == 0)
                    {
                        physicalData.LinearVelocity = Vector3.Zero;
                        return;
                    }

                    //wandering
                    if (timerCounter >= timerLength)
                    {
                        timerCounter = 0;
                        timerLength = rand.Next(1000, 4000);


                        //looking for nearby souls to unite with
                        GameEntity possOtherSoul = QueryNearEntity("loot", physicalData.Position, 2, soulSensorSize);
                        if (possOtherSoul != null)
                        {
                            targetedSoul = possOtherSoul;
                            targetData = targetedSoul.GetSharedData(typeof(Entity)) as Entity;
                            soulState = LootSoulState.Following;
                            return;
                        }


                        //wander towards player
                        GameEntity possPlayer = QueryNearEntity("localplayer", physicalData.Position, 0, 10000);
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

                    AdjustDir(groundSpeed, .045f);

                    break;
                case LootSoulState.Following:
                    if (Loot.Count == 0)
                    {
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
                            AdjustDir(groundSpeed, .045f);
                        }
                    }
                    break;
                case LootSoulState.Scared:
                    newDir += (float)rand.Next(-3, 3) / 10.0f;
                    AdjustDir(scaredSpeed, .09f);

                    if (timerCounter >= timerLength)
                    {
                        soulState = LootSoulState.Wandering;
                        timerCounter = 0;

                        newDir = curDir;
                        physicalData.LinearVelocity = Vector3.Zero;
                    }
                    break;
                case LootSoulState.BeingLooted:
                    if (timerCounter >= timerLength)
                    {
                        if (currentAni == "soul_loot")
                        {
                            currentAni = "soul_loot_spin";
                            animations.StartClip(currentAni);
                            timerLength = 10000;
                        }
                        else if (currentAni == "soul_loot_smash")
                        {
                            if (loot.Count == 0)
                            {
                                Entity.Kill();
                            }
                            else
                            {
                                currentAni = "soul_wander";
                                animations.StartClip(currentAni);
                                soulState = LootSoulState.Scared;
                                timerLength = 5000;
                                physicalData.Position = new Vector3(physicalData.Position.X, 10, physicalData.Position.Z);
                            }
                        }
                        timerCounter = 0;
                    }
                    break;
            }
        }


        string currentAni = "";
        public void OpenLoot(Vector3 position, Quaternion q)
        {
            physicalData.Position = position;
            physicalData.Orientation = q;
            soulState = LootSoulState.BeingLooted;
            currentAni = "soul_loot";
            animations.StartClip(currentAni);
            timerCounter = 0;
            timerLength = animations.GetAniMillis(currentAni);
            physicalData.LinearVelocity = Vector3.Zero;
        }

        public void CloseLoot()
        {
            currentAni = "soul_loot_smash";
            animations.StartClip(currentAni);
            timerCounter = 0;
            timerLength = animations.GetAniMillis(currentAni);
        }

        public Item GetLoot(int lootIndex)
        {
            if (loot.Count - 1 < lootIndex)
            {
                //that loot doesn't exist
                return null;
            }
            else
            {
                //TODO: ask server to remove this item from this soul's loot?
                if (RequestLootFromServer(lootIndex))
                {
                    Item retLoot = loot[lootIndex];
                    loot.RemoveAt(lootIndex);
                    return retLoot;
                }
                else
                {
                    //TODO: display "already looted" message?
                    (Game as MainGame).AddAlert("that has already been looted");
                    return null;
                }
            }
        }

        public List<Item> Unite()
        {
            soulState = LootSoulState.Dying;
            Entity.Kill();
            return loot;
        }

        public bool RequestLootFromServer(int i)
        {
            return true;
        }

        public override void End()
        {
            (Game.Services.GetService(typeof(LootManager)) as LootManager).SpawnSoulPoof(physicalData.Position);
            base.End();
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
                        LootSoulController otherSoul = hitEntity.GetComponent(typeof(AIComponent)) as LootSoulController;
                        if (otherSoul != null && otherSoul.soulState != LootSoulState.Dying)
                        {
                            List<Item> toAdd = otherSoul.Unite();

                            Vector3 newPos = physicalData.Position + (hitEntity.GetSharedData(typeof(Entity)) as Entity).Position;
                            newPos /= 2;

                            LootManager manager = (Game.Services.GetService(typeof(LootManager)) as LootManager);
                            manager.CreateLootSoul(newPos, loot, toAdd, totalSouls + otherSoul.totalSouls);
                            soulState = LootSoulState.Dying;
                            Entity.Kill();
                        }
                    }
                }
            }
        }

        protected override void TakeDamage(int damage, GameEntity from)
        {
            //this can't take damage... hmm. design problems.
        }
    }
}
