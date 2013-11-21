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
        private List<Item> loot;
        public List<Item> Loot
        {
            get
            {
                return soulState == LootSoulState.Dying ? new List<Item>() : loot;
            }
        }

        public LootSoulController(KazgarsRevengeGame game, GameEntity entity, int wanderSeed, List<Item> loot, int totalSouls)
            : base(game, entity)
        {
            this.loot = loot;
            this.totalSouls = totalSouls;
            soulState = LootSoulState.Wandering;

            rand = new Random();//new Random(wanderSeed);
            physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
            physicalData.CollisionInformation.Events.InitialCollisionDetected += HandleSoulCollision;
            animations = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;
            animations.StartClip("pig_walk");
            

            deathLength = 70;//animations.skinningDataValue.AnimationClips["pig_attack"].Duration.TotalMilliseconds;
        }

        const int soulSensorSize = 400;
        private BoundingBox GetSoulSensor()
        {
            Vector3 min = new Vector3(physicalData.Position.X - soulSensorSize, 0, physicalData.Position.Z - soulSensorSize);
            Vector3 max = new Vector3(physicalData.Position.X + soulSensorSize, 20, physicalData.Position.Z + soulSensorSize);
            BoundingBox b = new BoundingBox(min, max);
            return b;
        }
        GameEntity targetedSoul;
        Entity targetData;
        Random rand;
        double wanderCounter = 0;
        double wanderLength = 0;
        float groundSpeed = 5.0f;
        float followSpeed = 15.0f;
        double deathCounter = 0;
        double deathLength;
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
                        wanderLength = rand.Next(4000, 10000);
                        float newDir = rand.Next(1, 627) / 10.0f;
                        Vector3 newVel = new Vector3((float)Math.Cos(newDir), 0, (float)Math.Sin(newDir));
                        physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetYaw(newVel), 0, 0);
                        newVel *= groundSpeed;
                        physicalData.LinearVelocity = newVel;
                    }

                    //looking for nearby souls to unite with
                    GameEntity possNearestSoul = QueryNearest("loot", GetSoulSensor());
                    if (possNearestSoul != null && (possNearestSoul.GetComponent(typeof(LootSoulController)) as LootSoulController).soulState != LootSoulState.Dying)
                    {
                        targetedSoul = possNearestSoul;
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
                            if (move != Vector3.Zero)
                            {
                                move.Normalize();
                                move.Y = 0;
                            }
                            if (move.Z == 0)
                            {
                                move.Z = .000001f;
                            }
                            move *= followSpeed;
                            physicalData.LinearVelocity = move;
                            physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetYaw(move), 0, 0);
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

        public void OpenLoot()
        {
            soulState = LootSoulState.BeingLooted;
            animations.StartClip("pig_idle");
        }

        public void CloseLoot()
        {
            soulState = LootSoulState.Wandering;
            animations.StartClip("pig_walk");
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
