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
using KazgarsRevenge.Non_Component;

namespace KazgarsRevenge
{
    class LocalPlayerController : PlayerController, IDrawableComponent2D
    {
        public LocalPlayerController(KazgarsRevengeGame game, GameEntity entity, Account account)
            : base(game, entity, account)
        {
            showHealthWithOutline = false;
            //misc
            rand = new Random();
            rayCastFilter = RayCastFilter;

            //content
            InitDrawingParams();
            font = game.Content.Load<SpriteFont>("Verdana");
            texWhitePixel = Texture2DUtil.Instance.GetTexture(TextureStrings.WHITE);
            texChargeBarFront = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.CHARGE_BAR_FRONT);

            #region UI Frame Load
            texCursor = Texture2DUtil.Instance.GetTexture(TextureStrings.WHITE_CURSOR);
            icon_selected = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Frames.ICON_SEL);
            health_bar = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Frames.HEALTH_BAR);
            rightArrow = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Frames.RIGHT_ARROW);
            leftArrow = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Frames.LEFT_ARROW);
            helmetIcon = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Frames.HELMET);
            talentArrowUp = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Talent_Arrow_U);
            talentArrowUpLeft = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Talent_Arrow_UL);
            talentArrowUpRight = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Talent_Arrow_UR);
            talentArrowDown = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Talent_Arrow_D);
            talentArrowDownRight = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Talent_Arrow_DR);
            talentArrowDownLeft = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Talent_Arrow_DL);
            talentArrowRight = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Talent_Arrow_R);
            talentArrowLeft = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Talent_Arrow_L);

            #endregion

            #region Ability Image Load
            // TODO give these guys meaningful names in the TextureStrings class and follow the above convention
            texPlaceHolder = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Place_Holder);
            #endregion

            #region Item Image Load
            healthPot = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Potions.HEALTH);
            goldIcon = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Items.Coins.LOTS);
            #endregion

            #region ranged ability array
            rangedAbilities[0, 0] = new AbilityNode(AbilityName.AdrenalineRush, null, true, 3, abilityLearnedFlags);
            rangedAbilities[0, 1] = new AbilityNode(AbilityName.Snipe, null, true, 3, abilityLearnedFlags);

            rangedAbilities[1, 0] = new AbilityNode(AbilityName.Serrated, new AbilityName[] { AbilityName.AdrenalineRush }, false, 2, abilityLearnedFlags);
            rangedAbilities[1, 1] = new AbilityNode(AbilityName.Headshot, new AbilityName[] { AbilityName.Snipe }, false, 2, abilityLearnedFlags);
            rangedAbilities[1, 2] = new AbilityNode(AbilityName.GrapplingHook, null, true, 4, abilityLearnedFlags);

            rangedAbilities[2, 0] = new AbilityNode(AbilityName.Homing, new AbilityName[] { AbilityName.Serrated }, false, 2, abilityLearnedFlags);
            rangedAbilities[2, 1] = new AbilityNode(AbilityName.MagneticImplant, new AbilityName[] { AbilityName.Headshot }, false, 2, abilityLearnedFlags);
            rangedAbilities[2, 2] = new AbilityNode(AbilityName.SpeedyGrapple, new AbilityName[] { AbilityName.GrapplingHook }, false, 2, abilityLearnedFlags);
            rangedAbilities[2, 3] = new AbilityNode(AbilityName.Elusiveness, new AbilityName[] { AbilityName.Tumble }, false, 1, abilityLearnedFlags);


            rangedAbilities[3, 1] = new AbilityNode(AbilityName.LooseCannon, new AbilityName[] { AbilityName.Homing, AbilityName.MagneticImplant }, false, 5, abilityLearnedFlags);
            rangedAbilities[3, 2] = new AbilityNode(AbilityName.FlashBomb, new AbilityName[] { AbilityName.SpeedyGrapple, AbilityName.LooseCannon, AbilityName.Tumble }, false, 2, abilityLearnedFlags);
            rangedAbilities[3, 3] = new AbilityNode(AbilityName.Tumble, null, true, 4, abilityLearnedFlags);

            rangedAbilities[4, 0] = new AbilityNode(AbilityName.Leeching, new AbilityName[] { AbilityName.LooseCannon }, false, 2, abilityLearnedFlags);
            rangedAbilities[4, 1] = new AbilityNode(AbilityName.MakeItRain, new AbilityName[] { AbilityName.LooseCannon }, false, 4, abilityLearnedFlags);
            rangedAbilities[4, 2] = new AbilityNode(AbilityName.BiggerBombs, new AbilityName[] { AbilityName.FlashBomb }, false, 1, abilityLearnedFlags);
            rangedAbilities[4, 3] = new AbilityNode(AbilityName.TarBomb, new AbilityName[] { AbilityName.FlashBomb }, false, 1, abilityLearnedFlags);

            rangedAbilities[5, 0] = new AbilityNode(AbilityName.Penetrating, new AbilityName[] { AbilityName.Leeching }, false, 2, abilityLearnedFlags);
            rangedAbilities[5, 1] = new AbilityNode(AbilityName.MakeItHail, new AbilityName[] { AbilityName.MakeItRain }, false, 3, abilityLearnedFlags);
            rangedAbilities[5, 2] = new AbilityNode(AbilityName.MoltenBolt, new AbilityName[] { AbilityName.TarBomb }, false, 2, abilityLearnedFlags);

            rangedAbilities[6, 0] = new AbilityNode(AbilityName.Omnishot, new AbilityName[] { AbilityName.MakeItHail }, false, 10, abilityLearnedFlags);
            rangedAbilities[6, 1] = new AbilityNode(AbilityName.StrongWinds, new AbilityName[] { AbilityName.MakeItHail }, false, 3, abilityLearnedFlags);
            #endregion
        }

        AbilityTargetDecal groundIndicator;
        public override void Start()
        {
            groundIndicator = Entity.GetComponent(typeof(AbilityTargetDecal)) as AbilityTargetDecal;

            base.Start();
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
        Texture2D talentArrowDown;
        Texture2D talentArrowDownRight;
        Texture2D talentArrowLeft;
        Texture2D talentArrowDownLeft;
        Texture2D talentArrowRight;
        Texture2D talentArrowUp;
        Texture2D talentArrowUpRight;
        Texture2D talentArrowUpLeft;

        //Equipment Base
        Texture2D helmetIcon;
        #endregion

        #region Ability Icons
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
        bool showTalents = false;
        string abilityToUseString = null;
        int selectedItemSlot = -1;
        int selectedTalentSlot = -1;
        int lootScroll = 0;
        GearSlot selectedEquipPiece;
        bool selectedEquipSlot = false;
        bool dragging = false;
        String draggingSource;

        enum TalentTrees
        {
            ranged,
            melee,
            magic
        }

        TalentTrees currentTalentTree = TalentTrees.ranged;


        AbilityNode[,] rangedAbilities = new AbilityNode[7, 4];


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
            millisRunningCounter += elapsed;
            stateResetCounter += elapsed;

            if ((attState == AttackState.Locked || attState == AttackState.LockedMoving) && canInterrupt && stateResetCounter >= stateResetLength)
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
            for (int i = 0; i < 14; i++)
            {
                UISlotUsed[i] = false;
            }
            //ability CD updates
            foreach (KeyValuePair<Keys, Ability> k in boundAbilities)
            {
                if (k.Value != null) k.Value.update(elapsed);
            }
            //Mouse Ability Updates
            foreach (KeyValuePair<ButtonState, Ability> k in mouseBoundAbility)
            {
                if (k.Value != null) k.Value.update(elapsed);
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
                bool mouseOnGui = collides != null || selectedItemSlot != -1 || selectedEquipSlot;
                CheckMouseRay(newTarget, mouseOnGui);

                //if icon is removed from the inventory area
                if (collides == null && (selectedItemSlot != -1 || selectedEquipSlot || selectedTalentSlot != -1) && prevMouse.LeftButton == ButtonState.Pressed && curMouse.LeftButton == ButtonState.Released)
                {
                    //Trash item here?
                    selectedItemSlot = -1;
                    selectedTalentSlot = -1;
                    selectedEquipSlot = false;
                }

                CheckButtonClicks(collides);

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

                if ((   (attState != AttackState.Locked && attState != AttackState.LockedMoving)   || usingPrimary) 
                    && !looting)
                {
                    CheckAbilities(move, mouseOnGui);
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
                else if (attState == AttackState.None || attState == AttackState.LockedMoving)
                {
                    MoveCharacter(groundMove, closeEnough);
                }
                else
                {
                    if (velDir == Vector3.Zero)
                    {
                        ChangeVelocity(Vector3.Zero);
                    }
                    else
                    {
                        ChangeVelocity(velDir);
                    }
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
                && (gear[GearSlot.Righthand] != null && gear[GearSlot.Lefthand] != null && !(gear[GearSlot.Righthand] as Weapon).TwoHanded))
            {
                SwapWeapons();
            }

            if (looting && lootingSoul != null && lootingSoul.Remove && currentAniName != "k_loot_smash")
            {
                CloseLoot();
                lootScroll = 0;
            }

            //loot nearby soul
            if (!looting && attState == AttackState.None && curKeys.IsKeyDown(Keys.Space) && prevKeys.IsKeyUp(Keys.Space) && currentAniName != "k_loot_smash")
            {
                OpenLoot();
                if (!guiOutsideRects.ContainsKey("loot"))
                {
                    guiOutsideRects.Add("loot", lootRect);
                }
            }
            else if (looting && curKeys.IsKeyDown(Keys.Space) && prevKeys.IsKeyUp(Keys.Space) && currentAniName != "k_loot_smash")
            {
                CloseLoot();
                if (guiOutsideRects.ContainsKey("loot"))
                {
                    guiOutsideRects.Remove("loot");
                }
            }
            else if (looting && (lootingSoul == null || lootingSoul.Loot.Count == 0) && currentAniName != "k_loot_smash")
            {
                CloseLoot();
                if (guiOutsideRects.ContainsKey("loot"))
                {
                    guiOutsideRects.Remove("loot");
                }
            }
            else if (looting && currentAniName != "k_loot_smash" && curKeys.IsKeyDown(Keys.Escape) && prevKeys.IsKeyUp(Keys.Escape))
            {
                CloseLoot();
                if (guiOutsideRects.ContainsKey("loot"))
                {
                    guiOutsideRects.Remove("loot");
                }
            }


            //Inventory
            if (curKeys.IsKeyDown(Keys.I) && prevKeys.IsKeyUp(Keys.I))
            {
                showInventory = !showInventory;
                showEquipment = false;

                if (showInventory)
                {
                    if (!guiOutsideRects.ContainsKey("inventory"))
                    {
                        guiOutsideRects.Add("inventory", inventoryRect);
                    }
                }
                else
                {
                    if (guiOutsideRects.ContainsKey("inventory"))
                    {
                        guiOutsideRects.Remove("inventory");
                    }
                }

                if (guiOutsideRects.ContainsKey("equipment"))
                {
                    guiOutsideRects.Remove("equipment");
                }
            }
            //Equipment
            if (curKeys.IsKeyDown(Keys.U) && prevKeys.IsKeyUp(Keys.U))
            {
                showEquipment = !showEquipment;
                showInventory = showEquipment;

                if (showEquipment)
                {
                    if (!guiOutsideRects.ContainsKey("equipment"))
                    {
                        guiOutsideRects.Add("equipment", equipmentRect);
                    }
                    if (!guiOutsideRects.ContainsKey("inventory"))
                    {
                        guiOutsideRects.Add("inventory", inventoryRect);
                    }
                }
                else
                {
                    if (guiOutsideRects.ContainsKey("equipment"))
                    {
                        guiOutsideRects.Remove("equipment");
                    }
                    if (guiOutsideRects.ContainsKey("inventory"))
                    {
                        guiOutsideRects.Remove("inventory");
                    }
                }
            }
            //Talents
            if (curKeys.IsKeyDown(Keys.T) && prevKeys.IsKeyUp(Keys.T))
            {
                showTalents = !showTalents;

                if (showTalents)
                {
                    if (!guiOutsideRects.ContainsKey("talents"))
                    {
                        guiOutsideRects.Add("talents", talentRect);
                    }
                }
                else
                {
                    if (guiOutsideRects.ContainsKey("talents"))
                    {
                        guiOutsideRects.Remove("talents");
                    }
                }
            }
            //Esc closes all
            if (curKeys.IsKeyDown(Keys.Escape) && prevKeys.IsKeyUp(Keys.Escape))
            {
                showEquipment = false;
                if (guiOutsideRects.ContainsKey("equipment"))
                {
                    guiOutsideRects.Remove("equipment");
                }
                showInventory = false;
                if (guiOutsideRects.ContainsKey("inventory"))
                {
                    guiOutsideRects.Remove("inventory");
                }
                showTalents = false;
                if (guiOutsideRects.ContainsKey("talents"))
                {
                    guiOutsideRects.Remove("talents");
                }
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

        string lastAniSuffix = "";
        /// <summary>
        /// checks player input to see what ability to use
        /// </summary>
        /// <param name="gameTime"></param>
        private void CheckAbilities(Vector3 move, bool mouseOnGui)
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

            //check for click when aiming ground target ability
            if (targetingGroundLocation)
            {
                if (curMouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
                {
                    UpdateRotation(dir);
                    lastUsedAbility.Use();
                    UsePower(lastUsedAbility.PowerCost);
                    StartAbilitySequence(lastUsedAbility);
                    return;
                }
                else if (curMouse.LeftButton == ButtonState.Released && prevMouse.LeftButton == ButtonState.Pressed)
                {
                    targetingGroundLocation = false;
                    if (attState == AttackState.Charging)
                    {
                        attState = AttackState.None;
                        CancelFinishSequence();
                        StartSequence("fightingstance");
                    }
                }
            }




            bool useAbility = false;
            Ability abilityToUse = null;

            foreach (KeyValuePair<Keys, Ability> k in boundAbilities)
            {
                if (curKeys.IsKeyDown(k.Key) && prevKeys.IsKeyUp(k.Key) && !k.Value.onCooldown && abilityLearnedFlags[k.Value.AbilityName])
                {
                    if (k.Value.PrimaryType == AttackType.None || GetMainhandType() == k.Value.PrimaryType)
                    {
                        if ((gear[GearSlot.Righthand] as Weapon).TwoHanded)
                        {
                            aniSuffix = "_twohand";
                        }
                        else
                        {
                            aniSuffix = "_r";
                        }
                        useAbility = true;
                        abilityToUse = k.Value;
                    }
                    else if (GetOffhandType() == k.Value.PrimaryType)
                    {
                        aniSuffix = "_l";
                        useAbility = true;
                        abilityToUse = k.Value;
                    }
                    else
                    {
                        (Game as MainGame).AddAlert("You don't have a " + k.Value.PrimaryType.ToString() + " weapon equipped");
                        return;
                    }
                    break;

                }
            }

            foreach (KeyValuePair<ButtonState, Ability> k in mouseBoundAbility)
            {
                if (curMouse.RightButton == ButtonState.Released && prevMouse.RightButton == ButtonState.Pressed && !k.Value.onCooldown && !mouseOnGui && abilityLearnedFlags[k.Value.AbilityName])
                {
                    if (k.Value.PrimaryType == AttackType.None || GetMainhandType() == k.Value.PrimaryType)
                    {
                        if ((gear[GearSlot.Righthand] as Weapon).TwoHanded)
                        {
                            aniSuffix = "_twohand";
                        }
                        else
                        {
                            aniSuffix = "_r";
                        }
                        useAbility = true;
                        abilityToUse = k.Value;
                    }
                    else if (GetOffhandType() == k.Value.PrimaryType)
                    {
                        aniSuffix = "_l";
                        useAbility = true;
                        abilityToUse = k.Value;
                    }
                    else
                    {
                        (Game as MainGame).AddAlert("You don't have a " + k.Value.PrimaryType.ToString() + " weapon equipped");
                        return;
                    }
                    break;
                }
            }

            //mouse click check
            if (!useAbility && abilityToUseString != null)
            {
                for (int i = 0; i < 12; i++)
                {
                    if (abilityToUseString == "ability" + i)
                    {
                        if (!boundAbilities[i].Value.onCooldown)
                        {
                            if (boundAbilities[i].Value.PrimaryType == AttackType.None || GetMainhandType() == boundAbilities[i].Value.PrimaryType)
                            {
                                if ((gear[GearSlot.Righthand] as Weapon).TwoHanded)
                                {
                                    aniSuffix = "_twohand";
                                }
                                else
                                {
                                    aniSuffix = "_r";
                                }
                                useAbility = true;
                                abilityToUse = boundAbilities[i].Value;
                            }
                            else if (GetOffhandType() == boundAbilities[i].Value.PrimaryType)
                            {
                                aniSuffix = "_l";
                                useAbility = true;
                                abilityToUse = boundAbilities[i].Value;
                            }
                            else
                            {
                                (Game as MainGame).AddAlert("You don't have a " + boundAbilities[i].Value.PrimaryType.ToString() + " weapon equipped");
                                return;
                            }
                            abilityToUseString = null;
                        }
                    }
                }
                //rm check
                if (abilityToUseString == "ability12")
                {
                    if (!mouseBoundAbility[0].Value.onCooldown)
                    {
                        useAbility = true;
                        abilityToUse = mouseBoundAbility[0].Value;
                        abilityToUseString = null;
                    }
                }
            }

            if (useAbility)
            {
                targetingGroundLocation = false;
                if (abilityToUse.PowerCost > currentPower)
                {
                    (Game as MainGame).AddAlert("Not Enough Power");
                    return;
                }
                if (attState == AttackState.Charging && abilityToUse.ActionName == currentActionName)
                {
                    abilityToUse.Use();
                    UsePower(abilityToUse.PowerCost);
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
                        UsePower(abilityToUse.PowerCost);
                    }
                    UpdateRotation(dir);
                    StartAbilitySequence(abilityToUse);
                    targetedPhysicalData = null;

                }
                else if (abilityToUse.AbilityType == AbilityType.Instant)
                {
                    abilityToUse.Use();
                    UsePower(abilityToUse.PowerCost);
                    UpdateRotation(dir);
                    StartAbilitySequence(abilityToUse);
                    targetedPhysicalData = null;
                }
                else
                {
                    if (targetingGroundLocation && lastUsedAbility == abilityToUse)
                    {
                        UpdateRotation(dir);
                        abilityToUse.Use();
                        UsePower(abilityToUse.PowerCost);
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
            if (!usingPrimary && (curMouse.LeftButton == ButtonState.Pressed && (curKeys.IsKeyDown(Keys.LeftShift) || curKeys.IsKeyDown(Keys.Space)) 
                || targetedPhysicalData != null))
            {
                //used to differentiate between left hand, right hand, and two hand animations
                aniSuffix = "_r";

                //figure out what kind of weapon is in the main hand
                AttackType mainHandType = GetMainhandType();
                AttackType offHandType = GetOffhandType();

                //figure out if we're dual-wielding the same type of weapon, or using a two-hander, or just attacking with right hand
                if (mainHandType != AttackType.None)
                {
                    if ((gear[GearSlot.Righthand] as Weapon).TwoHanded)
                    {
                        aniSuffix = "_twohand";
                    }
                    else if(offHandType != AttackType.None)
                    {
                        if (lastAniSuffix == "_r")
                        {
                            aniSuffix = "_l";
                        }
                    }
                }
                else if (offHandType != AttackType.None)
                {
                    aniSuffix = "_l";
                }
                else
                {
                    aniSuffix = "_r";
                }

                //remove this once we get more animations
                //aniSuffix = "";

                //attack if in range
                AttackType aniDecidingType = mainHandType;
                if (aniSuffix == "_l")
                {
                    aniDecidingType = offHandType;
                }
                switch (aniDecidingType)
                {
                    case AttackType.None:
                        if (distance < meleeRange)
                        {
                            //need punch animation
                            StartSequence("punch");
                            UpdateRotation(dir);
                        }
                        break;
                    case AttackType.Melee:
                        if (distance < meleeRange)
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
                            StartSequence("magic");
                            UpdateRotation(dir);
                        }
                        break;
                }

                lastAniSuffix = aniSuffix;
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
                    if (attState == AttackState.LockedMoving)
                    {
                        ChangeVelocity(moveVec);
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
            }
            else if (closeEnough || millisRunningCounter >= millisRunTime)
            {
                if (attState == AttackState.LockedMoving)
                {
                    ChangeVelocity(moveVec);
                }
                else
                {
                    //stop if within stopRadius of targetted ground location
                    ChangeVelocity(Vector3.Zero);
                    //just stopped moving
                    if (currentAniName == "k_run")
                    {
                        StartSequence("fightingstance");
                    }
                }
            }
            else
            {
                if (attState == AttackState.LockedMoving)
                {
                    ChangeVelocity(moveVec);
                }
                else
                {
                    //just started running
                    if (currentAniName != "k_run" && attState != AttackState.LockedMoving)
                    {
                        PlayAnimationInterrupt("k_run", MixType.None);
                    }
                    ChangeVelocity(moveVec);
                    UpdateRotation(move);
                }
            }
        }

        #region Helpers
        /// <summary>
        /// Helps to check and equip gear
        /// </summary>
        /// <param name="slot"></param>
        private void equipHelp(GearSlot slot)
        {
            if (selectedItemSlot == -1) //Not equipping anything, pick up slot
            {   //check for weapon swap
                if ((slot == GearSlot.Righthand && selectedEquipSlot && selectedEquipPiece == GearSlot.Lefthand) || (slot == GearSlot.Lefthand && selectedEquipSlot && selectedEquipPiece == GearSlot.Righthand))
                {
                    SwapWeapons();
                    selectedEquipSlot = false;
                }
                else if (selectedEquipSlot) //not swapping weps but equipment already selected
                {
                    selectedEquipSlot = false;
                    if (selectedEquipPiece != slot)
                    {
                        //TODO floating text saying that can't be equipped in that slot. #Jared teach me your ways
                        ((MainGame)Game).AddAlert("I can't equip that there!");
                    }
                }
                else if (gear[slot] == null)
                {
                    //case where nothing is equipped.  TODO Decide if we want to do anything?
                }
                else
                {
                    selectedEquipSlot = true;
                    selectedEquipPiece = slot;
                }
            }
            else    //Bringing item in
            {
                Equippable e = inventory[selectedItemSlot] as Equippable;
                if (e != null && (e.Slot == slot || e.Slot2 == slot))
                {
                    inventory[selectedItemSlot] = null;
                    EquipGear(e, slot);
                    selectedItemSlot = -1;
                }
                else
                {
                    //TODO floating text saying that can't be equipped! #Jared
                }
            }
        }

        private void talentHelp(int check, AbilityNode[,] talents, bool rightclick)
        {
            if (talents[(int)check / 4, check % 4] != null)
            {       //if not learned
                if (!abilityLearnedFlags[talents[(int)check / 4, check % 4].name])
                {       //if can be unlocked
                    if (talents[(int)check / 4, check % 4].canUnlock() && (totalTalentPoints - spentTalentPoints) >= talents[(int)check / 4, check % 4].cost)
                    {       //unlock it
                        spentTalentPoints += talents[(int)check / 4, check % 4].cost;
                        abilityLearnedFlags[talents[(int)check / 4, check % 4].name] = true;
                    }
                    else
                    {   //floating error TODO decide if we like that it's over the interface or not
                        ((MainGame)Game).AddAlert("I can't unlock that yet!");
                    }
                }
                else        //if talent is already learned
                {   //equip in first open slot
                    if (GetAbility(talents[(int)check / 4, check % 4].name).AbilityType == AbilityType.Passive)
                    {
                        //Trying to put a passive on the bar.  Tell them no?
                        ((MainGame)Game).AddAlert("I can't use a passive ability!");
                    }
                    else
                    {
                        if (rightclick)
                        {
                            bool placed = false;
                            for (int i = 0; i < 12; i++)
                            {
                                if (boundAbilities[i].Value.AbilityName == AbilityName.None)
                                {
                                    selectedTalentSlot = check;
                                    checkTalentOnBar(talents);
                                    if (i == 12) mouseBoundAbility[0] = new KeyValuePair<ButtonState, Ability>(mouseBoundAbility[0].Key, GetAbility(talents[selectedTalentSlot / 4, selectedTalentSlot % 4].name));
                                    else boundAbilities[i] = new KeyValuePair<Keys, Ability>(boundAbilities[i].Key, GetAbility(talents[selectedTalentSlot / 4, selectedTalentSlot % 4].name));
                                    placed = true;
                                    selectedTalentSlot = -1;
                                    break;
                                }
                            }
                            if (!placed) selectedTalentSlot = check;
                        }
                        else selectedTalentSlot = check;
                    }
                }
            }
        }

        private void checkTalentOnBar(AbilityNode[,] curentTalentTree)
        {
            for (int i = 0; i < 12; i++)
            {   //if ability is already on bar
                if (boundAbilities[i].Value.AbilityName.Equals(curentTalentTree[selectedTalentSlot / 4, selectedTalentSlot % 4].name))
                {   //set that slot to empty
                    boundAbilities[i] = new KeyValuePair<Keys, Ability>(boundAbilities[i].Key, GetAbility(AbilityName.None));
                }
            }
            //if ability is already on right mouse
            if (mouseBoundAbility[0].Value.AbilityName.Equals(curentTalentTree[selectedTalentSlot / 4, selectedTalentSlot % 4].name))
            {   //set that slot to empty
                mouseBoundAbility[0] = new KeyValuePair<ButtonState, Ability>(mouseBoundAbility[0].Key, GetAbility(AbilityName.None));
            }
        }

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
        /// </summary>
        private string CollidingGuiFrame()
        {
            foreach (KeyValuePair<string, Rectangle> k in guiOutsideRects)
            {
                if (RectContains(k.Value, curMouse.X, curMouse.Y))
                {
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

        private void CheckButtonClicks(string collides)
        {
            //appropriate action for gui element collided with
            //happens on left mouse released
            //#Nate
            #region left click check
            if (collides != null && ((prevMouse.LeftButton == ButtonState.Pressed && curMouse.LeftButton == ButtonState.Released) || (curMouse.LeftButton == ButtonState.Pressed && !dragging) || (curMouse.LeftButton == ButtonState.Released && dragging)))
            {
                string innerClicked = CollidingInnerFrame(collides);
                //Set dragging to true if draggin mouse
                if (curMouse.LeftButton == ButtonState.Pressed && innerClicked != null && !dragging)
                {
                    draggingSource = innerClicked;
                    dragging = true;
                }
                else
                {
                    dragging = false;
                }
                if ((!dragging && draggingSource != innerClicked) || dragging)
                {
                    switch (collides)
                    {
                        #region inventory
                        case "inventory":
                            //if (selectedEquipSlot) selectedEquipSlot = !selectedEquipSlot; #Nate y u do dis
                            if (innerClicked == "equipArrow")
                            {
                                showEquipment = !showEquipment;
                                if (showEquipment)
                                {
                                    if (!guiOutsideRects.ContainsKey("equipment"))
                                    {
                                        guiOutsideRects.Add("equipment", equipmentRect);
                                    }
                                }
                                else
                                {
                                    if (guiOutsideRects.ContainsKey("equipment"))
                                    {
                                        guiOutsideRects.Remove("equipment");
                                    }
                                }
                            }
                            if (innerClicked != null && innerClicked.Contains("inventory"))
                            {
                                for (int i = 0; i <= maxInventorySlots; i++)
                                {
                                    if (innerClicked == "inventory" + i && (inventory[i] != null || inventory[i] == null && selectedItemSlot != -1 || selectedEquipSlot))
                                    {
                                        if (selectedEquipSlot)           //bringing equipment into inventory
                                        {
                                            if (inventory[i] == null)    //Unequip equipment being brought in
                                            {
                                                UnequipGear(selectedEquipPiece, i);
                                                gear[selectedEquipPiece] = null;
                                                selectedEquipSlot = false;
                                                selectedItemSlot = -1;
                                            }
                                            else                        //try and equip selected item
                                            {
                                                selectedItemSlot = i;
                                                equipHelp(selectedEquipPiece);
                                                selectedEquipSlot = false;
                                                selectedItemSlot = -1;
                                            }
                                        }
                                        else if (selectedItemSlot == -1)//no selected item
                                        {
                                            selectedItemSlot = i;       //select item
                                        }
                                        else if (inventory[i] == null)  //put selected item into empty spot
                                        {
                                            inventory[i] = inventory[selectedItemSlot];
                                            inventory[selectedItemSlot] = null;
                                            selectedItemSlot = -1;
                                        }
                                        else                            //switch inventory items
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
                        #endregion
                        #region loot
                        case "loot":
                            if (selectedEquipSlot) selectedEquipSlot = !selectedEquipSlot;
                            if (innerClicked == null) break;
                            //Loot All
                            else if (innerClicked.Equals("lootAll"))
                            {
                                if (lootingSoul != null)
                                {
                                    while (lootingSoul.Loot.Count() > 0)
                                    {
                                        //add items to inventory
                                        if (!AddToInventory(lootingSoul.GetLoot(0)))
                                        {
                                            ((MainGame)Game).AddAlert("Inventory is full");
                                            break;
                                        }
                                        lootingSoul.RemoveLoot(0);
                                    }
                                }
                            }
                            //Scroll Up
                            else if (innerClicked.Equals("upArrow") && lootingSoul.Loot.Count > NUM_LOOT_SHOWN)
                            {
                                lootScroll++;
                            }
                            //Scroll Down
                            else if (innerClicked.Equals("downArrow") && lootScroll > 0)
                            {
                                lootScroll--;
                            }
                            else
                            {//Normal Loot
                                for (int i = 0; i < NUM_LOOT_SHOWN; ++i)
                                {
                                    if (RectContains(guiInsideRects["loot"]["loot" + i], curMouse.X, curMouse.Y))
                                    {
                                        //clicked on item, add to inventory
                                        if (AddToInventory(lootingSoul.GetLoot(i + NUM_LOOT_SHOWN * lootScroll)))
                                        {
                                            lootingSoul.RemoveLoot(i + NUM_LOOT_SHOWN * lootScroll);
                                        }

                                    }
                                }
                            }
                            break;
                        #endregion
                        #region equipment
                        case "equipment":
                            //if (selectedEquipSlot) selectedEquipSlot = !selectedEquipSlot; #Nate  D:<
                            if (innerClicked == "equipWrist") //wrist
                            {
                                equipHelp(GearSlot.Wrist);
                            }
                            else if (innerClicked == "equipBling")  //bling
                            {
                                equipHelp(GearSlot.Bling);
                            }
                            else if (innerClicked == "equipLWep")  //LWep
                            {
                                equipHelp(GearSlot.Lefthand);
                            }

                            else if (innerClicked == "equipHead")  //head
                            {
                                equipHelp(GearSlot.Head);
                            }
                            else if (innerClicked == "equipChest")  //chest
                            {
                                equipHelp(GearSlot.Chest);
                            }
                            else if (innerClicked == "equipLegs")  //legs
                            {
                                equipHelp(GearSlot.Legs);
                            }
                            else if (innerClicked == "equipFeet")  //feet
                            {
                                equipHelp(GearSlot.Feet);
                            }

                            else if (innerClicked == "equipShoulder")  //shoulder
                            {
                                equipHelp(GearSlot.Shoulders);
                            }
                            else if (innerClicked == "equipCod")  //Cod
                            {
                                equipHelp(GearSlot.Codpiece);
                            }
                            else if (innerClicked == "equipRWep")  //RWep
                            {
                                equipHelp(GearSlot.Righthand);
                            }
                            break;
                        #endregion
                        #region abilities
                        case "abilities":
                            if (innerClicked != null && selectedTalentSlot != -1)
                            {
                                int check = Convert.ToInt32(innerClicked.Remove(0, 7));

                                if (currentTalentTree == TalentTrees.ranged)
                                {
                                    checkTalentOnBar(rangedAbilities);
                                    if (check == 12) mouseBoundAbility[0] = new KeyValuePair<ButtonState, Ability>(mouseBoundAbility[0].Key, GetAbility(rangedAbilities[selectedTalentSlot / 4, selectedTalentSlot % 4].name));
                                    else boundAbilities[check] = new KeyValuePair<Keys, Ability>(boundAbilities[check].Key, GetAbility(rangedAbilities[selectedTalentSlot / 4, selectedTalentSlot % 4].name));
                                }
                                else if (currentTalentTree == TalentTrees.melee)
                                {

                                }
                                else if (currentTalentTree == TalentTrees.magic)
                                {

                                }
                                selectedTalentSlot = -1;
                            }
                            else
                            {
                                abilityToUseString = innerClicked;
                            }
                            break;
                        #endregion
                        #region talents
                        case "talents":
                            if (innerClicked != null)
                            {
                                //TODO if we add any more innerFrames in abilities make sure we check those first
                                int check = Convert.ToInt32(innerClicked.Remove(0, 6));
                                if (currentTalentTree == TalentTrees.ranged)
                                {
                                    talentHelp(check, rangedAbilities, false);
                                }
                                else if (currentTalentTree == TalentTrees.melee)
                                {
                                    //talentHelp(check, meleeAbilities);
                                }
                                else if (currentTalentTree == TalentTrees.magic)
                                {
                                    //talentHelp(check, magicAbilities);
                                }
                            }
                            break;
                        #endregion
                    }
                }
            }
            #endregion

            #region right click check
            if (collides != null && prevMouse.RightButton == ButtonState.Pressed && curMouse.RightButton == ButtonState.Released)
            {
                string innerClicked = CollidingInnerFrame(collides);
                switch (collides)
                {
                    #region inventory
                    case "inventory":
                        if (innerClicked != null && innerClicked.Contains("inventory") && selectedItemSlot == -1)  //equip item right clicked on
                        {
                            for (int i = 0; i <= maxInventorySlots; i++)
                            {
                                if (innerClicked == "inventory" + i && inventory[i] != null)
                                {
                                    selectedItemSlot = i;
                                    Equippable e = inventory[i] as Equippable;        //set selectedEquipPiece as inventory[i]
                                    if (e != null)
                                    {
                                        if (e.Slot == GearSlot.Lefthand || e.Slot == GearSlot.Righthand)
                                        {
                                            if (gear[GearSlot.Righthand] == null) selectedEquipPiece = GearSlot.Righthand;      //no weapon in right hand.  Equip into right hand;
                                            else if (gear[GearSlot.Lefthand] == null)
                                            {
                                                if ((gear[GearSlot.Righthand] as Weapon).TwoHanded) selectedEquipPiece = GearSlot.Righthand;
                                                else selectedEquipPiece = GearSlot.Lefthand;
                                            }
                                            else selectedEquipPiece = GearSlot.Righthand;                                       //prefer replacing the right hand if two weapons are equipped
                                        }
                                        else
                                        {
                                            selectedEquipPiece = e.Slot;
                                        }

                                        equipHelp(selectedEquipPiece);
                                        selectedEquipSlot = false;
                                        selectedItemSlot = -1;
                                    }
                                    else
                                    {
                                        //floating text for not equipable
                                    }
                                }
                            }
                        }
                        break;
                    #endregion
                    #region equipment
                    case "equipment":
                        if (innerClicked == "equipWrist") //wrist
                        {
                            UnequipGear(GearSlot.Wrist);
                        }
                        else if (innerClicked == "equipBling")  //bling
                        {
                            UnequipGear(GearSlot.Bling);
                        }
                        else if (innerClicked == "equipLWep")  //LWep
                        {
                            UnequipGear(GearSlot.Lefthand);
                        }

                        else if (innerClicked == "equipHead")  //head
                        {
                            UnequipGear(GearSlot.Head);
                        }
                        else if (innerClicked == "equipChest")  //chest
                        {
                            UnequipGear(GearSlot.Chest);
                        }
                        else if (innerClicked == "equipLegs")  //legs
                        {
                            UnequipGear(GearSlot.Legs); ;
                        }
                        else if (innerClicked == "equipFeet")  //feet
                        {
                            UnequipGear(GearSlot.Feet);
                        }

                        else if (innerClicked == "equipShoulder")  //shoulder
                        {
                            UnequipGear(GearSlot.Shoulders);
                        }
                        else if (innerClicked == "equipCod")  //Cod
                        {
                            UnequipGear(GearSlot.Codpiece);
                        }
                        else if (innerClicked == "equipRWep")  //RWep
                        {
                            UnequipGear(GearSlot.Righthand);
                        }
                        break;
                    #endregion
                    #region talents
                    case "talents":
                        if (innerClicked != null)
                        {
                            //TODO if we add any more innerFrames in abilities make sure we check those first
                            int check = Convert.ToInt32(innerClicked.Remove(0, 6));
                            if (currentTalentTree == TalentTrees.ranged)
                            {
                                talentHelp(check, rangedAbilities, true);
                            }
                            else if (currentTalentTree == TalentTrees.melee)
                            {
                                //talentHelp(check, meleeAbilities);
                            }
                            else if (currentTalentTree == TalentTrees.magic)
                            {
                                //talentHelp(check, magicAbilities);
                            }
                        }
                        break;
                    #endregion
                }

            }
            #endregion
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
        Dictionary<string, Rectangle> tooltipDict;
        //Dictionary<string, Rectangle> chatDict;
        Dictionary<string, Rectangle> playerDict;
        Dictionary<string, Rectangle> mapDict;
        Dictionary<string, Rectangle> xpDict;
        Dictionary<string, Rectangle> inventoryDict;
        Dictionary<string, Rectangle> equipmentDict;
        Dictionary<string, Rectangle> abilityDict;
        Dictionary<string, Rectangle> lootDict;
        Dictionary<string, Rectangle> talentDict;

        Dictionary<string, Rectangle> talentArrowsDict;

        Rectangle inventoryRect;
        Rectangle equipmentRect;
        Rectangle talentRect;
        Rectangle lootRect;
        Rectangle portraitRect;
        Rectangle playerHPRect;
        Rectangle powerBackRect;
        Vector2 powerTextPos;
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
            guiOutsideRects.Add("tooltip", new Rectangle((int)((maxX - 300 * average)), (int)((maxY - 230 * average)), (int)(300 * average), (int)(230 * average)));
            guiOutsideRects.Add("map", new Rectangle((int)((maxX - 344 * average)), 0, (int)(344 * average), (int)(344 * average)));
            guiOutsideRects.Add("player", new Rectangle(0, 0, (int)(470 * average), (int)(160 * average)));

            inventoryRect = new Rectangle((int)(maxX - 400 * average), (int)(380 * average), (int)(402 * average), (int)(440 * average));
            equipmentRect = new Rectangle((int)(maxX - 704 * average), (int)(380 * average), (int)(304 * average), (int)(440 * average));
            talentRect = new Rectangle((int)(maxX / 2 - 203 * average), (int)(75 * average), (int)(406 * average), (int)(738 * average));
            lootRect = new Rectangle((int)(160 * average), (int)(160 * average), (int)(150 * average), (int)(300 * average));
            //guiOutsideRects.Add("chat", new Rectangle(0, (int)((maxY - 444 * average)), (int)(362 * average), (int)(444 * average)));

            guiInsideRects = new Dictionary<string, Dictionary<string, Rectangle>>();
            equipmentDict = new Dictionary<string, Rectangle>();
            inventoryDict = new Dictionary<string, Rectangle>();
            abilityDict = new Dictionary<string, Rectangle>();
            lootDict = new Dictionary<string, Rectangle>();
            //chatDict = new Dictionary<string, Rectangle>();
            playerDict = new Dictionary<string, Rectangle>();
            mapDict = new Dictionary<string, Rectangle>();
            xpDict = new Dictionary<string, Rectangle>();
            tooltipDict = new Dictionary<string, Rectangle>();
            talentDict = new Dictionary<string, Rectangle>();
            talentArrowsDict = new Dictionary<string, Rectangle>();


            //Add frame dictionaries
            guiInsideRects.Add("inventory", inventoryDict);
            guiInsideRects.Add("equipment", equipmentDict);
            guiInsideRects.Add("abilities", abilityDict);
            guiInsideRects.Add("loot", lootDict);
            guiInsideRects.Add("xp", xpDict);
            guiInsideRects.Add("tooltip", tooltipDict);
            guiInsideRects.Add("map", mapDict);
            //guiInsideRects.Add("chat", chatDict);
            guiInsideRects.Add("player", playerDict);
            guiInsideRects.Add("talents", talentDict);

            //Equipment inner
            equipmentDict.Add("equipWrist", new Rectangle((int)(maxX - 694 * average), (int)(458 * average), (int)(88 * average), (int)(88 * average)));
            equipmentDict.Add("equipBling", new Rectangle((int)(maxX - 694 * average), (int)(556 * average), (int)(88 * average), (int)(88 * average)));
            equipmentDict.Add("equipLWep", new Rectangle((int)(maxX - 694 * average), (int)(654 * average), (int)(88 * average), (int)(88 * average)));

            equipmentDict.Add("equipHead", new Rectangle((int)(maxX - 596 * average), (int)(409 * average), (int)(88 * average), (int)(88 * average)));
            equipmentDict.Add("equipChest", new Rectangle((int)(maxX - 596 * average), (int)(507 * average), (int)(88 * average), (int)(88 * average)));
            equipmentDict.Add("equipLegs", new Rectangle((int)(maxX - 596 * average), (int)(605 * average), (int)(88 * average), (int)(88 * average)));
            equipmentDict.Add("equipFeet", new Rectangle((int)(maxX - 596 * average), (int)(703 * average), (int)(88 * average), (int)(88 * average)));

            equipmentDict.Add("equipShoulder", new Rectangle((int)(maxX - 498 * average), (int)(458 * average), (int)(88 * average), (int)(88 * average)));
            equipmentDict.Add("equipCod", new Rectangle((int)(maxX - 498 * average), (int)(556 * average), (int)(88 * average), (int)(88 * average)));
            equipmentDict.Add("equipRWep", new Rectangle((int)(maxX - 498 * average), (int)(654 * average), (int)(88 * average), (int)(88 * average)));
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
            abilityDict.Add("ability12", new Rectangle((int)((maxX / 2 + 79 * average)), (int)((maxY - 111 * average)), (int)(64 * average), (int)(64 * average)));
            //abilities 0-7
            for (int i = 0; i < 4; ++i)
            {
                abilityDict.Add("ability" + i, new Rectangle((int)((maxX / 2 - (301 - 74 * i) * average)), (int)((maxY - 148 * average)), (int)(64 * average), (int)(64 * average)));
                abilityDict.Add("ability" + (i + 4), new Rectangle((int)((maxX / 2 - (301 - 74 * i) * average)), (int)((maxY - 74 * average)), (int)(64 * average), (int)(64 * average)));
            }
            abilityDict.Add("ability8", new Rectangle((int)((maxX / 2 + 163 * average)), (int)((maxY - 148 * average)), (int)(64 * average), (int)(64 * average)));
            abilityDict.Add("ability9", new Rectangle((int)((maxX / 2 + 237 * average)), (int)((maxY - 148 * average)), (int)(64 * average), (int)(64 * average)));
            abilityDict.Add("ability10", new Rectangle((int)((maxX / 2 + 163 * average)), (int)((maxY - 74 * average)), (int)(64 * average), (int)(64 * average)));
            abilityDict.Add("ability11", new Rectangle((int)((maxX / 2 + 237 * average)), (int)((maxY - 74 * average)), (int)(64 * average), (int)(64 * average)));

            //Talents
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 7; j++)
                {
                    //"q" is bottomright, "w" is bottomleft ("br" and "bl" made string too long)
                    talentDict.Add("talent" + (i + j * 4), new Rectangle((int)(maxX / 2 - 203 * average + 30 * average + i * 94 * average), (int)(75 * average + 30 * average + j * 94 * average), (int)(64 * average), (int)(64 * average)));
                    talentArrowsDict.Add("arrowL" + (i + j * 4), new Rectangle((int)(maxX / 2 - 233 * average + 30 * average + i * 94 * average), (int)(92 * average + 30 * average + j * 94 * average), (int)(30 * average), (int)(30 * average)));
                    talentArrowsDict.Add("arrowW" + (i + j * 4), new Rectangle((int)(maxX / 2 - 233 * average + 30 * average + i * 94 * average), (int)(139 * average + 30 * average + j * 94 * average), (int)(30 * average), (int)(30 * average)));
                    talentArrowsDict.Add("arrowB" + (i + j * 4), new Rectangle((int)(maxX / 2 - 186 * average + 30 * average + i * 94 * average), (int)(139 * average + 30 * average + j * 94 * average), (int)(30 * average), (int)(30 * average)));
                    talentArrowsDict.Add("arrowQ" + (i + j * 4), new Rectangle((int)(maxX / 2 - 139 * average + 30 * average + i * 94 * average), (int)(139 * average + 30 * average + j * 94 * average), (int)(30 * average), (int)(30 * average)));
                    talentArrowsDict.Add("arrowR" + (i + j * 4), new Rectangle((int)(maxX / 2 - 139 * average + 30 * average + i * 94 * average), (int)(92 * average + 30 * average + j * 94 * average), (int)(30 * average), (int)(30 * average)));
                }
            }

            //Loot
            for (int i = 0; i < NUM_LOOT_SHOWN; ++i)
            {
                lootDict.Add("loot" + i, new Rectangle((int)(175 * average), (int)(175 * average + i * 65), (int)(50 * average), (int)(50 * average)));
            }
            //up arrow
            lootDict.Add("upArrow", new Rectangle((int)(280 * average), (int)(400 * average), (int)(25 * average), (int)(25 * average)));
            //down arrow
            lootDict.Add("downArrow", new Rectangle((int)(280 * average), (int)(430 * average), (int)(25 * average), (int)(25 * average)));
            //loot all button
            lootDict.Add("lootAll", new Rectangle((int)(265 * average), (int)(165 * average), (int)(40 * average), (int)(40 * average)));



            portraitRect = new Rectangle(0, 0, (int)(160 * average), (int)(160 * average));
            playerHPRect = new Rectangle((int)(160 * average), 0, (int)(310 * average), (int)(52 * average));
            powerBackRect = new Rectangle(playerHPRect.X, playerHPRect.Y + playerHPRect.Height, playerHPRect.Width, playerHPRect.Height);
            powerTextPos = new Vector2((int)(265 * average), powerBackRect.Y + 15);
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
            //s.Draw(texWhitePixel, guiOutsideRects["chat"], Color.Black * 0.5f);

            #region Ability Bar
            //Ability Bar
            s.Draw(texWhitePixel, guiOutsideRects["abilities"], Color.Red * 0.5f);
            for (int i = 0; i < 12; ++i)
            {
                s.Draw(boundAbilities[i].Value.icon, guiInsideRects["abilities"]["ability" + i], Color.White);
            }

            //LM
            if (gear[GearSlot.Righthand] == null) s.Draw(texPlaceHolder, guiInsideRects["abilities"]["primary"], Color.White);
            else s.Draw(gear[GearSlot.Righthand].Icon, guiInsideRects["abilities"]["primary"], Color.White);

            //RM
            s.Draw(mouseBoundAbility[0].Value.icon, guiInsideRects["abilities"]["ability12"], Color.White);


            #endregion

            #region Ability Bar Mods
            #region Cooldown Mod
            //TODO make into for loop for all bound abilities / items
            //s.Draw(texWhitePixel, new Rectangle((int)((maxX / 2 - (301 - 74 * 0) * average)), (int)((maxY - (84 + 64 * (boundAbilities[0].timeRemaining / boundAbilities[0].cooldownSeconds)) * average)), (int)(64 * average), (int)(64 * (boundAbilities[0].timeRemaining / boundAbilities[0].cooldownSeconds) * average) + 1), Color.Black * 0.5f);

            for (int i = 0; i < 12; i++)
            {
                if (!abilityLearnedFlags[boundAbilities[i].Value.AbilityName])
                {
                    s.Draw(texWhitePixel, guiInsideRects["abilities"]["ability" + i], Color.Black * .5f);
                }
                else if (boundAbilities[i].Value.onCooldown)
                {
                    s.Draw(texWhitePixel, new Rectangle(guiInsideRects["abilities"]["ability" + i].X, (int)(guiInsideRects["abilities"]["ability" + i].Y + 64 * average - (64 * (boundAbilities[i].Value.cooldownMillisRemaining / boundAbilities[i].Value.cooldownMillisLength)) * average), (int)(64 * average), (int)(64 * (boundAbilities[i].Value.cooldownMillisRemaining / boundAbilities[i].Value.cooldownMillisLength) * average) + 1), Color.Black * 0.5f);
                    //s.Draw(texWhitePixel, new Rectangle((int)((maxX / 2 - (301 - 74 * i) * average)), (int)((maxY - (84 + 64 * (boundAbilities[i].Value.cooldownMillisRemaining / boundAbilities[i].Value.cooldownMillisLength)) * average)), (int)(64 * average), (int)(64 * (boundAbilities[i].Value.cooldownMillisRemaining / boundAbilities[i].Value.cooldownMillisLength) * average) + 1), Color.Black * 0.5f);
                }
            }
            //RM ability
            if (!abilityLearnedFlags[mouseBoundAbility[0].Value.AbilityName])
            {
                s.Draw(texWhitePixel, guiInsideRects["abilities"]["ability12"], Color.Black * .5f);
            }
            else if (mouseBoundAbility[0].Value.onCooldown)
            {
                s.Draw(texWhitePixel, new Rectangle(guiInsideRects["abilities"]["ability12"].X, (int)(guiInsideRects["abilities"]["ability12"].Y + 64 * average - (64 * (mouseBoundAbility[0].Value.cooldownMillisRemaining / mouseBoundAbility[0].Value.cooldownMillisLength)) * average), (int)(64 * average), (int)(64 * (mouseBoundAbility[0].Value.cooldownMillisRemaining / mouseBoundAbility[0].Value.cooldownMillisLength) * average) + 1), Color.Black * 0.5f);
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
            Rectangle xpRect = guiOutsideRects["xp"];
            s.Draw(texWhitePixel, xpRect, Color.Brown * 0.5f);
            s.Draw(texWhitePixel, new Rectangle(xpRect.X, xpRect.Y, xpRect.Width * experience / NextLevelXP, xpRect.Height), Color.Purple);
            //Damage Tracker
            
            s.Draw(texWhitePixel, guiOutsideRects["tooltip"], Color.Green * 0.5f);
            //Mini Map (square for now)
            s.Draw(texWhitePixel, guiOutsideRects["map"], Color.Orange * 0.5f);

            #region main player frame
            //Main Player Frame Pic
            s.Draw(texWhitePixel, portraitRect, Color.Blue * 0.5f);
            //Main Player Frame Health
            s.Draw(texWhitePixel, playerHPRect, Color.Blue * 0.5f);
            //power
            s.Draw(texWhitePixel, powerBackRect, Color.Black);
            s.Draw(texWhitePixel, new Rectangle(powerBackRect.X, powerBackRect.Y, powerBackRect.Width * currentPower / maxPower, powerBackRect.Height), Color.Yellow);
            s.Draw(texChargeBarFront, powerBackRect, Color.Red);
            s.DrawString(font, currentPower + "", powerTextPos, Color.White, 0, Vector2.Zero, average * .5f, SpriteEffects.None, 0);
            s.DrawString(font, "    /" + maxPower, powerTextPos, Color.White, 0, Vector2.Zero, average * .5f, SpriteEffects.None, 0);
            //main player health
            s.Draw(health_bar, new Rectangle((int)(163 * average), 3, (int)(304 * HealthPercent * average), (int)(46 * average)), new Rectangle(0, 0, (int)(health_bar.Width * HealthPercent * average), (int)health_bar.Height), Color.White);
            #endregion

            #region extra player frames
            /*
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
             */
            #endregion

            #region inventory / Equipment
            if (showInventory)
            {

                //Equipment pane
                if (showEquipment)
                {
                    s.Draw(texWhitePixel, guiOutsideRects["equipment"], Color.Black * .5f);
                    //Collapse arrow
                    s.Draw(rightArrow, guiInsideRects["inventory"]["equipArrow"], Color.White);

                    //Equip icons
                    if (gear[GearSlot.Wrist] == null) s.Draw(texPlaceHolder, guiInsideRects["equipment"]["equipWrist"], Color.White);
                    else s.Draw(gear[GearSlot.Wrist].Icon, guiInsideRects["equipment"]["equipWrist"], Color.White);

                    if (gear[GearSlot.Bling] == null) s.Draw(texPlaceHolder, guiInsideRects["equipment"]["equipBling"], Color.White);
                    else s.Draw(gear[GearSlot.Bling].Icon, guiInsideRects["equipment"]["equipBling"], Color.White);

                    if (gear[GearSlot.Lefthand] == null) s.Draw(texPlaceHolder, guiInsideRects["equipment"]["equipLWep"], Color.White);
                    else s.Draw(gear[GearSlot.Lefthand].Icon, guiInsideRects["equipment"]["equipLWep"], Color.White);

                    if (gear[GearSlot.Head] == null) s.Draw(texPlaceHolder, guiInsideRects["equipment"]["equipHead"], Color.White);
                    else s.Draw(gear[GearSlot.Head].Icon, guiInsideRects["equipment"]["equipHead"], Color.White);

                    if (gear[GearSlot.Chest] == null) s.Draw(texPlaceHolder, guiInsideRects["equipment"]["equipChest"], Color.White);
                    else s.Draw(gear[GearSlot.Chest].Icon, guiInsideRects["equipment"]["equipChest"], Color.White);

                    if (gear[GearSlot.Legs] == null) s.Draw(texPlaceHolder, guiInsideRects["equipment"]["equipLegs"], Color.White);
                    else s.Draw(gear[GearSlot.Legs].Icon, guiInsideRects["equipment"]["equipLegs"], Color.White);

                    if (gear[GearSlot.Feet] == null) s.Draw(texPlaceHolder, guiInsideRects["equipment"]["equipFeet"], Color.White);
                    else s.Draw(gear[GearSlot.Feet].Icon, guiInsideRects["equipment"]["equipFeet"], Color.White);

                    if (gear[GearSlot.Shoulders] == null) s.Draw(texPlaceHolder, guiInsideRects["equipment"]["equipShoulder"], Color.White);
                    else s.Draw(gear[GearSlot.Shoulders].Icon, guiInsideRects["equipment"]["equipShoulder"], Color.White);

                    if (gear[GearSlot.Codpiece] == null) s.Draw(texPlaceHolder, guiInsideRects["equipment"]["equipCod"], Color.White);
                    else s.Draw(gear[GearSlot.Codpiece].Icon, guiInsideRects["equipment"]["equipCod"], Color.White);

                    if (gear[GearSlot.Righthand] == null) s.Draw(texPlaceHolder, guiInsideRects["equipment"]["equipRWep"], Color.White);
                    else s.Draw(gear[GearSlot.Righthand].Icon, guiInsideRects["equipment"]["equipRWep"], Color.White);


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
                    if (inventory[i] != null)
                    {
                        s.Draw(inventory[i].Icon, guiInsideRects["inventory"]["inventory" + i], Color.White);
                    }

                }
            }
            //Draw Loot
            if (looting && lootingSoul != null && guiOutsideRects.ContainsKey("loot"))
            {
                s.Draw(texWhitePixel, guiOutsideRects["loot"], Color.Black * .5f);

                List<Item> loot = lootingSoul.Loot;
                for (int i = 0; i + lootScroll * NUM_LOOT_SHOWN < loot.Count && i < NUM_LOOT_SHOWN; ++i)
                {
                    s.Draw(loot[i + lootScroll*NUM_LOOT_SHOWN].Icon, guiInsideRects["loot"]["loot" + i], Color.White);
                }
                if (lootingSoul.Loot.Count() > NUM_LOOT_SHOWN + NUM_LOOT_SHOWN * lootScroll)
                {
                    s.Draw(talentArrowDown, guiInsideRects["loot"]["downArrow"], Color.White);
                }
                else
                {
                    s.Draw(texPlaceHolder, guiInsideRects["loot"]["downArrow"], Color.Black * .5f);
                }
                if (lootScroll > 0)
                {

                    s.Draw(talentArrowUp, guiInsideRects["loot"]["upArrow"], Color.White);
                }
                else
                {
                    s.Draw(texPlaceHolder, guiInsideRects["loot"]["upArrow"], Color.Black * .5f);
                }

                s.Draw(texPlaceHolder, guiInsideRects["loot"]["lootAll"], Color.White);
            }
            #endregion

            #region talents
            if (showTalents)
            {
                s.Draw(texWhitePixel, guiOutsideRects["talents"], Color.Pink * .5f);

                //Draw Talent Icons
                for (int i = 0; i < 4; ++i)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        if (rangedAbilities[j, i] != null)
                        {       //Draw active frames around active items
                            if (GetAbility(rangedAbilities[j, i].name).AbilityType != AbilityType.Passive && abilityLearnedFlags[rangedAbilities[j, i].name])
                            {
                                Rectangle temp = guiInsideRects["talents"]["talent" + (i + j * 4)];
                                temp.X = temp.X - 4;
                                temp.Y = temp.Y - 4;
                                temp.Width = temp.Width + 8;
                                temp.Height = temp.Height + 8;
                                s.Draw(texWhitePixel, temp, Color.Red * .8f);
                            }
                            //Draw Icon
                            s.Draw(GetAbility(rangedAbilities[j, i].name).icon, guiInsideRects["talents"]["talent" + (i + j * 4)], Color.White);
                            //Draw shadow over locked abilities
                            if (!abilityLearnedFlags[rangedAbilities[j, i].name])
                            {   //can be unlocked
                                if (rangedAbilities[j, i].canUnlock())
                                {
                                    s.Draw(texWhitePixel, guiInsideRects["talents"]["talent" + (i + j * 4)], Color.White * .5f);
                                    s.DrawString(font, rangedAbilities[j, i].cost.ToString(), new Vector2(guiInsideRects["talents"]["talent" + (i + j * 4)].X, guiInsideRects["talents"]["talent" + (i + j * 4)].Y), Color.Black, 0, Vector2.Zero, average, SpriteEffects.None, 0);
                                }
                                //locked
                                else s.Draw(texWhitePixel, guiInsideRects["talents"]["talent" + (i + j * 4)], Color.Black * .8f);
                            }
                        }
                    }
                }
                //Draw Ranged arrows
                s.Draw(talentArrowDown, talentArrowsDict["arrowB0"], Color.White);
                s.Draw(talentArrowDown, talentArrowsDict["arrowB1"], Color.White);

                s.Draw(talentArrowDown, talentArrowsDict["arrowB4"], Color.White);
                s.Draw(talentArrowDown, talentArrowsDict["arrowB5"], Color.White);
                s.Draw(talentArrowDown, talentArrowsDict["arrowB6"], Color.White);

                s.Draw(talentArrowDownRight, talentArrowsDict["arrowQ8"], Color.White);
                s.Draw(talentArrowDown, talentArrowsDict["arrowB9"], Color.White);
                s.Draw(talentArrowDown, talentArrowsDict["arrowB10"], Color.White);
                s.Draw(talentArrowUp, talentArrowsDict["arrowB11"], Color.White);

                s.Draw(talentArrowDownLeft, talentArrowsDict["arrowW13"], Color.White);
                s.Draw(talentArrowDown, talentArrowsDict["arrowB13"], Color.White);
                s.Draw(talentArrowRight, talentArrowsDict["arrowR13"], Color.White);
                s.Draw(talentArrowDown, talentArrowsDict["arrowB14"], Color.White);
                s.Draw(talentArrowDownRight, talentArrowsDict["arrowQ14"], Color.White);
                s.Draw(talentArrowLeft, talentArrowsDict["arrowL15"], Color.White);

                s.Draw(talentArrowDown, talentArrowsDict["arrowB16"], Color.White);
                s.Draw(talentArrowDown, talentArrowsDict["arrowB17"], Color.White);
                s.Draw(talentArrowDownLeft, talentArrowsDict["arrowW19"], Color.White);

                s.Draw(talentArrowDownLeft, talentArrowsDict["arrowW21"], Color.White);
                s.Draw(talentArrowDown, talentArrowsDict["arrowB21"], Color.White);



                //Draw Talent Points
                s.DrawString(font, (totalTalentPoints - spentTalentPoints) + "/" + totalTalentPoints, vecName, Color.Black, 0, Vector2.Zero, average, SpriteEffects.None, 0);
            }
            #endregion
            #endregion

            #region mouse
            rectMouse.X = curMouse.X;
            rectMouse.Y = curMouse.Y;
            if (selectedItemSlot != -1)
            {
                rectMouse.Width = (int)(60 * average);
                rectMouse.Height = (int)(60 * average);
                s.Draw(inventory[selectedItemSlot].Icon, rectMouse, Color.White);
            }
            else if (selectedEquipSlot)
            {
                rectMouse.Width = (int)(60 * average);
                rectMouse.Height = (int)(60 * average);
                s.Draw(gear[selectedEquipPiece].Icon, rectMouse, Color.White);
            }
            else if (selectedTalentSlot != -1)
            {
                rectMouse.Width = (int)(60 * average);
                rectMouse.Height = (int)(60 * average);
                if (currentTalentTree == TalentTrees.ranged)
                {
                    s.Draw(GetAbility(rangedAbilities[selectedTalentSlot / 4, selectedTalentSlot % 4].name).icon, rectMouse, Color.White);
                }
                else if (currentTalentTree == TalentTrees.melee)
                {

                }
                else
                {

                }

            }
            else
            {
                rectMouse.Width = 25;
                rectMouse.Height = 25;
                s.Draw(texCursor, rectMouse, Color.White);
            }
            #endregion mouse
        }


        protected override void KillAlive()
        {
            currentPower = 0;
            if (currentActionName != "death")
            {
                if (pulling)
                {
                    StopPull();
                }
                StartSequence("death");
            }
        }
    }
}
