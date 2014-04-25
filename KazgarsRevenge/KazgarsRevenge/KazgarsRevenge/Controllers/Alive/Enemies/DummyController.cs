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
    public class DummyController : AliveComponent
    {
        AnimationPlayer anims;        
        public DummyController(KazgarsRevengeGame game, GameEntity entity, int level)
            : base(game, entity, level)
        {
            this.anims = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;
            PlayAnimation("dummy_idle", MixType.None);

            originalBaseStats[StatType.Vitality] *= 5;
            RecalculateStats();
        }

        double aniCounter = 0;
        string curAnim = "";
        private void PlayAnimation(string animation, MixType t)
        {
            curAnim = animation;
            anims.StartClip(animation, MixType.None);
            aniCounter = anims.GetAniMillis(animation);
        }


        public override void Update(GameTime gameTime)
        {
            aniCounter -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (curAnim == "dummy_hit" && aniCounter <= 0)
            {
                PlayAnimation("dummy_idle", MixType.None);
            }


            base.Update(gameTime);
        }

        protected override void TakeDamage(int damage, GameEntity from)
        {
            if (damage > 0)
            {
                PlayAnimation("dummy_hit", MixType.PauseAtEnd);
            }
        }

        protected override void KillAlive()
        {
            Heal(MaxHealth);
        }
    }
}
