using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    class AnimatedModelComponent : Component
    {
        public override void Update(Microsoft.Xna.Framework.GameTime gametime)
        {
            animationPlayer.Update(gameTime.ElapsedGameTime, true,
                drawRotation * Matrix.CreateScale(drawScale) * Matrix.CreateTranslation(Position + localOffset));


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
