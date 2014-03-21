﻿using System;
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


        MouseState curMouse = Mouse.GetState();
        MouseState prevMouse = Mouse.GetState();

        KeyboardState curKeys = Keyboard.GetState();
        KeyboardState prevKeys = Keyboard.GetState();
        public override void Update(GameTime gameTime)
        {
            Vector3 characterPos = Vector3.Zero;
            if (physicalData != null)
            {
                characterPos = physicalData.Position;
            }

            Vector3 min = new Vector3(characterPos.X - playerSightRadius, 0, characterPos.Z - playerSightRadius);
            Vector3 max = new Vector3(characterPos.X + playerSightRadius, 100, characterPos.Z + playerSightRadius);
            this.CameraBox = new BoundingBox(min, max);
            

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
                if (zoom < minZoom)
                {
                    zoom = minZoom;
                }
                if (zoom > maxZoom)
                {
                    zoom = maxZoom;
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

        #region Lights
        public const int MAX_ACTIVE_LIGHTS = 20;
        public int LastLightUpdate { get; private set; }
        public BoundingBox CameraBox { get; private set; }
        public Vector3[] lightPositions { get; private set; }
        public Vector3[] lightColors { get; private set; }
        float playerSightRadius = 1000;
        Space physics;
        private Vector3 inactiveLightPos = new Vector3(-10000, 0, 0);
        private void UpdateLights()
        {
            //check which lights are in camera cube, add to lights
            int i = 0;
            
            var entries = Resources.GetBroadPhaseEntryList();
            physics.BroadPhase.QueryAccelerator.GetEntries(CameraBox, entries);
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

            //update effects?
            ++LastLightUpdate;
        }

        #endregion
    }
}
