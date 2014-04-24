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
            Digging,
            BeingLooted,
        }

        string currentAni = "";
        private void PlayAnimation(string aniName, MixType type)
        {
            currentAni = aniName;
            animations.StartClip(aniName, type);
        }

        AnimationPlayer animations;
        GopherState state = GopherState.Idle;
        public TreasureGopherController(KazgarsRevengeGame game, GameEntity entity, List<Item> loot)
            : base(game, entity, loot)
        {
            animations = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;
            this.camera = game.Services.GetService(typeof(CameraComponent)) as CameraComponent;
            PlayAnimation("g_idle", MixType.None);
        }

        float senseRadius = LevelManager.BLOCK_SIZE * 4;
        double timerCounter;
        CameraComponent camera;
        Entity avoidData = null;
        public override void Update(GameTime gameTime)
        {
            double elapsed = gameTime.ElapsedGameTime.TotalMilliseconds;
            switch (state)
            {
                case GopherState.Idle:
                    if (InsideCameraBox(camera.CameraBox))
                    {
                        if (currentAni != "g_idle")
                        {
                            PlayAnimation("g_idle", MixType.None);
                        }
                        GameEntity possPlayer = QueryNearEntityFaction(FactionType.Players, physicalData.Position, 0, senseRadius, false);
                        if (possPlayer != null)
                        {
                            avoidData = possPlayer.GetSharedData(typeof(Entity)) as Entity;
                            PlayAnimation("g_surprise", MixType.PauseAtEnd);
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
                        GameEntity possPlayer = QueryNearEntityFaction(FactionType.Players, physicalData.Position, 0, senseRadius, false);
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
                        PlayAnimation("g_run", MixType.None);
                    }

                    Vector3 diff = physicalData.Position - avoidData.Position;
                    if (diff != Vector3.Zero)
                    {
                        diff.Normalize();
                    }

                    //TODO round direction to 45 degree angle, check for collision with wall and start dig state

                    break;
                case GopherState.Digging:

                    break;
                case GopherState.BeingLooted:
                    timerCounter -= elapsed;
                    if (timerCounter <= 0)
                    {
                        if (currentAni == "g_loot_smash")
                        {
                            if (loot.Count == 0)
                            {
                                Entity.KillEntity();
                            }
                            else
                            {
                                Vector3 smashedPos = GetBoneTranslation(1);
                                physicalData.Position = new Vector3(smashedPos.X, 10, smashedPos.Z);
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
            physicalData.Position = position;
            physicalData.Orientation = q;
            state = GopherState.BeingLooted;
            PlayAnimation("g_loot", MixType.PauseAtEnd);
            timerCounter = animations.GetAniMillis(currentAni);
            physicalData.LinearVelocity = Vector3.Zero;
        }

        public override void StartSpin()
        {
            PlayAnimation("g_loot_spin", MixType.PauseAtEnd);
        }

        public override void CloseLoot()
        {
            PlayAnimation("g_loot_smash", MixType.MixInto);
            timerCounter = 0;
            timerCounter = animations.GetAniMillis(currentAni) - 30;
        }

        public override bool CanLoot()
        {
            return true;
        }
    }
}
