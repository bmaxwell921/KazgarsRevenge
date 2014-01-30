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
    public class NetworkPlayerController : PlayerController
    {
        enum NetPlayerState
        {
            Standing,
            Running,
        }

        private NetPlayerState state;

        Vector3 targetPos = Vector3.Zero;
        AnimationPlayer animations;

        Dictionary<GearSlot, Equippable> gear = new Dictionary<GearSlot, Equippable>();
        Dictionary<string, AttachableModel> attached;
        public NetworkPlayerController(KazgarsRevengeGame game, GameEntity entity, PlayerSave savefile)
            : base(game, entity, savefile)
        {
            this.targetPos = physicalData.Position;
            this.animations = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;
            this.attached = Entity.GetSharedData(typeof(Dictionary<string, AttachableModel>)) as Dictionary<string, AttachableModel>;
            LootManager gearGenerator = Game.Services.GetService(typeof(LootManager)) as LootManager;
            gear[GearSlot.Righthand] = null;
            gear[GearSlot.Lefthand] = null;
            EquipGear(gearGenerator.GenerateSword(), GearSlot.Righthand);
            EquipGear(gearGenerator.GenerateBow(), GearSlot.Lefthand);

            stopRadius = 2;
        }

        /// <summary>
        /// Copied from PlayerController, pull up hierarchy
        /// </summary>
        /// <param name="equipMe"></param>
        /// <param name="slot"></param>
        public void EquipGear(Equippable equipMe, GearSlot slot)
        {
            float xRot = 0;
            //if the player is trying to equip a two-handed weapon to the offhand, unequip both current weapons and equip it to the main hand
            Weapon possWep = equipMe as Weapon;
            if (possWep != null)
            {
                if (possWep.TwoHanded)
                {
                    if (slot == GearSlot.Lefthand)
                    {
                        EquipGear(equipMe, GearSlot.Righthand);
                        return;
                    }
                    if (!UnequipGear(GearSlot.Lefthand))
                    {
                        return;
                    }
                }

                if (slot == GearSlot.Righthand)
                {
                    xRot = MathHelper.Pi;
                }
            }

            //otherwise, carry on
            if (UnequipGear(slot))
            {
                gear[slot] = equipMe;
                RecalculateStats();

                attached.Add(slot.ToString(), new AttachableModel(equipMe.GearModel, GearSlotToBoneName(slot), xRot));
            }
        }

        /// <summary>
        /// Copied from PlayerController, pull up hierarchy
        /// </summary>
        public string GearSlotToBoneName(GearSlot s)
        {
            switch (s)
            {
                case GearSlot.Lefthand:
                    return "Bone_001_L_005";
                case GearSlot.Righthand:
                    return "Hand_R";
                default:
                    return "RootNode";
            }
        }


        /// <summary>
        /// Copied from PlayerController, pull up hierarchy
        /// </summary>
        public void AddStats(Dictionary<StatType, float> statsToAdd)
        {
            foreach (KeyValuePair<StatType, float> k in statsToAdd)
            {
                stats[k.Key] += k.Value;
            }
        }

        /// <summary>
        ///  Copied from PlayerController, pull up hierarchy
        /// </summary>
        protected override bool UnequipGear(GearSlot slot)
        {
            Equippable oldEquipped = gear[slot];
            if (oldEquipped != null)
            {
                // Shouldn't need to do this check with networked players
                //if (AddToInventory(oldEquipped))
                //{
                //    attached.Remove(slot.ToString());
                //    return true;
                //}
                //else
                //{
                //    return false;
                //}
                return true;
            }
            else
            {
                //if there was nothing in there to start with, return true
                return true;
            }
        }

        public void SetPosition(Vector3 pos)
        {
            this.targetPos = pos;
            Vector3 diff = targetPos - physicalData.Position;
            if (Math.Abs(diff.X) > stopRadius && Math.Abs(diff.Z) > stopRadius)
            {
                physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(GetGraphicsYaw(diff), 0, 0);
            }


            //get turning speed (should be faster the greater the difference in direction)

            newDir = GetPhysicsYaw(diff);
            //in case one is around 2pi and the other is around 0 (the difference would be larger than it should)
            float dirdiff = Math.Min(Math.Abs(newDir - curDir), Math.Abs(newDir + MathHelper.TwoPi - curDir));

            //min speed is at 30 degree diff
            turnSpeed = Math.Max(0, dirdiff - .52f);
            float c = turnSpeed / 2.89f;
            turnSpeed *= c;
            turnSpeed += .045f;
        }

        float runSpeed = 120;
        float turnSpeed = .045f;
        public override void Update(GameTime gameTime)
        {

            Vector3 diff = targetPos - physicalData.Position;
            switch (state)
            {
                case NetPlayerState.Running:
                    if (Math.Abs(diff.X) < stopRadius && Math.Abs(diff.Z) < stopRadius)
                    {
                        physicalData.LinearVelocity = Vector3.Zero;
                        state = NetPlayerState.Standing;
                        animations.StartClip("k_fighting_stance", MixType.None);
                    }
                    else
                    {
                        AdjustDir(runSpeed, turnSpeed);
                    }
                    break;
                case NetPlayerState.Standing:
                    if (Math.Abs(diff.X) > stopRadius && Math.Abs(diff.Z) > stopRadius)
                    {
                        curDir = GetGraphicsYaw(diff);
                        physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(curDir, 0, 0);
                        animations.StartClip("k_run", MixType.None);
                        state = NetPlayerState.Running;
                    }
                    break;
            }
        }

        protected override void TakeDamage(int damage, GameEntity from)
        {
            //particles?
        }
    }
}
