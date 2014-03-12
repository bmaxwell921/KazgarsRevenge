using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;
using SkinnedModelLib;

namespace KazgarsRevenge
{
    public class DragonController : EnemyController
    {
        private enum DragonState
        {
            Phase1,
            Phase2,
            Enrage,
        }

        DragonState state = DragonState.Phase1;

        public DragonController(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity, 10)
        {
            settings.aniPrefix = "d_";
            settings.attackAniName = "snap";

            settings.stopChasingRange = 2000;

            ResetEncounter();

            usesPath = false;

            chargingBoneNames.Add("d_mouth_emittor_R");
            chargingBoneNames.Add("d_mouth_emittor_L");

        }

        public override void Update(GameTime gameTime)
        {
            if (targetHealth != null && targetHealth.Dead)
            {
                ResetEncounter();
            }
            base.Update(gameTime);
        }

        private void StartPhase1()
        {
            state = DragonState.Phase1;
            currentUpdateFunction = new AIUpdateFunction(AIDragonPhase1);
            timerCounter = 0;
            timerLength = 8000;
            raycastCheckTarget = false;

            settings.attackLength = 10000;
            settings.attackRange = 10000;
            attackAniLength = animations.GetAniMillis("d_fireball_lob_L");
        }

        private void StartPhase2()
        {
            raycastCheckTarget = true;
            state = DragonState.Phase2;
            currentUpdateFunction = new AIUpdateFunction(AIDragonPhase2);
            settings.attackRange = 80;

            nextSpitBomb = 8000;
        }

        private void StartEnrage()
        {
            raycastCheckTarget = true;
            state = DragonState.Enrage;
            currentUpdateFunction = new AIUpdateFunction(AIDragonEnrage);
            PlayAnimation("d_enrage", MixType.PauseAtEnd);
            timerLength = animations.GetAniMillis("d_enrage");
            timerCounter = 0;
        }

        private void ResetEncounter()
        {
            state = DragonState.Phase1;
            targetHealth = null;
            targetData = null;
            enrageTimer = 120000;
            nextSpitBomb = 1000;

            //reset position here?

            //heal and go back to waiting state
            Heal(MaxHealth);
            currentUpdateFunction = new AIUpdateFunction(AIDragonWaiting);
            PlayAnimation(settings.aniPrefix + settings.idleAniName);

            if (firePillar != null)
            {
                firePillar.KillEntity();
            }
            if (frostPillar != null)
            {
                frostPillar.KillEntity();
            }

            firePillar = (Game.Services.GetService(typeof(LevelManager)) as LevelManager).CreateDragonFirePillar(physicalData.Position + Vector3.Right * 150);
            frostPillar = (Game.Services.GetService(typeof(LevelManager)) as LevelManager).CreateDragonFrostPillar(physicalData.Position + Vector3.Left * 150);
        }

        private void AIDragonWaiting(double millis)
        {
            GameEntity possTargetPlayer = QueryNearEntityFaction(FactionType.Players, physicalData.Position, 0, settings.noticePlayerRange, false);
            if (possTargetPlayer != null)
            {
                targetHealth = possTargetPlayer.GetComponent(typeof(AliveComponent)) as AliveComponent;
                if (targetHealth != null && !targetHealth.Dead)
                {
                    targetData = possTargetPlayer.GetSharedData(typeof(Entity)) as Entity;
                    StartPhase1();
                    return;
                }
            }
        }

        bool iceHead = true;

        GameEntity frostPillar;
        GameEntity firePillar;

        private void AIDragonPhase1(double millis)
        {
            //if pillars are destroyed, go to phase 2
            if (frostPillar == null || firePillar == null || (frostPillar.Dead && firePillar.Dead))
            {
                StartPhase2();
            }

            //launch ice or fire attacks (alternating)
            AIAutoAttackingTarget(millis);
        }

        double nextSpitBomb = 1000;

        double enrageTimer = 5000;

        protected override void AIRunningToTarget(double millis)
        {
            if (state == DragonState.Phase2)
            {
                nextSpitBomb -= millis;
                if (nextSpitBomb <= 0)
                {
                    settings.attackRange = 2000; 
                }
            }

            //if 2 minutes pass, enrage
            enrageTimer -= millis;
            if (enrageTimer <= 0)
            {
                StartEnrage();
                return;
            }
            base.AIRunningToTarget(millis);
        }

        private void AIDragonPhase2(double millis)
        {
            //run towards player
            //if in range, bite player (alternating heads)
            //every so often seconds, launch fire or ice ground attack (alternating)

            nextSpitBomb -= millis;
            if (nextSpitBomb <= 0)
            {
                settings.attackRange = 2000;
            }

            AIAutoAttackingTarget(millis);

            //if 2 minutes pass, enrage
            enrageTimer -= millis;
            if (enrageTimer <= 0)
            {
                StartEnrage();
            }
        }

        private void AIDragonEnrage(double millis)
        {
            Vector3 diff = targetData.Position - physicalData.Position;
            diff.Y = 0;
            physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetGraphicsYaw(diff), 0, 0);

