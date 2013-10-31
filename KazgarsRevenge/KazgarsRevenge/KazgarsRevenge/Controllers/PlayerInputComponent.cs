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
        GameEntity entity;
        Entity mouseHoveredPhysicalData;
        GameEntity mouseHoveredEntity;
        HealthComponent mouseHoveredHealth;
        EntityManager entities;
        AnimationPlayer animations;
        Dictionary<string, AttachableModel> attached;
        Random rand;

        const float stopRadius = 10;
        const float targetResetDistance = 1000;
        Vector3 targettedLocation = Vector3.Zero;
        //dont want kazgar to run until first click
        double millisRunningCounter = 2000;
        double millisRunTime = 2000;
        bool attacking = false;
        bool attackOnce = false;

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


        public PlayerInputComponent(MainGame game, GameEntity entity, Entity physicalData, AnimationPlayer animations, Dictionary<string, AttachableModel> attached)
            : base(game)
        {
            rand = new Random();
            this.physicalData = physicalData;
            this.entity = entity;
            physics = Game.Services.GetService(typeof(Space)) as Space;
            rayCastFilter = RayCastFilter;
            camera = game.Services.GetService(typeof(CameraComponent)) as CameraComponent;
            entities = game.Services.GetService(typeof(EntityManager)) as EntityManager;
            this.animations = animations;
            this.attached = attached;
            texWhitePixel = Game.Content.Load<Texture2D>("Textures\\whitePixel");
            font = game.Content.Load<SpriteFont>("Georgia");
            InitDrawingParams();
            PlayAnimation("k_idle1");
            attached.Add("sword", Game.GetAttachable("sword01", "sword", "Bone_001_R_004"));
            attached.Add("bow", Game.GetAttachable("bow01", "sword", "Bone_001_L_004"));

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
            animations.StartClip(clip);
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
                Vector3 groundHitPos = CheckMouseRay();
                if (mouseHoveredHealth != null && 
                    (mouseHoveredHealth.Dead || ((physicalData.Position - mouseHoveredPhysicalData.Position).Length() >= targetResetDistance)))
                {
                    ResetTargettedEntity();
                }

                Vector3 move = new Vector3(groundHitPos.X - physicalData.Position.X, 0, groundHitPos.Z - physicalData.Position.Z);
                move.Normalize();
                if (attState == AttackState.None)
                {
                    CheckAbilities(move);
                }

                if (attState == AttackState.None)
                {
                    MoveCharacter(move);
                }
                else
                {
                    physicalData.LinearVelocity = new Vector3(0, physicalData.LinearVelocity.Y, 0);
                }


                CheckAnimations();
            }
            else
            {
                physicalData.LinearVelocity = new Vector3(0, physicalData.LinearVelocity.Y, 0);
            }

            prevMouse = curMouse;
            prevKeys = curKeys;
        }

        /// <summary>
        /// creates a raycast from the mouse, and returns the position on the ground that it hits.
        /// Also does a bepu raycast and finds the first enemy it hits, and keeps its healthcomponent
        /// for gui purposes
        /// </summary>
        /// <returns></returns>
        private Vector3 CheckMouseRay()
        {
            //creating ray from mouse location
            Vector3 castOrigin = Game.GraphicsDevice.Viewport.Unproject(new Vector3(curMouse.X, curMouse.Y, 0), camera.Projection, camera.View, Matrix.Identity);
            Vector3 castdir = Game.GraphicsDevice.Viewport.Unproject(new Vector3(curMouse.X, curMouse.Y, 1), camera.Projection, camera.View, Matrix.Identity) - castOrigin;
            castdir.Normalize();
            Ray r = new Ray(castOrigin, castdir);

            //targets ground location to start running to if holding mouse button
            if (curMouse.LeftButton == ButtonState.Pressed && attState == AttackState.None)
            {
                //check where on the zero plane the ray hits, to guide the character by the mouse
                float? distance;
                Plane p = new Plane(Vector3.Up, 0);
                r.Intersects(ref p, out distance);
                if (distance.HasValue)
                {
                    targettedLocation = r.Position + r.Direction * distance.Value;
                }
                millisRunningCounter = 0;
            }
            else if(curMouse.LeftButton == ButtonState.Released && !attacking)
            {
                //check if ray hits GameEntity if not holding down mouse button
                List<RayCastResult> results = new List<RayCastResult>();
                physics.RayCast(r, 10000, rayCastFilter, results);

                GameEntity oldTarget = mouseHoveredEntity;
                ResetTargettedEntity();
                foreach (RayCastResult result in results)
                {
                    if (result.HitObject != null)
                    {
                        mouseHoveredEntity = result.HitObject.Tag as GameEntity;
                        if (mouseHoveredEntity != null)
                        {
                            mouseHoveredHealth = mouseHoveredEntity.GetComponent(typeof(HealthComponent)) as HealthComponent;
                            mouseHoveredPhysicalData = (mouseHoveredEntity.GetComponent(typeof(PhysicsComponent)) as PhysicsComponent).collidable;
                            if (mouseHoveredHealth == null || mouseHoveredPhysicalData == null)
                            {
                                ResetTargettedEntity();
                            }
                            break;
                        }
                    }
                }
            }

            return targettedLocation;
        }

        /// <summary>
        /// checks player input to see what ability to use
        /// </summary>
        /// <param name="gameTime"></param>
        private void CheckAbilities(Vector3 move)
        {
            attackOnce = attackOnce || (curMouse.LeftButton == ButtonState.Pressed && 
                (curKeys.IsKeyDown(Keys.LeftShift) || (prevMouse.LeftButton == ButtonState.Released && mouseHoveredEntity != null)));
            attacking = (attacking || attackOnce);// && curMouse.LeftButton == ButtonState.Pressed;
            Vector3 dir;
            dir.X = move.X;
            dir.Y = move.Y;
            dir.Z = move.Z;
            float distance = float.MaxValue;
            if (mouseHoveredEntity != null)
            {
                Vector3 otherPos = (mouseHoveredEntity.GetComponent(typeof(PhysicsComponent)) as PhysicsComponent).Position;
                distance = (otherPos - physicalData.Position).Length();
                dir = new Vector3(otherPos.X - physicalData.Position.X, 0, otherPos.Z - physicalData.Position.Z);
            }

            if (curKeys.IsKeyDown(Keys.Tab) && prevKeys.IsKeyUp(Keys.Tab))
            {
                selectedPrimary += 1;
                if ((int)selectedPrimary >= 2)
                {
                    selectedPrimary = 0;
                }
            }

            if (attacking )//|| attackOnce)
            {
                switch (selectedPrimary)
                {
                    case PrimaryAttack.Melle:
                        if (distance < melleRange)
                        {
                            PlayAnimation("k_onehanded_swing");
                            attState = AttackState.InitialSwing;
                            UpdateRotation(dir);
                            millisMelleCounter = 0;
                            attackOnce = false;
                            attacking = false;
                            targettedLocation = physicalData.Position;
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
                            attackOnce = false;
                            attacking = false;
                            targettedLocation = physicalData.Position;
                        }
                        break;
                }
            }

            /*if (!attacking)
            {
                if (curKeys.IsKeyDown(Keys.D1))
                {
                    PlayAnimation("k_flip");
                    attState = AttackState.InitialSwing;
                    UpdateRotation(dir);
                    millisMelleCounter = 0;
                }
            }*/
        }

        /// <summary>
        /// handles character movement
        /// </summary>
        /// <param name="groundHitPos"></param>
        private void MoveCharacter(Vector3 move)
        {
            Vector3 moveVec = move * maxSpeed;
            //running
            if (curMouse.LeftButton == ButtonState.Pressed)
            {
                UpdateRotation(move);
                moveVec.Y = physicalData.LinearVelocity.Y;
                physicalData.LinearVelocity = moveVec;

                //just started running
                if (currentAniName != "k_run")
                {
                    PlayAnimation("k_run");
                }
            }
            else if ((physicalData.Position - targettedLocation).Length() <= stopRadius || millisRunningCounter >= millisRunTime)
            {
                //stop if within stopRadius of targetted ground location
                physicalData.LinearVelocity = new Vector3(0, physicalData.LinearVelocity.Y, 0);
                //just stopped moving
                if (currentAniName == "k_run")
                {
                    PlayAnimation("k_fighting_stance");
                }
            }
            else
            {
                physicalData.LinearVelocity = moveVec;
            }
        }

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
                    entities.CreateArrow(physicalData.Position + forward * 10, forward * 450, 25, "bad");
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
                    entities.CreateMelleAttack(physicalData.Position + forward * 35, 25, "bad");
                    attState = AttackState.FinishSwing;
                    millisMelleCounter = 0;
                }
            }
        }



        #region helpers
        private void ResetTargettedEntity()
        {
            mouseHoveredEntity = null;
            mouseHoveredHealth = null;
            mouseHoveredPhysicalData = null;
            attacking = false;
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
                s.DrawString(font, mouseHoveredEntity.Name, vecName, Color.Red);
            }
            if(mouseHoveredHealth != null)
            {
                s.Draw(texWhitePixel, RectEnemyHealthBar, Color.Red);
                s.Draw(texWhitePixel, new Rectangle(RectEnemyHealthBar.X, RectEnemyHealthBar.Y, (int)(RectEnemyHealthBar.Width * mouseHoveredHealth.HealthPercent), RectEnemyHealthBar.Height), Color.Green);
            }
        }
    }
}
