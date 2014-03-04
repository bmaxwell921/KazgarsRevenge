﻿using System;
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
        }

        public override void Update(GameTime gameTime)
        {
            if (targetHealth != null && targetHealth.Dead)
            {
                ResetEncounter();
            }
            base.Update(gameTime);
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
                    currentUpdateFunction = new AIUpdateFunction(AIDragonPhase1);
                    PlayAnimation(settings.aniPrefix + settings.moveAniName);
                    timerCounter = 0;
                    timerLength = 8000;
                    return;
                }
            }
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

        bool leftHead = true;
        
        private void AIDragonPhase1(double millis)
        {
            timerCounter += millis;
            
            //sit still and rotate towards player
            Vector3 diff = new Vector3(targetData.Position.X - physicalData.Position.X, 0, targetData.Position.Z - physicalData.Position.Z);
            physicalData.LinearVelocity = Vector3.Zero;
            
                if (startedAttack)
                {
                    if (timerCounter >= timerLength)
                    {
                    }
                }
                else
                {
                    if (timerCounter >= timerLength)
                    {
                    }

                }
            
            //every 4 seconds, launch ice or fire attack (alternating)
            /*
            if (startingAttack)
            {
                physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetGraphicsYaw(diff), 0, 0);
                attackCreateCounter += millis;
                attackCounter += millis;
                attackCheckCounter += millis;
                if (attackCheckCounter >= attackCheckLength)
                {
                    DuringAttack(numChecks);
                    ++numChecks;
                    attackCheckCounter = 0;
                }
                if (attackCreateCounter >= attackLength / 2)
                {
                    CreateAttack();
                    attackCreateCounter = 0;
                    startingAttack = false;
                }
            }
            else if (targetHealth != null && targetData != null && !targetHealth.Dead)
            {
                attackCounter += millis;
                if (attackCounter >= attackLength)
                {
                    physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetGraphicsYaw(diff), 0, 0);
                    attackCounter = 0;
                    //if the player is within attack radius, swing
                    if (Math.Abs(diff.X) < settings.attackRange && Math.Abs(diff.Z) < settings.attackRange)
                    {
                        startingAttack = true;
                        attackCounter = 0;
                        attackCreateCounter = 0;
                        numChecks = 0;
                        attackCheckCounter = double.MaxValue;
                        PlayAnimation(settings.aniPrefix + settings.attackAniName);
                    }
                    else
                    {
                        //otherwise, go to run state
                        currentUpdateFunction = new AIUpdateFunction(AIRunningToTarget);
                        attackCounter = double.MaxValue;
                    }
                }
            }
            else
            {
                SwitchToWandering();
            }*/


            //if pillars are destroyed, go to phase 2
        }

        private void AIDragonPhase2(double millis)
        {
            //run towards player

            //if in range, bite player (alternating heads)

            //every 6 seconds, launch fire or ice ground attack (alternating)

            //if 2 minutes pass, enrage
        }

        private void AIDragonEnrage(double millis)
        {
            //breathe fire continuously at player
        }
    }
}
