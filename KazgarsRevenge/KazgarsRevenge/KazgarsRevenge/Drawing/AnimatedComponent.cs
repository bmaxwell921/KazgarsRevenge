using System;
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
        protected CameraComponent camera;

        //fields
        protected Model model;
        protected Vector3 drawScale = new Vector3(1);
        protected Vector3 localOffset = Vector3.Zero;
        protected Matrix drawRotation;
        protected EffectParameter epLightDirection;
        protected EffectParameter epToonMap;

        public AnimatedModelComponent(MainGame game, Entity physicalData, Model model, SkinningData skinningData)
            : base(game)
        {
            this.physicalData = physicalData;
            this.model = model;
            this.camera = Game.Services.GetService(typeof(CameraComponent)) as CameraComponent;

            if (skinningData == null)
                throw new ArgumentNullException("skinningData");

            animationPlayer = new AnimationPlayer(skinningData);
            PlayAnimation(skinningData.AnimationClips.Keys.First());
        }

        public override void Start()
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (CustomSkinnedEffect effect in mesh.Effects)
                {
                    epToonMap = effect.Parameters["CelMap"];
                    epToonMap.SetValue(Game.ToonMap);

                    epLightDirection = effect.Parameters["vLightDirection"];
                    epLightDirection.SetValue(vLightDirection);
                }
            }
        }

        protected Vector4 vLightDirection = new Vector4(-1.0f, -.5f, 1.0f, 1.0f);
        public override void Update(GameTime gameTime)
        {
            animationPlayer.Update(gameTime.ElapsedGameTime, true,
                drawRotation * Matrix.CreateScale(drawScale) * Matrix.CreateTranslation(physicalData.Position + localOffset));
        }

        public void Draw(Matrix view, Matrix projection)
        {
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
                        * Matrix.CreateTranslation(physicalData.Position + localOffset);

                    effect.Parameters["matInverseWorld"].SetValue(Matrix.Invert(worldMatrix));
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
