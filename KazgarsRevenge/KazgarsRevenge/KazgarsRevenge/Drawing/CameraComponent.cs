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
        #region Camera Fields
        private float fov = MathHelper.PiOver4;
        private float nearPlane = 10f;
        private float farPlane = 8000;

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

        float nextYaw = MathHelper.PiOver4;
        float yaw = MathHelper.PiOver4;
        float pitch = -MathHelper.PiOver4;
        public float zoom = 4.0f;
        private float maxZoom = 10;
        private float minZoom = 2f;
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
            lightColors = new Vector3[MAX_ACTIVE_LIGHTS];
            for (int i = 0; i < MAX_ACTIVE_LIGHTS; ++i)
            {
                lightPositions[i] = inactiveLightPos;
                lightColors[i] = Color.White.ToVector3();
            }

            rand = RandSingleton.U_Instance;
        }

        public override void Initialize()
        {
            physics = Game.Services.GetService(typeof(Space)) as Space;

            proj = Matrix.CreatePerspectiveFieldOfView(fov, Game.GraphicsDevice.Viewport.AspectRatio, nearPlane, farPlane);

            rot = Matrix.CreateRotationX(pitch) * Matrix.CreateRotationY(yaw);
            rotatedTarget = Vector3.Transform(new Vector3(0, 0, -1), rot);
            rotatedUpVector = Vector3.Transform(new Vector3(0, 1, 0), rot);

            int sheight = Game.GraphicsDevice.Viewport.Height;
            int swidth = Game.GraphicsDevice.Viewport.Width;

            LastLightUpdate = int.MinValue;
        }

        double shakeTimer = -1;
        float shakeMagnitude = 1;
        public void ShakeCamera(float magnitude)
        {
            shakeMagnitude = magnitude;
            shakeTimer = 500;
        }

        double lightUpdateCounter;
        Random rand;
        MouseState curMouse = Mouse.GetState();
        MouseState prevMouse = Mouse.GetState();

        KeyboardState curKeys = Keyboard.GetState();
        KeyboardState prevKeys = Keyboard.GetState();
        public override void Update(GameTime gameTime)
        {
            //update bounding boxes for AI and Lights
            Vector3 characterPos = Vector3.Zero;
            if (physicalData != null)
            {
                characterPos = physicalData.Position;
            }
            PlayerLightPos = new Vector3(characterPos.X + 20, 50, characterPos.Z + 20);

            Vector3 min = new Vector3(characterPos.X - playerSightRadius, 0, characterPos.Z - playerSightRadius);
            Vector3 max = new Vector3(characterPos.X + playerSightRadius, 100, characterPos.Z + playerSightRadius);
            this.CameraBox = new BoundingBox(min, max);

            min = new Vector3(characterPos.X - lightSensingRadius, 0, characterPos.Z - lightSensingRadius);
            max = new Vector3(characterPos.X + lightSensingRadius, 100, characterPos.Z + lightSensingRadius);
            this.LightSensingBox = new BoundingBox(min, max);

            min = new Vector3(characterPos.X - aiUpdateRadius, 0, characterPos.Z - aiUpdateRadius);
            max = new Vector3(characterPos.X + aiUpdateRadius, 100, characterPos.Z + aiUpdateRadius);
            this.AIBox = new BoundingBox(min, max);

            //update input
            curMouse = Mouse.GetState();
            curKeys = Keyboard.GetState();
            if (Game.IsActive)
            {

                double elapsedMillis = gameTime.ElapsedGameTime.TotalMilliseconds;
                float amount = (float)elapsedMillis / 1000.0f;

                //zoom based on scrollwheel
                if (curMouse.ScrollWheelValue < prevMouse.ScrollWheelValue)
                {
                    zoom *= 1.2f;
                }
                else if (curMouse.ScrollWheelValue > prevMouse.ScrollWheelValue)
                {
                    zoom /= 1.2f;
                }
                if (zoom < minZoom)
                {
                    zoom = minZoom;
                }
                if (zoom > maxZoom)
                {
                    zoom = maxZoom;
                }

                
                //yaw rotating
                /*if (curKeys.IsKeyDown(Keys.Right) && prevKeys.IsKeyUp(Keys.Right))
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
                }*/

            }

            //updating yaw rotation
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

            //camera shake
            shakeTimer -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (shakeTimer > 0)
            {
                target += new Vector3((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble()) * shakeMagnitude;
            }

            position = target + rot.Backward * distanceFromTarget;
            view = Matrix.CreateLookAt(position, target, rotatedUpVector);

            inverseViewProj = Matrix.Invert(view * proj);

            lightUpdateCounter -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (lightUpdateCounter <= 0)
            {
                lightUpdateCounter = 5000;
                UpdateLights();
            }
            
            prevMouse = curMouse;
            prevKeys = curKeys;




        }

        #region Lights
        public Vector3 PlayerLightPos { get; private set; }

        public const int MAX_ACTIVE_LIGHTS = 30;
        public int LastLightUpdate { get; private set; }
        public Vector3[] lightPositions { get; private set; }
        public Vector3[] lightColors { get; private set; }
        Space physics;
        private Vector3 inactiveLightPos = new Vector3(-10000, 0, 0);

        float aiUpdateRadius = 600;
        public BoundingBox AIBox { get; private set; }

        float playerSightRadius = 800;
        public BoundingBox CameraBox { get; private set; }

        float lightSensingRadius = 1500;
        BoundingBox LightSensingBox;
        private void UpdateLights()
        {
            //check which lights are in camera cube, add to lights
            int i = 0;
            
            var entries = Resources.GetBroadPhaseEntryList();
            physics.BroadPhase.QueryAccelerator.GetEntries(LightSensingBox, entries);
            foreach (BroadPhaseEntry entry in entries)
            {
                GameEntity other = entry.Tag as GameEntity;
                if (other != null && other.Name == "light")
                {
                    Vector3 pos = (other.GetSharedData(typeof(Entity)) as Entity).Position;
                    lightColors[i] = ((Color)other.GetSharedData(typeof(Color))).ToVector3();
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

            //tell effects to update
            ++LastLightUpdate;
        }

        #endregion
    }
}
