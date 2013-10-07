using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.MathExtensions;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Collidables;
using SkinnedModelLib;

namespace KazgarsRevenge
{
    class PlayerInputComponent : DrawableComponent2D
    {
        Entity physicalData;
        Space physics;
        CameraComponent camera;
        GameEntity mouseHoveredEntity;
        HealthComponent mouseHoveredHealth;
        EntityManager entities;
        AnimationPlayer animations;
        public void PlayAnimation(string animationName)
        {
            animations.StartClip(animations.skinningDataValue.AnimationClips[animationName]);
        }

        float maxSpeed = 20;

        public PlayerInputComponent(MainGame game, Entity physicalData, AnimationPlayer animations)
            : base(game)
        {
            this.physicalData = physicalData;
            physicalData.LocalInertiaTensorInverse = new BEPUphysics.MathExtensions.Matrix3X3();
            physics = Game.Services.GetService(typeof(Space)) as Space;
            rayCastFilter = RayCastFilter;
            camera = game.Services.GetService(typeof(CameraComponent)) as CameraComponent;
            entities = game.Services.GetService(typeof(EntityManager)) as EntityManager;
            this.animations = animations;
            texWhitePixel = Game.Content.Load<Texture2D>("Textures\\whitePixel");
            font = game.Content.Load<SpriteFont>("Georgia");
        }

        public override void Start()
        {
        }

        Func<BroadPhaseEntry, bool> rayCastFilter;
        bool RayCastFilter(BroadPhaseEntry entry)
        {
            return entry != physicalData.CollisionInformation
                && entry.CollisionRules.Personal <= CollisionRule.Normal
                && (entry as StaticMesh) == null;
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
                int xmid = Game.GraphicsDevice.Viewport.Width / 2;
                int ymid = Game.GraphicsDevice.Viewport.Height / 2;

                Vector3 castOrigin = Game.GraphicsDevice.Viewport.Unproject(new Vector3(curMouse.X, curMouse.Y, 0), camera.Projection, camera.View, Matrix.Identity);
                Vector3 castdir = Game.GraphicsDevice.Viewport.Unproject(new Vector3(curMouse.X, curMouse.Y, 1), camera.Projection, camera.View, Matrix.Identity) - castOrigin;
                castdir.Normalize();

                RayCastResult result;
                physics.RayCast(new Ray(castOrigin, castdir), 200, rayCastFilter, out result);

                if (result.HitObject != null)
                {
                    mouseHoveredEntity = result.HitObject.Tag as GameEntity;
                    mouseHoveredHealth = mouseHoveredEntity.GetComponent(typeof(HealthComponent)) as HealthComponent;
                }
                else
                {
                    mouseHoveredEntity = null;
                    mouseHoveredHealth = null;
                }


                //moving
                Vector3 move = new Vector3(curMouse.X - xmid, 0, curMouse.Y - ymid);
                move.Normalize();
                if (curMouse.LeftButton == ButtonState.Pressed)
                {
                    CalculateRotation(move);

                    physicalData.LinearVelocity = move * maxSpeed;

                    if (prevMouse.LeftButton == ButtonState.Released)
                    {
                        PlayAnimation("Walk");
                    }

                }
                else
                {
                    physicalData.LinearVelocity = new Vector3(0, physicalData.LinearVelocity.Y, 0);
                    if (prevMouse.LeftButton == ButtonState.Pressed)
                    {
                        PlayAnimation("Idle");
                    }
                }

                if (curMouse.RightButton == ButtonState.Pressed)
                {
                    CalculateRotation(move);
                }


                //abilities
                if (curMouse.RightButton == ButtonState.Pressed && prevMouse.RightButton == ButtonState.Released)
                {
                    Matrix rot = Matrix.CreateFromQuaternion(physicalData.Orientation);
                    entities.CreateArrow(physicalData.Position + rot.Forward * 6, rot.Forward, 25);
                }
            }
            else
            {
                physicalData.LinearVelocity = new Vector3(0, physicalData.LinearVelocity.Y, 0);
            }

            prevMouse = curMouse;
            prevKeys = curKeys;
        }
        
        private void CalculateRotation(Vector3 move)
        {
            //orientation
            float yaw = (float)Math.Atan(move.X / move.Z);
            if (move.Z < 0 && move.X >= 0
                || move.Z < 0 && move.X < 0)
            {
                //TODO
                yaw += MathHelper.Pi;
            }
            physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(yaw, 0, 0);
        }


        //questionable part of this component's design
        SpriteFont font;
        Texture2D texWhitePixel;
        Rectangle RectEnemyHealthBar;
        Vector2 vecName;
        Vector2 mid;
        private void InitDrawingParams()
        {
            mid = new Vector2(Game.GraphicsDevice.Viewport.Width / 2, Game.GraphicsDevice.Viewport.Height / 2);
            RectEnemyHealthBar = new Rectangle((int)mid.X - 75, 40, 150, 20);
            vecName = new Vector2((int)mid.X - 75, 10);
        }
        public override void Draw(SpriteBatch s)
        {
            if (mouseHoveredEntity != null)
            {
                s.DrawString(font, mouseHoveredEntity.Name, vecName, Color.White);
            }
            if(mouseHoveredHealth != null)
            {
                s.Draw(texWhitePixel, RectEnemyHealthBar, Color.Red);
                s.Draw(texWhitePixel, new Rectangle(RectEnemyHealthBar.X, RectEnemyHealthBar.Y, (int)(RectEnemyHealthBar.Width * mouseHoveredHealth.HealthPercent), RectEnemyHealthBar.Height), Color.Green);
            }
        }
    }
}
