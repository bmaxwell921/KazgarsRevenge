using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    public class HealthBarBillboard : DrawableComponentBillboard
    {
        AliveComponent owner;
        Entity ownerData;
        float ycoord;
        public HealthBarBillboard(KazgarsRevengeGame game, GameEntity entity, float size, float ycoord)
            : base(game, entity, Vector3.Forward, Vector3.Up, new Vector2(size * 5, size))
        {
            this.ycoord = ycoord;
            effect = (game.Services.GetService(typeof(BillBoardManager)) as BillBoardManager).HealthBarEffect;
        }

        public override void Start()
        {
            this.owner = Entity.GetComponent(typeof(AliveComponent)) as AliveComponent;
            this.ownerData = Entity.GetSharedData(typeof(Entity)) as Entity;

            base.Start();
        }

        float percent = 1;
        public override void Update(GameTime gameTime)
        {
            percent = owner.HealthPercent;
            size.X = originalSize.X * percent;
            source.X = percent;

            origin = ownerData.Position;
            origin.Y = ycoord;
            //adjust left edge to always be in the same place
            origin += (left * originalSize.X / 2) * (1 - percent);

            base.Update(gameTime);
        }

        public override void Draw(CameraComponent camera)
        {
            if (percent < 1 && InsideCameraBox(camera.CameraBox))
            {
                normal = camera.Position - ownerData.Position;
                //normal.Y = 0;
                if (normal != Vector3.Zero)
                {
                    normal.Normalize();
                }
                this.left = Vector3.Cross(normal, up);
                base.Draw(camera);
            }
        }

        protected bool InsideCameraBox(BoundingBox cameraBox)
        {
            Vector3 pos = origin;
            return !(pos.X < cameraBox.Min.X
                || pos.X > cameraBox.Max.X
                || pos.Z < cameraBox.Min.Z
                || pos.Z > cameraBox.Max.Z);
        }
    }
}
