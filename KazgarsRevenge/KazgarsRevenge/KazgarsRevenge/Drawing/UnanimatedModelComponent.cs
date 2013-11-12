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
        private Entity physicalData;
        private Vector3 drawScale;
        private Vector3 localOffset;
        private Matrix rotOffset;
        public UnanimatedModelComponent(MainGame game, GameEntity entity, Model model, Entity physicalData, Vector3 drawScale, Vector3 localOffset, Matrix rotOffset)
            : base(game, entity)
        {
            this.model = model;
            this.physicalData = physicalData;
            this.drawScale = drawScale;
            this.localOffset = localOffset;
            this.rotOffset = rotOffset;
        }

        public override void Draw(GameTime gameTime, Matrix view, Matrix projection, bool edgeDetection)
        {
            Matrix3X3 bepurot = physicalData.OrientationMatrix;
            Matrix rot = new Matrix(bepurot.M11, bepurot.M12, bepurot.M13, 0, bepurot.M21, bepurot.M22, bepurot.M23, 0, bepurot.M31, bepurot.M32, bepurot.M33, 0, 0, 0, 0, 1);

            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.CurrentTechnique = effect.Techniques[edgeDetection? "NormalDepth" : "Toon"];
                    Matrix world = transforms[mesh.ParentBone.Index]
                        * rotOffset
                        * rot
                        * Matrix.CreateScale(drawScale)
                        * Matrix.CreateTranslation(physicalData.Position + localOffset);
                    effect.Parameters["World"].SetValue(world);
                    effect.Parameters["ViewProj"].SetValue(view * projection);
                    effect.Parameters["InverseWorld"].SetValue(Matrix.Invert(world));
                }
                mesh.Draw();
            }
        }
    }
}