            //breathe fire continuously at player
            timerCounter += millis;
            if (timerCounter >= timerLength)
            {
                if (currentAniName == "d_enrage")
                {
                    model.AddEmitter(typeof(FlameThrowerSystem), "flamethrower", 100, 5, Vector3.Right * 15, "d_mouth_emittor_R");
                    model.AddEmitter(typeof(FrostThrowerSystem), "frostthrower", 100, 5, Vector3.Zero, "d_mouth_emittor_L");

                    PlayAnimation("d_enrage_fire");
                    attacks.CreateDragonFlamethrower(this as AliveComponent, GeneratePrimaryDamage(StatType.Strength) * 2);
                    timerLength = animations.GetAniMillis("d_enrage_fire") * 3;
                    timerCounter = 0;
                }
                else if (currentAniName == "d_enrage_fire")
                {
                    model.RemoveEmitter("flamethrower");
                    model.RemoveEmitter("frostthrower");
                    PlayAnimation("d_enrage_end", MixType.PauseAtEnd);
                    timerLength = animations.GetAniMillis("d_enrage_end");
                    timerCounter = 0;
                }
                else if (currentAniName == "d_enrage_end")
                {
                    StartEnrage();
                }
            }
            model.SetEmitterVel("flamethrower", 1000, "d_mouth_emittor_R", Vector3.Down * 5);
            model.SetEmitterVel("frostthrower", 1000, "d_mouth_emittor_L", Vector3.Down * 5);
        }

        int chosenAttack = 0;
        protected override void CreateAttack()
        {
            switch (state)
            {
                case DragonState.Phase1:
                    if (iceHead)
                    {
                        attacks.CreateDragonFrostbolt(model.GetBonePosition("d_mouth_emittor_R"), physicalData.OrientationMatrix.Forward, 20, this as AliveComponent);
                    }
                    else
                    {
                        attacks.CreateDragonFirebolt(model.GetBonePosition("d_mouth_emittor_L"), physicalData.OrientationMatrix.Forward, 20, this as AliveComponent);
                    }
                    iceHead = !iceHead;
                    break;
                case DragonState.Phase2:
                    if(chosenAttack == 0)
                    {
                        if (iceHead)
                        {
                            attacks.CreateFireSpitBomb(model.GetBonePosition("d_mouth_emittor_L"), targetData.Position, 1, this as AliveComponent);
                        }
                        else
                        {
                            attacks.CreateFrostSpitBomb(model.GetBonePosition("d_mouth_emittor_L"), targetData.Position, 1, this as AliveComponent);
                        }
                        iceHead = !iceHead;
                        settings.attackRange = 80;
                    }
                    else
                    {
                        Vector3 dir = targetData.Position - physicalData.Position;
                        dir.Y = 0;
                        attacks.CreateCleave(physicalData.Position + physicalData.OrientationMatrix.Forward * 80, GetPhysicsYaw(dir), 35, this as AliveComponent);
                    }
                    break;
            }
        }

        protected override void StartAttack()
        {
            attackAniCounter = 0;
            startedAttack = true;
            attackCounter = 0;
            attackCreateCounter = 0;
            switch (state)
            {
                case DragonState.Phase1:
                    //TODO: replace with fire/ice spit animation length 
                    //and put in correct animation names when we get the dragon model
                    if (iceHead)
                    {
                        //AddChargeParticles(typeof(FrostChargeSystem));
                        PlayAnimation("d_fireball_R");
                        settings.attackLength = animations.GetAniMillis("d_fireball_R");
                        settings.attackCreateMillis = settings.attackLength / 2;
                    }
                    else
                    {
                        //AddChargeParticles(typeof(DragonFireChargeSystem));
                        PlayAnimation("d_fireball_L");
                        settings.attackLength = animations.GetAniMillis("d_fireball_R");
                        settings.attackCreateMillis = settings.attackLength / 2;
                    }
                    attackAniCounter = 0;
                    attackAniLength = settings.attackLength;
                    break;
                case DragonState.Phase2:
                    //shoot spit bomb
                    if (nextSpitBomb <= 0)
                    {
                        chosenAttack = 0;
                        nextSpitBomb = rand.Next(3000, 8000);
                        if (iceHead)
                        {
                            PlayAnimation("d_fireball_lob_R");
                            settings.attackLength = animations.GetAniMillis("d_fireball_lob_R");
                            settings.attackCreateMillis = settings.attackLength / 2;
                        }
                        else
                        {
                            PlayAnimation("d_fireball_lob_L");
                            settings.attackLength = animations.GetAniMillis("d_fireball_lob_L");
                            settings.attackCreateMillis = settings.attackLength / 2;
                        }
                        attackAniCounter = 0;
                        attackAniLength = settings.attackLength;
                    }
                    else
                    {
                        chosenAttack = rand.Next(1, 4);
                        switch (chosenAttack)
                        {
                            case 1:
                                // bite
                                PlayAnimation("d_snap");
                                settings.attackLength = animations.GetAniMillis("d_snap");
                                settings.attackCreateMillis = settings.attackLength / 2;
                                break;
                            case 2:
                                //left claw
                                PlayAnimation("d_claw_L");
                                settings.attackLength = animations.GetAniMillis("d_claw_L");
                                settings.attackCreateMillis = settings.attackLength / 2;
                                break;
                            case 3:
                                //right claw
                                PlayAnimation("d_claw_R");
                                settings.attackLength = animations.GetAniMillis("d_claw_R");
                                settings.attackCreateMillis = settings.attackLength / 2;
                                break;
                        }
                        attackAniCounter = 0;
                        attackAniLength = settings.attackLength;
                    }
                    break;
            }
        }

        protected override void SwitchToAttacking()
        {
            switch (state)
            {
                case DragonState.Phase1:
                    currentUpdateFunction = new AIUpdateFunction(AIDragonPhase1);
                    break;
                case DragonState.Phase2:
                    currentUpdateFunction = new AIUpdateFunction(AIDragonPhase2);
                    break;
                case DragonState.Enrage:
                    currentUpdateFunction = new AIUpdateFunction(AIDragonEnrage);
                    break;
            }
        }

        protected override void SwitchToWandering()
        {
            ResetEncounter();
        }

        protected override void DoDamagedGraphics()
        {
            
        }

        protected override void KillAlive()
        {
            model.RemoveEmitter("flamethrower");
            model.RemoveEmitter("frostthrower");
            base.KillAlive();
        }
    }
}
