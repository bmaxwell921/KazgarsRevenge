using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KazgarsRevenge;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.MathExtensions;
using BEPUphysics.Collidables;
using BEPUphysics.DataStructures;

namespace KazgarsRevengeServer
{
    public class SAttackManager : SEntityManager
    {
        IList<GameEntity> attacks;

        public SAttackManager(KazgarsRevengeGame game)
            : base(game)
        {
            attacks = new List<GameEntity>();
        }

        public void CreateArrow(Vector3 position, Vector3 initialTrajectory, int damage, string factionToHit)
        {
            GameEntity arrow = new GameEntity("arrow", "good");
            position.Y += 20;
            Entity arrowData = new Box(position, 10, 17, .001f);
            arrowData.CollisionInformation.CollisionRules.Group = factionToHit == "good" ? MainGame.GoodProjectileCollisionGroup : MainGame.BadProjectileCollisionGroup;
            arrowData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            arrowData.LinearVelocity = initialTrajectory;
            arrowData.Orientation = Quaternion.CreateFromRotationMatrix(CreateRotationFromForward(initialTrajectory));
            arrow.AddSharedData(typeof(Entity), arrowData);

            PhysicsComponent arrowPhysics = new PhysicsComponent(game, arrow);

            AttackController arrowAI = new AttackController(game, arrow, arrowData, damage, 3000, factionToHit);

            arrow.AddComponent(typeof(PhysicsComponent), arrowPhysics);
            gcm.AddComponent(arrowPhysics);

            arrow.AddComponent(typeof(AttackController), arrowAI);
            gcm.AddComponent(arrowAI);

            attacks.Add(arrow);
        }

        public void CreateMeleeAttack(Vector3 position, int damage, string factionToHit)
        {
            GameEntity newAttack = new GameEntity("sword", "good");
            Entity attackData = new Box(position, 35, 47, 35, .01f);
            attackData.CollisionInformation.CollisionRules.Group = factionToHit == "good" ? MainGame.GoodProjectileCollisionGroup : MainGame.BadProjectileCollisionGroup;
            attackData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            attackData.LinearVelocity = Vector3.Zero;
            newAttack.AddSharedData(typeof(Entity), attackData);

            PhysicsComponent attackPhysics = new PhysicsComponent(game, newAttack);

            AttackController attackAI = new AttackController(game, newAttack, attackData, damage, 300, factionToHit);

            newAttack.AddComponent(typeof(PhysicsComponent), attackPhysics);
            gcm.AddComponent(attackPhysics);

            newAttack.AddComponent(typeof(AttackController), attackAI);
            gcm.AddComponent(attackAI);

            attacks.Add(newAttack);
        }

        public void CreateMagicAttack()
        {
        }

        private Matrix CreateRotationFromForward(Vector3 forward)
        {
            Matrix rotation = Matrix.Identity;
            rotation.Forward = Vector3.Normalize(forward);
            rotation.Right = Vector3.Normalize(Vector3.Cross(rotation.Forward, Vector3.Up));
            rotation.Up = Vector3.Up;
            return rotation;
        }
    }
}
