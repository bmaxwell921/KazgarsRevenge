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
        public DragonController(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity, 10)
        {
            settings.stopChasingRange = 2000;
            settings.attackLength = 1337;
            settings.attackCreateMillis = settings.attackLength / 2;
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
            currentUpdateFunction = new AIUpdateFunction(AIDragonPhase1);
            PlayAnimation(settings.aniPrefix + settings.moveAniName);
            timerCounter = 0;
            timerLength = 8000;
        }

        private void StartPhase2()
        {

        }

        private void StartPhase3()
        {

        }

        private void ResetEncounter()
        {
            targetHealth = null;
            targetData = null;

            //reset position here?

            //heal and go back to waiting state
            Heal(MaxHealth);
            currentUpdateFunction = new AIUpdateFunction(AIDragonWaiting);
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


        bool leftHead = true;

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

        double enrageTimer = 120000;
        private void AIDragonPhase2(double millis)
        {
            //run towards player

            //if in range, bite player (alternating heads)

            //every 6 seconds, launch fire or ice ground attack (alternating)

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


        protected override void SwitchToWandering()
        {
            ResetEncounter();
        }
    }
}
