using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BEPUphysics;
using BEPUphysics.Entities;

namespace KazgarsRevenge
{
    public class CameraComponent : GameComponent
    {
        #region fields
        private float fov = MathHelper.PiOver4;
        private float nearPlane = .1f;
        private float farPlane = 1000;

        private Entity physicalData;

        private Matrix inverseViewProj = Matrix.Identity;
        public Matrix InverseViewProj { get { return inverseViewProj; } }

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

        private Vector3 headOffset = new Vector3(0, 13, 0);

        float distanceFromTarget = 200.0f;

        float yaw = 0;
        float pitch = -MathHelper.PiOver4;
        public float zoom = 4.0f;

        public GBufferTarget RenderTargets { get; private set; }
        public RenderTarget2D OutputTarget { get; private set; }
        #endregion

        public void AssignEntity(Entity followMe)
        {
            this.physicalData = followMe;
        }

        public CameraComponent(MainGame game)
            : base(game)
        {

        }

        public override void  Initialize()
        {
            proj = Matrix.CreatePerspectiveFieldOfView(fov, Game.GraphicsDevice.Viewport.AspectRatio, nearPlane, farPlane);

            rot = Matrix.CreateRotationX(pitch) * Matrix.CreateRotationY(yaw);
            rotatedTarget = Vector3.Transform(new Vector3(0, 0, -1), rot);
            rotatedUpVector = Vector3.Transform(new Vector3(0, 1, 0), rot);

            Game.GraphicsDevice.DeviceReset += new EventHandler<EventArgs>(resetOutputTarget);

            int sheight = Game.GraphicsDevice.Viewport.Height;
            int swidth = Game.GraphicsDevice.Viewport.Width;

            RenderTargets = new GBufferTarget(Game.GraphicsDevice, Game.GraphicsDevice.Viewport);
            OutputTarget = new RenderTarget2D(Game.GraphicsDevice, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);
        }

        void resetOutputTarget(object sender, EventArgs e)
        {
            if (!OutputTarget.IsDisposed)
            {
                OutputTarget.Dispose();
            }
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
                if (zoom < 1f)
                {
                    zoom = 1f;
                }
                //if (zoom > 8)
                //{
                //    zoom = 8;
                //}


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

            inverseViewProj = Matrix.Invert(view * proj);
        }
    }
}
