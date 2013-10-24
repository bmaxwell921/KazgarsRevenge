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
using Microsoft.Xna.Framework.Input;

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
        protected List<AttachableModel> attachedModels;

        public AnimatedModelComponent(MainGame game, Entity physicalData, Model model, AnimationPlayer animations, Vector3 drawScale, Vector3 drawOffset, List<AttachableModel> attachedModels)
            : base(game)
        {
            this.physicalData = physicalData;
            this.model = model;
            this.camera = Game.Services.GetService(typeof(CameraComponent)) as CameraComponent;
            this.drawScale = drawScale;
            this.localOffset = drawOffset;
            this.attachedModels = attachedModels;

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
        int ind = 0;
        public override void Draw(GameTime gameTime, Matrix view, Matrix projection, string technique)
        {
            Matrix[] bones = animationPlayer.GetSkinTransforms();
            //need to do this conversion from Matrix3x3 to Matrix; Matrix3x3 is just a bepu thing
            Matrix3X3 bepurot = physicalData.OrientationMatrix;

            //either do this or Matrix.CreateFromQuaternion(physicalData.Orientation);
            //this is probably faster? not sure how CreateFromQuaternion works
            Matrix rot = new Matrix(bepurot.M11, bepurot.M12, bepurot.M13, 0, bepurot.M21, bepurot.M22, bepurot.M23, 0, bepurot.M31, bepurot.M32, bepurot.M33, 0, 0, 0, 0, 1);

            Matrix worldWithoutBone = rot
                //* Matrix.CreateFromQuaternion(physicalData.Orientation)
                        * Matrix.CreateScale(drawScale)
                        * Matrix.CreateTranslation(physicalData.Position + localOffset);
            //drawing with toon shader
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (CustomSkinnedEffect effect in mesh.Effects)
                {
                    effect.CurrentTechnique = effect.Techniques[technique];
                    effect.SetBoneTransforms(bones);

                    effect.View = view;
                    effect.Projection = projection;
                }

                mesh.Draw();
            }

            Matrix[] worldbones = animationPlayer.GetWorldTransforms();
            Matrix[] transforms;
            foreach (AttachableModel a in attachedModels)
            {
                transforms = new Matrix[a.model.Bones.Count];
                a.model.CopyAbsoluteBoneTransformsTo(transforms);
                foreach (ModelMesh mesh in a.model.Meshes)
                {

                    /*foreach (CustomSkinnedEffect effect in mesh.Effects)
                    {
                        effect.CurrentTechnique = effect.Techniques[technique];
                        effect.World = transforms[mesh.ParentBone.Index] * worldbones[model.Bones[a.otherBoneName].Index - 2];
                        effect.View = view;
                        effect.Projection = projection;
                    }*/
                    foreach (Effect effect in mesh.Effects)
                    {
                        effect.CurrentTechnique = effect.Techniques[technique];
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
            animationPlayer.StartClip(animationPlayer.skinningDataValue.AnimationClips[animationName]);
        }
    }
}
