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
using BEPUphysics.ResourceManagement;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Collidables;
using SkinnedModelLib;
using KazgarsRevenge.Libraries;

namespace KazgarsRevenge
{
    class LocalPlayerController : PlayerController
    {
        public LocalPlayerController(KazgarsRevengeGame game, GameEntity entity, PlayerSave savefile)
            : base(game, entity, savefile)
        {
            //misc
            rand = new Random();
            rayCastFilter = RayCastFilter;

            //content
            InitDrawingParams();
            font = game.Content.Load<SpriteFont>("Verdana");
            texWhitePixel = game.Content.Load<Texture2D>("white");

            #region UI Frame Load
            icon_selected = Game.Content.Load<Texture2D>("Textures\\UI\\Frames\\icon_selected");
            health_bar = Game.Content.Load<Texture2D>("Textures\\UI\\Frames\\health_bar");
            rightArrow = Game.Content.Load<Texture2D>("Textures\\UI\\Frames\\rightArrow");
            leftArrow = Game.Content.Load<Texture2D>("Textures\\UI\\Frames\\leftArrow");
            helmetIcon = Game.Content.Load<Texture2D>("Textures\\UI\\Frames\\helmetIcon");
            #endregion

            #region Ability Image Load
            texMelee = Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\DB");
            texRange = Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\LW");
            texMagic = Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\BR");
            texPlaceHolder = Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\BN");
            texHeartStrikeIcon = Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\HS");
            I1 = Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\I1");
            I2 = Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\I2");
            I3 = Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\I3");
            I4 = Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\I4");
            I5 = Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\I5");
            I6 = Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\I6");
            I7 = Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\I7");
            #endregion

            #region Item Image Load
            healthPot = Game.Content.Load<Texture2D>("Textures\\UI\\Items\\HP");
            goldIcon = Game.Content.Load<Texture2D>("Textures\\UI\\Items\\gold3");
            #endregion
        }

        //variables for target
        Entity targetedPhysicalData;
        GameEntity mouseHoveredEntity;
        AliveComponent mouseHoveredHealth;

        #region UI Textures
        #region UI Frames
        Texture2D icon_selected;
        Texture2D health_bar;
        Texture2D rightArrow;
        Texture2D leftArrow;
        //Equipment Base
        Texture2D helmetIcon;
        #endregion

        #region Ability Icons
        Texture2D texMelee;
        Texture2D texRange;
        Texture2D texMagic;
        Texture2D texPlaceHolder;
        Texture2D texHeartStrikeIcon;
        Texture2D I1;
        Texture2D I2;
        Texture2D I3;
        Texture2D I4;
        Texture2D I5;
        Texture2D I6;
        Texture2D I7;
        #endregion

        #region Item Icons
        Texture2D healthPot;
        Texture2D goldIcon;
        #endregion
        #endregion

        #region Variables for UI
        bool[] UISlotUsed = new bool[14];     //0-7 abilities, 8 primary, 9 secondary, 10-13 items
        bool showInventory = false;
        bool showEquipment = false;
        #endregion

