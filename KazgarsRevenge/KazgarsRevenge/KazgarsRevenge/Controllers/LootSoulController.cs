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
    class LootSoulController : AIComponent
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
            

            deathLength = 70;//animations.skinningDataValue.AnimationClips["pig_attack"].Duration.TotalMilliseconds;
        }

        const float soulSensorSize = 400;
        GameEntity targetedSoul;
        Entity targetData;
        Random rand;
        double wanderCounter = 0;
        double wanderLength = 0;
        double deathCounter = 0;
        double deathLength;
        protected float groundSpeed = 25.0f;
        public override void Update(GameTime gameTime)
        {
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
                        wanderLength = rand.Next(1000, 4000);


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
                            AdjustDir(groundSpeed, .045f);
                        }
                    }
                    break;
                case LootSoulState.BeingLooted:
                    break;
                case LootSoulState.Dying:
                    deathCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (deathCounter > deathLength)
                    {
                        Entity.Kill();
                    }
                    break;
            }
        }



        public void OpenLoot()
        {
            soulState = LootSoulState.BeingLooted;
            animations.StartClip("soul_wander");
            physicalData.LinearVelocity = Vector3.Zero;
        }

        public void CloseLoot()
        {
            if (soulState != LootSoulState.Dying)
            {
                soulState = LootSoulState.Wandering;
                animations.StartClip("soul_wander");
            }
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
                        if (otherSoul != null)
                        {
                            if (otherSoul.soulState != LootSoulState.Dying)
                            {
                                List<Item> toAdd = otherSoul.Unite();
                                
                                Vector3 newPos = physicalData.Position + (hitEntity.GetSharedData(typeof(Entity)) as Entity).Position;
                                newPos /= 2;

                                soulState = LootSoulState.Dying;
                                physicalData.LinearVelocity = Vector3.Zero;

                                LootManager manager = (Game.Services.GetService(typeof(LootManager)) as LootManager);
                                manager.CreateLootSoul(newPos, loot, toAdd, totalSouls + otherSoul.totalSouls);
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

        protected override void TakeDamage(int damage, GameEntity from)
        {
            //this can't take damage... hmm. architecture design problems.
        }
    }
}
