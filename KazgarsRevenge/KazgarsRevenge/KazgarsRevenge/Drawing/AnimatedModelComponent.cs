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
        protected AnimationPlayer animationPlayer;

        //fields
        protected Model model;
        protected Vector3 localOffset = Vector3.Zero;
        protected Dictionary<string, AttachableModel> attachedModels;
        protected Dictionary<string, Model> syncedModels;
        protected Matrix yawOffset = Matrix.CreateFromYawPitchRoll(MathHelper.Pi, 0, 0);

        protected SharedGraphicsParams modelParams;

        public AnimatedModelComponent(KazgarsRevengeGame game, GameEntity entity, Model model, float drawScale, Vector3 drawOffset)
            : base(game, entity)
        {
            this.model = model;
            this.localOffset = drawOffset;
            this.attachedModels = entity.GetSharedData(typeof(Dictionary<string, AttachableModel>)) as Dictionary<string, AttachableModel>;
            this.syncedModels = entity.GetSharedData(typeof(Dictionary<string, Model>)) as Dictionary<string, Model>;
            this.animationPlayer = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;

            animationPlayer.StartClip(animationPlayer.skinningDataValue.AnimationClips.Keys.First(), MixType.None);

            modelParams = new SharedGraphicsParams();
            modelParams.size = drawScale;
            entity.AddSharedData(typeof(SharedGraphicsParams), modelParams);

        }

        bool drawMainModel = true;
        public void TurnOffMainModel()
        {
            drawMainModel = false;
        }

        public ParticleEmitter AddEmitter(Type particleType, string systemName, float particlesPerSecond, int maxOffset, Vector3 offsetFromCenter, string attachBoneName)
        {
            return AddEmitter(particleType, systemName, particlesPerSecond, maxOffset, offsetFromCenter, model.Bones[attachBoneName].Index - 2);
        }

        public ParticleEmitter AddEmitter(Type particleType, string systemName, float particlesPerSecond, int maxOffset, Vector3 offsetFromCenter, int attachIndex)
        {
            ParticleEmitter toAdd = new ParticleEmitter((Game.Services.GetService(typeof(ParticleManager)) as ParticleManager).GetSystem(particleType), particlesPerSecond, physicalData.Position, maxOffset, offsetFromCenter, attachIndex);
            if (!emitters.ContainsKey(systemName))
            {
                emitters.Add(systemName, toAdd);
            }
            else
            {
                emitters[systemName] = toAdd;
            }

            return toAdd;
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

            Matrix conglomeration = Matrix.CreateScale(new Vector3(modelParams.size));
            conglomeration *= Matrix.CreateTranslation(localOffset);
            conglomeration *= rot;
            conglomeration *= Matrix.CreateTranslation(physicalData.Position);

            animationPlayer.Update(gameTime.ElapsedGameTime, true, conglomeration);


            List<string> toRemove = new List<string>();
            foreach (KeyValuePair<string, ParticleEmitter> k in emitters)
            {
                if (k.Value.BoneIndex < 0)
                {
                    k.Value.Update(gameTime, physicalData.Position, rot);
                }
                else
                {
                    Vector3 bonePos = animationPlayer.GetWorldTransforms()[k.Value.BoneIndex].Translation;
                    k.Value.Update(gameTime, bonePos, rot);
                }

                if (k.Value.Dead)
                {
                    toRemove.Add(k.Key);
                }
            }

            for (int i = toRemove.Count - 1; i >= 0; --i)
            {
                emitters.Remove(toRemove[i]);
            }
        }

        private int lastLightUpdate = 0;
        public override void Draw(GameTime gameTime, CameraComponent camera, bool edgeDetection)
        {
            if (InsideCameraBox(camera.CameraBox))
            {
                Matrix[] bones = animationPlayer.GetSkinTransforms();

                Matrix view = camera.View;
                Matrix projection = camera.Projection;
                int currentLightUpdate = camera.LastLightUpdate;

                if (model != null && drawMainModel)
                {
                    //drawing with toon shader
                    foreach (ModelMesh mesh in model.Meshes)
                    {
                        foreach (CustomSkinnedEffect effect in mesh.Effects)
                        {
                            effect.Parameters["alpha"].SetValue(modelParams.alpha);
                            effect.Parameters["lineIntensity"].SetValue(modelParams.lineIntensity);
                            effect.CurrentTechnique = effect.Techniques[edgeDetection ? "NormalDepth" : "Toon"];
                            effect.SetBoneTransforms(bones);
                            if (lastLightUpdate != currentLightUpdate)
                            {
                                effect.LightPositions = camera.lightPositions;
                                effect.LightColors = camera.lightColors;
                            }

                            effect.View = view;
                            effect.Projection = projection;
                        }

                        mesh.Draw();
                    }
                }

                //drawing armor (weighted to the same bones as kazgar's animation
                if (syncedModels != null)
                {
                    foreach (Model m in syncedModels.Values)
                    {
                        foreach (ModelMesh mesh in m.Meshes)
                        {
                            foreach (CustomSkinnedEffect effect in mesh.Effects)
                            {
                                effect.Parameters["alpha"].SetValue(modelParams.alpha);
                                effect.Parameters["lineIntensity"].SetValue(modelParams.lineIntensity);
                                effect.CurrentTechnique = effect.Techniques[edgeDetection ? "NormalDepth" : "Toon"];
                                effect.SetBoneTransforms(bones);
                                if (lastLightUpdate != currentLightUpdate)
                                {
                                    effect.LightPositions = camera.lightPositions;
                                    effect.LightColors = camera.lightColors;
                                }

                                effect.View = view;
                                effect.Projection = projection;
                            }

                            mesh.Draw();
                        }
                    }
                }

                //drawing attachables
                if (attachedModels != null)
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
                                    Matrix world =
                                        Matrix.CreateFromYawPitchRoll(a.xRotation, a.yRotation, 0)
                                        * transforms[mesh.ParentBone.Index]
                                        * worldbones[model.Bones[a.otherBoneName].Index - 2];

                                    effect.Parameters["World"].SetValue(world);
                                    effect.Parameters["ViewProj"].SetValue(view * projection);
                                    effect.Parameters["InverseWorld"].SetValue(Matrix.Invert(world));

                                    if (lastLightUpdate != currentLightUpdate)
                                    {
                                        effect.Parameters["lightPositions"].SetValue(camera.lightPositions);
                                        effect.Parameters["lightColors"].SetValue(camera.lightColors);
                                    }

                                }
                                mesh.Draw();
                            }
                        }
                    }
                }

                lastLightUpdate = currentLightUpdate;
            }
        }

        public AnimationClip GetAnimationClip(string clipName)
        {
            return animationPlayer.skinningDataValue.AnimationClips[clipName];
        }

    }
}
