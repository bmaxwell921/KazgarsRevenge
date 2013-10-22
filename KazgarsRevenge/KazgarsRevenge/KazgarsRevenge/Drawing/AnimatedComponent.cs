using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkinnedModelLib;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.MathExtensions;

namespace KazgarsRevenge
{
    class AnimatedModelComponent : DrawableComponent3D
    {
        //components
        protected Entity physicalData;
        protected AnimationPlayer animationPlayer;
        protected CameraComponent camera;

        //fields
        protected Model model;
        protected Vector3 drawScale = new Vector3(1);
        protected Vector3 localOffset = Vector3.Zero;
        protected EffectParameter epLightDirection;
        protected EffectParameter epToonMap;
        protected AttachableModel[] attachedModels;

        public AnimatedModelComponent(MainGame game, Entity physicalData, Model model, AnimationPlayer animations, Vector3 drawScale, Vector3 drawOffset, AttachableModel[] attachedModels)
            : base(game)
        {
            this.physicalData = physicalData;
            this.model = model;
            this.camera = Game.Services.GetService(typeof(CameraComponent)) as CameraComponent;
            this.drawScale = drawScale;
            this.localOffset = drawOffset;

            this.animationPlayer = animations;
            PlayAnimation(animationPlayer.skinningDataValue.AnimationClips.Keys.First());
        }

        public override void Start()
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (CustomSkinnedEffect effect in mesh.Effects)
                {
                    epToonMap = effect.Parameters["CelMap"];
                    epToonMap.SetValue(Game.ToonMap);

                    //epLightDirection = effect.Parameters["vLightDirection"];
                    //epLightDirection.SetValue(vLightDirection);
                }
            }
        }

        protected Vector3 vLightDirection = new Vector3(-1.0f, -.5f, 1.0f);
        public override void Update(GameTime gameTime)
        {
            animationPlayer.Update(gameTime.ElapsedGameTime, true,
                Matrix.CreateFromQuaternion(physicalData.Orientation) * Matrix.CreateScale(drawScale) * Matrix.CreateTranslation(physicalData.Position + localOffset));
        }

        public override void Draw(GameTime gameTime, Matrix view, Matrix projection, string technique)
        {
            Matrix[] bones = animationPlayer.GetSkinTransforms();

            //need to do this conversion from Matrix3x3 to Matrix; Matrix3x3 is just a bepu thing
            Matrix3X3 bepurot = physicalData.OrientationMatrix;

            //either do this or Matrix.CreateFromQuaternion(physicalData.Orientation);
            //this is probably faster? not sure how CreateFromQuaternion works
            Matrix rot = new Matrix(bepurot.M11, bepurot.M12, bepurot.M13, 0, bepurot.M21, bepurot.M22, bepurot.M23, 0, bepurot.M31, bepurot.M32, bepurot.M33, 0, 0, 0, 0, 1);
            
            //drawing with toon shader
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (CustomSkinnedEffect effect in mesh.Effects)
                {
                    effect.CurrentTechnique = effect.Techniques[technique];
                    effect.SetBoneTransforms(bones);

                    effect.View = view;
                    effect.Projection = projection;

                    Matrix worldMatrix = bones[mesh.ParentBone.Index]
                        * rot
                        //* Matrix.CreateFromQuaternion(physicalData.Orientation)
                        * Matrix.CreateScale(drawScale)
                        * Matrix.CreateTranslation(physicalData.Position + localOffset);

                    //effect.World = worldMatrix;
                    effect.Parameters["matInverseWorld"].SetValue(Matrix.Invert(worldMatrix));
                    //effect.Parameters["World"].SetValue(worldMatrix);
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
