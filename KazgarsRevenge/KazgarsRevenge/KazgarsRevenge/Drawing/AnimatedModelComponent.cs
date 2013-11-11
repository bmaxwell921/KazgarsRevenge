﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkinnedModelLib;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.MathExtensions;
using Microsoft.Xna.Framework.Input;

namespace KazgarsRevenge
{
    class AnimatedModelComponent : DrawableComponent3D
    {
        //components
        protected Entity physicalData;
        protected AnimationPlayer animationPlayer;

        //fields
        protected Model model;
        protected Vector3 drawScale = new Vector3(1);
        protected Vector3 localOffset = Vector3.Zero;
        protected Dictionary<string, AttachableModel> attachedModels;
        protected Matrix yawOffset = Matrix.CreateFromYawPitchRoll(MathHelper.Pi, 0, 0);

        public AnimatedModelComponent(MainGame game, Entity physicalData, Model model, AnimationPlayer animations, Vector3 drawScale, Vector3 drawOffset, Dictionary<string, AttachableModel> attachedModels)
            : base(game)
        {
            this.physicalData = physicalData;
            this.model = model;
            this.drawScale = drawScale;
            this.localOffset = drawOffset;
            this.attachedModels = attachedModels;

            this.animationPlayer = animations;
            PlayAnimation(animationPlayer.skinningDataValue.AnimationClips.Keys.First());
        }

        public override void Start()
        {

        }

        protected Vector3 vLightDirection = new Vector3(-1.0f, -.5f, 1.0f);
        Matrix rot = Matrix.Identity;
        public override void Update(GameTime gameTime)
        {
            //need to do this conversion from Matrix3x3 to Matrix; Matrix3x3 is just a bepu thing
            Matrix3X3 bepurot = physicalData.OrientationMatrix;
            //either do this or Matrix.CreateFromQuaternion(physicalData.Orientation);
            //this is probably faster? not sure how CreateFromQuaternion works
            rot = new Matrix(bepurot.M11, bepurot.M12, bepurot.M13, 0, bepurot.M21, bepurot.M22, bepurot.M23, 0, bepurot.M31, bepurot.M32, bepurot.M33, 0, 0, 0, 0, 1);
            rot *= yawOffset;
            animationPlayer.Update(gameTime.ElapsedGameTime, true,
                rot * Matrix.CreateScale(drawScale) * Matrix.CreateTranslation(physicalData.Position + localOffset));
        }
        public override void Draw(GameTime gameTime, Matrix view, Matrix projection, bool edgeDetection)
        {
            Matrix[] bones = animationPlayer.GetSkinTransforms();
            Matrix3X3 bepurot = physicalData.OrientationMatrix;

            Matrix worldWithoutBone = rot
                //* Matrix.CreateFromQuaternion(physicalData.Orientation)
                        * Matrix.CreateScale(drawScale)
                        * Matrix.CreateTranslation(physicalData.Position + localOffset);
            //drawing with toon shader
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (CustomSkinnedEffect effect in mesh.Effects)
                {
                    effect.CurrentTechnique = effect.Techniques[edgeDetection? "NormalDepth" : "Toon"];
                    effect.SetBoneTransforms(bones);

                    effect.View = view;
                    effect.Projection = projection;
                }

                mesh.Draw();
            }

            Matrix[] worldbones = animationPlayer.GetWorldTransforms();
            Matrix[] transforms;
            Matrix attachedRot = Matrix.CreateFromYawPitchRoll(0, MathHelper.Pi, 0);
            foreach (AttachableModel a in attachedModels.Values)
            {
                transforms = new Matrix[a.model.Bones.Count];
                a.model.CopyAbsoluteBoneTransformsTo(transforms);
                foreach (ModelMesh mesh in a.model.Meshes)
                {
                    foreach (Effect effect in mesh.Effects)
                    {
                        effect.CurrentTechnique = effect.Techniques[edgeDetection ? "NormalDepth" : "Toon"];
                        Matrix world = transforms[mesh.ParentBone.Index] * worldbones[model.Bones[a.otherBoneName].Index - 2];
                        effect.Parameters["World"].SetValue(world);
                        effect.Parameters["ViewProj"].SetValue(view * projection);
                        effect.Parameters["InverseWorld"].SetValue(Matrix.Invert(world));
                    }
                    mesh.Draw();
                }
            }
        }

        public AnimationClip GetAnimationClip(string clipName)
        {
            return animationPlayer.skinningDataValue.AnimationClips[clipName];
        }

        public void PlayAnimation(string animationName)
        {
            animationPlayer.StartClip(animationPlayer.skinningDataValue.AnimationClips[animationName], false);
        }
    }
}