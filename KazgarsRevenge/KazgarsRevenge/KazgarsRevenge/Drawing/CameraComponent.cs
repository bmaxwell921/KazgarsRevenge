using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BEPUphysics;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    class CameraComponent : GameComponent
    {
        #region fields
        private Entity physicalData;

        private Matrix proj;
        public Matrix Projection { get { return proj; } }

        private Matrix view;
        public Matrix View { get { return view; } }

        private Vector3 position;
        public Vector3 Position { get { return position; } }

        private Vector3 target;
        public Vector3 Target { get { return target; } }

        private Matrix rot;
        private Vector3 rotatedTarget;
        private Vector3 rotatedUpVector;

        private Vector3 headOffset = new Vector3(0, 1, 0);

        float distanceFromTarget = 10.0f;

        float yaw = 0;
        float pitch = -MathHelper.PiOver4;
        float zoom = 1.0f;
        #endregion

        public CameraComponent(MainGame game)
            : base(game)
        {
        }

        public override void  Initialize()
        {
            proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Game.GraphicsDevice.Viewport.AspectRatio, .3f, 1000.0f);

            rot = Matrix.CreateRotationX(pitch) * Matrix.CreateRotationY(yaw);
            rotatedTarget = Vector3.Transform(new Vector3(0, 0, -1), rot);
            rotatedUpVector = Vector3.Transform(new Vector3(0, 1, 0), rot);

            int sheight = Game.GraphicsDevice.Viewport.Height;
            int swidth = Game.GraphicsDevice.Viewport.Width;
        }

        MouseState curMouse = Mouse.GetState();
        MouseState prevMouse = Mouse.GetState();
        public override void Update(GameTime gameTime)
        {
            curMouse = Mouse.GetState();
            if (Game.IsActive)
            {
                double elapsedMillis = gameTime.ElapsedGameTime.TotalMilliseconds;
                float amount = (float)elapsedMillis / 1000.0f;

                if (curMouse.ScrollWheelValue < prevMouse.ScrollWheelValue)
                {
                    zoom *= 1.2f;
                }
                else if (curMouse.ScrollWheelValue > prevMouse.ScrollWheelValue)
                {
                    zoom /= 1.2f;
                }
                if (zoom < .01f)
                {
                    zoom = .01f;
                }
                if (zoom > 3)
                {
                    zoom = 3;
                }
            }

            distanceFromTarget = 50 * zoom;

            //camera just sits at zero until given a collidable reference to follow
            if (physicalData == null)
            {
                target = headOffset;
            }
            else
            {
                target = physicalData.Position + headOffset;
            }
            position = target + rot.Backward * distanceFromTarget;

            view = Matrix.CreateLookAt(position, target, rotatedUpVector);

            prevMouse = curMouse;
        }
    }
}
