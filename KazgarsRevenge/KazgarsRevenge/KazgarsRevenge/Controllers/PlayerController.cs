﻿using System;
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
    class PlayerController : DrawableComponent2D
    {
        Entity physicalData;
        Space physics;
        CameraComponent camera;
        GameEntity entity;
        AttackManager attacks;
        AnimationPlayer animations;
        Dictionary<string, AttachableModel> attached;
        Random rand;

        Entity targettedPhysicalData;
        
        GameEntity mouseHoveredEntity;
        HealthData mouseHoveredHealth;

        const float stopRadius = 10;
        const float targetResetDistance = 1000;
        Vector3 mouseHoveredLocation = Vector3.Zero;
        Vector3 groundTargetLocation = Vector3.Zero;
        //dont want kazgar to run until first click
        double millisRunningCounter = 2000;
        double millisRunTime = 2000;

        #region Ability Icons
        Texture2D melee;
        Texture2D range;
        Texture2D magic;
        Texture2D placeHolder;
        #endregion

        #region Item Icons
        Texture2D healthPot;
        #endregion

        enum PrimaryAttack
        {
            Melle,
            Ranged,
            Magic,
        }
        PrimaryAttack selectedPrimary = PrimaryAttack.Melle;
        float attackSpeed = 0.05f;
        float maxSpeed = 120;
        const float melleRange = 50;
        const float bowRange = 1000;


        public PlayerController(MainGame game, GameEntity entity, Entity physicalData, AnimationPlayer animations, Dictionary<string, AttachableModel> attached)
            : base(game)
        {
            rand = new Random();
            this.physicalData = physicalData;
            this.entity = entity;
            physics = Game.Services.GetService(typeof(Space)) as Space;
            rayCastFilter = RayCastFilter;
            camera = game.Services.GetService(typeof(CameraComponent)) as CameraComponent;
            attacks = game.Services.GetService(typeof(AttackManager)) as AttackManager;
            this.animations = animations;
            this.attached = attached;
            texWhitePixel = Game.Content.Load<Texture2D>("Textures\\whitePixel");
            font = game.Content.Load<SpriteFont>("Georgia");
            InitDrawingParams();
            PlayAnimation("k_idle1");
            attached.Add("sword", Game.GetAttachable("sword01", "sword", "Bone_001_R_004"));
            attached.Add("bow", Game.GetAttachable("bow01", "sword", "Bone_001_L_004"));

            #region Ability Image Load
            melee = Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\DB");
            range = Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\LW");
            magic = Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\BR");
            placeHolder = Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\BN");
            #endregion

            #region Item Image Load
            healthPot = Game.Content.Load<Texture2D>("Textures\\UI\\Items\\HP");
            #endregion

            millisShotRelease = animations.skinningDataValue.AnimationClips["k_fire_arrow"].Duration.TotalMilliseconds / 2;
            millisMelleDamage = animations.skinningDataValue.AnimationClips["k_onehanded_swing"].Duration.TotalMilliseconds / 2;
        }

        #region animations
        /* animations:
        "k_idle1" - standing still
        "k_idle2" - looking around
        "k_idle3" - flex squat
        "k_idle4" - fancy flex
        "k_idle5" - butt scratch
        "k_onehanded_swing"
        "k_run"
        "k_fighting_stance"
        "k_flip"
        "k_from_fighting_stance" - transition from fighting to idle
         */

        /* Bones:
         "Bone_001_L_004" - left hand
         "Bone_001_R_004" - right hand 
         */
        private enum AttackState
        {
            None,
            GrabbingArrow,
            DrawingString,
            LettingGo,
            InitialSwing,
            FinishSwing,
            CastingSpell,
        }




        private double millisAniCounter;
        private double millisAniDuration;

        private AttackState attState = AttackState.None;
        private double millisShotRelease;
        private double millisShotAniCounter;
        private double millisShotArrowAttachLength = 200;
        private double millisShotArrowAttachCounter;

        private double millisMelleDamage;
        private double millisMelleCounter;

        private string currentAniName;
        public void PlayAnimation(string animationName)
        {
            AnimationClip clip =  animations.skinningDataValue.AnimationClips[animationName];
            animations.StartClip(clip, false);
            if (animationName == "k_idle1")
            {
                millisAniDuration = rand.Next(2000, 4000);
            }
            else if (animationName == "k_fighting_stance")
            {
                millisAniDuration = clip.Duration.TotalMilliseconds * 2;
            }
            else
            {
                millisAniDuration = clip.Duration.TotalMilliseconds;
            }
            millisAniDuration -= 10;
            currentAniName = animationName;
            millisAniCounter = 0;
            millisShotAniCounter = 0;
        }
        #endregion

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
            double elapsed = gameTime.ElapsedGameTime.TotalMilliseconds;
            millisAniCounter += elapsed;
            millisShotAniCounter += elapsed;
            millisShotArrowAttachCounter += elapsed;
            millisMelleCounter += elapsed;
            millisRunningCounter += elapsed;

            if (Game.IsActive)
            {
                if (curMouse.LeftButton == ButtonState.Pressed)
                {
                    millisRunningCounter = 0;
                }
                else
                {
                    if (attState != AttackState.None)
                    {
                        targettedPhysicalData = null;
                    }
                }
                bool newTarget = curMouse.LeftButton == ButtonState.Released || prevMouse.LeftButton == ButtonState.Released || (curMouse.RightButton == ButtonState.Pressed && prevMouse.RightButton == ButtonState.Released);
                foreach (Keys k in abilityKeys)
                {
                    if (curKeys.IsKeyDown(k) && prevKeys.IsKeyUp(k))
                    {
                        newTarget = true;
                        break;
                    }
                }

                CheckMouseRay(newTarget);

                Vector3 move = new Vector3(mouseHoveredLocation.X - physicalData.Position.X, 0, mouseHoveredLocation.Z - physicalData.Position.Z);
                if (targettedPhysicalData != null)
                {
                    move = new Vector3(targettedPhysicalData.Position.X - physicalData.Position.X, 0, targettedPhysicalData.Position.Z - physicalData.Position.Z);
                }
                if (move != Vector3.Zero)
                {
                    move.Normalize();
                }

                if (curMouse.LeftButton == ButtonState.Pressed && mouseHoveredHealth != null && mouseHoveredHealth.Dead)
                {
                    ResetTargettedEntity();
                    targettedPhysicalData = null;
                }
                if (attState == AttackState.None)
                {
                    CheckAbilities(move);
                }

                //targets ground location to start running to if holding mouse button
                if (curMouse.LeftButton == ButtonState.Pressed && attState == AttackState.None)
                {
                    groundTargetLocation = mouseHoveredLocation;
                    if (prevMouse.LeftButton == ButtonState.Released && targettedPhysicalData == null)
                    {
                        attacks.CreateMouseSpikes(groundTargetLocation);
                    }
                }
                Vector3 groundMove = move;
                if (targettedPhysicalData == null)
                {
                    groundMove = new Vector3(groundTargetLocation.X - physicalData.Position.X, 0, groundTargetLocation.Z - physicalData.Position.Z);
                    if (groundMove != Vector3.Zero)
                    {
                        groundMove.Normalize();
                    }
                }

                if (attState == AttackState.None)
                {
                    MoveCharacter(groundMove);
                }
                else
                {
                    ChangeVelocity(new Vector3(0, physicalData.LinearVelocity.Y, 0));
                }


                CheckAnimations();
            }
            else
            {
                ChangeVelocity(new Vector3(0, physicalData.LinearVelocity.Y, 0));
            }

            prevMouse = curMouse;
            prevKeys = curKeys;
        }

        List<Keys> abilityKeys = new List<Keys> { Keys.D1, Keys.D2, Keys.D3, Keys.D4 };
        /// <summary>
        /// creates a raycast from the mouse, and returns the position on the ground that it hits.
        /// Also does a bepu raycast and finds the first enemy it hits, and keeps its healthcomponent
        /// for gui purposes
        /// </summary>
        /// <returns></returns>
        private void CheckMouseRay(bool newTarget)
        {
            //creating ray from mouse location
            Vector3 castOrigin = Game.GraphicsDevice.Viewport.Unproject(new Vector3(curMouse.X, curMouse.Y, 0), camera.Projection, camera.View, Matrix.Identity);
            Vector3 castdir = Game.GraphicsDevice.Viewport.Unproject(new Vector3(curMouse.X, curMouse.Y, 1), camera.Projection, camera.View, Matrix.Identity) - castOrigin;
            castdir.Normalize();
            Ray r = new Ray(castOrigin, castdir);

            //check where on the zero plane the ray hits, to guide the character by the mouse
            float? distance;
            Plane p = new Plane(Vector3.Up, 0);
            r.Intersects(ref p, out distance);
            if (distance.HasValue)
            {
                mouseHoveredLocation = r.Position + r.Direction * distance.Value;
            }


            //if the left mouse button was either just clicked or not pressed down at all, or if any other ability input was just clicked
            if (newTarget)
            {
                //check if ray hits GameEntity if not holding down mouse button
                List<RayCastResult> results = new List<RayCastResult>();
                physics.RayCast(r, 10000, rayCastFilter, results);

                ResetTargettedEntity();
                foreach (RayCastResult result in results)
                {
                    if (result.HitObject != null)
                    {
                        mouseHoveredEntity = result.HitObject.Tag as GameEntity;
                        if (mouseHoveredEntity != null)
                        {
                            mouseHoveredHealth = mouseHoveredEntity.GetHealth();
                            if (mouseHoveredHealth == null)
                            {
                                ResetTargettedEntity();
                            }
                            else if (curMouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
                            {
                                targettedPhysicalData = (mouseHoveredEntity.GetComponent(typeof(PhysicsComponent)) as PhysicsComponent).collidable;
                            }
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// checks player input to see what ability to use
        /// </summary>
        /// <param name="gameTime"></param>
        private void CheckAbilities(Vector3 move)
        {
            Vector3 dir;
            dir.X = move.X;
            dir.Y = move.Y;
            dir.Z = move.Z;
            float distance = 0;

            //switch primary
            if (curKeys.IsKeyDown(Keys.Tab) && prevKeys.IsKeyUp(Keys.Tab))
            {
                selectedPrimary += 1;
                if ((int)selectedPrimary >= 2)
                {
                    selectedPrimary = 0;
                }
            }

            if (targettedPhysicalData != null)
            {
                dir = targettedPhysicalData.Position - physicalData.Position;
                dir.Y = 0;
                distance = (dir).Length();
                dir.Normalize();
            }


            //primary attack (autos)
            if (curMouse.LeftButton == ButtonState.Pressed && curKeys.IsKeyDown(Keys.LeftShift) || targettedPhysicalData != null)
            {
                //attack if in range
                switch (selectedPrimary)
                {
                    case PrimaryAttack.Melle:
                        if (distance < melleRange)
                        {
                            PlayAnimation("k_onehanded_swing");
                            attState = AttackState.InitialSwing;
                            UpdateRotation(dir);
                            millisMelleCounter = 0;
                            mouseHoveredLocation = physicalData.Position;
                            groundTargetLocation = physicalData.Position;
                        }
                        break;
                    case PrimaryAttack.Ranged:
                        if (distance < bowRange)
                        {
                            PlayAnimation("k_fire_arrow");
                            attState = AttackState.GrabbingArrow;
                            UpdateRotation(dir);
                            millisShotAniCounter = 0;
                            millisShotArrowAttachCounter = 0;
                            mouseHoveredLocation = physicalData.Position;
                            groundTargetLocation = physicalData.Position;
                        }
                        break;
                    case PrimaryAttack.Magic:
                        if (distance < bowRange)
                        {
                            PlayAnimation("k_fire_arrow");
                            attState = AttackState.GrabbingArrow;
                            UpdateRotation(dir);
                            millisShotAniCounter = 0;
                            millisShotArrowAttachCounter = 0;
                            mouseHoveredLocation = physicalData.Position;
                            groundTargetLocation = physicalData.Position;
                        }
                        break;
                }
            }
           /* if (curMouse.RightButton == ButtonState.Pressed && curKeys.IsKeyDown(Keys.LeftShift))
            {
                PlayAnimation("k_flip");
                attState = AttackState.InitialSwing;
                UpdateRotation(dir);
                millisMelleCounter = 0;
                mouseHoveredLocation = physicalData.Position;
                groundTargetLocation = physicalData.Position;
                targettedPhysicalData = null;
            }*/

            if (curKeys.IsKeyDown(Keys.D1))
            {
                PlayAnimation("k_flip");
                attState = AttackState.InitialSwing;
                UpdateRotation(dir);
                millisMelleCounter = 0;
                mouseHoveredLocation = physicalData.Position;
                groundTargetLocation = physicalData.Position;
                targettedPhysicalData = null;
            }
        }

        /// <summary>
        /// handles character movement
        /// </summary>
        /// <param name="groundHitPos"></param>
        private void MoveCharacter(Vector3 move)
        {
            //click:      target enemy if ray catches one to move to / attack
            //held down:  path towards mousehovered location (or targetted entity if its data isn't null and it's not in range) / if in range of targetted entity, auto attack
            //up:         if there is a targetted enemy, run up and attack it once. if not, run towards targetted location if not already at it

            Vector3 moveVec = move * maxSpeed;
            moveVec.Y = physicalData.LinearVelocity.Y;
            if (curMouse.LeftButton == ButtonState.Pressed)
            {
                ChangeVelocity(moveVec);

                UpdateRotation(move);
                //just started running
                if (currentAniName != "k_run")
                {
                    PlayAnimation("k_run");
                }
            }
            else if ((physicalData.Position - groundTargetLocation).Length() <= stopRadius || millisRunningCounter >= millisRunTime)
            {
                //stop if within stopRadius of targetted ground location
                ChangeVelocity(new Vector3(0, physicalData.LinearVelocity.Y, 0));
                //just stopped moving
                if (currentAniName == "k_run")
                {
                    PlayAnimation("k_fighting_stance");
                }
            }
            else
            {
                ChangeVelocity(moveVec);
                UpdateRotation(move);
                //just started running
                if (currentAniName != "k_run")
                {
                    PlayAnimation("k_run");
                }
            }
        }


        /// <summary>
        /// handles animation transitions
        /// </summary>
        private void CheckAnimations()
        {
            //if the animation has played through its duration, do stuff
            //(mainly to handle idle animations and fighting stance to idle
            if (millisAniCounter > millisAniDuration)
            {
                switch (currentAniName)
                {
                    case "k_idle1":
                        int aniRand = rand.Next(1, 7);
                        if (aniRand < 4)
                        {
                            PlayAnimation("k_idle2");
                        }
                        else
                        {
                            switch (aniRand)
                            {
                                case 4:
                                    PlayAnimation("k_idle3");
                                    break;
                                case 5:
                                    PlayAnimation("k_idle4");
                                    break;
                                case 6:
                                    PlayAnimation("k_idle5");
                                    break;
                            }
                        }
                        break;
                    case "k_flip":
                    case "k_onehanded_swing":
                    case "k_fire_arrow":
                        PlayAnimation("k_fighting_stance");
                        attState = AttackState.None;
                        break;
                    case "k_fighting_stance":
                        PlayAnimation("k_from_fighting_stance");
                        break;
                    case "k_from_fighting_stance":
                    case "k_idle2":
                    case "k_idle3":
                    case "k_idle4":
                    case "k_idle5":
                        PlayAnimation("k_idle1");
                        break;
                }
                millisAniCounter = 0;
            }

            //shooting animation / arrow attachment / projectile creation
            if (currentAniName == "k_fire_arrow")
            {
                if(attState == AttackState.GrabbingArrow 
                    && millisShotArrowAttachCounter >= millisShotArrowAttachLength)
                {
                    attached.Add("arrow", Game.GetAttachable("arrow", "sword", "Bone_001_R_004"));
                    attState = AttackState.DrawingString;
                    millisShotArrowAttachCounter = 0;
                }
                else if (attState == AttackState.DrawingString && millisShotAniCounter >= millisShotRelease)
                {
                    Vector3 forward = GetForward();
                    attached.Remove("arrow");
                    attacks.CreateArrow(physicalData.Position + forward * 10, forward * 450, 25, "bad");
                    attState = AttackState.LettingGo;
                    millisShotAniCounter = 0;
                }
                else if (attState == AttackState.LettingGo && millisShotAniCounter >= millisShotRelease * attackSpeed)
                {
                    attState = AttackState.None;
                    millisShotAniCounter = 0;
                }
            }

            //swinging animation
            if (currentAniName == "k_onehanded_swing" || currentAniName == "k_flip")
            {
                if (attState == AttackState.InitialSwing && millisMelleCounter >= millisMelleDamage)
                {
                    Vector3 forward = GetForward();
                    attacks.CreateMelleAttack(physicalData.Position + forward * 35, 25, "bad");
                    attState = AttackState.FinishSwing;
                    millisMelleCounter = 0;
                }
            }
        }



        #region helpers
        /// <summary>
        /// rotates kazgar towards mouse
        /// </summary>
        /// <param name="move"></param>
        private void UpdateRotation(Vector3 move)
        {
            //orientation
            float yaw = (float)Math.Atan(move.X / move.Z);
            if (move.Z < 0 && move.X >= 0
                || move.Z < 0 && move.X < 0)
            {
                //TODO
                yaw += MathHelper.Pi;
            }
            yaw += MathHelper.Pi;
            physicalData.Orientation = Quaternion.CreateFromYawPitchRoll(yaw, 0, 0);
        }
        private void ChangeVelocity(Vector3 newVel)
        {
            physicalData.LinearVelocity = newVel;
        }


        private void ResetTargettedEntity()
        {
            mouseHoveredEntity = null;
            mouseHoveredHealth = null;
        }
        private Matrix GetRotation()
        {
            Matrix3X3 bepurot = physicalData.OrientationMatrix;
            return new Matrix(bepurot.M11, bepurot.M12, bepurot.M13, 0, bepurot.M21, bepurot.M22, bepurot.M23, 0, bepurot.M31, bepurot.M32, bepurot.M33, 0, 0, 0, 0, 1);
        }

        private Vector3 GetForward()
        {
            return physicalData.OrientationMatrix.Forward;
        }
        #endregion

        //questionable part of this component's design
        SpriteFont font;
        Texture2D texWhitePixel;
        Rectangle RectEnemyHealthBar;
        Vector2 vecName;
        Vector2 mid;
        float xRatio;
        float yRatio;
        Vector2 screenRatio;
        int maxX;
        int maxY;
        private void InitDrawingParams()
        {
            mid = new Vector2(Game.GraphicsDevice.Viewport.Width / 2, Game.GraphicsDevice.Viewport.Height / 2);
            maxX = Game.GraphicsDevice.Viewport.Width;
            maxY = Game.GraphicsDevice.Viewport.Height;
            xRatio = maxX/1920f;
            yRatio = maxY/1080f;
            screenRatio = new Vector2(xRatio, yRatio);
            RectEnemyHealthBar = new Rectangle((int)mid.X - 75, 40, 150, 20);
            vecName = new Vector2((int)mid.X - 75, 10);


        }
        public override void Draw(SpriteBatch s)
        {
            if (mouseHoveredEntity != null)
            {
                s.DrawString(font, mouseHoveredEntity.Name, vecName, Color.Red);
            }
            if(mouseHoveredHealth != null)
            {
                s.Draw(texWhitePixel, RectEnemyHealthBar, Color.Red);
                s.Draw(texWhitePixel, new Rectangle(RectEnemyHealthBar.X, RectEnemyHealthBar.Y, (int)(RectEnemyHealthBar.Width * mouseHoveredHealth.HealthPercent), RectEnemyHealthBar.Height), Color.Green);
            }

            #region UIBase
            //Chat Pane
            s.Draw(texWhitePixel, new Rectangle(0, (int) ((maxY-444*yRatio)), (int) (362*xRatio), (int)(444*yRatio)) , Color.Black*0.5f);

            #region Ability Bar
            //Ability Bar
            s.Draw(texWhitePixel, new Rectangle((int)((maxX/2 -311 * xRatio)), (int)((maxY - 158 * yRatio)), (int)(622 * xRatio), (int)(158 * yRatio)), Color.Red * 0.5f);
            //Q
            s.Draw(placeHolder, new Rectangle((int)((maxX / 2 - 301 * xRatio)), (int)((maxY - 148 * yRatio)), (int)(64 * xRatio), (int)(64 * yRatio)), Color.White);
            //W
            s.Draw(placeHolder, new Rectangle((int)((maxX / 2 - 227 * xRatio)), (int)((maxY - 148 * yRatio)), (int)(64 * xRatio), (int)(64 * yRatio)), Color.White);
            //E
            s.Draw(placeHolder, new Rectangle((int)((maxX / 2 - 153 * xRatio)), (int)((maxY - 148 * yRatio)), (int)(64 * xRatio), (int)(64 * yRatio)), Color.White);
            //R
            s.Draw(placeHolder, new Rectangle((int)((maxX / 2 - 79 * xRatio)), (int)((maxY - 148 * yRatio)), (int)(64 * xRatio), (int)(64 * yRatio)), Color.White);
            //A
            s.Draw(placeHolder, new Rectangle((int)((maxX / 2 - 301 * xRatio)), (int)((maxY - 74 * yRatio)), (int)(64 * xRatio), (int)(64 * yRatio)), Color.White);
            //S
            s.Draw(placeHolder, new Rectangle((int)((maxX / 2 - 227 * xRatio)), (int)((maxY - 74 * yRatio)), (int)(64 * xRatio), (int)(64 * yRatio)), Color.White);
            //D
            s.Draw(placeHolder, new Rectangle((int)((maxX / 2 - 153 * xRatio)), (int)((maxY - 74 * yRatio)), (int)(64 * xRatio), (int)(64 * yRatio)), Color.White);
            //F
            s.Draw(placeHolder, new Rectangle((int)((maxX / 2 - 79 * xRatio)), (int)((maxY - 74 * yRatio)), (int)(64 * xRatio), (int)(64 * yRatio)), Color.White);

            //LM
            //TODO Change when we change attState based on hands?
            switch (selectedPrimary)
            {
                case PrimaryAttack.Melle:
                    s.Draw(melee, new Rectangle((int)((maxX / 2 + 5 * xRatio)), (int)((maxY - 111 * yRatio)), (int)(64 * xRatio), (int)(64 * yRatio)), Color.White);
                    break;
                case PrimaryAttack.Magic:
                    s.Draw(magic, new Rectangle((int)((maxX / 2 + 5 * xRatio)), (int)((maxY - 111 * yRatio)), (int)(64 * xRatio), (int)(64 * yRatio)), Color.White);
                    break;
                case PrimaryAttack.Ranged:
                    s.Draw(range, new Rectangle((int)((maxX / 2 + 5 * xRatio)), (int)((maxY - 111 * yRatio)), (int)(64 * xRatio), (int)(64 * yRatio)), Color.White);
                    break;
            }
                
            //RM
            s.Draw(placeHolder, new Rectangle((int)((maxX / 2 + 79 * xRatio)), (int)((maxY - 111 * yRatio)), (int)(64 * xRatio), (int)(64 * yRatio)), Color.White);

            //Item 1
            s.Draw(healthPot, new Rectangle((int)((maxX / 2 + 163 * xRatio)), (int)((maxY - 148 * yRatio)), (int)(64 * xRatio), (int)(64 * yRatio)), Color.White);
            //Item 2
            s.Draw(healthPot, new Rectangle((int)((maxX / 2 + 237 * xRatio)), (int)((maxY - 148 * yRatio)), (int)(64 * xRatio), (int)(64 * yRatio)), Color.White);
            //Item 3
            s.Draw(healthPot, new Rectangle((int)((maxX / 2 + 163 * xRatio)), (int)((maxY - 74 * yRatio)), (int)(64 * xRatio), (int)(64 * yRatio)), Color.White);
            //Item 4
            s.Draw(healthPot, new Rectangle((int)((maxX / 2 + 237 * xRatio)), (int)((maxY - 74 * yRatio)), (int)(64 * xRatio), (int)(64 * yRatio)), Color.White);

            #endregion

            //XP Area
            s.Draw(texWhitePixel, new Rectangle((int)((maxX / 2 - 311 * xRatio)), (int)((maxY - 178 * yRatio)), (int)(622 * xRatio), (int)(20 * yRatio)), Color.Brown * 0.5f);
            //Damage Tracker
            s.Draw(texWhitePixel, new Rectangle((int)((maxX - 300 * xRatio)), (int)((maxY - 230 * yRatio)), (int)(300 * xRatio), (int)(230 * yRatio)), Color.Green * 0.5f);
            //Mini Map (square for now)
            s.Draw(texWhitePixel, new Rectangle((int)((maxX - 344 * xRatio)), 0, (int)(344 * xRatio), (int)(344 * yRatio)), Color.Orange * 0.5f);
            //Main Player Frame Pic
            s.Draw(texWhitePixel, new Rectangle(0, 0, (int)(160 * xRatio), (int)(160 * yRatio)), Color.Blue * 0.5f);
            //Main Player Frame Health
            s.Draw(texWhitePixel, new Rectangle((int)(160 * xRatio), 0, (int)(310 * xRatio), (int)(52 * yRatio)), Color.Blue * 0.5f);
            //Second Player Frame Pic
            s.Draw(texWhitePixel, new Rectangle((int)(20 * xRatio), (int)(180 * yRatio), (int)(54 * xRatio), (int)(54 * yRatio)), Color.Blue * 0.5f);
            //Second Player Frame Health
            s.Draw(texWhitePixel, new Rectangle((int)(74 * xRatio), (int)(180 * yRatio), (int)(74 * xRatio), (int)(30 * yRatio)), Color.Blue * 0.5f);
            //Third Player Frame Pic
            s.Draw(texWhitePixel, new Rectangle((int)(20 * xRatio), (int)(254 * yRatio), (int)(54 * xRatio), (int)(54 * yRatio)), Color.Blue * 0.5f);
            //Third Player Frame Health
            s.Draw(texWhitePixel, new Rectangle((int)(74 * xRatio), (int)(254 * yRatio), (int)(74 * xRatio), (int)(30 * yRatio)), Color.Blue * 0.5f);
            //Fourth Player Frame Pic
            s.Draw(texWhitePixel, new Rectangle((int)(20 * xRatio), (int)(328 * yRatio), (int)(54 * xRatio), (int)(54 * yRatio)), Color.Blue * 0.5f);
            //Fourth Player Frame Health
            s.Draw(texWhitePixel, new Rectangle((int)(74 * xRatio), (int)(328 * yRatio), (int)(74 * xRatio), (int)(30 * yRatio)), Color.Blue * 0.5f);
            //Fifth Player Frame Pic
            s.Draw(texWhitePixel, new Rectangle((int)(20 * xRatio), (int)(402 * yRatio), (int)(54 * xRatio), (int)(54 * yRatio)), Color.Blue * 0.5f);
            //Fifth Player Frame Health
            s.Draw(texWhitePixel, new Rectangle((int)(74 * xRatio), (int)(402 * yRatio), (int)(74 * xRatio), (int)(30 * yRatio)), Color.Blue * 0.5f);

            #endregion
        }
    }
}
