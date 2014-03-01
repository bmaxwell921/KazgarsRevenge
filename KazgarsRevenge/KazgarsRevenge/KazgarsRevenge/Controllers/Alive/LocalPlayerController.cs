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
    class LocalPlayerController : PlayerController, IDrawableComponent2D
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
            texChargeBarFront = game.Content.Load<Texture2D>("Textures\\UI\\chargeBarFront");

            #region UI Frame Load
            texCursor = Game.Content.Load<Texture2D>("Textures\\whiteCursor");
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
            #endregion

            #region Item Image Load
            healthPot = Game.Content.Load<Texture2D>("Textures\\UI\\Items\\HP");
            goldIcon = Game.Content.Load<Texture2D>("Textures\\UI\\Items\\gold3");
            #endregion
        }

        AbilityTargetDecal groundIndicator;
        public override void Start()
        {
            groundIndicator = Entity.GetComponent(typeof(AbilityTargetDecal)) as AbilityTargetDecal;
        }

        //variables for target
        Entity targetedPhysicalData;
        GameEntity mouseHoveredEntity;
        AliveComponent mouseHoveredHealth;

        #region UI Textures
        Texture2D texCursor;

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
        string abilityToUseString = null;
        int selectedItemSlot = -1;
        GearSlot selectedEquipPiece;
        int selectedEquipSlot = -1;
        #endregion

        MouseState curMouse = Mouse.GetState();
        MouseState prevMouse = Mouse.GetState();
        KeyboardState curKeys = Keyboard.GetState();
        KeyboardState prevKeys = Keyboard.GetState();
        bool guiClick = false;
        public override void Update(GameTime gameTime)
        {
            Entity sharedData = Entity.GetSharedData(typeof(Entity)) as Entity;
            curMouse = Mouse.GetState();
            curKeys = Keyboard.GetState();
            double elapsed = gameTime.ElapsedGameTime.TotalMilliseconds;
            millisRunningCounter += elapsed;
            stateResetCounter += elapsed;

            if (attState == AttackState.Locked && canInterrupt && stateResetCounter >= stateResetLength)
            {
                attState = AttackState.None;
            }

            UpdateActionSequences(elapsed);

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
                k.Value.update(elapsed);
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
                newTarget = CheckGUIButtons() || newTarget || mouseHoveredHealth == null;

                string collides = CollidingGuiFrame();
                bool mouseOnGui = collides != null || selectedItemSlot != -1 || selectedEquipSlot != -1;
                CheckMouseRay(newTarget, mouseOnGui);

                //#jared Equip gear needs to 1. not take in a slot, 2. output floating text if equip fails
                //if item is removed from the inventory area
                if (collides == null && (selectedItemSlot != -1 || selectedEquipSlot != -1) && prevMouse.LeftButton == ButtonState.Pressed && curMouse.LeftButton == ButtonState.Released)
                {
                    //Trash item here?
                    selectedItemSlot = -1;
                    selectedEquipSlot = -1;
                }

                //appropriate action for gui element collided with
                //happens on left mouse released
                //#Nate
                #region left click check
                if (collides != null && prevMouse.LeftButton == ButtonState.Pressed && curMouse.LeftButton == ButtonState.Released)
                {
                    string innerClicked = CollidingInnerFrame(collides);
                    switch (collides)
                    {
                        case "inventory":
                            if (innerClicked == "equipArrow") showEquipment = !showEquipment;
                            if (innerClicked != null && innerClicked.Contains("inventory"))
                            {
                                for (int i = 0; i <= maxInventorySlots; i++)
                                {
                                    if (innerClicked == "inventory" + i && (inventory[i] != null || inventory[i] == null && selectedItemSlot != -1))
                                    {
                                        if (selectedItemSlot == -1)
                                        {
                                            selectedItemSlot = i;
                                        }
                                        else if (inventory[i] == null)
                                        {
                                            inventory[i] = inventory[selectedItemSlot];
                                            inventory[selectedItemSlot] = null;
                                            selectedItemSlot = -1;
                                        }
                                        else
                                        {
                                            Item temp = inventory[i];
                                            inventory[i] = inventory[selectedItemSlot];
                                            inventory[selectedItemSlot] = temp;
                                            selectedItemSlot = -1;
                                        }
                                    }
                                }
                            }
                            break;
                        case "loot":
                            for (int i = 0; i < NUM_LOOT_SHOWN; ++i)
                            {
                                if (RectContains(guiInsideRects["loot"]["loot" + i], curMouse.X, curMouse.Y))
                                {
                                    //clicked on item, add to inventory
                                    if (AddToInventory(lootingSoul.GetLoot(i)))
                                    {
                                        lootingSoul.RemoveLoot(i);
                                    }

                                }
                            }
                            break;

                        case "equipment":
                            if (innerClicked == "equip0") //wrist
                            {
                                if (selectedItemSlot == -1) //Not equipping anything, pick up wrist
                                {

                                }
                                else    //Bringing item in
                                {

                                }
                            }
                            else if (innerClicked == "equip1")  //bling
                            {

                            }
                            else if (innerClicked == "equip2")  //LWep
                            {
                                if (selectedItemSlot == -1) //Not equipping anything, pick up wep
                                {
                                    selectedEquipSlot = 2;
                                    selectedEquipPiece = GearSlot.Lefthand;
                                }
                                else    //Bringing item in
                                {
                                    Equippable e = inventory[selectedItemSlot] as Equippable;
                                    if (e != null)
                                    {
                                        EquipGear(e, GearSlot.Lefthand);
                                        selectedItemSlot = -1;
                                    }
                                }
                            }
                            //Equippable e = inventory[selectedItemSlot] as Equippable;
                            //if( e != null)
                            //equip;
                            break;

                        case "abilities":
                            abilityToUseString = innerClicked;
                            break;

                    }
                }
                #endregion

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
                if (attState != AttackState.Locked && !looting)
                {
                    CheckAbilities(move);
                }

                if (curMouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
                {
                    guiClick = mouseOnGui;
                }

                //targets ground location to start running to if holding mouse button
                if ((!mouseOnGui || Math.Abs(physicalData.LinearVelocity.X) + Math.Abs(physicalData.LinearVelocity.Z) > .01f && !guiClick) && curMouse.LeftButton == ButtonState.Pressed && attState != AttackState.Locked && !looting)
                {
                    groundTargetLocation = mouseHoveredLocation;
                    if (!mouseOnGui && prevMouse.LeftButton == ButtonState.Released && targetedPhysicalData == null)
                    {
                        attacks.CreateMouseSpikes(groundTargetLocation);
                    }
                }

                groundMove = move;

            }
            else
            {
                mouseHoveredLocation = physicalData.Position;
            }

            if (targetedPhysicalData == null)
            {
                groundMove = new Vector3(groundTargetLocation.X - physicalData.Position.X, 0, groundTargetLocation.Z - physicalData.Position.Z);
            }

            if (groundMove != Vector3.Zero)
            {
                groundMove.Normalize();
            }

            Vector3 distToTarget = physicalData.Position - groundTargetLocation;
            distToTarget.Y = 0;
            bool closeEnough = distToTarget.Length() <= stopRadius;
            if (!looting && (!guiClick || Math.Abs(physicalData.LinearVelocity.X) + Math.Abs(physicalData.LinearVelocity.Z) > .01f))
            {
                if ((attState == AttackState.Charging || attState == AttackState.CastingSpell) && !closeEnough)
                {
                    CancelFinishSequence();
                    MoveCharacter(groundMove, closeEnough);
                }
                else if (attState == AttackState.None)
                {
                    MoveCharacter(groundMove, closeEnough);
                }
                else
                {
                    ChangeVelocity(Vector3.Zero);
                }
            }
            else
            {
                ChangeVelocity(Vector3.Zero);
            }

            prevMouse = curMouse;
            prevKeys = curKeys;

            groundIndicator.UpdateMouseLocation(mouseHoveredLocation, targetingGroundLocation, targetedGroundSize);

            base.Update(gameTime);
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
        private void CheckMouseRay(bool newTarget, bool mouseOnGui)
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
            if (curKeys.IsKeyDown(Keys.LeftShift))
            {
                targetedPhysicalData = null;
                ResetTargettedEntity();
                newTarget = true;
            }

            if (newTarget && !mouseOnGui)
            {
                ResetTargettedEntity();

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
                            //if the left mouse button was either just clicked or not pressed down at all,
                            //or if any other ability input was just clicked
                            else// if (newTarget)
                            {
                                mouseHoveredHealth = mouseHoveredEntity.GetComponent(typeof(AliveComponent)) as AliveComponent;
                                if (mouseHoveredHealth != null)
                                {
                                    mouseHoveredHealth.Target();
                                }

                                if (mouseHoveredHealth == null)
                                {
                                    ResetTargettedEntity();
                                }
                                else if (curMouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
                                {
                                    targetedPhysicalData = mouseHoveredEntity.GetSharedData(typeof(Entity)) as Entity;
                                }
                                break;
                            }
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
        private void CheckAbilities(Vector3 move)
        {
            if (curKeys.IsKeyDown(Keys.Escape))
            {
                targetingGroundLocation = false;
                if (attState == AttackState.Charging)
                {
                    attState = AttackState.None;
                    CancelFinishSequence();
                    StartSequence("fightingstance");
                }
            }

            //check for click when aiming ground target ability
            if (targetingGroundLocation && curMouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
            {
                StartAbilitySequence(lastUsedAbility);
                return;
            }

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



            bool useAbility = false;

            Ability abilityToUse = null;

            foreach (KeyValuePair<Keys, Ability> k in boundAbilities)
            {//#Nate a
                if (curKeys.IsKeyDown(k.Key) && prevKeys.IsKeyUp(k.Key) && !k.Value.onCooldown)
                {
                    useAbility = true;
                    abilityToUse = k.Value;
                    break;
                }
            }

            //mouse click check
            if (!useAbility && abilityToUseString != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (abilityToUseString == "ability" + i)
                    {
                        if (!boundAbilities[i].Value.onCooldown)
                        {
                            useAbility = true;
                            abilityToUse = boundAbilities[i].Value;
                            abilityToUseString = null;
                        }
                    }
                }
            }

            if (useAbility)
            {
                if (attState == AttackState.Charging && abilityToUse.ActionName == currentActionName)
                {
                    abilityToUse.Use();
                    InterruptCurrentSequence();
                    CancelFinishSequence();
                    targetedPhysicalData = null;
                }
                else if (attState == AttackState.Charging)
                {
                    CancelFinishSequence();
                    if (abilityToUse.AbilityType == AbilityType.Instant)
                    {
                        abilityToUse.Use();
                    }
                    UpdateRotation(dir);
                    StartAbilitySequence(abilityToUse);
                    targetedPhysicalData = null;

                }
                else if (abilityToUse.AbilityType == AbilityType.Instant)
                {
                    abilityToUse.Use();
                    UpdateRotation(dir);
                    StartAbilitySequence(abilityToUse);
                    targetedPhysicalData = null;
                }
                else
                {
                    if (targetingGroundLocation && lastUsedAbility == abilityToUse)
                    {
                        abilityToUse.Use();
                    }
                    StartAbilitySequence(abilityToUse);
                    targetedPhysicalData = null;
                }
                return;
            }

            if (attState == AttackState.Charging)
            {
                UpdateRotation(dir);
                return;
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
                        }
                        break;
                    case AttackType.Melle:
                        if (distance < melleRange)
                        {
                            StartSequence("swing");
                            UpdateRotation(dir);
                        }
                        break;
                    case AttackType.Ranged:
                        if (distance < bowRange)
                        {
                            StartSequence("shoot");
                            UpdateRotation(dir);
                        }
                        break;
                    case AttackType.Magic:
                        if (distance < bowRange)
                        {
                            //need magic item animation here
                            StartSequence("shoot");
                            UpdateRotation(dir);
                        }
                        break;
                }
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

            Vector3 moveVec = move * GetStat((int)StatType.RunSpeed);
            moveVec.Y = 0;

            if (curMouse.LeftButton == ButtonState.Pressed)
            {
                if (!closeEnough)
                {
                    //just started running
                    if (currentAniName != "k_run")
                    {
                        PlayAnimationInterrupt("k_run", MixType.None);
                    }

                    ChangeVelocity(moveVec);

                    UpdateRotation(move);
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
                //just started running
                if (currentAniName != "k_run")
                {
                    PlayAnimationInterrupt("k_run", MixType.None);
                }
                ChangeVelocity(moveVec);
                UpdateRotation(move);
            }
        }

        #region Helpers
        private void ResetTargettedEntity()
        {
            if (mouseHoveredHealth != null)
            {
                mouseHoveredHealth.UnTarget();
            }
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

        /// <summary>
        /// Checks each inside gui component for given string value for click action and returns name of what was clicked.
        /// </summary>
        /// <returns></returns>
        private string CollidingInnerFrame(string outsideRec)
        {

            foreach (KeyValuePair<string, Rectangle> k in guiInsideRects[outsideRec])
            {
                if (RectContains(k.Value, curMouse.X, curMouse.Y))
                {
                    return k.Key;
                }

            }
            return null;
        }
        #endregion

        SpriteFont font;
        Texture2D texWhitePixel;
        Texture2D texChargeBarFront;
        Rectangle rectEnemyHealthBar;
        Rectangle rectCharged;
        Vector2 vecName;
        Vector2 mid;
        float xRatio;
        float yRatio;
        Vector2 screenRatio;
        int maxX;
        int maxY;
        float average = 1;
        Dictionary<string, Rectangle> guiOutsideRects;
        Dictionary<string, Dictionary<string, Rectangle>> guiInsideRects;
        Rectangle rectMouse;

        //Inside Rect Dictionaries
        Dictionary<string, Rectangle> damageDict;
        Dictionary<string, Rectangle> chatDict;
        Dictionary<string, Rectangle> playerDict;
        Dictionary<string, Rectangle> mapDict;
        Dictionary<string, Rectangle> xpDict;
        Dictionary<string, Rectangle> inventoryDict;
        Dictionary<string, Rectangle> equipmentDict;
        Dictionary<string, Rectangle> abilityDict;
        Dictionary<string, Rectangle> lootDict;

        private void InitDrawingParams()
        {
            mid = new Vector2(Game.GraphicsDevice.Viewport.Width / 2, Game.GraphicsDevice.Viewport.Height / 2);
            maxX = Game.GraphicsDevice.Viewport.Width;
            maxY = Game.GraphicsDevice.Viewport.Height;
            xRatio = maxX / 1920f;
            yRatio = maxY / 1080f;
            average = (xRatio + yRatio) / 2;
            screenRatio = new Vector2(xRatio, yRatio);
            rectEnemyHealthBar = new Rectangle((int)(mid.X - 75 * average), (int)(53 * average), (int)(200 * average), (int)(40 * average));
            float chargeBarLength = 400 * average;
            rectCharged = new Rectangle((int)(mid.X - chargeBarLength / 2), (int)(maxY * 3 / 4), (int)(chargeBarLength), (int)(30 * average));
            vecName = new Vector2(rectEnemyHealthBar.X, 5);

            //mouse
            rectMouse = new Rectangle(0, 0, 25, 25);

            //Outside Rectangles
            guiOutsideRects = new Dictionary<string, Rectangle>();
            guiOutsideRects.Add("abilities", new Rectangle((int)((maxX / 2 - 311 * average)), (int)((maxY - 158 * average)), (int)(622 * average), (int)(158 * average)));
            guiOutsideRects.Add("xp", new Rectangle((int)((maxX / 2 - 311 * average)), (int)((maxY - 178 * average)), (int)(622 * average), (int)(20 * average)));
            guiOutsideRects.Add("damage", new Rectangle((int)((maxX - 300 * average)), (int)((maxY - 230 * average)), (int)(300 * average), (int)(230 * average)));
            guiOutsideRects.Add("map", new Rectangle((int)((maxX - 344 * average)), 0, (int)(344 * average), (int)(344 * average)));
            guiOutsideRects.Add("player", new Rectangle(0, 0, (int)(470 * average), (int)(160 * average)));
            //Nate Here
            guiOutsideRects.Add("inventory", new Rectangle((int)(maxX - 400 * average), (int)(380 * average), (int)(402 * average), (int)(440 * average)));
            guiOutsideRects.Add("equipment", new Rectangle((int)(maxX - 704 * average), (int)(380 * average), (int)(304 * average), (int)(440 * average)));

            guiOutsideRects.Add("loot", new Rectangle((int)(150 * average), (int)(150 * average), 150, 300));
            guiOutsideRects.Add("chat", new Rectangle(0, (int)((maxY - 444 * average)), (int)(362 * average), (int)(444 * average)));

            guiInsideRects = new Dictionary<string, Dictionary<string, Rectangle>>();
            equipmentDict = new Dictionary<string, Rectangle>();
            inventoryDict = new Dictionary<string, Rectangle>();
            abilityDict = new Dictionary<string, Rectangle>();
            lootDict = new Dictionary<string, Rectangle>();
            chatDict = new Dictionary<string, Rectangle>();
            playerDict = new Dictionary<string, Rectangle>();
            mapDict = new Dictionary<string, Rectangle>();
            xpDict = new Dictionary<string, Rectangle>();
            damageDict = new Dictionary<string, Rectangle>();
            //Add frame dictionaries
            guiInsideRects.Add("inventory", inventoryDict);
            guiInsideRects.Add("equipment", equipmentDict);
            guiInsideRects.Add("abilities", abilityDict);
            guiInsideRects.Add("loot", lootDict);
            guiInsideRects.Add("xp", xpDict);
            guiInsideRects.Add("damage", damageDict);
            guiInsideRects.Add("map", mapDict);
            guiInsideRects.Add("chat", chatDict);
            guiInsideRects.Add("player", playerDict);

            //Equipment inner
            equipmentDict.Add("equip0", new Rectangle((int)(maxX - 694 * average), (int)(458 * average), (int)(88 * average), (int)(88 * average)));    //wrist
            equipmentDict.Add("equip1", new Rectangle((int)(maxX - 694 * average), (int)(556 * average), (int)(88 * average), (int)(88 * average)));    //bling
            equipmentDict.Add("equip2", new Rectangle((int)(maxX - 694 * average), (int)(654 * average), (int)(88 * average), (int)(88 * average)));    //LWep

            equipmentDict.Add("equip3", new Rectangle((int)(maxX - 596 * average), (int)(409 * average), (int)(88 * average), (int)(88 * average)));    //head
            equipmentDict.Add("equip4", new Rectangle((int)(maxX - 596 * average), (int)(507 * average), (int)(88 * average), (int)(88 * average)));    //chest
            equipmentDict.Add("equip5", new Rectangle((int)(maxX - 596 * average), (int)(605 * average), (int)(88 * average), (int)(88 * average)));    //legs
            equipmentDict.Add("equip6", new Rectangle((int)(maxX - 596 * average), (int)(703 * average), (int)(88 * average), (int)(88 * average)));    //feet

            equipmentDict.Add("equip7", new Rectangle((int)(maxX - 498 * average), (int)(458 * average), (int)(88 * average), (int)(88 * average)));    //shoulder
            equipmentDict.Add("equip8", new Rectangle((int)(maxX - 498 * average), (int)(556 * average), (int)(88 * average), (int)(88 * average)));    //Cod
            equipmentDict.Add("equip9", new Rectangle((int)(maxX - 498 * average), (int)(654 * average), (int)(88 * average), (int)(88 * average)));    //RWep
            //Inventory inner
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; j++)
                {
                    inventoryDict.Add("inventory" + (i + j * 4), new Rectangle((int)(maxX - 390 * average + i * 98 * average), (int)(430 * average + j * 98 * average), (int)(88 * average), (int)(88 * average)));
                }
            }
            inventoryDict.Add("equipArrow", new Rectangle((int)(maxX - 380 * average), (int)(385 * average), (int)(40 * average), (int)(40 * average)));

            //ability
            abilityDict.Add("primary", new Rectangle((int)((maxX / 2 + 5 * average)), (int)((maxY - 111 * average)), (int)(64 * average), (int)(64 * average)));
            abilityDict.Add("rightmouse", new Rectangle((int)((maxX / 2 + 79 * average)), (int)((maxY - 111 * average)), (int)(64 * average), (int)(64 * average)));
            //abilities 0-7
            for (int i = 0; i < 4; ++i)
            {
                abilityDict.Add("ability" + i, new Rectangle((int)((maxX / 2 - (301 - 74 * i) * average)), (int)((maxY - 148 * average)), (int)(64 * average), (int)(64 * average)));
                abilityDict.Add("ability" + (i + 4), new Rectangle((int)((maxX / 2 - (301 - 74 * i) * average)), (int)((maxY - 74 * average)), (int)(64 * average), (int)(64 * average)));
            }
            abilityDict.Add("item1", new Rectangle((int)((maxX / 2 + 163 * average)), (int)((maxY - 148 * average)), (int)(64 * average), (int)(64 * average)));
            abilityDict.Add("item2", new Rectangle((int)((maxX / 2 + 237 * average)), (int)((maxY - 148 * average)), (int)(64 * average), (int)(64 * average)));
            abilityDict.Add("item3", new Rectangle((int)((maxX / 2 + 163 * average)), (int)((maxY - 74 * average)), (int)(64 * average), (int)(64 * average)));
            abilityDict.Add("item4", new Rectangle((int)((maxX / 2 + 237 * average)), (int)((maxY - 74 * average)), (int)(64 * average), (int)(64 * average)));

            //Loot
            for (int i = 0; i < NUM_LOOT_SHOWN; ++i)
            {
                lootDict.Add("loot" + i, new Rectangle((int)(165 * average), (int)(165 * average + i * 65), 50, 50));
            }


        }

        public void Draw(SpriteBatch s)
        {


            if (mouseHoveredEntity != null)
            {
                s.DrawString(font, mouseHoveredEntity.Name, vecName, Color.Red, 0, Vector2.Zero, average, SpriteEffects.None, 0);
            }
            if (mouseHoveredHealth != null)
            {
                s.Draw(texWhitePixel, rectEnemyHealthBar, Color.Red);
                s.Draw(texWhitePixel, new Rectangle(rectEnemyHealthBar.X, rectEnemyHealthBar.Y, (int)(rectEnemyHealthBar.Width * mouseHoveredHealth.HealthPercent), rectEnemyHealthBar.Height), Color.Green);
            }
            float chargedPercent = GetPercentCharged();
            if (chargedPercent > 0)
            {
                s.Draw(texWhitePixel, rectCharged, Color.Black * .75f);
                s.Draw(texWhitePixel, new Rectangle(rectCharged.X, rectCharged.Y, (int)(rectCharged.Width * chargedPercent), rectCharged.Height), Color.Yellow);
                s.Draw(texChargeBarFront, rectCharged, Color.White);
            }

            #region UIBase
            //Chat Pane
            s.Draw(texWhitePixel, guiOutsideRects["chat"], Color.Black * 0.5f);

            #region Ability Bar
            //Ability Bar
            s.Draw(texWhitePixel, guiOutsideRects["abilities"], Color.Red * 0.5f);
            for (int i = 0; i < 4; ++i)
            {//Convert below to inside Rects
                s.Draw(boundAbilities[i].Value.icon, guiInsideRects["abilities"]["ability" + i], Color.White);
                s.Draw(boundAbilities[i + 4].Value.icon, guiInsideRects["abilities"]["ability" + (i + 4)], Color.White);
            }

            //LM
            switch (GetMainhandType())
            {
                case AttackType.None:
                case AttackType.Melle:
                    s.Draw(texMelee, guiInsideRects["abilities"]["primary"], Color.White);
                    break;
                case AttackType.Magic:
                    s.Draw(texMagic, guiInsideRects["abilities"]["primary"], Color.White);
                    break;
                case AttackType.Ranged:
                    s.Draw(texRange, guiInsideRects["abilities"]["primary"], Color.White);
                    break;
            }

            //RM
            s.Draw(texPlaceHolder, guiInsideRects["abilities"]["rightmouse"], Color.White);

            //Item 1
            s.Draw(healthPot, guiInsideRects["abilities"]["item1"], Color.White);
            //Item 2
            s.Draw(healthPot, guiInsideRects["abilities"]["item2"], Color.White);
            //Item 3
            s.Draw(healthPot, guiInsideRects["abilities"]["item3"], Color.White);
            //Item 4
            s.Draw(healthPot, guiInsideRects["abilities"]["item4"], Color.White);

            #endregion

            #region Ability Bar Mods
            #region Cooldown Mod
            //TODO make into for loop for all bound abilities / items
            //s.Draw(texWhitePixel, new Rectangle((int)((maxX / 2 - (301 - 74 * 0) * average)), (int)((maxY - (84 + 64 * (boundAbilities[0].timeRemaining / boundAbilities[0].cooldownSeconds)) * average)), (int)(64 * average), (int)(64 * (boundAbilities[0].timeRemaining / boundAbilities[0].cooldownSeconds) * average) + 1), Color.Black * 0.5f);

            for (int i = 0; i < 4; i++)
            {
                if (boundAbilities[i].Value.onCooldown)
                {
                    s.Draw(texWhitePixel, new Rectangle((int)((maxX / 2 - (301 - 74 * i) * average)), (int)((maxY - (84 + 64 * (boundAbilities[i].Value.cooldownMillisRemaining / boundAbilities[i].Value.cooldownMillisLength)) * average)), (int)(64 * average), (int)(64 * (boundAbilities[i].Value.cooldownMillisRemaining / boundAbilities[i].Value.cooldownMillisLength) * average) + 1), Color.Black * 0.5f);
                }
                if (boundAbilities[i + 4].Value.onCooldown)
                {
                    s.Draw(texWhitePixel, new Rectangle((int)((maxX / 2 - (301 - 74 * i) * average)), (int)((maxY - (10 + 64 * (boundAbilities[i + 4].Value.cooldownMillisRemaining / boundAbilities[i + 4].Value.cooldownMillisLength)) * average)), (int)(64 * average), (int)(64 * (boundAbilities[i + 4].Value.cooldownMillisRemaining / boundAbilities[i + 4].Value.cooldownMillisLength) * average) + 1), Color.Black * 0.5f);
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
                    s.Draw(rightArrow, guiInsideRects["inventory"]["equipArrow"], Color.White);

                    //Equip icons
                    for (int i = 0; i < 10; i++)
                    {
                        s.Draw(texPlaceHolder, guiInsideRects["equipment"]["equip"+i], Color.White);
                    }
                }
                else
                {
                    //Open arrow
                    s.Draw(leftArrow, guiInsideRects["inventory"]["equipArrow"], Color.White);
                }

                s.Draw(texWhitePixel, guiOutsideRects["inventory"], Color.Black * .5f);
                //Gold display

                s.Draw(goldIcon, new Rectangle((int)(maxX - 320 * average), (int)(385 * average), (int)(40 * average), (int)(40 * average)), Color.White);
                s.DrawString(font, gold.ToString(), new Vector2(maxX - 280 * average, 380 * average), Color.White, 0, Vector2.Zero, average, SpriteEffects.None, 0);
                //Draw inventory items
                for (int i = 0; i < inventory.Length; ++i)
                {
                    //Nate working here
                    if (inventory[i] != null)
                    {
                        s.Draw(inventory[i].Icon, guiInsideRects["inventory"]["inventory" + i], Color.White);
                    }

                }
            }

            if (looting && lootingSoul != null)
            {
                s.Draw(texWhitePixel, guiOutsideRects["loot"], Color.Black * .5f);

                List<Item> loot = lootingSoul.Loot;
                for (int i = 0; i < loot.Count && i < NUM_LOOT_SHOWN; ++i)
                {
                    s.Draw(loot[i].Icon, guiInsideRects["loot"]["loot" + i], Color.White);
                }
            }
            #endregion

            #endregion

            //Mouse
            rectMouse.X = curMouse.X;
            rectMouse.Y = curMouse.Y;
            if (selectedItemSlot == -1 && selectedEquipSlot == -1)
            {
                rectMouse.Width = 25;
                rectMouse.Height = 25;
                s.Draw(texCursor, rectMouse, Color.White);
            }
            else if (selectedItemSlot != -1)
            {
                rectMouse.Width = (int)(60 * average);
                rectMouse.Height = (int)(60 * average);
                s.Draw(inventory[selectedItemSlot].Icon, rectMouse, Color.White);
            }
            else
            {
                rectMouse.Width = (int)(60 * average);
                rectMouse.Height = (int)(60 * average);
                s.Draw(gear[selectedEquipPiece].Icon, rectMouse, Color.White);
            }
        }

    }
}
