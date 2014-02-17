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
        private Matrix rotOffset;
        public UnanimatedModelComponent(KazgarsRevengeGame game, GameEntity entity, Model model, Vector3 drawScale, Vector3 localOffset, Matrix rotOffset)
            : base(game, entity)
        {
            this.model = model;
            this.drawScale = drawScale;
            this.localOffset = localOffset;
            this.rotOffset = rotOffset;
        }

        Matrix rotation = Matrix.Identity;
        public override void Update(GameTime gameTime)
        {
            Matrix3X3 bepurot = physicalData.OrientationMatrix;
            rotation = new Matrix(bepurot.M11, bepurot.M12, bepurot.M13, 0, bepurot.M21, bepurot.M22, bepurot.M23, 0, bepurot.M31, bepurot.M32, bepurot.M33, 0, 0, 0, 0, 1);


            List<Type> toRemove = new List<Type>();
            foreach (KeyValuePair<Type, ParticleEmitter> k in emitters)
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

        EffectParameter paramWorld;
        EffectParameter paramViewProj;
        EffectParameter paramInverseWorld;
        EffectParameter paramLightPositions;
        EffectParameter paramLightColors;

        

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
                        Matrix world = transforms[mesh.ParentBone.Index]
                            * Matrix.CreateScale(drawScale)
                            * Matrix.CreateTranslation(localOffset)
                            * rotOffset
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


                        /*
                        Matrix world = transforms[mesh.ParentBone.Index]
                            * Matrix.CreateScale(drawScale)
                            * Matrix.CreateTranslation(localOffset)
                            * rotOffset
                            * rotation
                            * Matrix.CreateTranslation(physicalData.Position);

                        effect.Parameters["World"].SetValue(world);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);
                        */
                    }
                    mesh.Draw();
                }

                lastLightUpdate = currentLightUpdate;
            }
        }
    }
}
