using System;
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
        private Vector3 drawScale;
        private Vector3 localOffset;

        private Matrix currentRot = Matrix.Identity;
        public UnanimatedModelComponent(KazgarsRevengeGame game, GameEntity entity, Model model, Vector3 drawScale, Vector3 localOffset, float xRot, float yRot, float zRot)
            : base(game, entity)
        {
            this.model = model;
            this.drawScale = drawScale;
            this.localOffset = localOffset;

            this.yaw = xRot;
            this.pitch = yRot;
            this.roll = zRot;
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

        float rollPerFrame = 0;

        float yaw;
        float pitch;
        float roll;
        public void AddRollSpeed(float roll)
        {
            this.rollPerFrame = roll;
        }

        float alpha = 1;
        public void SetAlpha(float alpha)
        {
            this.alpha = alpha;
        }

        Matrix rotation = Matrix.Identity;
        public override void Update(GameTime gameTime)
        {
            roll += rollPerFrame;

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
                        effect.CurrentTechnique = effect.Techniques[edgeDetection ? "NormalDepth" : "Toon"];
                        effect.Parameters["alpha"].SetValue(alpha);
                        Matrix world = transforms[mesh.ParentBone.Index]
                            * Matrix.CreateScale(drawScale)
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
                        }
                    }
                    mesh.Draw();
                }

                lastLightUpdate = currentLightUpdate;
            }
        }
    }
}
