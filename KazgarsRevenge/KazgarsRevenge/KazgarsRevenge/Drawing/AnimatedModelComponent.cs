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
    public class SharedGraphicsParams
    {
        public float alpha;
        public float lineIntensity;
        public float size;
        public SharedGraphicsParams()
        {
            alpha = 1f;
            lineIntensity = 1f;
            size = 1;
        }
    }

    public class AnimatedModelComponent : DrawableComponent3D
    {
        //components
        protected Entity physicalData;
        protected AnimationPlayer animationPlayer;

        //fields
        protected Model model;
        protected Vector3 localOffset = Vector3.Zero;
        protected Dictionary<string, AttachableModel> attachedModels;
        protected Matrix yawOffset = Matrix.CreateFromYawPitchRoll(MathHelper.Pi, 0, 0);

        protected SharedGraphicsParams modelParams;

        public AnimatedModelComponent(KazgarsRevengeGame game, GameEntity entity, Model model, float drawScale, Vector3 drawOffset)
            : base(game, entity)
        {
            this.model = model;
            this.localOffset = drawOffset;
            this.physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
            this.attachedModels = entity.GetSharedData(typeof(Dictionary<string, AttachableModel>)) as Dictionary<string, AttachableModel>;
            this.animationPlayer = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;

            animationPlayer.StartClip(animationPlayer.skinningDataValue.AnimationClips.Keys.First(), MixType.None);

            modelParams = new SharedGraphicsParams();
            modelParams.size = drawScale;
            entity.AddSharedData(typeof(SharedGraphicsParams), modelParams);

        }


        Dictionary<Type, ParticleEmitter> emitters = new Dictionary<Type, ParticleEmitter>();
        public void AddEmitter(Type particleType, float particlesPerSecond, int maxOffset, Vector3 offsetFromCenter)
        {
            if (!emitters.ContainsKey(particleType))
            {
                emitters.Add(particleType, new ParticleEmitter((Game.Services.GetService(typeof(ParticleManager)) as ParticleManager).GetSystem(particleType),
                    particlesPerSecond, physicalData.Position, maxOffset, offsetFromCenter));
            }
        }

        public void AddEmitter(Type particleType, float particlesPerSecond, int maxOffset, Vector3 offsetFromCenter, int attachIndex)
        {
            if (!emitters.ContainsKey(particleType))
            {
                emitters.Add(particleType, new ParticleEmitter((Game.Services.GetService(typeof(ParticleManager)) as ParticleManager).GetSystem(particleType),
                    particlesPerSecond, physicalData.Position, maxOffset, offsetFromCenter, attachIndex));
            }
        }

        public void RemoveEmitter(Type particleType)
        {
            emitters.Remove(particleType);
        }


        protected Vector3 vLightDirection = new Vector3(-1.0f, -.5f, 1.0f);
        Matrix rot = Matrix.Identity;
        public override void Update(GameTime gameTime)
        {
            foreach (KeyValuePair<Type, ParticleEmitter> k in emitters)
            {
                if (k.Value.BoneIndex < 0)
                {
                    k.Value.Update(gameTime, physicalData.Position);
                }
                else
                {
                    Vector3 bonePos = animationPlayer.GetWorldTransforms()[k.Value.BoneIndex].Translation;
                    k.Value.Update(gameTime, bonePos);
                }
            }


            //need to do this conversion from Matrix3x3 to Matrix; Matrix3x3 is just a bepu thing
            Matrix3X3 bepurot = physicalData.OrientationMatrix;
            //either do this or Matrix.CreateFromQuaternion(physicalData.Orientation);
            //this is probably faster? not sure how CreateFromQuaternion works
            rot = new Matrix(bepurot.M11, bepurot.M12, bepurot.M13, 0, bepurot.M21, bepurot.M22, bepurot.M23, 0, bepurot.M31, bepurot.M32, bepurot.M33, 0, 0, 0, 0, 1);
            rot *= yawOffset;
            animationPlayer.Update(gameTime.ElapsedGameTime, true,
                rot * Matrix.CreateScale(new Vector3(modelParams.size)) * Matrix.CreateTranslation(physicalData.Position + localOffset));
        }
        public override void Draw(GameTime gameTime, Matrix view, Matrix projection, bool edgeDetection)
        {
            Matrix[] bones = animationPlayer.GetSkinTransforms();

            Matrix worldWithoutBone = rot
                //* Matrix.CreateFromQuaternion(physicalData.Orientation)
                        * Matrix.CreateScale(new Vector3(modelParams.size))
                        * Matrix.CreateTranslation(physicalData.Position + localOffset);
            //drawing with toon shader
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (CustomSkinnedEffect effect in mesh.Effects)
                {
                    effect.Parameters["alpha"].SetValue(modelParams.alpha);
                    effect.Parameters["lineIntensity"].SetValue(modelParams.lineIntensity);
                    effect.CurrentTechnique = effect.Techniques[edgeDetection? "NormalDepth" : "Toon"];
                    effect.SetBoneTransforms(bones);

                    effect.View = view;
                    effect.Projection = projection;
                }

                mesh.Draw();
            }

            if (attachedModels.Count > 0)
            {
                Matrix[] worldbones = animationPlayer.GetWorldTransforms();
                Matrix[] transforms;
                foreach (AttachableModel a in attachedModels.Values)
                {
                    if (a.Draw)
                    {
                        transforms = new Matrix[a.model.Bones.Count];
                        a.model.CopyAbsoluteBoneTransformsTo(transforms);
                        foreach (ModelMesh mesh in a.model.Meshes)
                        {
                            foreach (Effect effect in mesh.Effects)
                            {
                                effect.Parameters["alpha"].SetValue(modelParams.alpha);
                                effect.Parameters["lineIntensity"].SetValue(modelParams.lineIntensity);
                                effect.CurrentTechnique = effect.Techniques[edgeDetection ? "NormalDepth" : "Toon"];
                                Matrix world = Matrix.CreateFromYawPitchRoll(a.xRotation, 0, 0) * transforms[mesh.ParentBone.Index] * worldbones[model.Bones[a.otherBoneName].Index - 2];
                                effect.Parameters["World"].SetValue(world);
                                effect.Parameters["ViewProj"].SetValue(view * projection);
                                effect.Parameters["InverseWorld"].SetValue(Matrix.Invert(world));
                            }
                            mesh.Draw();
                        }
                    }
                }
            }
        }

        public AnimationClip GetAnimationClip(string clipName)
        {
            return animationPlayer.skinningDataValue.AnimationClips[clipName];
        }

    }
}
