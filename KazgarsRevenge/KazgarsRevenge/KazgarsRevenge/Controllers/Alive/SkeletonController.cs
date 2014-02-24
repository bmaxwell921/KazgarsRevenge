using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public class SkeletonController : EnemyController
    {
        AnimatedModelComponent model;
        public SkeletonController(KazgarsRevengeGame game, GameEntity entity, EnemyControllerSettings settings)
            : base(game, entity, settings)
        {

        }

        public override void Start()
        {
            this.model = Entity.GetComponent(typeof(AnimatedModelComponent)) as AnimatedModelComponent;
        }

        protected override void CreateAttack()
        {
            attacks.CreateFrostbolt(physicalData.Position + physicalData.OrientationMatrix.Forward * 8, physicalData.OrientationMatrix.Forward, 1, this as AliveComponent);

            model.RemoveEmitter("frostchargeleft");
            model.RemoveEmitter("frostchargeright");
        }

        protected override void StartNormalAttack()
        {
            model.AddEmitter(typeof(FrostCharge4System), "frostchargeleft", 10, 0, Vector3.Zero, "s_hand_L");
            model.AddEmitter(typeof(FrostCharge4System), "frostchargeright", 10, 0, Vector3.Zero, "s_hand_R");
        }

        protected override void SpawnHitParticles()
        {

        }
    }
}
