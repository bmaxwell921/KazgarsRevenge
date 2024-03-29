﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.MathExtensions;

namespace KazgarsRevenge
{
    class UnanimatedModelComponent : DrawableComponent3D
    {
        private Model model;
        private Vector3 localOffset;

        private Matrix currentRot = Matrix.Identity;


        protected SharedGraphicsParams modelParams;



        /// <summary>
        /// constructs a new model component for rendering models without animations
        /// </summary>
        public UnanimatedModelComponent(KazgarsRevengeGame game, GameEntity entity, Model model, Vector3 drawScale, Vector3 localOffset, float yaw, float pitch, float roll)
            : base(game, entity)
        {
            this.model = model;
            this.localOffset = localOffset;

            modelParams = new SharedGraphicsParams();
            modelParams.size = drawScale;
            entity.AddSharedData(typeof(SharedGraphicsParams), modelParams);

            this.yaw = yaw;
            this.pitch = pitch;
            this.roll = roll;

            Matrix3X3 bepurot = physicalData.OrientationMatrix;
            rotation = new Matrix(bepurot.M11, bepurot.M12, bepurot.M13, 0, bepurot.M21, bepurot.M22, bepurot.M23, 0, bepurot.M31, bepurot.M32, bepurot.M33, 0, 0, 0, 0, 1);
        }

        /// <summary>
        /// constructs an empty model component, basically just for rendering particles
        /// </summary>
        public UnanimatedModelComponent(KazgarsRevengeGame game, GameEntity entity)
            :base(game, entity)
        {
            this.model = null;
        }

        public void TurnOffOutline()
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["lineIntensity"].SetValue(0);
                }
            }
        }

        public void AddColorTint(Color tint)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["colorTint"].SetValue(tint.ToVector3());
                }
            }
        }

        float yawPerFrame = 0;
        float rollPerFrame = 0;

        float yaw;
        float pitch;
        float roll;
        public void AddRollSpeed(float roll)
        {
            this.rollPerFrame = roll;
        }

        public void AddYawSpeed(float yaw)
        {
            this.yawPerFrame = yaw;
        }

        public void SetAlpha(float alpha)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["alpha"].SetValue(alpha);
                }
            }
        }

        bool animated = false;
        public void SlideAnimateTexture(float pixelsPerSec)
        {
            animated = true;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["slidePerSec"].SetValue(pixelsPerSec);
                }
            }
        }

        Matrix rotation = Matrix.Identity;
        public override void Update(GameTime gameTime)
        {
            roll += rollPerFrame;
            yaw += yawPerFrame;

            this.currentRot = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);

            Matrix3X3 bepurot = physicalData.OrientationMatrix;
            rotation = new Matrix(bepurot.M11, bepurot.M12, bepurot.M13, 0, bepurot.M21, bepurot.M22, bepurot.M23, 0, bepurot.M31, bepurot.M32, bepurot.M33, 0, 0, 0, 0, 1);

            List<string> toRemove = new List<string>();
            foreach (KeyValuePair<string, ParticleEmitter> k in emitters)
            {
                k.Value.Update(gameTime, physicalData.Position, rotation);
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
            if (model != null && InsideCameraBox(camera.CameraBox))
            {
                Matrix view = camera.View;
                Matrix projection = camera.Projection;
                int currentLightUpdate = camera.LastLightUpdate;

                Matrix[] transforms = new Matrix[model.Bones.Count];
                model.CopyAbsoluteBoneTransformsTo(transforms);
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (Effect effect in mesh.Effects)
                    {
                        if (animated)
                        {
                            effect.Parameters["CurrentTime"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
                        }
                        effect.Parameters["playerLightPosition"].SetValue(camera.PlayerLightPos);
                        effect.CurrentTechnique = effect.Techniques[edgeDetection ? "NormalDepth" : "Toon"];
                        Matrix world = transforms[mesh.ParentBone.Index]
                            * Matrix.CreateScale(modelParams.size)
                            * Matrix.CreateTranslation(localOffset)
                            * currentRot
                            * rotation
                            * Matrix.CreateTranslation(physicalData.Position);
                        effect.Parameters["World"].SetValue(world);
                        effect.Parameters["ViewProj"].SetValue(view * projection);
                        effect.Parameters["InverseWorld"].SetValue(Matrix.Invert(world));
                        if (lastLightUpdate != currentLightUpdate)
                        {
                            effect.Parameters["lightPositions"].SetValue(camera.lightPositions);
                            effect.Parameters["lightColors"].SetValue(camera.lightColors);
                            effect.Parameters["ambient"].SetValue(camera.AmbientLight);

                        }
                    }
                    mesh.Draw();
                }

                lastLightUpdate = currentLightUpdate;
            }
        }
    }
}
