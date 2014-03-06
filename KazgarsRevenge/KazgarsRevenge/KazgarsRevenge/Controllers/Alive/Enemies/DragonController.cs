using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    public class DragonController : EnemyController
    {
        private enum DragonState
        {
            Phase1,
            Phase2,
            Phase3,
        }

        DragonState state = DragonState.Phase1;

        public DragonController(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity, 10)
        {
            settings.aniPrefix = "d_";
            settings.moveAniName = "walk";
            settings.attackAniName = "fireball";
            settings.deathAniName = "fireball";
            settings.hitAniName = "fireball";
            settings.idleAniName = "fireball";

            settings.stopChasingRange = 2000;
            settings.attackLength = 7250;
            settings.attackCreateMillis = 5000;

            currentUpdateFunction = new AIUpdateFunction(AIDragonWaiting);
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
            PlayAnimation(settings.aniPrefix + settings.moveAniName);
            timerCounter = 0;
            timerLength = 8000;

            settings.attackRange = 10000;
        }

        private void StartPhase2()
        {
            state = DragonState.Phase2;
            currentUpdateFunction = new AIUpdateFunction(AIDragonPhase2);
            settings.attackRange = 300;

            nextSpitBomb = 8000;
        }

        private void StartPhase3()
        {
            state = DragonState.Phase3;
            currentUpdateFunction = new AIUpdateFunction(AIDragonEnrage);
        }

        private void ResetEncounter()
        {
            state = DragonState.Phase1;
            targetHealth = null;
            targetData = null;
            enrageTimer = 120000;

            //reset position here?

            //heal and go back to waiting state
            Heal(MaxHealth);
            currentUpdateFunction = new AIUpdateFunction(AIDragonWaiting);
            PlayAnimation(settings.aniPrefix + settings.idleAniName);
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

        double nextSpitBomb = 8000;

        double enrageTimer = 120000;
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
                StartPhase3();
            }
        }

        private void AIDragonEnrage(double millis)
        {
            //breathe fire continuously at player

        }

        int chosenAttack = 0;
        protected override void CreateAttack()
        {
            switch (state)
            {
                case DragonState.Phase1:
                    if (iceHead)
                    {
                        attacks.CreateFrostbolt(model.GetBonePosition("d_mouth_emittor_R"), physicalData.OrientationMatrix.Forward, 50, this as AliveComponent);
                    }
                    else
                    {
                        attacks.CreateFirebolt(model.GetBonePosition("d_mouth_emittor_L"), physicalData.OrientationMatrix.Forward, 50, this as AliveComponent);
                    }
                    iceHead = !iceHead;
                    break;
                case DragonState.Phase2:
                    if(chosenAttack == 0)
                    {
                        if (iceHead)
                        {
                            attacks.CreateFireSpitBomb(model.GetBonePosition("d_mouth_emittor_R"), targetData.Position, 50, this as AliveComponent);
                        }
                        else
                        {
                            attacks.CreateFireSpitBomb(model.GetBonePosition("d_mouth_emittor_L"), targetData.Position, 50, this as AliveComponent);
                        }
                        iceHead = !iceHead;
                        settings.attackRange = 300;
                    }
                    else
                    {
                        attacks.CreateMeleeAttack(model.GetBonePosition("d_mouth_emittor_R"), 35, false, this as AliveComponent);
                        switch (chosenAttack)
                        {
                            case 1:

                                break;
                            case 2:
                                break;
                            case 3:
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case DragonState.Phase3:
                    //breathe fire
                    if (iceHead)
                    {
                        attacks.CreateFrostbolt(model.GetBonePosition("d_mouth_emittor_R"), physicalData.OrientationMatrix.Forward, 50, this as AliveComponent);
                    }
                    else
                    {
                        attacks.CreateFirebolt(model.GetBonePosition("d_mouth_emittor_L"), physicalData.OrientationMatrix.Forward, 50, this as AliveComponent);
                    }
                    iceHead = !iceHead;
                    break;
            }
        }

        protected override void StartAttack()
        {
            startedAttack = true;
            attackCounter = 0;
            attackCreateCounter = 0;
            switch (state)
            {
                case DragonState.Phase1:
                    //TODO: replace with fire/ice spit animation length 
                    //and put in correct animation names when we get the dragon model
                    settings.attackLength = settings.attackLength;
                    settings.attackCreateMillis = settings.attackCreateMillis;
                    if (iceHead)
                    {
                        PlayAnimation(settings.aniPrefix + settings.attackAniName);
                    }
                    else
                    {
                        PlayAnimation(settings.aniPrefix + settings.attackAniName);
                    }
                    break;
                case DragonState.Phase2:
                    if (nextSpitBomb <= 0)
                    {
                        chosenAttack = 0;
                        nextSpitBomb = rand.Next(4000, 15000);
                        if (iceHead)
                        {
                            PlayAnimation(settings.aniPrefix + settings.attackAniName);
                        }
                        else
                        {
                            PlayAnimation(settings.aniPrefix + settings.attackAniName);
                        }
                        settings.attackLength = settings.attackLength;
                        settings.attackCreateMillis = settings.attackCreateMillis;
                    }
                    else
                    {
                        chosenAttack = rand.Next(1, 5);
                        switch (chosenAttack)
                        {
                            case 0:
                                //left bite
                                PlayAnimation(settings.aniPrefix + settings.attackAniName);
                                settings.attackLength = settings.attackLength;
                                settings.attackCreateMillis = settings.attackCreateMillis;

                                break;
                            case 1:
                                //right bite
                                PlayAnimation(settings.aniPrefix + settings.attackAniName);
                                settings.attackLength = settings.attackLength;
                                settings.attackCreateMillis = settings.attackCreateMillis;

                                break;
                            case 2:
                                //left claw
                                PlayAnimation(settings.aniPrefix + settings.attackAniName);
                                settings.attackLength = settings.attackLength;
                                settings.attackCreateMillis = settings.attackCreateMillis;

                                break;
                            case 3:
                                //right claw
                                PlayAnimation(settings.aniPrefix + settings.attackAniName);
                                settings.attackLength = settings.attackLength;
                                settings.attackCreateMillis = settings.attackCreateMillis;

                                break;
                        }
                    }
                    break;
                case DragonState.Phase3:
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
                case DragonState.Phase3:
                    currentUpdateFunction = new AIUpdateFunction(AIDragonEnrage);
                    break;
            }
        }

        protected override void SwitchToWandering()
        {
            ResetEncounter();
        }
    }
}
