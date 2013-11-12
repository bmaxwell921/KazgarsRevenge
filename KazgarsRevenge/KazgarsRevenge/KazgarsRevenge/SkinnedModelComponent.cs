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
    class SkinnedModelComponent : DrawableComponent3D
    {
        //components
        protected Entity physicalData;

        //fields
        protected Model model;
        protected Vector3 drawScale = new Vector3(1);
        protected Vector3 localOffset = Vector3.Zero;
        protected Matrix yawOffset;

        public SkinnedModelComponent(MainGame game, GameEntity entity, Entity physicalData, Model model, float yaw)
            : base(game, entity)
        {
            this.physicalData = physicalData;
            this.model = model;
            this.drawScale = new Vector3(10);
            this.localOffset = Vector3.Zero;

            this.yawOffset = Matrix.CreateFromYawPitchRoll(yaw, 0, 0);
        }

        public override void Start()
        {

        }

        public override void Draw(GameTime gameTime, Matrix view, Matrix projection, bool edgeDetection)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            //drawing with toon shader
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (CustomSkinnedEffect effect in mesh.Effects)
                {
                    effect.CurrentTechnique = effect.Techniques[edgeDetection ? "NormalDepth" : "Toon"];
                    effect.SetBoneTransforms(transforms);

                    effect.View = view;
                    effect.Projection = projection;
                }

                mesh.Draw();
            }
        }
    }
}
