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
    public class TreasureGopherController : LootableController
    {
        enum GopherState
        {
            Idle,
            Surprised,
            Avoiding,
            DiggingDown,
            DiggingUp,
            BeingLooted,
        }

        string currentAni = "";
        private void PlayAnimation(string aniName, MixType type, float rate)
        {
            currentAni = aniName;
            animations.StartClip(aniName, type, rate);
        }

        LevelManager levels;
        AttackManager attacks;
        SoundEffectLibrary sounds;

        AnimationPlayer animations;
        GopherState state = GopherState.Idle;
        public TreasureGopherController(KazgarsRevengeGame game, GameEntity entity, List<Item> loot)
            : base(game, entity, loot)
        {
            animations = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;
            this.camera = game.Services.GetService(typeof(CameraComponent)) as CameraComponent;
            PlayAnimation("g_idle", MixType.None, 1);


            physicalData.CollisionInformation.Events.DetectingInitialCollision += HandleCollision;

            levels = Game.Services.GetService(typeof(LevelManager)) as LevelManager;
            attacks = Game.Services.GetService(typeof(AttackManager)) as AttackManager;
            sounds = Game.Services.GetService(typeof(SoundEffectLibrary)) as SoundEffectLibrary;
        }

        float senseRadius = 325f;
        double timerCounter;
        CameraComponent camera;
        Entity avoidData = null;
        float runSpeed = 50;
        
        public override void Update(GameTime gameTime)
        {
            double elapsed = gameTime.ElapsedGameTime.TotalMilliseconds;
            switch (state)
            {
                case GopherState.Idle:
                    if (InsideCameraBox(camera.CameraBox))
                    {
                        if (physicalData.Position.Y > 5)
                        {
                            physicalData.Position = new Vector3(physicalData.Position.X, 4, physicalData.Position.Z);
                        }
                        if (currentAni != "g_idle")
                        {
                            PlayAnimation("g_idle", MixType.None, 1);
                        }
                        GameEntity possPlayer = QueryNearEntityName("localplayer", physicalData.Position, 0, senseRadius);
                        if (possPlayer != null)
                        {
                            avoidData = possPlayer.GetSharedData(typeof(Entity)) as Entity;
                            PlayAnimation("g_surprise", MixType.PauseAtEnd, 1);
                            state = GopherState.Surprised;
                            timerCounter = animations.GetAniMillis(currentAni);
                        }
                    }
                    break;
                case GopherState.Surprised:
                    timerCounter -= elapsed;
                    if (timerCounter <= 0)
                    {
                        state = GopherState.Avoiding;
                    }
                    break;
                case GopherState.Avoiding:
                    if (avoidData == null)
                    {
                        GameEntity possPlayer = QueryNearEntityName("localplayer", physicalData.Position, 0, senseRadius);
                        if (possPlayer != null)
                        {
                            avoidData = possPlayer.GetSharedData(typeof(Entity)) as Entity;
                        }
                        if (avoidData == null)
                        {
                            state = GopherState.Idle;
                        }
                    }

                    if (currentAni != "g_run")
                    {
                        PlayAnimation("g_run", MixType.None, 1);
                    }

                    Vector3 diff = physicalData.Position - avoidData.Position;
                    diff.Y = 0;
                    if (Math.Abs(diff.X) + Math.Abs(diff.Z) > LevelManager.BLOCK_SIZE * 6)
                    {
                        physicalData.LinearVelocity = Vector3.Zero;
                        state = GopherState.Idle;
                        return;
                    }
                    if (diff != Vector3.Zero)
                    {
                        diff.Normalize();
                    }

                    //run away from player
                    physicalData.OrientationMatrix = CreateRotationFromForward(diff);
                    physicalData.LinearVelocity = diff * runSpeed;


                    break;
                case GopherState.DiggingDown:
                    physicalData.LinearVelocity = Vector3.Zero;
                    timerCounter -= gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (timerCounter <= 0)
                    {
                        state = GopherState.DiggingUp;
                        timerCounter = animations.GetAniMillis("g_dig") - 1000;
                        attacks.SpawnGopherDigPoof(physicalData.Position);

                        Vector3 newPos = levels.FindClosestNodeTo(physicalData.Position + physicalData.OrientationMatrix.Forward * LevelManager.BLOCK_SIZE * 3, Vector3.Zero);
                        if (Math.Abs(physicalData.Position.X - newPos.X) + Math.Abs(physicalData.Position.Z - newPos.Z) < LevelManager.BLOCK_SIZE)
                        {
                            newPos = levels.FindClosestNodeTo(physicalData.Position + physicalData.OrientationMatrix.Backward * LevelManager.BLOCK_SIZE * 3, Vector3.Zero);
                        }

                        newPos.Y = 8;
                        physicalData.Position = newPos;
                        attacks.SpawnGopherDigPoof(physicalData.Position);
                    }
                    break;
                case GopherState.DiggingUp:
                    physicalData.LinearVelocity = Vector3.Zero;
                    timerCounter -= gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (timerCounter <= 0)
                    {
                        state = GopherState.Avoiding;
                        timerCounter = 10000;
                    }
                    break;
                case GopherState.BeingLooted:
                    physicalData.LinearVelocity = Vector3.Zero;
                    timerCounter -= elapsed;
                    if (timerCounter <= 0)
                    {
                        if (currentAni == "g_loot_smash")
                        {
                            if (loot.Count == 0)
                            {
                                Vector3 smashedPos = GetBoneTranslation(1);
                                attacks.SpawnHitBlood(smashedPos);
                                sounds.endGopherSpin(true);
                                Entity.KillEntity();
                            }
                            else
                            {
                                Vector3 smashedPos = GetBoneTranslation(1);
                                physicalData.Position = new Vector3(smashedPos.X, 4, smashedPos.Z);
                                PlayAnimation("g_run", MixType.None, 1);
                                state = GopherState.Avoiding;
                            }
                        }
                        timerCounter = 0;
                    }
                    break;
            }
        }

        public override void OpenLoot(Vector3 position, Quaternion q)
        {
            spun = 0;
            sounds.playGopherPickUp();
            physicalData.Position = position;
            physicalData.Orientation = q;
            state = GopherState.BeingLooted;
            PlayAnimation("g_loot", MixType.PauseAtEnd, 1.217f);
            physicalData.LinearVelocity = Vector3.Zero;
        }

        int spun = 0;
        public override void StartSpin()
        {
            ++spun;
            if (spun == 2)
            {
                sounds.startGopherSpin();
            }
            PlayAnimation("g_loot_spin", MixType.PauseAtEnd, 1.217f);
        }

        public override void CloseLoot()
        {
            PlayAnimation("g_loot_smash", MixType.MixInto, 1.217f);
            timerCounter = animations.GetAniMillis(currentAni) * .75f - 17;
            sounds.endGopherSpin(false);
        }

        public override bool CanLoot()
        {
            return true;
        }

        protected void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            if (state == GopherState.Avoiding)
            {
                GameEntity ent = other.Tag as GameEntity;
                if (ent != null)
                {
                    if (ent.Name == "room")
                    {
                        state = GopherState.DiggingDown;
                        PlayAnimation("g_dig", MixType.None, 1);
                        timerCounter = 1000;
                    }
                }
            }
        }
    }
}
