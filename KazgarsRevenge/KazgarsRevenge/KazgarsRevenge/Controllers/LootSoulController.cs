using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.CollisionTests;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.Collidables;
using BEPUphysics.Collidables.MobileCollidables;
using SkinnedModelLib;

namespace KazgarsRevenge
{
    class LootSoulController : AIController
    {
        Entity physicalData;
        AnimationPlayer animations;
        public Dictionary<int, Item> Loot;
        public LootSoulController(KazgarsRevengeGame game, GameEntity entity, int wanderSeed, List<Item> loot)
            : base(game, entity)
        {
            this.Loot = new Dictionary<int, Item>();
            for(int i=0; i<loot.Count; ++i)
            {
                this.Loot.Add(i, loot[i]);
            }

            rand = new Random(wanderSeed);
            physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
            animations = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;
            animations.StartClip("pig_walk");

            //creating a box on top of this one to find any other loot souls around it to join with
            sensingData = new Box(physicalData.Position, 50, 20, 50);
            sensingData.CollisionInformation.CollisionRules.Group = game.LootCollisionGroup;
        }

        GameEntity targettedSoul;
        Entity targetData;
        Entity sensingData;
        Random rand;
        double wanderCounter = 0;
        double wanderLength = 0;
        float groundSpeed = 5.0f;
        public override void Update(GameTime gameTime)
        {
            //locking sensor onto it's position
            sensingData.Position = physicalData.Position;

            if (targettedSoul != null)
            {
                if (targettedSoul.Dead)
                {
                    targettedSoul = null;
                    targetData = null;
                }
                else
                {
                    Vector3 move = targetData.Position - physicalData.Position;
                    move.Normalize();
                    move.Y = 0;
                    move *= groundSpeed;
                    physicalData.LinearVelocity = move;


                    physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetYaw(move), 0, 0);
                }
            }
            else
            {
                //look for contacts
                foreach (var c in physicalData.CollisionInformation.Pairs)
                {
                    if (PairIsColliding(c))
                    {
                        //getting other entity
                        Entity e;
                        if (c.EntityA == physicalData)
                        {
                            e = c.EntityB;
                        }
                        else
                        {
                            e = c.EntityA;
                        }

                        if (e != null)
                        {
                            //target the first other soul found
                            GameEntity other = e.CollisionInformation.Tag as GameEntity;
                            if (other != null && other.Name == "loot")
                            {

                            }
                        }
                    }
                }
            }

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

            if (Loot.Count == 0)
            {
                this.Kill();
            }
        }

        protected void HandleSoulCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            GameEntity hitEntity = other.Tag as GameEntity;
            if (hitEntity != null)
            {
                //if we hit another soul, merge with it
                if (hitEntity.Name == "loot")
                {

                }
            }
        }

        private bool PairIsColliding(CollidablePairHandler pair)
        {
            foreach (var contactInformation in pair.Contacts)
            {
                if (contactInformation.Contact.PenetrationDepth >= 0)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
