using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.ResourceManagement;

namespace KazgarsRevenge
{
    public class CameraComponent : GameComponent
    {
        public const int MAX_ACTIVE_LIGHTS = 30;
        #region camera-ish fields
        private float fov = MathHelper.PiOver4;
        private float nearPlane = .1f;
        private float farPlane = 10000;

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

        float nextYaw = 0;
        float yaw = 0;
        float pitch = -MathHelper.PiOver4;
        public float zoom = 4.0f;
        #endregion

        #region rendertarget stuff
        public GBufferTarget RenderTargets { get; private set; }
        public RenderTarget2D OutputTarget { get; private set; }
        #endregion

        #region lights
        public Vector3[] lightPositions{get; private set;}
        private Vector3 inactiveLightPos = new Vector3(-10000, 0, 0);
        #endregion

        public void AssignEntity(Entity followMe)
        {
            this.physicalData = followMe;
        }

        public CameraComponent(MainGame game)
            : base(game)
        {
            this.UpdateOrder = 2;

            lightPositions = new Vector3[MAX_ACTIVE_LIGHTS];
            for (int i = 0; i < MAX_ACTIVE_LIGHTS; ++i)
            {
                lightPositions[i] = inactiveLightPos;
            }
        }

        public override void Initialize()
        {
            physics = Game.Services.GetService(typeof(Space)) as Space;

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

        KeyboardState curKeys = Keyboard.GetState();
        KeyboardState prevKeys = Keyboard.GetState();
        public override void Update(GameTime gameTime)
        {
            curMouse = Mouse.GetState();
            curKeys = Keyboard.GetState();
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
                if (zoom > 8)
                {
                    //zoom = 8;
                }

                if (curKeys.IsKeyDown(Keys.Right) && prevKeys.IsKeyUp(Keys.Right))
                {
                    nextYaw += MathHelper.PiOver2;
                    if (nextYaw > MathHelper.Pi * 2)
                    {
                        nextYaw -= MathHelper.Pi * 2;
                    }
                }
                else if (curKeys.IsKeyDown(Keys.Left) && prevKeys.IsKeyUp(Keys.Left))
                {


                    nextYaw -= MathHelper.PiOver2;
                    if (nextYaw < 0)
                    {
                        nextYaw += MathHelper.Pi * 2;
                    }
                }

            }

            if (yaw != nextYaw)
            {
                float add = .05f;
                float diff = yaw - nextYaw;
                if (Math.Abs(diff) < .051f)
                {
                    yaw = nextYaw;
                }
                else
                {
                    if (diff > 0 && diff < MathHelper.Pi || diff < 0 && -diff > MathHelper.Pi)
                    {
                        add *= -1;
                    }
                    yaw += add;
                    if (yaw > MathHelper.TwoPi)
                    {
                        yaw -= MathHelper.TwoPi;
                    }
                    else if (yaw < 0)
                    {
                        yaw += MathHelper.TwoPi;
                    }
                }


                rot = Matrix.CreateRotationX(pitch) * Matrix.CreateRotationY(yaw);
                rotatedTarget = Vector3.Transform(new Vector3(0, 0, -1), rot);
                rotatedUpVector = Vector3.Transform(new Vector3(0, 1, 0), rot);
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

            inverseViewProj = Matrix.Invert(view * proj);


            UpdateLights();
            
            prevMouse = curMouse;
            prevKeys = curKeys;




        }

        float playerSightRadius = 700;
        Space physics;
        private void UpdateLights()
        {
            //check which lights are in camera cube, add to lights
            int i = 0;

            Vector3 min = new Vector3(position.X - playerSightRadius, 0, position.Z - playerSightRadius);
            Vector3 max = new Vector3(position.X + playerSightRadius, 20, position.Z + playerSightRadius);
            BoundingBox b = new BoundingBox(min, max);

            var entries = Resources.GetBroadPhaseEntryList();
            physics.BroadPhase.QueryAccelerator.GetEntries(b, entries);
            foreach (BroadPhaseEntry entry in entries)
            {
                GameEntity other = entry.Tag as GameEntity;
                if (other != null && other.Name == "light")
                {
                    Vector3 pos = (other.GetSharedData(typeof(Entity)) as Entity).Position;
                    lightPositions[i++] = pos;
                    if (i >= MAX_ACTIVE_LIGHTS)
                    {
                        break;
                    }
                }
            }

            //add inactive lights to fill rest of array up, if it's not full
            while (i < MAX_ACTIVE_LIGHTS)
            {
                lightPositions[i++] = inactiveLightPos;
            }

            //update effects?

        }
    }
}
