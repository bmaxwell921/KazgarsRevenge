﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkinnedModelLib;
using BEPUphysics;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    class AnimatedModelComponent : Component
    {
        //components
        protected Entity physicalData;
        protected AnimationPlayer animationPlayer;
        protected 

        //fields
        protected Model model;
        protected Vector3 drawScale = new Vector3(1);
        protected Vector3 localOffset = Vector3.Zero;
        protected Matrix drawRotation;

        public AnimatedModelComponent(MainGame game, Entity physicalData, Model model)
            : base(game)
        {
            this.physicalData = physicalData;
            this.model = model;
        }

        public override void Update(GameTime gameTime)
        {
            animationPlayer.Update(gameTime.ElapsedGameTime, true,
                drawRotation * Matrix.CreateScale(drawScale) * Matrix.CreateTranslation(physicalData.Position + localOffset));


            Matrix[] bones = animationPlayer.GetSkinTransforms();


            //drawing with toon shader
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (CustomSkinnedEffect effect in mesh.Effects)
                {
                    effect.SetBoneTransforms(bones);

                    effect.View = view;
                    effect.Projection = projection;

                    Matrix worldMatrix = bones[mesh.ParentBone.Index]
                        * drawRotation
                        * Matrix.CreateScale(drawScale)
                        * Matrix.CreateTranslation(Position + localOffset);

                    effect.Parameters["matInverseWorld"].SetValue(Matrix.Invert(worldMatrix));
                    effect.Parameters["vLightDirection"].SetValue(vLightDirection);
                    effect.Parameters["CelMap"].SetValue(toonMap);
                }

                mesh.Draw();
            }
        }

        public AnimationClip GetAnimationClip(string clipName)
        {
            return animationPlayer.skinningDataValue.AnimationClips[clipName];
        }

        public void PlayAnimation(string animationName)
        {
            animationPlayer.StartClip(animationPlayer.skinningDataValue.AnimationClips[animationName]);
        }
    }
}