        MouseState curMouse = Mouse.GetState();
        MouseState prevMouse = Mouse.GetState();
        KeyboardState curKeys = Keyboard.GetState();
        KeyboardState prevKeys = Keyboard.GetState();
        bool guiClick = false;
        public override void Update(GameTime gameTime)
        {
            curMouse = Mouse.GetState();
            curKeys = Keyboard.GetState();
            double elapsed = gameTime.ElapsedGameTime.TotalMilliseconds;
            float currentTime = (float)gameTime.TotalGameTime.TotalSeconds;
            millisActionCounter += elapsed;
            millisRunningCounter += elapsed;
            stateResetCounter += elapsed;

            
            if (attState == AttackState.Attacking && stateResetCounter >= stateResetLength && canInterrupt)
            {
                attState = AttackState.None;
            }

            UpdateActionSequences();

            if (inCombat && millisCombatCounter >= millisCombatLength)
            {
                inCombat = false;
                millisCombatCounter = 0;
            }
            else
            {
                millisCombatCounter += elapsed;
            }

            #region UI Update Section
            //reset the currently used abilities
            //TODO change for ability use on key up
            for (int i = 0; i < 14; i++)
            {
                UISlotUsed[i] = false;
            }
            //ability CD updates
            //TODO make into loop for all bound abilities and items
            foreach (KeyValuePair<Keys, Ability> k in boundAbilities)
            {
                k.Value.update(currentTime);
            }
            #endregion

            if (curMouse.LeftButton == ButtonState.Pressed)
            {
                millisRunningCounter = 0;
            }
            else
            {
                if (attState != AttackState.None)
                {
                    targetedPhysicalData = null;
                }
            }


            Vector3 groundMove = Vector3.Zero;
            bool moveTowardsTarget = curMouse.LeftButton == ButtonState.Pressed;
            if (Game.IsActive)
            {
                bool newTarget = curMouse.LeftButton == ButtonState.Released || prevMouse.LeftButton == ButtonState.Released || (curMouse.RightButton == ButtonState.Pressed && prevMouse.RightButton == ButtonState.Released);
                newTarget = CheckGUIButtons() || newTarget;

                string collides = CollidingGuiFrame();
                bool mouseOnGui = collides != null;
                CheckMouseRay(newTarget, mouseOnGui);

                //appropriate action for gui element collided with
                //happens on left mouse released
                if (collides != null && prevMouse.LeftButton == ButtonState.Pressed && curMouse.LeftButton == ButtonState.Released)
                {
                    switch (collides)
                    {
                        case "inventory":

                            break;
                        case "loot":
                            for (int i = 0; i < NUM_LOOT_SHOWN; ++i)
                            {
                                if (RectContains(guiInsideRects["loot" + i], curMouse.X, curMouse.Y))
                                {
                                    if (inventory.Count < maxInventorySlots)
                                    {
                                        //clicked on item, add to inventory
                                        AddToInventory(lootingSoul.GetLoot(i));
                                    }
                                }
                            }
                            break;

                    }
                }


                Vector3 move = new Vector3(mouseHoveredLocation.X - physicalData.Position.X, 0, mouseHoveredLocation.Z - physicalData.Position.Z);
                if (targetedPhysicalData != null)
                {
                    move = new Vector3(targetedPhysicalData.Position.X - physicalData.Position.X, 0, targetedPhysicalData.Position.Z - physicalData.Position.Z);
                }
                if (move != Vector3.Zero)
                {
                    move.Normalize();
                }

                if (curMouse.LeftButton == ButtonState.Pressed && mouseHoveredHealth != null && mouseHoveredHealth.Dead)
                {
                    ResetTargettedEntity();
                }
                if (attState == AttackState.None && !looting)
                {
                    CheckAbilities(move, (float)currentTime);
                }

                if (curMouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
                {
                    guiClick = mouseOnGui;
                }

                //targets ground location to start running to if holding mouse button
                if ((!mouseOnGui || Math.Abs(physicalData.LinearVelocity.X) + Math.Abs(physicalData.LinearVelocity.Z) > .01f && !guiClick) && curMouse.LeftButton == ButtonState.Pressed && attState == AttackState.None && !looting)
                {
                    groundTargetLocation = mouseHoveredLocation;
                    if (!mouseOnGui && prevMouse.LeftButton == ButtonState.Released && targetedPhysicalData == null)
                    {
                        attacks.CreateMouseSpikes(groundTargetLocation);
                    }
                }

                groundMove = move;

            }

            if (targetedPhysicalData == null)
            {
                groundMove = new Vector3(groundTargetLocation.X - physicalData.Position.X, 0, groundTargetLocation.Z - physicalData.Position.Z);
            }

            if (groundMove != Vector3.Zero)
            {
                groundMove.Normalize();
            }

            bool closeEnough = (physicalData.Position - groundTargetLocation).Length() <= stopRadius;
            if (attState == AttackState.None && !looting)
            {
                MoveCharacter(groundMove, closeEnough);
            }
            else
            {
                ChangeVelocity(Vector3.Zero);
            }

            prevMouse = curMouse;
            prevKeys = curKeys;
        }

        Func<BroadPhaseEntry, bool> rayCastFilter;
        bool RayCastFilter(BroadPhaseEntry entry)
        {
            return entry != physicalData.CollisionInformation
                && entry.CollisionRules.Personal <= CollisionRule.Normal
                && (entry as StaticMesh) == null;
        }

        /// <summary>
        /// creates a raycast from the mouse, and returns the position on the ground that it hits.
        /// Also does a bepu raycast and finds the first enemy it hits, and keeps its healthcomponent
        /// for gui purposes
        /// </summary>
        /// <returns></returns>
        private void CheckMouseRay(bool newTarget, bool guiclick)
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

            //check if ray hits GameEntity if not holding down mouse button
            List<RayCastResult> results = new List<RayCastResult>();
            physics.RayCast(r, 10000, rayCastFilter, results);

            if (curMouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
            {
                targetedPhysicalData = null;
            }

            if (newTarget)
            {
                ResetTargettedEntity();
            }
            foreach (RayCastResult result in results)
            {
                if (result.HitObject != null)
                {
                    mouseHoveredEntity = result.HitObject.Tag as GameEntity;
                    if (mouseHoveredEntity != null)
                    {
                        if (mouseHoveredEntity.Type == EntityType.Misc)
                        {
                            ResetTargettedEntity();
                        }
                        //if the left mouse button was either just clicked or not pressed down at all, or if any other ability input was just clicked
                        else if (newTarget)
                        {
                            mouseHoveredHealth = mouseHoveredEntity.GetComponent(typeof(AliveComponent)) as AliveComponent;
                            if (mouseHoveredHealth == null)
                            {
                                ResetTargettedEntity();
                            }
                            else if (!guiclick && curMouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
                            {
                                targetedPhysicalData = mouseHoveredEntity.GetSharedData(typeof(Entity)) as Entity;
                            }
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks which buttons are pressed down
        /// </summary>
        /// <returns></returns>
        private bool CheckGUIButtons()
        {
            //switch weapon hands (for demo)
            if (curKeys.IsKeyDown(Keys.Tab) && prevKeys.IsKeyUp(Keys.Tab)
                && !(gear[GearSlot.Righthand] as Weapon).TwoHanded)
            {
                SwapWeapons();
            }

            if (looting && lootingSoul != null && lootingSoul.Remove && currentAniName != "k_loot_smash")
            {
                CloseLoot();
            }

            //loot nearby soul
            if (attState == AttackState.None && curKeys.IsKeyDown(Keys.Space) && prevKeys.IsKeyUp(Keys.Space) && currentAniName != "k_loot_smash")
            {
                if (!looting)
                {
                    OpenLoot();
                }
                else
                {
                    CloseLoot();
                }
            }
            else if (looting && (lootingSoul == null || lootingSoul.Loot.Count == 0) && currentAniName != "k_loot_smash")
            {
                CloseLoot();
            }
            else if (looting && currentAniName != "k_loot_smash" && curKeys.IsKeyDown(Keys.Escape) && prevKeys.IsKeyUp(Keys.Escape))
            {
                CloseLoot();
            }

            
            //Inventory
            if (curKeys.IsKeyDown(Keys.I) && prevKeys.IsKeyUp(Keys.I))
            {
                showInventory = !showInventory;
                showEquipment = false;
            }
            //Equipment
            if (curKeys.IsKeyDown(Keys.O) && prevKeys.IsKeyUp(Keys.O))
            {
                showEquipment = !showEquipment;
                if (showEquipment) showInventory = true;
                else showInventory = false;
            }
            //Esc closes all
            if (curKeys.IsKeyDown(Keys.Escape) && prevKeys.IsKeyUp(Keys.Escape))
            {
                showEquipment = false;
                showInventory = false;
            }


            //UI Abilities Used Frame
            if (curKeys.IsKeyDown(Keys.Q)) UISlotUsed[0] = true;
            if (curKeys.IsKeyDown(Keys.W)) UISlotUsed[1] = true;
            if (curKeys.IsKeyDown(Keys.E)) UISlotUsed[2] = true;
            if (curKeys.IsKeyDown(Keys.R)) UISlotUsed[3] = true;
            if (curKeys.IsKeyDown(Keys.A)) UISlotUsed[4] = true;
            if (curKeys.IsKeyDown(Keys.S)) UISlotUsed[5] = true;
            if (curKeys.IsKeyDown(Keys.D)) UISlotUsed[6] = true;
            if (curKeys.IsKeyDown(Keys.F)) UISlotUsed[7] = true;

            if (curMouse.LeftButton == ButtonState.Pressed) UISlotUsed[8] = true;
            if (curMouse.RightButton == ButtonState.Pressed) UISlotUsed[9] = true;

            if (curKeys.IsKeyDown(Keys.D1)) UISlotUsed[10] = true;
            if (curKeys.IsKeyDown(Keys.D2)) UISlotUsed[11] = true;
            if (curKeys.IsKeyDown(Keys.D3)) UISlotUsed[12] = true;
            if (curKeys.IsKeyDown(Keys.D4)) UISlotUsed[13] = true;

            return false;
        }

        /// <summary>
        /// checks player input to see what ability to use
        /// </summary>
        /// <param name="gameTime"></param>
        private void CheckAbilities(Vector3 move, float currentTime)
        {
            Vector3 dir;
            dir.X = move.X;
            dir.Y = move.Y;
            dir.Z = move.Z;
            float distance = 0;


            if (targetedPhysicalData != null)
            {
                dir = targetedPhysicalData.Position - physicalData.Position;
                dir.Y = 0;
                distance = (dir).Length();
                dir.Normalize();
            }


            //primary attack (autos)
            if (curMouse.LeftButton == ButtonState.Pressed && curKeys.IsKeyDown(Keys.LeftShift) || targetedPhysicalData != null)
            {
                //used to differentiate between left hand, right hand, and two hand animations
                aniSuffix = "right";

                //figure out what kind of weapon is in the main hand
                AttackType mainHandType = GetMainhandType();
                AttackType offHandType = GetOffhandType();

                //figure out if we're dual-wielding the same type of weapon, or using a two-hander, or just attacking with right hand
                if (mainHandType != AttackType.None)
                {
                    if ((gear[GearSlot.Righthand] as Weapon).TwoHanded)
                    {
                        aniSuffix = "twohanded";
                    }
                }

                //remove this once we get more animations
                aniSuffix = "";

                //attack if in range
                switch (mainHandType)
                {
                    case AttackType.None:
                        if (distance < melleRange)
                        {
                            //need punch animation
                            StartSequence("punch");
                            UpdateRotation(dir);
                            mouseHoveredLocation = physicalData.Position;
                            groundTargetLocation = physicalData.Position;
                        }
                        break;
                    case AttackType.Melle:
                        if (distance < melleRange)
                        {
                            StartSequence("swing");
                            UpdateRotation(dir);
                            mouseHoveredLocation = physicalData.Position;
                            groundTargetLocation = physicalData.Position;
                        }
                        break;
                    case AttackType.Ranged:
                        if (distance < bowRange)
                        {
                            StartSequence("shoot");
                            UpdateRotation(dir);
                            mouseHoveredLocation = physicalData.Position;
                            groundTargetLocation = physicalData.Position;
                        }
                        break;
                    case AttackType.Magic:
                        if (distance < bowRange)
                        {
                            //need magic item animation here
                            StartSequence("shoot");
                            UpdateRotation(dir);
                            mouseHoveredLocation = physicalData.Position;
                            groundTargetLocation = physicalData.Position;
                        }
                        break;
                }
            }

            bool useAbility = false;
            Ability abilityToUse = null;

            foreach (KeyValuePair<Keys, Ability> k in boundAbilities)
            {
                if (curKeys.IsKeyDown(k.Key) && k.Value.tryUse(currentTime))
                {
                    useAbility = true;
                    abilityToUse = k.Value;
                    break;
                }
            }

            if (useAbility)
            {
                UpdateRotation(dir);
                mouseHoveredLocation = physicalData.Position;
                groundTargetLocation = physicalData.Position;
                targetedPhysicalData = null;


                StartSequence(abilityToUse.ActionName);
            }
        }

        /// <summary>
        /// handles character movement
        /// </summary>
        /// <param name="groundHitPos"></param>
        private void MoveCharacter(Vector3 move, bool closeEnough)
        {
            //click:      target enemy if ray catches one to move to / attack
            //held down:  path towards mousehovered location (or targetted entity if its data isn't null and it's not in range) / if in range of targetted entity, auto attack
            //up:         if there is a targetted enemy, run up and attack it once. if not, run towards targetted location if not already at it

            Vector3 moveVec = move * stats[(int)StatType.RunSpeed];
            moveVec.Y = 0;

            if (curMouse.LeftButton == ButtonState.Pressed)
            {
                if (!closeEnough)
                {
                    ChangeVelocity(moveVec);

                    UpdateRotation(move);
                    //just started running
                    if (currentAniName != "k_run")
                    {
                        PlayAnimationInterrupt("k_run", MixType.None);
                    }
                }
            }
            else if (closeEnough || millisRunningCounter >= millisRunTime)
            {
                //stop if within stopRadius of targetted ground location
                ChangeVelocity(Vector3.Zero);
                //just stopped moving
                if (currentAniName == "k_run")
                {
                    StartSequence("fightingstance");
                }
            }
            else
            {
                ChangeVelocity(moveVec);
                UpdateRotation(move);
                //just started running
                if (currentAniName != "k_run")
                {
                    PlayAnimationInterrupt("k_run", MixType.None);
                }
            }
        }

        #region Helpers
        protected override void RecalculateStats()
        {
            base.RecalculateStats();

            stateResetLength = 3000 * (1 - Math.Min(.8f, stats[StatType.AttackSpeed]));
        }
        private void ResetTargettedEntity()
        {
            mouseHoveredEntity = null;
            mouseHoveredHealth = null;
        }
        
        /// <summary>
        /// Checks what frame of the gui that the mouse is colliding with and returns its name.
        /// If the mouse is not colliding with the gui, returns null.
        /// #Nate :P
        /// </summary>
        private string CollidingGuiFrame()
        {

            foreach (KeyValuePair<string, Rectangle> k in guiOutsideRects)
            {
                if (RectContains(k.Value, curMouse.X, curMouse.Y))
                {
                    //only check against inventory or loot if they are shown
                    if (k.Key == "inventory" && !showInventory || k.Key == "equipment" && !showEquipment || k.Key == "loot" && !looting)
                    {
                        continue;
                    }

                    return k.Key;
                }

            }
            return null;
        }
        #endregion


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
        float average = 1;
        Dictionary<string, Rectangle> guiOutsideRects;
        Dictionary<string, Rectangle> guiInsideRects;
        private void InitDrawingParams()
        {
            mid = new Vector2(Game.GraphicsDevice.Viewport.Width / 2, Game.GraphicsDevice.Viewport.Height / 2);
            maxX = Game.GraphicsDevice.Viewport.Width;
            maxY = Game.GraphicsDevice.Viewport.Height;
            xRatio = maxX / 1920f;
            yRatio = maxY / 1080f;
            average = (xRatio + yRatio) / 2;
            screenRatio = new Vector2(xRatio, yRatio);
            RectEnemyHealthBar = new Rectangle((int)(mid.X - 75 * average), (int)(53 * average), (int)(200 * average), (int)(40 * average));
            vecName = new Vector2(RectEnemyHealthBar.X, 5);

            guiOutsideRects = new Dictionary<string, Rectangle>();
            guiOutsideRects.Add("abilities", new Rectangle((int)((maxX / 2 - 311 * average)), (int)((maxY - 158 * average)), (int)(622 * average), (int)(158 * average)));
            guiOutsideRects.Add("xp", new Rectangle((int)((maxX / 2 - 311 * average)), (int)((maxY - 178 * average)), (int)(622 * average), (int)(20 * average)));
            guiOutsideRects.Add("damage", new Rectangle((int)((maxX - 300 * average)), (int)((maxY - 230 * average)), (int)(300 * average), (int)(230 * average)));
            guiOutsideRects.Add("map", new Rectangle((int)((maxX - 344 * average)), 0, (int)(344 * average), (int)(344 * average)));
            //Nate Here
            guiOutsideRects.Add("inventory", new Rectangle((int)(maxX - 400 * average), (int)(380 * average), (int)(402 * average), (int)(440 * average)));
            guiOutsideRects.Add("equipment", new Rectangle((int)(maxX - 700 * average), (int)(380 * average), (int)(300 * average), (int)(440 * average)));

            guiOutsideRects.Add("loot", new Rectangle((int)(150 * average), (int)(150 * average), 150, 300));
            guiOutsideRects.Add("chat", new Rectangle(0, (int)((maxY - 444 * average)), (int)(362 * average), (int)(444 * average)));

            guiInsideRects = new Dictionary<string, Rectangle>();
            //Equipment Icons
            guiInsideRects.Add("equipHead", new Rectangle((int)(maxX - 690 * average), (int)(390 * average), (int)(88 * average), (int)(88 * average)));
            guiInsideRects.Add("equipShoulder", new Rectangle((int)(maxX - 500 * average), (int)(390 * average), (int)(88 * average), (int)(88 * average)));
            //Inventory icons
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; j++)
                {
                    guiInsideRects.Add("inventory" + (i + j * 4), new Rectangle((int)(maxX - 390 * average + i * 98 * average), (int)(430 * average + j * 98 * average), (int)(88 * average), (int)(88 * average)));
                }
            }

            //Gui elements
            guiInsideRects.Add("equipArrow", new Rectangle((int)(maxX - 380 * average), (int)(385 * average), (int)(40 * average), (int)(40 * average)));
            guiInsideRects.Add("primary", new Rectangle((int)((maxX / 2 + 5 * average)), (int)((maxY - 111 * average)), (int)(64 * average), (int)(64 * average)));
            guiInsideRects.Add("rightmouse", new Rectangle((int)((maxX / 2 + 79 * average)), (int)((maxY - 111 * average)), (int)(64 * average), (int)(64 * average)));
            guiInsideRects.Add("item1", new Rectangle((int)((maxX / 2 + 163 * average)), (int)((maxY - 148 * average)), (int)(64 * average), (int)(64 * average)));
            guiInsideRects.Add("item2", new Rectangle((int)((maxX / 2 + 237 * average)), (int)((maxY - 148 * average)), (int)(64 * average), (int)(64 * average)));
            guiInsideRects.Add("item3", new Rectangle((int)((maxX / 2 + 163 * average)), (int)((maxY - 74 * average)), (int)(64 * average), (int)(64 * average)));
            guiInsideRects.Add("item4", new Rectangle((int)((maxX / 2 + 237 * average)), (int)((maxY - 74 * average)), (int)(64 * average), (int)(64 * average)));
            for (int i = 0; i < NUM_LOOT_SHOWN; ++i)
            {
                guiInsideRects.Add("loot" + i, new Rectangle((int)(165 * average), (int)(165 * average + i * 65), 50, 50));
            }


        }

        public override void Draw(SpriteBatch s)
        {
            if (mouseHoveredEntity != null)
            {
                s.DrawString(font, mouseHoveredEntity.Name, vecName, Color.Red, 0, Vector2.Zero, average, SpriteEffects.None, 0);
            }
            if (mouseHoveredHealth != null)
            {
                s.Draw(texWhitePixel, RectEnemyHealthBar, Color.Red);
                s.Draw(texWhitePixel, new Rectangle(RectEnemyHealthBar.X, RectEnemyHealthBar.Y, (int)(RectEnemyHealthBar.Width * mouseHoveredHealth.HealthPercent), RectEnemyHealthBar.Height), Color.Green);
            }

            #region UIBase
            //Chat Pane
            s.Draw(texWhitePixel, guiOutsideRects["chat"], Color.Black * 0.5f);

            #region Ability Bar
            //Ability Bar
            s.Draw(texWhitePixel, guiOutsideRects["abilities"], Color.Red * 0.5f);
            for (int i = 0; i < 4; ++i)
            {
                s.Draw(boundAbilities[i].Value.icon, new Rectangle((int)((maxX / 2 - (301 - 74 * i) * average)), (int)((maxY - 148 * average)), (int)(64 * average), (int)(64 * average)), Color.White);
                s.Draw(boundAbilities[i + 4].Value.icon, new Rectangle((int)((maxX / 2 - (301 - 74 * i) * average)), (int)((maxY - 74 * average)), (int)(64 * average), (int)(64 * average)), Color.White);
            }

            //LM
            switch (GetMainhandType())
            {
                case AttackType.None:
                case AttackType.Melle:
                    s.Draw(texMelee, guiInsideRects["primary"], Color.White);
                    break;
                case AttackType.Magic:
                    s.Draw(texMagic, guiInsideRects["primary"], Color.White);
                    break;
                case AttackType.Ranged:
                    s.Draw(texRange, guiInsideRects["primary"], Color.White);
                    break;
            }

            //RM
            s.Draw(texPlaceHolder, guiInsideRects["rightmouse"], Color.White);

            //Item 1
            s.Draw(healthPot, guiInsideRects["item1"], Color.White);
            //Item 2
            s.Draw(healthPot, guiInsideRects["item2"], Color.White);
            //Item 3
            s.Draw(healthPot, guiInsideRects["item3"], Color.White);
            //Item 4
            s.Draw(healthPot, guiInsideRects["item4"], Color.White);

            #endregion

            #region Ability Bar Mods
            #region Cooldown Mod
            //TODO make into for loop for all bound abilities / items
            //s.Draw(texWhitePixel, new Rectangle((int)((maxX / 2 - (301 - 74 * 0) * average)), (int)((maxY - (84 + 64 * (boundAbilities[0].timeRemaining / boundAbilities[0].cooldownSeconds)) * average)), (int)(64 * average), (int)(64 * (boundAbilities[0].timeRemaining / boundAbilities[0].cooldownSeconds) * average) + 1), Color.Black * 0.5f);

            for (int i = 0; i < 4; i++)
            {
                if (boundAbilities[i].Value.onCooldown)
                {
                    s.Draw(texWhitePixel, new Rectangle((int)((maxX / 2 - (301 - 74 * i) * average)), (int)((maxY - (84 + 64 * (boundAbilities[i].Value.timeRemaining / boundAbilities[i].Value.cooldownSeconds)) * average)), (int)(64 * average), (int)(64 * (boundAbilities[i].Value.timeRemaining / boundAbilities[i].Value.cooldownSeconds) * average) + 1), Color.Black * 0.5f);
                }
                if (boundAbilities[i + 4].Value.onCooldown)
                {
                    s.Draw(texWhitePixel, new Rectangle((int)((maxX / 2 - (301 - 74 * i) * average)), (int)((maxY - (10 + 64 * (boundAbilities[i + 4].Value.timeRemaining / boundAbilities[i + 4].Value.cooldownSeconds)) * average)), (int)(64 * average), (int)(64 * (boundAbilities[i + 4].Value.timeRemaining / boundAbilities[i + 4].Value.cooldownSeconds) * average) + 1), Color.Black * 0.5f);
                }
            }
            #endregion
            #region Ability Frames
            //Draw the frames around abilities being used
            for (int i = 0; i < 4; i++)
            {
                if (UISlotUsed[i])
                {
                    s.Draw(icon_selected, new Rectangle((int)((maxX / 2 - (301 - 74 * i) * average)), (int)((maxY - 148 * average)), (int)(64 * average), (int)(64 * average)), Color.White);
                }
                if (UISlotUsed[i + 4])
                {
                    s.Draw(icon_selected, new Rectangle((int)((maxX / 2 - (301 - 74 * i) * average)), (int)((maxY - 74 * average)), (int)(64 * average), (int)(64 * average)), Color.White);
                }
            }
            //Primary
            //if (UISlotUsed[8])
            //{
            //    s.Draw(icon_selected, new Rectangle((int)((maxX / 2 + 5 * average)), (int)((maxY - 111 * average)), (int)(64 * average), (int)(64 * average)), Color.White);
            //}
            if (UISlotUsed[9])
            {
                s.Draw(icon_selected, new Rectangle((int)((maxX / 2 + 79 * average)), (int)((maxY - 111 * average)), (int)(64 * average), (int)(64 * average)), Color.White);
            }
            if (UISlotUsed[10])
            {
                s.Draw(icon_selected, new Rectangle((int)((maxX / 2 + 163 * average)), (int)((maxY - 148 * average)), (int)(64 * average), (int)(64 * average)), Color.White);
            }
            if (UISlotUsed[11])
            {
                s.Draw(icon_selected, new Rectangle((int)((maxX / 2 + 237 * average)), (int)((maxY - 148 * average)), (int)(64 * average), (int)(64 * average)), Color.White);
            }
            if (UISlotUsed[12])
            {
                s.Draw(icon_selected, new Rectangle((int)((maxX / 2 + 163 * average)), (int)((maxY - 74 * average)), (int)(64 * average), (int)(64 * average)), Color.White);
            }
            if (UISlotUsed[13])
            {
                s.Draw(icon_selected, new Rectangle((int)((maxX / 2 + 237 * average)), (int)((maxY - 74 * average)), (int)(64 * average), (int)(64 * average)), Color.White);
            }
            #endregion

            #endregion

            //XP Area
            s.Draw(texWhitePixel, guiOutsideRects["xp"], Color.Brown * 0.5f);
            //Damage Tracker
            s.Draw(texWhitePixel, guiOutsideRects["damage"], Color.Green * 0.5f);
            //Mini Map (square for now)
            s.Draw(texWhitePixel, guiOutsideRects["map"], Color.Orange * 0.5f);

            #region main player frame
            //Main Player Frame Pic
            s.Draw(texWhitePixel, new Rectangle(0, 0, (int)(160 * average), (int)(160 * average)), Color.Blue * 0.5f);
            //Main Player Frame Health
            s.Draw(texWhitePixel, new Rectangle((int)(160 * average), 0, (int)(310 * average), (int)(52 * average)), Color.Blue * 0.5f);
            //main player health
            s.Draw(health_bar, new Rectangle((int)(163 * average), 3, (int)(304 * HealthPercent * average), (int)(46 * average)), new Rectangle(0, 0, (int)(health_bar.Width * HealthPercent * average), (int)health_bar.Height), Color.White);
            #endregion

            #region second player frame
            //Second Player Frame Pic
            s.Draw(texWhitePixel, new Rectangle((int)(20 * average), (int)(180 * average), (int)(54 * average), (int)(54 * average)), Color.Blue * 0.5f);
            //Second Player Frame Health
            s.Draw(texWhitePixel, new Rectangle((int)(74 * average), (int)(180 * average), (int)(74 * average), (int)(30 * average)), Color.Blue * 0.5f);
            #endregion

            #region third player frame
            //Third Player Frame Pic
            s.Draw(texWhitePixel, new Rectangle((int)(20 * average), (int)(254 * average), (int)(54 * average), (int)(54 * average)), Color.Blue * 0.5f);
            //Third Player Frame Health
            s.Draw(texWhitePixel, new Rectangle((int)(74 * average), (int)(254 * average), (int)(74 * average), (int)(30 * average)), Color.Blue * 0.5f);
            #endregion

            #region fourth player frame
            //Fourth Player Frame Pic
            s.Draw(texWhitePixel, new Rectangle((int)(20 * average), (int)(328 * average), (int)(54 * average), (int)(54 * average)), Color.Blue * 0.5f);
            //Fourth Player Frame Health
            s.Draw(texWhitePixel, new Rectangle((int)(74 * average), (int)(328 * average), (int)(74 * average), (int)(30 * average)), Color.Blue * 0.5f);
            #endregion

            #region fifth player frame
            //Fifth Player Frame Pic
            s.Draw(texWhitePixel, new Rectangle((int)(20 * average), (int)(402 * average), (int)(54 * average), (int)(54 * average)), Color.Blue * 0.5f);
            //Fifth Player Frame Health
            s.Draw(texWhitePixel, new Rectangle((int)(74 * average), (int)(402 * average), (int)(74 * average), (int)(30 * average)), Color.Blue * 0.5f);
            #endregion

            #region inventory
            if (showInventory)
            {
                
                //Equipment pane
                if (showEquipment)
                {
                    s.Draw(texWhitePixel, guiOutsideRects["equipment"], Color.Black * .5f);
                    //Collapse arrow
                    s.Draw(rightArrow, guiInsideRects["equipArrow"], Color.White);
                    s.Draw(helmetIcon, guiInsideRects["equipHead"], Color.White);
                    s.Draw(texPlaceHolder, guiInsideRects["equipShoulder"], Color.White);
                }
                else
                {
                    //Open arrow
                    s.Draw(leftArrow, guiInsideRects["equipArrow"], Color.White);
                }

                s.Draw(texWhitePixel, guiOutsideRects["inventory"], Color.Black * .5f);
                //Gold display
               
                s.Draw(goldIcon, new Rectangle((int)(maxX - 320 * average), (int)(385 * average), (int)(40 * average), (int)(40 * average)), Color.White);
                s.DrawString(font, gold.ToString() , new Vector2(maxX - 280 * average, 380 * average), Color.White, 0, Vector2.Zero, average, SpriteEffects.None, 0);
                //Draw inventory items
                for (int i = 0; i < inventory.Count; ++i)
                {
                    //Nate working here
                    s.Draw(inventory[i].Icon, guiInsideRects["inventory" + i], Color.White);
                    //s.DrawString(font, inventory[i].Name, new Vector2(maxX - 400 * average, (420 + i * 40 )* average), Color.White, 0, Vector2.Zero, average, SpriteEffects.None, 0);
                }
            }

            if (looting && lootingSoul != null)
            {
                s.Draw(texWhitePixel, guiOutsideRects["loot"], Color.Black * .5f);

                List<Item> loot = lootingSoul.Loot;
                for (int i = 0; i < loot.Count && i < NUM_LOOT_SHOWN; ++i)
                {
                    s.Draw(loot[i].Icon, guiInsideRects["loot" + i], Color.White);
                }
            }
            #endregion

            #endregion

        }

    }
}
