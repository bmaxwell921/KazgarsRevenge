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
            texHover = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.HOVER);
            texChargeBarFront = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.CHARGE_BAR_FRONT);

            #region UI Frame Load
            texCursor = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.WHITE_CURSOR);
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
            mapIcon = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.MapIcon);
            characterIcon = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.CharacterIcon);
            inventoryIcon = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.InventoryIcon);
            talentIcon = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.TalentIcon);
            shopFrame = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.Frames.shopFrame);
            #endregion

            #region Ability Image Load
            texActiveTalent = Texture2DUtil.Instance.GetTexture(TextureStrings.UI.ActiveTalent);
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

            #region melee ability array
            meleeAbilities[0, 1] = new AbilityNode(AbilityName.Cleave, null, true, 3, abilityLearnedFlags);
            meleeAbilities[0, 2] = new AbilityNode(AbilityName.Garrote, null, true, 3, abilityLearnedFlags);

            meleeAbilities[1, 1] = new AbilityNode(AbilityName.Decapitation, new AbilityName[] { AbilityName.Cleave }, false, 2, abilityLearnedFlags);
            meleeAbilities[1, 2] = new AbilityNode(AbilityName.ExcruciatingTwist, new AbilityName[] { AbilityName.Garrote }, false, 2, abilityLearnedFlags);

            meleeAbilities[2, 1] = new AbilityNode(AbilityName.Invigoration, new AbilityName[] { AbilityName.Decapitation }, false, 2, abilityLearnedFlags);
            meleeAbilities[2, 2] = new AbilityNode(AbilityName.SadisticFrenzy, new AbilityName[] { AbilityName.ExcruciatingTwist }, false, 2, abilityLearnedFlags);

            meleeAbilities[3, 0] = new AbilityNode(AbilityName.Berserk, new AbilityName[] { AbilityName.ObsidianCoagulation}, false, 2, abilityLearnedFlags);
            meleeAbilities[3, 1] = new AbilityNode(AbilityName.ObsidianCoagulation, new AbilityName[] { AbilityName.Invigoration, AbilityName.SadisticFrenzy, AbilityName.Bash }, false, 4, abilityLearnedFlags);
            meleeAbilities[3, 2] = new AbilityNode(AbilityName.Bash, new AbilityName[] { AbilityName.Charge }, false, 3, abilityLearnedFlags);

            meleeAbilities[4, 0] = new AbilityNode(AbilityName.SecondWind, new AbilityName[] { AbilityName.Berserk }, false, 1, abilityLearnedFlags);
            meleeAbilities[4, 1] = new AbilityNode(AbilityName.DevastatingStrike, new AbilityName[] { AbilityName.ObsidianCoagulation }, false, 5, abilityLearnedFlags);
            meleeAbilities[4, 2] = new AbilityNode(AbilityName.DevastatingReach, new AbilityName[] { AbilityName.DevastatingStrike }, false, 3, abilityLearnedFlags);
            meleeAbilities[4, 3] = new AbilityNode(AbilityName.Charge, null, true, 4, abilityLearnedFlags);

            meleeAbilities[5, 0] = new AbilityNode(AbilityName.RiskyRegeneration, new AbilityName[] { AbilityName.SecondWind }, false, 1, abilityLearnedFlags);
            meleeAbilities[5, 1] = new AbilityNode(AbilityName.Execute, new AbilityName[] { AbilityName.DevastatingStrike }, false, 4, abilityLearnedFlags);
            meleeAbilities[5, 2] = new AbilityNode(AbilityName.Reflect, new AbilityName[] { AbilityName.DevastatingStrike, AbilityName.Headbutt }, false, 2, abilityLearnedFlags);
            meleeAbilities[5, 3] = new AbilityNode(AbilityName.Headbutt, null, true, 3, abilityLearnedFlags);

            meleeAbilities[6, 0] = new AbilityNode(AbilityName.RejuvenatingStrikes, new AbilityName[] { AbilityName.RiskyRegeneration }, false, 1, abilityLearnedFlags);
            meleeAbilities[6, 1] = new AbilityNode(AbilityName.Swordnado, new AbilityName[] { AbilityName.Execute }, false, 10, abilityLearnedFlags);
            meleeAbilities[6, 2] = new AbilityNode(AbilityName.ForcefulThrow, new AbilityName[] { AbilityName.ChainSpear }, false, 1, abilityLearnedFlags);
            meleeAbilities[6, 3] = new AbilityNode(AbilityName.ChainSpear, new AbilityName[] { AbilityName.Headbutt }, false, 2, abilityLearnedFlags);
            #endregion
        }

        AbilityTargetDecal groundIndicator;
        public override void Start()
        {
            groundIndicator = Entity.GetComponent(typeof(AbilityTargetDecal)) as AbilityTargetDecal;
            helpPoPs = HelpPopUp.getTutorial(guiOutsideRects, guiInsideRects);
            base.Start();
        }

        //variables for target
        Entity targetedPhysicalData;
        GameEntity mouseHoveredEntity;
        AliveComponent mouseHoveredHealth;
        PlayerInteractiveController mouseHoveredThing;
        Vector3 lastInteractedPosition = Vector3.Zero;

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
        Texture2D mapIcon;
        Texture2D characterIcon;
        Texture2D inventoryIcon;
        Texture2D talentIcon;
        Texture2D shopFrame;

        //Equipment Base
        Texture2D helmetIcon;
        #endregion

        #region Ability Icons
        Texture2D texActiveTalent;
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
        bool showMegaMap = false;
        string lastClick = null;
        string abilityToUseString = null;
        int selectedItemSlot = -1;
        int itemToDelete = -1;
        int selectedTalentSlot = -1;
        int lootScroll = 0;
        GearSlot selectedEquipPiece;
        bool selectedEquipSlot = false;
        bool dragging = false;
        String draggingSource;
        Tooltip currentTooltip = null;
        bool hovering = false;
        List<HelpPopUp> helpPoPs = new List<HelpPopUp>();
        bool isHelpPopupShown = false;

        List<Buff> buffs = new List<Buff>();
        List<DeBuff> debuffs = new List<DeBuff>();

        List<Item> shopStock = new List<Item>();
        List<Item> buyBack = new List<Item>();

        enum TalentTrees
        {
            ranged,
            melee,
            magic
        }

        TalentTrees currentTalentTree = TalentTrees.melee;


        AbilityNode[,] rangedAbilities = new AbilityNode[7, 4];
        AbilityNode[,] meleeAbilities = new AbilityNode[7, 4];


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
                inPrimarySequence = false;
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

                string outerCollides = CollidingGuiFrame();
                bool mouseOnGui = outerCollides != null || selectedItemSlot != -1 || selectedEquipSlot;
                CheckMouseRay(newTarget, mouseOnGui);

                //if icon is removed from the inventory area
                if (outerCollides == null && (selectedItemSlot != -1 || selectedEquipSlot || selectedTalentSlot != -1) && prevMouse.LeftButton == ButtonState.Pressed && curMouse.LeftButton == ButtonState.Released)
                {
                    //Trash item here?
                    if (selectedItemSlot != -1)
                    {
                        if (!guiOutsideRects.ContainsKey("trashItem"))
                        {
                            guiOutsideRects.Add("trashItem", trashItemRect);
                            itemToDelete = selectedItemSlot;
                        }
                    }

                    selectedItemSlot = -1;
                    selectedTalentSlot = -1;
                    selectedEquipSlot = false;
                }

                string innerCollides = CollidingInnerFrame(outerCollides);

                //Buff stuff
                buffs.Clear();
                debuffs.Clear();

                foreach (KeyValuePair<Buff, PositiveEffect> k in activeBuffs)
                {
                    buffs.Add(k.Key);
                }

                foreach (KeyValuePair<DeBuff, NegativeEffect> k in activeDebuffs)
                {
                    debuffs.Add(k.Key);
                }

                CheckButtonClicks(outerCollides, innerCollides);
                CheckMouseHover(outerCollides, innerCollides);
                

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

                if (((attState != AttackState.Locked && attState != AttackState.LockedMoving) || inPrimarySequence) 
                    && !looting && !inShop && !inEssenceShop && !inBank)
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
                    if (forcedVelocityDir == Vector3.Zero)
                    {
                        ChangeVelocity(Vector3.Zero);
                    }
                    else
                    {
                        ChangeVelocity(forcedVelocityDir);
                    }
                }


                if (Math.Abs(physicalData.Position.X - lastInteractedPosition.X) + Math.Abs(physicalData.Position.Z - lastInteractedPosition.Z) > 75)
                {
                    if (inSoulevator)
                    {
                        ExitSoulevator();
                    }
                    else if (inShop)
                    {
                        ExitShop();
                    }
                    else if (inEssenceShop)
                    {
                        ExitEssenceShop();
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

        #region Update Helpers
        Func<BroadPhaseEntry, bool> rayCastFilter;
        bool RayCastFilter(BroadPhaseEntry entry)
        {
            return entry != physicalData.CollisionInformation
                && entry.CollisionRules.Personal <= CollisionRule.Normal
                && (entry as StaticMesh) == null;
        }

        /// <summary>
        /// creates a raycast from the mouse, and returns the position on the ground that it hits.
        /// Also does a bepu raycast and finds the first enemy it hits, and keeps its AliveComponent
        /// for gui purposes
        /// </summary>
        /// <returns></returns>
        private void CheckMouseRay(bool newTarget, bool mouseOnGui)
        {
            //creating ray from mouse location
            Vector3 castOrigin = Game.GraphicsDevice.Viewport.Unproject(new Vector3(curMouse.X, curMouse.Y, 0), camera.Projection, camera.View, Matrix.Identity);
            Vector3 castdir = Game.GraphicsDevice.Viewport.Unproject(new Vector3(curMouse.X, curMouse.Y, 1), camera.Projection, camera.View, Matrix.Identity) - castOrigin;
            if (castdir != Vector3.Zero)
            {
                castdir.Normalize();
            }
            Ray r = new Ray(castOrigin, castdir);

            //check where on the zero plane the ray hits, to guide the character by the mouse
            float? distance;
            Plane p = new Plane(Vector3.Up, 0);
            r.Intersects(ref p, out distance);
            if (distance.HasValue)
            {
                mouseHoveredLocation = r.Position + r.Direction * distance.Value;
            }

            if (float.IsNaN(mouseHoveredLocation.X) || float.IsNaN(mouseHoveredLocation.Y) || float.IsNaN(mouseHoveredLocation.Z))
            {
                //trying to find nan
                throw new Exception("NaN happened because of raycast?");
                mouseHoveredLocation.X = 0;
                mouseHoveredLocation.Y = 0;
                mouseHoveredLocation.Z = 0;
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
                            if (mouseHoveredEntity.Type == EntityType.Misc || mouseHoveredEntity.Type == EntityType.None || mouseHoveredEntity.Type == EntityType.Player)
                            {
                                ResetTargettedEntity();
                            }
                            //if the left mouse button was either just clicked or not pressed down at all,
                            //or if any other ability input was just clicked
                            else// if (newTarget)
                            {
                                mouseHoveredHealth = mouseHoveredEntity.GetComponent(typeof(AliveComponent)) as AliveComponent;
                                mouseHoveredThing = mouseHoveredEntity.GetComponent(typeof(PlayerInteractiveController)) as PlayerInteractiveController;
                                if (mouseHoveredHealth != null)
                                {
                                    mouseHoveredHealth.Target(); 
                                    if (curMouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
                                    {
                                        targetedPhysicalData = mouseHoveredEntity.GetSharedData(typeof(Entity)) as Entity;
                                    }
                                }
                                else if (mouseHoveredThing != null)
                                {
                                    mouseHoveredThing.Target();
                                    Entity hoveredEnt = mouseHoveredEntity.GetSharedData(typeof(Entity)) as Entity;
                                    lastInteractedPosition = hoveredEnt.Position;
                                    if (curMouse.RightButton == ButtonState.Pressed && prevMouse.RightButton == ButtonState.Released &&
                                        (Math.Abs(physicalData.Position.X - lastInteractedPosition.X) + Math.Abs(physicalData.Position.Z - lastInteractedPosition.Z) < 75.0f))
                                    {
                                        InteractWithEntity(mouseHoveredThing.Type);
                                    }
                                }
                                else
                                {
                                    ResetTargettedEntity();
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
            if (!inCombat && !looting && attState == AttackState.None && curKeys.IsKeyDown(Keys.Space) && prevKeys.IsKeyUp(Keys.Space) && currentAniName != "k_loot_smash")
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
            if ((curKeys.IsKeyDown(Keys.I) && prevKeys.IsKeyUp(Keys.I)) || (curKeys.IsKeyDown(Keys.B) && prevKeys.IsKeyUp(Keys.B)))
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
            if ((curKeys.IsKeyDown(Keys.U) && prevKeys.IsKeyUp(Keys.U)) || (curKeys.IsKeyDown(Keys.C) && prevKeys.IsKeyUp(Keys.C)))
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

            // M brings up big map
            if (curKeys.IsKeyDown(Keys.M) && prevKeys.IsKeyUp(Keys.M))
            {
                showMegaMap = !showMegaMap;
            }

            if (curKeys.IsKeyDown(Keys.OemPlus))
            {
                zoom -= 30;
            }

            if (curKeys.IsKeyDown(Keys.OemMinus))
            {
                zoom += 30;
            }

            if (zoom < 100)
            {
                zoom = 100;
            }

            if (zoom > 1200)
            {
                zoom = 1200;
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
                showMegaMap = false;
                if (inSoulevator)
                {
                    ExitSoulevator();
                }
            }

            //HelpPopUps
            if (!isHelpPopupShown)
            {
                if (helpPoPs.Count > 0 && !guiOutsideRects.ContainsKey("helpPop"))
                {
                    isHelpPopupShown = true;
                    guiOutsideRects.Add("helpPop", helpPopRect);
                }
            }
            else if(helpPoPs.Count <= 0 && guiOutsideRects.ContainsKey("helpPop"))
            {
                guiOutsideRects.Remove("helpPop");
                isHelpPopupShown = false;
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
            //reset targeting state if you press escape
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
                if (dir != Vector3.Zero)
                {
                    dir.Normalize();
                }
            }

            //check for click when aiming ground target ability
            if (targetingGroundLocation)
            {
                if (curMouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
                {
                    UpdateRotation(dir);
                    lastUsedAbility.Use(GetStat(StatType.CooldownReduction));
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
                        if (boundAbilities[i].Value.AbilityName == AbilityName.None)
                        {
                            abilityToUse = null;
                            abilityToUseString = null;
                            useAbility = false;
                            return;
                        }
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
                if (abilityToUseString == "ability12" && mouseBoundAbility[0].Value.AbilityName != AbilityName.None)
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
                    abilityToUse.Use(GetStat(StatType.CooldownReduction));
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
                        abilityToUse.Use(GetStat(StatType.CooldownReduction));
                        UsePower(abilityToUse.PowerCost);
                    }
                    UpdateRotation(dir);
                    StartAbilitySequence(abilityToUse);
                    targetedPhysicalData = null;

                }
                else if (abilityToUse.AbilityType == AbilityType.Instant)
                {
                    abilityToUse.Use(GetStat(StatType.CooldownReduction));
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
                        abilityToUse.Use(GetStat(StatType.CooldownReduction));
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
            if (!inPrimarySequence && (curMouse.LeftButton == ButtonState.Pressed && (curKeys.IsKeyDown(Keys.LeftShift) || curKeys.IsKeyDown(Keys.Space)) 
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

                AttackType aniDecidingType = mainHandType;
                if (aniSuffix == "_l")
                {
                    if (offHandType == AttackType.Melee && distance > meleeRange && (mainHandType == AttackType.Magic || mainHandType == AttackType.Ranged))
                    {
                        aniSuffix = "_r";
                    }
                    else
                    {
                        aniDecidingType = offHandType;
                    }
                }

                //attack if in range
                switch (aniDecidingType)
                {
                    case AttackType.None:
                        if (distance <= meleeRange)
                        {
                            StartSequence("punch");
                            UpdateRotation(dir);
                            lastAniSuffix = aniSuffix;
                        }
                        break;
                    case AttackType.Melee:
                        if (distance <= meleeRange)
                        {
                            StartSequence("swing");
                            UpdateRotation(dir);
                            lastAniSuffix = aniSuffix;
                        }
                        break;
                    case AttackType.Ranged:
                        if (distance <= bowRange)
                        {
                            StartSequence("shoot");
                            UpdateRotation(dir);
                            lastAniSuffix = aniSuffix;
                        }
                        break;
                    case AttackType.Magic:
                        if (distance <= bowRange)
                        {
                            StartSequence("magic");
                            UpdateRotation(dir);
                            lastAniSuffix = aniSuffix;
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
                    if (attState == AttackState.LockedMoving)
                    {
                        ChangeVelocity(moveVec);
                    }
                    else
                    {
                        //just started running
                        if (currentAniName != "k_walk")
                        {
                            PlayAnimationInterrupt("k_walk", MixType.None);
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
                    if (currentAniName == "k_walk")
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
                    if (currentAniName != "k_walk" && attState != AttackState.LockedMoving)
                    {
                        PlayAnimationInterrupt("k_walk", MixType.None);
                    }
                    ChangeVelocity(moveVec);
                    UpdateRotation(move);
                }
            }
        }
        #endregion

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
                    if (!EquipGear(e, slot))
                    {
                        inventory[selectedItemSlot] = e;
                    }
                    selectedItemSlot = -1;
                }
                else
                {
                    ((MainGame)Game).AddAlert("I can't equip that!");
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
                    {   
                        ((MainGame)Game).AddAlert("I can't unlock that yet!");
                    }
                }
                else        //if talent is already learned
                {   //equip in first open slot
                    if (GetCachedAbility(talents[(int)check / 4, check % 4].name).AbilityType == AbilityType.Passive)
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
                                    if (i == 12) mouseBoundAbility[0] = new KeyValuePair<ButtonState, Ability>(mouseBoundAbility[0].Key, GetCachedAbility(talents[selectedTalentSlot / 4, selectedTalentSlot % 4].name));
                                    else boundAbilities[i] = new KeyValuePair<Keys, Ability>(boundAbilities[i].Key, GetCachedAbility(talents[selectedTalentSlot / 4, selectedTalentSlot % 4].name));
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
                    boundAbilities[i] = new KeyValuePair<Keys, Ability>(boundAbilities[i].Key, GetCachedAbility(AbilityName.None));
                }
            }
            //if ability is already on right mouse
            if (mouseBoundAbility[0].Value.AbilityName.Equals(curentTalentTree[selectedTalentSlot / 4, selectedTalentSlot % 4].name))
            {   //set that slot to empty
                mouseBoundAbility[0] = new KeyValuePair<ButtonState, Ability>(mouseBoundAbility[0].Key, GetCachedAbility(AbilityName.None));
            }
        }

        private void ResetTargettedEntity()
        {
            if (mouseHoveredHealth != null)
            {
                mouseHoveredHealth.UnTarget();
            }
            if (mouseHoveredThing != null)
            {
                mouseHoveredThing.UnTarget();
            }
            mouseHoveredEntity = null;
            mouseHoveredHealth = null;
            mouseHoveredThing = null;
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
            if (outsideRec == null)
            {
                return null;
            }
            if (guiInsideRects.ContainsKey(outsideRec))
            {
                foreach (KeyValuePair<string, Rectangle> k in guiInsideRects[outsideRec])
                {
                    if (RectContains(k.Value, curMouse.X, curMouse.Y))
                    {
                        return k.Key;
                    }

                }
            }
            return null;
        }

        private void CheckButtonClicks(string outerCollides, string innerCollides)
        {
            //appropriate action for gui element collided with
            //happens on left mouse released
            #region left click check
            if (curMouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released  && outerCollides == null)
            {
                lastClick = null;
            }
            else if (curMouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released && outerCollides != null)
            {
                lastClick = outerCollides;
            }
            if (outerCollides != null && ((prevMouse.LeftButton == ButtonState.Pressed && curMouse.LeftButton == ButtonState.Released) || (curMouse.LeftButton == ButtonState.Pressed && !dragging) || (curMouse.LeftButton == ButtonState.Released && dragging)))
            {
                //Set dragging to true if draggin mouse
                if (lastClick != null && curMouse.LeftButton == ButtonState.Pressed && innerCollides != null && !dragging)
                {
                    draggingSource = innerCollides;
                    dragging = true;
                }
                else
                {
                    dragging = false;
                }

                if (innerCollides == null)
                {
                    return;
                }

                if (lastClick != null && (draggingSource != innerCollides || dragging))
                {
                    switch (outerCollides)
                    {
                        #region inventory
                        case "inventory":
                            if (innerCollides == "equipArrow")
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
                            if (innerCollides != null && innerCollides.Contains("inventory"))
                            {
                                for (int i = 0; i <= maxInventorySlots; i++)
                                {
                                    if (innerCollides == "inventory" + i && (inventory[i] != null || inventory[i] == null && selectedItemSlot != -1 || selectedEquipSlot))
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
                            if (innerCollides == null || !looting) break;
                            //Loot All
                            else if (innerCollides.Equals("lootAll"))
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
                            else if (innerCollides.Equals("downArrow") && lootingSoul.Loot.Count > NUM_LOOT_SHOWN * (lootScroll + 1))
                            {
                                lootScroll++;
                            }
                            //Scroll Down
                            else if (innerCollides.Equals("upArrow") && lootScroll > 0)
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
                            for (int i = 1; i < Enum.GetNames(typeof(GearSlot)).Length; ++i)
                            {
                                if (innerCollides == ((GearSlot)i).ToString())
                                {
                                    equipHelp((GearSlot)i);
                                }
                            }
                            break;
                        #endregion
                        #region abilities
                        case "abilities":
                            if (innerCollides != null && selectedTalentSlot != -1 && !innerCollides.Equals("primary"))
                            {
                                int check = Convert.ToInt32(innerCollides.Remove(0, 7));

                                if (currentTalentTree == TalentTrees.ranged)
                                {
                                    checkTalentOnBar(rangedAbilities);
                                    if (check == 12)
                                        mouseBoundAbility[0] = new KeyValuePair<ButtonState, Ability>(mouseBoundAbility[0].Key, GetCachedAbility(rangedAbilities[selectedTalentSlot / 4, selectedTalentSlot % 4].name));
                                    else
                                        boundAbilities[check] = new KeyValuePair<Keys, Ability>(boundAbilities[check].Key, GetCachedAbility(rangedAbilities[selectedTalentSlot / 4, selectedTalentSlot % 4].name));
                                }
                                else if (currentTalentTree == TalentTrees.melee)
                                {
                                    checkTalentOnBar(meleeAbilities);
                                    if (check == 12)
                                        mouseBoundAbility[0] = new KeyValuePair<ButtonState, Ability>(mouseBoundAbility[0].Key, GetCachedAbility(meleeAbilities[selectedTalentSlot / 4, selectedTalentSlot % 4].name));
                                    else
                                        boundAbilities[check] = new KeyValuePair<Keys, Ability>(boundAbilities[check].Key, GetCachedAbility(meleeAbilities[selectedTalentSlot / 4, selectedTalentSlot % 4].name));
                                }
                                else if (currentTalentTree == TalentTrees.magic)
                                {

                                }
                                selectedTalentSlot = -1;
                            }
                            else if (selectedItemSlot != -1) //Putting an item onto the ability bar
                            {
                                Potion p = inventory[selectedItemSlot] as Potion;
                                if (p != null)
                                {
                                    int check = Convert.ToInt32(innerCollides.Remove(0, 7));
                                    if (check == 12)
                                    {
                                        mouseBoundAbility[0] = new KeyValuePair<ButtonState, Ability>(mouseBoundAbility[0].Key, GetCachedAbility(p.PotionAbility));
                                    }
                                    else
                                    {
                                        boundAbilities[check] = new KeyValuePair<Keys, Ability>(boundAbilities[check].Key, GetCachedAbility(p.PotionAbility));
                                    }
                                    selectedItemSlot = -1;
                                }
                                else
                                {
                                    //possible future implementation of other items on hotbar
                                    selectedItemSlot = -1;
                                }
                            }
                            else
                            {
                                abilityToUseString = innerCollides;
                            }
                            break;
                        #endregion
                        #region talents
                        case "talents":
                            if (innerCollides != null)
                            {
                                if (innerCollides.Equals("Ranged"))
                                {
                                    currentTalentTree = TalentTrees.ranged;
                                    break;
                                }
                                else if (innerCollides.Equals("Melee"))
                                {
                                    currentTalentTree = TalentTrees.melee;
                                    break;
                                }
                                else if (innerCollides.Equals("Magic"))
                                {
                                    currentTalentTree = TalentTrees.magic;
                                    break;
                                }
                                else if (innerCollides.Equals("Points"))
                                {
                                    break;
                                }
                                //TODO if we add any more innerFrames in abilities make sure we check those first
                                int check = Convert.ToInt32(innerCollides.Remove(0, 6));
                                if (currentTalentTree == TalentTrees.ranged)
                                {
                                    talentHelp(check, rangedAbilities, false);
                                }
                                else if (currentTalentTree == TalentTrees.melee)
                                {
                                    talentHelp(check, meleeAbilities, false);
                                }
                                else if (currentTalentTree == TalentTrees.magic)
                                {
                                    //talentHelp(check, magicAbilities);
                                }
                            }
                            break;
                        #endregion
                        #region help pop ups
                        case "helpPop":
                            if (innerCollides.Equals("ok"))
                            {
                                helpPoPs.RemoveAt(0);
                            }
                            break;
                        #endregion
                        #region buttons
                        case "buttons":
                            if (innerCollides.Equals("map"))
                            {
                                showMegaMap = !showMegaMap;
                            }
                            else if (innerCollides.Equals("inventory"))
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
                            else if (innerCollides.Equals("character"))
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
                            else if (innerCollides.Equals("talents"))
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
                            break;
                        #endregion
                        #region soulevator
                        case "soulevator":
                            int result = -1;
                            if (Int32.TryParse(innerCollides, out result))
                            {
                                if (result >= 0 && result < Enum.GetNames(typeof(FloorName)).Length)
                                {
                                    (Game as MainGame).LoadNextLevel((FloorName)result);
                                    ExitSoulevator();
                                }
                            }
                            break;
                        #endregion
                        #region trashItem
                        case "trashItem":
                            if (innerCollides.Equals("ok")){
                                inventory[itemToDelete] = null;
                                itemToDelete = -1;
                            }
                            else if (innerCollides.Equals("cancel"))
                            {
                                itemToDelete = -1;
                            }
                            guiOutsideRects.Remove("trashItem");
                            break;
                        #endregion
                    }
                }
            }
            #endregion


            #region right click check
            if (outerCollides != null && prevMouse.RightButton == ButtonState.Pressed && curMouse.RightButton == ButtonState.Released)
            {
                switch (outerCollides)
                {
                    #region inventory
                    case "inventory":
                        if (innerCollides != null && innerCollides.Contains("inventory") && selectedItemSlot == -1 && !inShop)  //equip item right clicked on, NOT SHOPPING
                        {
                            for (int i = 0; i <= maxInventorySlots; i++)
                            {
                                if (innerCollides == "inventory" + i && inventory[i] != null)
                                {
                                    Equippable e = inventory[i] as Equippable;        //set selectedEquipPiece as inventory[i]
                                    if (e != null)
                                    {
                                        selectedItemSlot = i;
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
                                        //check for potion use
                                        Potion p = inventory[i] as Potion;
                                        if (p != null)
                                        {
                                            UseSequenceParallel(p.PotionAbility.ToString());
                                        }
                                        else
                                        {
                                            (Game as MainGame).AddAlert("Can't use that");
                                        }
                                    }
                                }
                            }
                        }
                        //Shopping
                        if (innerCollides != null && innerCollides.Contains("inventory") && inShop)
                        {
                            for (int i = 0; i <= maxInventorySlots; i++)
                            {
                                if (innerCollides == "inventory" + i && inventory[i] != null)
                                {
                                    gold += inventory[i].GoldCost;
                                    buyBack.Add(inventory[i]);
                                    currentTooltip = null;
                                    inventory[i] = null;
                                }
                            }
                        }
                        break;
                    #endregion
                    #region equipment
                    case "equipment":
                        for (int i = 1; i < Enum.GetNames(typeof(GearSlot)).Length; ++i)
                        {
                            if (innerCollides == ((GearSlot)i).ToString())
                            {
                                UnequipGear((GearSlot)i);
                            }
                        }
                        break;
                    #endregion
                    #region talents
                    case "talents":
                        if (innerCollides != null)
                        {
                            if (innerCollides.Equals("Ranged") || innerCollides.Equals("Melee") || innerCollides.Equals("Magic") || innerCollides.Equals("Points"))
                                {
                                    break;
                                }
                            //TODO if we add any more innerFrames in abilities make sure we check those first
                            int check = Convert.ToInt32(innerCollides.Remove(0, 6));
                            if (currentTalentTree == TalentTrees.ranged)
                            {
                                talentHelp(check, rangedAbilities, true);
                            }
                            else if (currentTalentTree == TalentTrees.melee)
                            {
                                talentHelp(check, meleeAbilities, true);
                            }
                            else if (currentTalentTree == TalentTrees.magic)
                            {
                                //talentHelp(check, magicAbilities);
                            }
                        }
                        break;
                    #endregion
                    #region player frame
                    case "player":
                        if (innerCollides != null)
                        {                           
                            for (int i = 0; i < 6; i++)
                            {
                                if (innerCollides.Equals("buff" + i) && i < buffs.Count) //&& buffs i is not null
                                {
                                    //remove buff
                                    activeBuffs.Remove(buffs[i]);
                                    buffs.RemoveAt(i);
                                    break;
                                }
                                else
                                {
                                    //do nothing / future implementation
                                }
                            }
                        }
                        break;
                    #endregion
                    #region player frame
                    case "shopKeeper":
                        if (innerCollides != null)
                        {
                            for (int i = 0; i < 6; i++)
                            {
                                if (innerCollides.Equals("itemIcon" + i) || innerCollides.Equals("itemFrame" + i) && i < buffs.Count) 
                                {
                                    
                                }
                                else
                                {
                                    //do nothing / future implementation
                                }
                            }
                        }
                        break;
                    #endregion
                }

            }
            #endregion
        }

        private void CheckMouseHover(string outerCollides, string innerCollides)
        {   //Holding an item.  Set hover over slot to equip
            if (showEquipment && selectedItemSlot != -1)
            {
                Equippable e = inventory[selectedItemSlot] as Equippable;
                if (e != null)
                {
                    hovering = true;
                    hoverRect = guiInsideRects["equipment"][e.Slot.ToString()];
                    return;
                }
                else
                {
                    if (innerCollides == null)
                    {
                        hovering = false;
                        currentTooltip = null;
                        return;
                    }
                }
            }
            else if (innerCollides == null)
            {
                hovering = false;
                currentTooltip = null;
                return;
            }

            switch (outerCollides)
            {
                #region inventory
                case "inventory":
                    if (innerCollides != null && innerCollides.Contains("inventory"))  //hover over inventory slot
                    {
                        for (int i = 0; i <= maxInventorySlots; i++)
                        {
                            if (innerCollides == "inventory" + i && inventory[i] != null)
                            {
                                currentTooltip = inventory[i].Tooltip;
                                hovering = true;
                                hoverRect = guiInsideRects["inventory"]["inventory" + i];
                                break;
                            }
                        }
                    }
                    break;
                #endregion
                #region loot
                case "loot":
                    if (selectedEquipSlot) selectedEquipSlot = !selectedEquipSlot;
                    if (innerCollides == null || !looting) break;
                    //Loot All
                    else if (innerCollides.Equals("lootAll"))
                    {
                        //TODO Loot All Tooltip
                    }
                    else
                    {//Normal Loot
                        for (int i = 0; i < NUM_LOOT_SHOWN; ++i)
                        {
                            if (innerCollides.Equals("loot" + i) && lootingSoul.GetLoot(i) != null)
                            {
                                currentTooltip = lootingSoul.GetLoot(i).Tooltip;
                                hoverRect = guiInsideRects["loot"]["loot" + i];
                            }
                        }
                    }
                    break;
                #endregion
                #region equipment
                case "equipment":
                    for (int i = 0; i < Enum.GetNames(typeof(GearSlot)).Length; ++i)
                    {
                        if (innerCollides == ((GearSlot)i).ToString() && gear[(GearSlot)i] != null)
                        {
                            currentTooltip = gear[(GearSlot)i].Tooltip;
                            hovering = true;
                            hoverRect = guiInsideRects["equipment"][((GearSlot)i).ToString()];
                            break;
                        }
                    }
                    break;
                #endregion
                #region abilities
                case "abilities":
                    if (innerCollides != null && !innerCollides.Equals("primary"))
                    {
                        int check = Convert.ToInt32(innerCollides.Remove(0, 7));
                        if (check == 12)
                        {
                            currentTooltip = mouseBoundAbility[0].Value.Tooltip;
                        }
                        else
                        {
                            currentTooltip = boundAbilities[check].Value.Tooltip;
                        }
                        hovering = true;
                        hoverRect = guiInsideRects["abilities"]["ability"+check];
                    }
                    break;
                #endregion
                #region player frame
                case "player":
                    if (innerCollides != null)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            if(innerCollides.Equals("buff"+i) && i < buffs.Count()) //&& buffs i is not null
                            {
                                currentTooltip = buffTooltips[buffs[i]];
                                hovering = true;
                                hoverRect = guiInsideRects["player"]["buff" + i];
                            }
                            else if (innerCollides.Equals("debuff" + i) && i < debuffs.Count()) //&& debuffs i is not null
                            {
                                currentTooltip = debuffTooltips[debuffs[i]];
                                hovering = true;
                                hoverRect = guiInsideRects["player"]["debuff" + i];
                            }
                            else{
                                //do nothing / future implementation
                            }
                        }
                    }
                    break;
                #endregion
                #region talents
                case "talents":
                    if (innerCollides != null)
                    {
                        if (innerCollides.Equals("Ranged"))
                        {
                            hovering = true;
                            hoverRect = guiInsideRects["talents"]["Ranged"];
                            return;
                        }
                        else if (innerCollides.Equals("Melee"))
                        {
                            hovering = true;
                            hoverRect = guiInsideRects["talents"]["Melee"];
                            return;
                        }
                        else if (innerCollides.Equals("Magic"))
                        {
                            hovering = true;
                            hoverRect = guiInsideRects["talents"]["Magic"];
                            return;
                        }
                        else if (innerCollides.Equals("Points"))
                        {
                            currentTooltip = new Tooltip(new List<TooltipLine> { new TooltipLine(Color.White, "Talent Points", .65f), new TooltipLine(Color.Gold, "Spend these on talents!", .4f) });
                            break;
                        }
                        //TODO if we add any more innerFrames in abilities make sure we check those first
                        int check = Convert.ToInt32(innerCollides.Remove(0, 6));
                        int i = check / 4;
                        int j = check % 4;
                        if (currentTalentTree == TalentTrees.ranged && rangedAbilities[i, j] != null && rangedAbilities[i, j].name != AbilityName.None)
                        {
                            currentTooltip = GetCachedAbility(rangedAbilities[i, j].name).Tooltip;
                            hovering = true;
                            hoverRect = guiInsideRects["talents"][innerCollides];
                        }
                        else if (currentTalentTree == TalentTrees.melee && meleeAbilities[i, j] != null && meleeAbilities[i, j].name != AbilityName.None)
                        {
                            currentTooltip = GetCachedAbility(meleeAbilities[i, j].name).Tooltip;
                            hovering = true;
                            hoverRect = guiInsideRects["talents"][innerCollides];
                        }
                        else if (currentTalentTree == TalentTrees.magic)  //TODO add check as above
                        {
                            //TODO currentToolTip = GetAbility(magicAbilities[(int)check / 4, check % 4].name).tooltip;
                            //hovering = true;
                            //hoverRect = guiInsideRects["talents"][innerCollides];
                        }

                    }
                    break;
                #endregion
                #region soulevator
                case "soulevator":
                    int result = -1;
                    if (Int32.TryParse(innerCollides, out result))
                    {
                        if (result >= 0 && result < Enum.GetNames(typeof(FloorName)).Length)
                        {
                            hovering = true;
                            hoverRect = guiInsideRects["soulevator"][result + ""];
                        }
                    }
                    break;
                #endregion
                #region trashItem
                case "trashItem":
                    if (innerCollides.Equals("ok"))
                    {
                        hovering = true;
                        hoverRect = guiInsideRects["trashItem"]["ok"];
                    }
                    else if (innerCollides.Equals("cancel"))
                    {
                        hovering = true;
                        hoverRect = guiInsideRects["trashItem"]["cancel"];
                    }
                    break;
                #endregion
                case "helpPop":
                    if (innerCollides == "ok")
                    {
                        hovering = true;
                        hoverRect = guiInsideRects["helpPop"]["ok"];
                    }
                    break;
                case "buttons":
                    hovering = true;
                    hoverRect = guiInsideRects[outerCollides][innerCollides];
                    break;
                default:
                    currentTooltip = null;
                    break;
            }
        }

        private void drawTalentHelper(SpriteBatch s, AbilityNode[,] currentTree)
        {
            //Draw Talent Icons
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 7; j++)
                {
                    if (currentTree[j, i] != null)
                    {
                        //Draw Icon
                        s.Draw(GetCachedAbility(currentTree[j, i].name).icon, guiInsideRects["talents"]["talent" + (i + j * 4)], Color.White);
                        //Draw shadow over locked abilities
                        if (!abilityLearnedFlags[currentTree[j, i].name])
                        {   //can be unlocked
                            if (currentTree[j, i].canUnlock())
                            {
                                s.Draw(texWhitePixel, guiInsideRects["talents"]["talent" + (i + j * 4)], Color.White * .5f);
                                s.DrawString(font, currentTree[j, i].cost.ToString(), new Vector2(guiInsideRects["talents"]["talent" + (i + j * 4)].X, guiInsideRects["talents"]["talent" + (i + j * 4)].Y), Color.Black, 0, Vector2.Zero, average, SpriteEffects.None, 0);
                            }
                            //locked
                            else s.Draw(texWhitePixel, guiInsideRects["talents"]["talent" + (i + j * 4)], Color.Black * .8f);
                        }   
                        
                        //Draw active frames around active items
                        if (GetCachedAbility(currentTree[j, i].name).AbilityType != AbilityType.Passive && abilityLearnedFlags[currentTree[j, i].name])
                        {
                            Rectangle temp = guiInsideRects["talents"]["talent" + (i + j * 4)];
                            temp.X = (int)(temp.X - 6 * average);
                            temp.Y = (int)(temp.Y - 6 * average);
                            temp.Width = (int)(temp.Width + 12 * average);
                            temp.Height = (int)(temp.Height + 12 * average);
                            s.Draw(texActiveTalent, temp, Color.Red * .8f);
                        }
                    }
                }
            }

        }

        //variables for menus
        protected bool inSoulevator = false;
        protected bool inShop = false;
        protected bool inEssenceShop = false;
        protected bool inBank = false;
        /// <summary>
        /// called by the soulevator controller when the player runs into it
        /// </summary>
        public virtual void EnterSoulevator(Vector3 position)
        {
            lastInteractedPosition = position;
            inSoulevator = true;
            if (!guiOutsideRects.ContainsKey("soulevator"))
            {
                guiOutsideRects.Add("soulevator", soulevatorRect);
            }
        }
        /// <summary>
        /// called by the player when exiting the soulevator
        /// </summary>
        protected virtual void ExitSoulevator()
        {
            inSoulevator = false;
            if (guiOutsideRects.ContainsKey("soulevator"))
            {
                guiOutsideRects.Remove("soulevator");
            }
        }

        private void InteractWithEntity(InteractiveType type)
        {
            switch (type)
            {
                case InteractiveType.Shopkeeper:
                    //#Nate start drawing shop gui here
                    if (!guiOutsideRects.ContainsKey("shopKeeper"))
                    {
                        inShop = true;
                        guiOutsideRects.Add("shopKeeper", shopKeeperRect);
                        showInventory = true;
                        if (!guiOutsideRects.ContainsKey("inventory"))
                        {
                            guiOutsideRects.Add("inventory", inventoryRect);
                        }
                    }
                    break;
            }
        }

        private void ExitShop()
        {
            if (guiOutsideRects.ContainsKey("shopKeeper"))
            {
                guiOutsideRects.Remove("shopKeeper");
            }
            inShop = false;
        }

        private void ExitEssenceShop()
        {
            inEssenceShop = false;
        }
        #endregion

        #region Draw Parameter Setup
        SpriteFont font;
        Texture2D texWhitePixel;
        Texture2D texHover;
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
        Dictionary<string, Rectangle> megaMapDict;
        IDictionary<string, string> mapImgDict;
        Dictionary<string, Rectangle> xpDict;
        Dictionary<string, Rectangle> inventoryDict;
        Dictionary<string, Rectangle> equipmentDict;
        Dictionary<string, Rectangle> abilityDict;
        Dictionary<string, Rectangle> lootDict;
        Dictionary<string, Rectangle> talentDict;
        Dictionary<string, Rectangle> helpPopDict;
        Dictionary<string, Rectangle> talentArrowsDict;
        Dictionary<string, Rectangle> buttonsDict;
        Dictionary<string, Rectangle> soulevatorDict;
        Dictionary<string, Rectangle> trashItemDict;
        Dictionary<string, Rectangle> shopKeeperDict;


        Rectangle inventoryRect;
        Rectangle equipmentRect;
        Rectangle talentRect;
        Rectangle lootRect;
        Rectangle portraitRect;
        Rectangle playerHPRect;
        Rectangle powerBackRect;
        Rectangle megaMapRect;
        Rectangle helpPopRect;
        Rectangle soulevatorRect;
        Rectangle trashItemRect;
        Rectangle shopKeeperRect;
        Vector2 powerTextPos;
        Vector2 helpPopPos;

        Rectangle hoverRect;
        Rectangle tooltipRect;
        private void InitDrawingParams()
        {
            #region variables
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
            vecName = new Vector2(rectEnemyHealthBar.X + rectEnemyHealthBar.Width / 2, 5);

            //mouse
            rectMouse = new Rectangle(0, 0, 25, 25);

            //Outside Rectangles
            Vector2 abilitiesUL = new Vector2((int)((maxX / 2 - 311 * average)), (int)((maxY - 158 * average)));

            guiOutsideRects = new Dictionary<string, Rectangle>();
            guiOutsideRects.Add("abilities", new Rectangle((int)abilitiesUL.X, (int)abilitiesUL.Y, (int)(622 * average), (int)(158 * average)));
            guiOutsideRects.Add("xp", new Rectangle((int)((maxX / 2 - 311 * average)), (int)((maxY - 178 * average)), (int)(622 * average), (int)(20 * average)));
            guiOutsideRects.Add("map", new Rectangle((int)((maxX - 344 * average)), 0, (int)(344 * average), (int)(344 * average)));
            //guiOutsideRects.Add("megaMap", new Rectangle((int)(((maxX / 2) - (622 / 2)) * average), (int) (160 * average), (int) (622 * average), (int) (622 * average))); // TODO does this look ok?
            guiOutsideRects.Add("player", new Rectangle(0, 0, (int)(470 * average), (int)(160 * average)));
            soulevatorRect = new Rectangle((int)(470 * average), 0, (int)(400 * average), (int)(475 * average));
            //guiOutsideRects.Add("soulevator", soulRect);
            guiOutsideRects.Add("buttons", new Rectangle((int)((maxX - 430 * average)), (int)(0), (int)(86 * average), (int)(344 * average)));
            trashItemRect = new Rectangle((int)(((maxX / 2) - 175 * average)), (int)((150 * average)), (int)(350 * average), (int)(150 * average));
            shopKeeperRect = new Rectangle((int)(5 * average), (int)(180 * average), (int)(545 * average), (int)(648 * average));


            Vector2 inventoryUL = new Vector2((int)(maxX - 440 * average), (int)(380 * average));
            Vector2 equipmentUL = new Vector2((int)(maxX - 744 * average), (int)(296 * average));
            Vector2 talentUL = new Vector2(5 * average, 180 * average);
            Vector2 lootUL = new Vector2((int)(420 * average), (int)(180 * average));

            inventoryRect = new Rectangle((int)inventoryUL.X, (int)inventoryUL.Y, (int)(440 * average), (int)(440 * average));
            equipmentRect = new Rectangle((int)equipmentUL.X, (int)equipmentUL.Y, (int)(304 * average), (int)(608 * average));
            talentRect = new Rectangle((int)talentUL.X, (int)talentUL.Y, (int)(406 * average), (int)(812 * average));
            lootRect = new Rectangle((int)lootUL.X, (int)lootUL.Y, (int)(150 * average), (int)(300 * average));
            megaMapRect = new Rectangle((int)(((maxX / 2) - (622 / 2)) * average), (int)(160 * average), (int)(622 * average), (int)(622 * average));
            tooltipRect = new Rectangle((int)((maxX - 300 * average)), (int)((maxY - 230 * average)), (int)(300 * average), (int)(230 * average));

            helpPopPos = new Vector2((int)(maxX / 2 - 150 * average), (int)((maxY - 283 * average)));
            helpPopRect = new Rectangle((int)helpPopPos.X, (int)helpPopPos.Y, (int)(300 * average), (int)(95 * average));
            //guiOutsideRects.Add("chat", new Rectangle(0, (int)((maxY - 444 * average)), (int)(362 * average), (int)(444 * average)));

            guiInsideRects = new Dictionary<string, Dictionary<string, Rectangle>>();
            equipmentDict = new Dictionary<string, Rectangle>();
            inventoryDict = new Dictionary<string, Rectangle>();
            abilityDict = new Dictionary<string, Rectangle>();
            lootDict = new Dictionary<string, Rectangle>();
            //chatDict = new Dictionary<string, Rectangle>();
            playerDict = new Dictionary<string, Rectangle>();
            mapDict = new Dictionary<string, Rectangle>();
            megaMapDict = new Dictionary<string, Rectangle>();
            xpDict = new Dictionary<string, Rectangle>();
            tooltipDict = new Dictionary<string, Rectangle>();
            talentDict = new Dictionary<string, Rectangle>();
            helpPopDict = new Dictionary<string, Rectangle>();
            talentArrowsDict = new Dictionary<string, Rectangle>();
            buttonsDict = new Dictionary<string, Rectangle>();
            soulevatorDict = new Dictionary<string, Rectangle>();
            trashItemDict = new Dictionary<string, Rectangle>();
            shopKeeperDict = new Dictionary<string, Rectangle>();



            //Add frame dictionaries
            guiInsideRects.Add("inventory", inventoryDict);
            guiInsideRects.Add("equipment", equipmentDict);
            guiInsideRects.Add("abilities", abilityDict);
            guiInsideRects.Add("loot", lootDict);
            guiInsideRects.Add("xp", xpDict);
            guiInsideRects.Add("tooltip", tooltipDict);
            guiInsideRects.Add("megaMap", megaMapDict);
            //guiInsideRects.Add("chat", chatDict);
            guiInsideRects.Add("player", playerDict);
            guiInsideRects.Add("talents", talentDict);
            guiInsideRects.Add("helpPop", helpPopDict);
            guiInsideRects.Add("buttons", buttonsDict);
            guiInsideRects.Add("soulevator", soulevatorDict);
            guiInsideRects.Add("trashItem", trashItemDict);
            guiInsideRects.Add("shopKeeper", shopKeeperDict);
            #endregion

            //Equipment inner
            equipmentDict.Add(GearSlot.Wrist.ToString(), new Rectangle((int)(equipmentUL.X + 8 * average), (int)(equipmentUL.Y + 78 * average), (int)(88 * average), (int)(88 * average)));
            equipmentDict.Add(GearSlot.Bling.ToString(), new Rectangle((int)(equipmentUL.X + 8 * average), (int)(equipmentUL.Y + 176 * average), (int)(88 * average), (int)(88 * average)));
            equipmentDict.Add(GearSlot.Lefthand.ToString(), new Rectangle((int)(equipmentUL.X + 8 * average), (int)(equipmentUL.Y + 274 * average), (int)(88 * average), (int)(88 * average)));

            equipmentDict.Add(GearSlot.Head.ToString(), new Rectangle((int)(equipmentUL.X + 108 * average), (int)(equipmentUL.Y + 29 * average), (int)(88 * average), (int)(88 * average)));
            equipmentDict.Add(GearSlot.Chest.ToString(), new Rectangle((int)(equipmentUL.X + 108 * average), (int)(equipmentUL.Y + 127 * average), (int)(88 * average), (int)(88 * average)));
            equipmentDict.Add(GearSlot.Legs.ToString(), new Rectangle((int)(equipmentUL.X + 108 * average), (int)(equipmentUL.Y + 225 * average), (int)(88 * average), (int)(88 * average)));
            equipmentDict.Add(GearSlot.Feet.ToString(), new Rectangle((int)(equipmentUL.X + 108 * average), (int)(equipmentUL.Y + 323 * average), (int)(88 * average), (int)(88 * average)));

            equipmentDict.Add(GearSlot.Shoulders.ToString(), new Rectangle((int)(equipmentUL.X + 206 * average), (int)(equipmentUL.Y + 78 * average), (int)(88 * average), (int)(88 * average)));
            equipmentDict.Add(GearSlot.Codpiece.ToString(), new Rectangle((int)(equipmentUL.X + 206 * average), (int)(equipmentUL.Y + 176 * average), (int)(88 * average), (int)(88 * average)));
            equipmentDict.Add(GearSlot.Righthand.ToString(), new Rectangle((int)(equipmentUL.X + 206 * average), (int)(equipmentUL.Y + 274 * average), (int)(88 * average), (int)(88 * average)));

            // Map inner Brandon
            Rectangle totalMiniArea = guiOutsideRects["map"];
            int miniInnerWidth = totalMiniArea.Width / 3;
            int miniInnerHeight = totalMiniArea.Height / 3;
            mapDict["Total"] = totalMiniArea;
            mapDict["Loc0"] = new Rectangle(totalMiniArea.X, totalMiniArea.Y, miniInnerWidth, miniInnerHeight);
            mapDict["Loc1"] = new Rectangle(totalMiniArea.X + miniInnerWidth, totalMiniArea.Y, miniInnerWidth, miniInnerHeight);
            mapDict["Loc2"] = new Rectangle(totalMiniArea.X + miniInnerWidth * 2, totalMiniArea.Y, miniInnerWidth, miniInnerHeight);

            mapDict["Loc3"] = new Rectangle(totalMiniArea.X, totalMiniArea.Y + miniInnerHeight, miniInnerWidth, miniInnerHeight);
            mapDict["Loc4"] = new Rectangle(totalMiniArea.X + miniInnerWidth, totalMiniArea.Y + miniInnerHeight, miniInnerWidth, miniInnerHeight);
            mapDict["Loc5"] = new Rectangle(totalMiniArea.X + miniInnerWidth * 2, totalMiniArea.Y + miniInnerHeight, miniInnerWidth, miniInnerHeight);

            mapDict["Loc6"] = new Rectangle(totalMiniArea.X, totalMiniArea.Y + miniInnerHeight * 2, miniInnerWidth, miniInnerHeight);
            mapDict["Loc7"] = new Rectangle(totalMiniArea.X + miniInnerWidth, totalMiniArea.Y + miniInnerHeight * 2, miniInnerWidth, miniInnerHeight);
            mapDict["Loc8"] = new Rectangle(totalMiniArea.X + miniInnerWidth * 2, totalMiniArea.Y + miniInnerHeight * 2, miniInnerWidth, miniInnerHeight);

            mapImgDict = (Game.Services.GetService(typeof(LevelManager)) as LevelManager).getMiniImageMap();

            megaMapDict["megaMap"] = megaMapRect;
            megaMapDict["playerPos"] = new Rectangle(0, 0, (int) (5 * average), (int) (5 * average));

            //Inventory inner
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; j++)
                {
                    inventoryDict.Add("inventory" + (i + j * 4), new Rectangle((int)(inventoryUL.X + 10 * average + i * 98 * average), (int)(inventoryUL.Y + 50 * average + j * 98 * average), (int)(88 * average), (int)(88 * average)));
                }
            }
            inventoryDict.Add("equipArrow", new Rectangle((int)(inventoryUL.X + 20 * average), (int)(inventoryUL.Y + 5 * average), (int)(40 * average), (int)(40 * average)));

            //ability
            abilityDict.Add("primary", new Rectangle((int)(abilitiesUL.X + 316 * average), (int)(abilitiesUL.Y + 47 * average), (int)(64 * average), (int)(64 * average)));
            abilityDict.Add("ability12", new Rectangle((int)(abilitiesUL.X + 390 * average), (int)(abilitiesUL.Y + 47 * average), (int)(64 * average), (int)(64 * average)));
            //abilities 0-7
            for (int i = 0; i < 4; ++i)
            {
                abilityDict.Add("ability" + i, new Rectangle((int)(abilitiesUL.X + (10 + 74 * i) * average), (int)(abilitiesUL.Y + 10 * average), (int)(64 * average), (int)(64 * average)));
                abilityDict.Add("ability" + (i + 4), new Rectangle((int)(abilitiesUL.X + (10 + 74 * i) * average), (int)(abilitiesUL.Y + 84 * average), (int)(64 * average), (int)(64 * average)));
            }
            abilityDict.Add("ability8", new Rectangle((int)(abilitiesUL.X + 474 * average), (int)(abilitiesUL.Y + 10 * average), (int)(64 * average), (int)(64 * average)));
            abilityDict.Add("ability9", new Rectangle((int)(abilitiesUL.X + 548 * average), (int)(abilitiesUL.Y + 10 * average), (int)(64 * average), (int)(64 * average)));
            abilityDict.Add("ability10", new Rectangle((int)(abilitiesUL.X + 474 * average), (int)(abilitiesUL.Y + 84 * average), (int)(64 * average), (int)(64 * average)));
            abilityDict.Add("ability11", new Rectangle((int)(abilitiesUL.X + 548 * average), (int)(abilitiesUL.Y + 84 * average), (int)(64 * average), (int)(64 * average)));

            //Talents
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 7; j++)
                {
                    //"q" is bottomright, "w" is bottomleft ("br" and "bl" made string too long)
                    talentDict.Add("talent" + (i + j * 4), new Rectangle((int)(talentUL.X + 30 * average + i * 94 * average), (int)(talentUL.Y + 30 * average + j * 94 * average), (int)(64 * average), (int)(64 * average)));
                    talentArrowsDict.Add("arrowL" + (i + j * 4), new Rectangle((int)(talentUL.X + i * 94 * average), (int)(talentUL.Y + 17 * average + 30 * average + j * 94 * average), (int)(30 * average), (int)(30 * average)));
                    talentArrowsDict.Add("arrowW" + (i + j * 4), new Rectangle((int)(talentUL.X + i * 94 * average), (int)(talentUL.Y + 64 * average + 30 * average + j * 94 * average), (int)(30 * average), (int)(30 * average)));
                    talentArrowsDict.Add("arrowB" + (i + j * 4), new Rectangle((int)(talentUL.X + 47 * average + i * 94 * average), (int)(talentUL.Y + 64 * average + 30 * average + j * 94 * average), (int)(30 * average), (int)(30 * average)));
                    talentArrowsDict.Add("arrowQ" + (i + j * 4), new Rectangle((int)(talentUL.X + 94 * average + i * 94 * average), (int)(talentUL.Y + 64 * average + 30 * average + j * 94 * average), (int)(30 * average), (int)(30 * average)));
                    talentArrowsDict.Add("arrowR" + (i + j * 4), new Rectangle((int)(talentUL.X + 94 * average + i * 94 * average), (int)(talentUL.Y + 17 * average + 30 * average + j * 94 * average), (int)(30 * average), (int)(30 * average)));
                }
            }

            talentDict.Add("Ranged", new Rectangle((int)(talentUL.X + 30 * average), (int)(talentUL.Y + 693 * average), (int)(95 * average), (int)(40 * average)));
            talentDict.Add("Melee", new Rectangle((int)(talentUL.X + 30 * average + 1.25 * 94 * average), (int)(talentUL.Y + 693 * average), (int)(95 * average), (int)(40 * average)));
            talentDict.Add("Magic", new Rectangle((int)(talentUL.X + 30 * average + 2.5 * 94 * average), (int)(talentUL.Y + 693 * average), (int)(95 * average), (int)(40 * average)));
            talentDict.Add("Points", new Rectangle((int)(talentUL.X + 30 * average + 94 * average), (int)(talentUL.Y + 755 * average), (int)(64 * average), (int)(40 * average)));

            //Loot
            for (int i = 0; i < NUM_LOOT_SHOWN; ++i)
            {
                lootDict.Add("loot" + i, new Rectangle((int)(lootUL.X + 15 * average), (int)(lootUL.Y + 15 * average + i * 65 * average), (int)(50 * average), (int)(50 * average)));
            }
            //up arrow
            lootDict.Add("upArrow", new Rectangle((int)(lootUL.X + 120 * average), (int)(lootUL.Y + 240 * average), (int)(25 * average), (int)(25 * average)));
            //down arrow
            lootDict.Add("downArrow", new Rectangle((int)(lootUL.X + 120 * average), (int)(lootUL.Y + 270 * average), (int)(25 * average), (int)(25 * average)));
            //loot all button
            lootDict.Add("lootAll", new Rectangle((int)(lootUL.X + 105 * average), (int)(lootUL.Y + 5 * average), (int)(40 * average), (int)(40 * average)));

            //Buffs and Debuffs
            for (int i = 0; i < 6; i++)
            {
                playerDict.Add("buff" + i, new Rectangle((int)((164 + (i * 40)) * average), (int)(68 * average), (int)(35 * average), (int)(35 * average)));
                playerDict.Add("timebuff" + i, new Rectangle((int)((164 + (i * 40)) * average), (int)(103 * average), (int)(1 * average), (int)(1 * average)));

                playerDict.Add("debuff" + i, new Rectangle((int)((164 + (i * 40)) * average), (int)(125 * average), (int)(35 * average), (int)(35 * average)));
                playerDict.Add("timedebuff" + i, new Rectangle((int)((164 + (i * 40)) * average), (int)(160 * average), (int)(1 * average), (int)(1 * average)));
            }
            portraitRect = new Rectangle(0, 0, (int)(160 * average), (int)(160 * average));
            playerHPRect = new Rectangle((int)(160 * average), 0, (int)(256 * average), (int)(32 * average));
            powerBackRect = new Rectangle(playerHPRect.X, playerHPRect.Y + playerHPRect.Height, playerHPRect.Width, playerHPRect.Height);
            powerTextPos = new Vector2((int)(170 * average), (int)(35 * average));
            playerDict.Add("hp", playerHPRect);
            playerDict.Add("power",powerBackRect);

            //OK button on PopUps
            helpPopDict.Add("ok", new Rectangle((int)(maxX / 2 + 120 * average), (int)helpPopPos.Y, (int)(30 * average), (int)(30 * average)));

            //Buttons
            buttonsDict.Add("map", new Rectangle((int)(maxX - 430 * average),(int)(0*average),(int)(86*average),(int)(86*average)));
            buttonsDict.Add("character", new Rectangle((int)(maxX - 430 * average), (int)(172 * average), (int)(86 * average), (int)(86 * average)));
            buttonsDict.Add("inventory", new Rectangle((int)(maxX - 430 * average), (int)(86 * average), (int)(86 * average), (int)(86 * average)));
            buttonsDict.Add("talents", new Rectangle((int)(maxX - 430 * average), (int)(258 * average), (int)(86 * average), (int)(86 * average)));

            //soulevator menu
            for (int i = 0; i < Enum.GetNames(typeof(FloorName)).Length; ++i)
            {
                soulevatorDict.Add(i + "", new Rectangle(soulevatorRect.X + (int)(10 * average), soulevatorRect.Y + (int)(75 * i * average + 10 * average), soulevatorRect.Width - (int)(20 * average), (int)(50 * average)));
            }

            //Trash Item
            trashItemDict.Add("ok", new Rectangle((int)(((maxX / 2 - 125) * average)), (int)((235 * average)), (int)(75 * average), (int)(50 * average)));
            trashItemDict.Add("cancel", new Rectangle((int)(((maxX / 2 + 50) * average)), (int)((235 * average)), (int)(75 * average), (int)(50 * average)));

            //Shop Keeper
            for (int i = 0; i < 10; i += 2)
            {
                shopKeeperDict.Add("itemIcon" + i, new Rectangle((int)(25 * average), (int)(273 + i/2 * 111 * average), (int)(78 * average), (int)(78 * average)));
                shopKeeperDict.Add("itemFrame" + i, new Rectangle((int)(103 * average), (int)(273 + i/2 * 111 * average), (int)(135 * average), (int)(78 * average)));
                shopKeeperDict.Add("itemIcon" + (i + 1), new Rectangle((int)(258 * average), (int)(273 + i/2 * 111 * average), (int)(78 * average), (int)(78 * average)));
                shopKeeperDict.Add("itemFrame" + (i + 1), new Rectangle((int)(336 * average), (int)(273 + i/2 * 111 * average), (int)(135 * average), (int)(78 * average)));
            }
            
        }
        #endregion

        public void Draw(SpriteBatch s)
        {
            if (mouseHoveredEntity != null)
            {
                Color enemyColor = Color.White;
                if (mouseHoveredEntity.Type == EntityType.EliteEnemy)
                {
                    enemyColor = new Color(0, 3, 204);
                }
                else if (mouseHoveredEntity.Type == EntityType.Boss)
                {
                    enemyColor = Color.Purple;
                }
                s.DrawString(font, mouseHoveredEntity.Name, vecName, enemyColor, 0, new Vector2(font.MeasureString(mouseHoveredEntity.Name).X * average / 2, 0), average, SpriteEffects.None, 0);
               
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
            s.Draw(texWhitePixel, guiOutsideRects["abilities"], Color.Black * 0.5f);
            for (int i = 0; i < 12; ++i)
            {
                Rectangle temprect = guiInsideRects["abilities"]["ability" + i];
                s.Draw(boundAbilities[i].Value.icon, temprect, Color.White);
                /*
                if (boundAbilities[i].Value.PowerCost > 0)
                {
                    s.DrawString(font, "" + boundAbilities[i].Value.PowerCost, new Vector2(rect.X, rect.Y + rect.Height - 50 * average * .4f), Color.Yellow, 0, Vector2.Zero, average * .4f, SpriteEffects.None, 0);
                }*/
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

            #region xp area
            Rectangle xpRect = guiOutsideRects["xp"];
            s.Draw(texWhitePixel, xpRect, Color.Black * 0.5f);
            s.Draw(texWhitePixel, new Rectangle(xpRect.X, xpRect.Y, xpRect.Width * experience / NextLevelXP, xpRect.Height), Color.Purple);
            #endregion

            #region minimap
            DrawMiniMap(s);

            if (showMegaMap)
            {
                DrawMegaMap(s);
            }
            #endregion

            #region main player frame
            //Main Player Frame Pic
            s.Draw(texWhitePixel, portraitRect, Color.Black * 0.5f);
            //Main Player Frame Health
            s.Draw(texWhitePixel, playerHPRect, Color.Black * 0.5f);
            //power
            s.Draw(texWhitePixel, powerBackRect, Color.Black);
            s.Draw(texWhitePixel, new Rectangle(powerBackRect.X, powerBackRect.Y, powerBackRect.Width * currentPower / maxPower, powerBackRect.Height), Color.Yellow);
            s.Draw(texChargeBarFront, powerBackRect, Color.Red);
            s.DrawString(font, currentPower + "", powerTextPos, Color.White, 0, Vector2.Zero, average * .5f, SpriteEffects.None, 0);
            s.DrawString(font, "    /" + maxPower, powerTextPos, Color.White, 0, Vector2.Zero, average * .5f, SpriteEffects.None, 0);
            //main player health
            s.Draw(health_bar, new Rectangle(playerHPRect.X, playerHPRect.Y, (int)(playerHPRect.Width * HealthPercent), (int)(playerHPRect.Height)), new Rectangle(0, 0, (int)(health_bar.Width * HealthPercent), (int)health_bar.Height), Color.White);

            for (int i = 0; i < 6; i++)
            {
                if (i < buffs.Count() && activeBuffs.ContainsKey(buffs[i]))
                {
                    s.Draw(buffIcons[buffs[i]], guiInsideRects["player"]["buff" + i], Color.White);
                    s.DrawString(font, (int)activeBuffs[buffs[i]].timeLeft/1000 + "s", new Vector2(guiInsideRects["player"]["timebuff" + i].X, guiInsideRects["player"]["timebuff" + i].Y), Color.Gold, 0, Vector2.Zero, 0.4f * average, SpriteEffects.None, 0);
                }
                if (i < debuffs.Count() && activeDebuffs.ContainsKey(debuffs[i]))
                {
                    s.Draw(debuffIcons[debuffs[i]], guiInsideRects["player"]["debuff" + i], Color.Red);
                    s.DrawString(font, (int)activeDebuffs[debuffs[i]].timeLeft / 1000 + "s", new Vector2(guiInsideRects["player"]["timedebuff" + i].X, guiInsideRects["player"]["timedebuff" + i].Y), Color.Gold, 0, Vector2.Zero, 0.4f * average, SpriteEffects.None, 0);
                }
            }
                            
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
                    for (int i = 1; i < Enum.GetNames(typeof(GearSlot)).Length; ++i)
                    {
                        Texture2D tempGearTex = gear[(GearSlot)i] == null ? texPlaceHolder : gear[(GearSlot)i].Icon;
                        s.Draw(tempGearTex, guiInsideRects["equipment"][((GearSlot)i).ToString()], Color.White);
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
                    if (inventory[i] != null)
                    {
                        Rectangle rect = guiInsideRects["inventory"]["inventory" + i];
                        s.Draw(inventory[i].Icon, rect, Color.White);
                        if (inventory[i].Quantity > 1)
                        {
                            s.DrawString(font, "x" + inventory[i].Quantity, new Vector2(rect.X, rect.Y + rect.Height - 50 * average * .5f), Color.White, 0, Vector2.Zero, average * .5f, SpriteEffects.None, 0);
                        }
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
                    Rectangle rect = guiInsideRects["loot"]["loot" + i];
                    Item ltemp = loot[i + lootScroll * NUM_LOOT_SHOWN];
                    s.Draw(ltemp.Icon, rect, Color.White);
                    if (ltemp.Quantity > 1)
                    {
                        s.DrawString(font, "x" + ltemp.Quantity, new Vector2(rect.X, rect.Y + rect.Height - 50 * average * .5f), Color.White, 0, Vector2.Zero, average * .5f, SpriteEffects.None, 0);
                    }
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
                s.Draw(texWhitePixel, guiOutsideRects["talents"], Color.Black * .5f);
                if (currentTalentTree == TalentTrees.ranged)
                {
                    drawTalentHelper(s, rangedAbilities);

                    #region draw ranged arrows
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
                    #endregion
                }
                else if (currentTalentTree == TalentTrees.melee)
                {
                    drawTalentHelper(s, meleeAbilities);

                    #region draw melee arrows
                    s.Draw(talentArrowDown, talentArrowsDict["arrowB1"], Color.White);
                    s.Draw(talentArrowDown, talentArrowsDict["arrowB2"], Color.White);

                    s.Draw(talentArrowDown, talentArrowsDict["arrowB5"], Color.White);
                    s.Draw(talentArrowDown, talentArrowsDict["arrowB6"], Color.White);

                    s.Draw(talentArrowDown, talentArrowsDict["arrowB9"], Color.White);
                    s.Draw(talentArrowDownLeft, talentArrowsDict["arrowW10"], Color.White);

                    s.Draw(talentArrowDown, talentArrowsDict["arrowB12"], Color.White);
                    s.Draw(talentArrowLeft, talentArrowsDict["arrowL13"], Color.White);
                    s.Draw(talentArrowDown, talentArrowsDict["arrowB13"], Color.White);
                    s.Draw(talentArrowLeft, talentArrowsDict["arrowL14"], Color.White);
                    s.Draw(talentArrowUpLeft, talentArrowsDict["arrowQ14"], Color.White);

                    s.Draw(talentArrowDown, talentArrowsDict["arrowB16"], Color.White);
                    s.Draw(talentArrowDown, talentArrowsDict["arrowB17"], Color.White);
                    s.Draw(talentArrowDownRight, talentArrowsDict["arrowQ17"], Color.White);
                    s.Draw(talentArrowRight, talentArrowsDict["arrowR17"], Color.White);

                    s.Draw(talentArrowDown, talentArrowsDict["arrowB20"], Color.White);
                    s.Draw(talentArrowDown, talentArrowsDict["arrowB21"], Color.White);
                    s.Draw(talentArrowDown, talentArrowsDict["arrowB23"], Color.White);
                    s.Draw(talentArrowLeft, talentArrowsDict["arrowL23"], Color.White);

                    s.Draw(talentArrowLeft, talentArrowsDict["arrowL27"], Color.White);

                    #endregion
                }
                else if (currentTalentTree == TalentTrees.magic)
                {
                    //drawTalentHelper(s, magicAbilities);
                }

                //Draw Tree Selectors
                s.Draw(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.RangedBanner), guiInsideRects["talents"]["Ranged"], Color.White);
                s.Draw(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.MeleeBanner), guiInsideRects["talents"]["Melee"], Color.White);
                s.Draw(Texture2DUtil.Instance.GetTexture(TextureStrings.UI.MagicBanner), guiInsideRects["talents"]["Magic"], Color.White);

                //Draw Talent Points
                s.DrawString(font, (totalTalentPoints - spentTalentPoints) + "/" + totalTalentPoints, new Vector2(guiInsideRects["talents"]["Points"].X, guiInsideRects["talents"]["Points"].Y), Color.White, 0, Vector2.Zero, 0.5f * average, SpriteEffects.None, 0);
            }
            #endregion

            #region soulevator
            if (inSoulevator && guiOutsideRects.ContainsKey("soulevator"))
            {
                s.Draw(texWhitePixel, guiOutsideRects["soulevator"], Color.Black);
                for (int i = 0; i < Enum.GetNames(typeof(FloorName)).Length; ++i)
                {
                    Rectangle tmpSoulRect = guiInsideRects["soulevator"][i + ""];
                    s.DrawString(font, ((FloorName)i).ToString(), new Vector2(tmpSoulRect.X, tmpSoulRect.Y), Color.White, 0, Vector2.Zero, average, SpriteEffects.None, 0);
                }
            }
            #endregion

            #region trash item
            if(guiOutsideRects.ContainsKey("trashItem")){
                s.Draw(texWhitePixel, guiOutsideRects["trashItem"], Color.Black *.5f);
                s.Draw(texPlaceHolder, guiInsideRects["trashItem"]["ok"], Color.Green);
                s.Draw(texPlaceHolder, guiInsideRects["trashItem"]["cancel"], Color.Red);
                s.DrawString(font, "Are you sure you want", new Vector2((int)(((maxX / 2 - 120) * average)), (int)((160 * average))), Color.White, 0, Vector2.Zero, average * .5f, SpriteEffects.None, 0);
                s.DrawString(font, "to delete this item?", new Vector2((int)(((maxX / 2 - 110) * average)), (int)((185 * average))), Color.White, 0, Vector2.Zero, average * .5f, SpriteEffects.None, 0);

                if (itemToDelete != -1) s.Draw(texHover, guiInsideRects["inventory"]["inventory" + itemToDelete], Color.Red);
            }
            #endregion

            #region shopKeeper
            if (guiOutsideRects.ContainsKey("shopKeeper"))
            {
                s.Draw(shopFrame, guiOutsideRects["shopKeeper"], Color.White);
                
                for (int i = 0; i < 10 && i < buyBack.Count; i++)
                {
                    if (buyBack[i] != null)
                    {
                        s.Draw(buyBack[buyBack.Count - i - 1].Icon, guiInsideRects["shopKeeper"]["itemIcon" + i], Color.White);
                        s.Draw(texWhitePixel, guiInsideRects["shopKeeper"]["itemFrame" + i], Color.Black * .8f);
                        s.DrawString(font, buyBack[buyBack.Count - i - 1].Name, new Vector2(guiInsideRects["shopKeeper"]["itemFrame" + i].X + 5, guiInsideRects["shopKeeper"]["itemFrame" + i].Y + 5), Color.White, 0, Vector2.Zero, average * .25f, SpriteEffects.None, 0);
                        s.Draw(goldIcon, new Rectangle(guiInsideRects["shopKeeper"]["itemFrame" + i].X + 5, guiInsideRects["shopKeeper"]["itemFrame" + i].Y + guiInsideRects["shopKeeper"]["itemFrame" + i].Height - 25, 20, 20), Color.White);
                        s.DrawString(font, buyBack[buyBack.Count - i - 1].GoldCost.ToString(), new Vector2(guiInsideRects["shopKeeper"]["itemFrame" + i].X + 25, guiInsideRects["shopKeeper"]["itemFrame" + i].Y + guiInsideRects["shopKeeper"]["itemFrame" + i].Height - 25), Color.Gold, 0, Vector2.Zero, average * .5f, SpriteEffects.None, 0);
                    }
                    
                }
            }
            #endregion

            #region buttons
            //backing
            s.Draw(texWhitePixel, guiOutsideRects["buttons"], Color.Black * .5f);
            s.Draw(mapIcon, guiInsideRects["buttons"]["map"], Color.White);
            s.Draw(inventoryIcon, guiInsideRects["buttons"]["inventory"], Color.White);
            s.Draw(characterIcon, guiInsideRects["buttons"]["character"], Color.White);
            s.Draw(talentIcon, guiInsideRects["buttons"]["talents"], Color.White);
            #endregion

            #region helpPopUps
            if (helpPoPs.Count > 0)
            {
                s.Draw(texWhitePixel, helpPopRect, Color.Black * 0.5f);
                helpPoPs[0].Draw(s, helpPopPos, font, average, 50f, guiInsideRects["helpPop"]["ok"], guiInsideRects["helpPop"]["ok"]);
            }
            
            
            #endregion

            //always keep this last so hover image is drawn on top
            #region tooltip and hover
            if (currentTooltip != null)
            {
                s.Draw(texWhitePixel, tooltipRect, Color.Black * 0.8f);
                currentTooltip.Draw(s, new Vector2(tooltipRect.X, tooltipRect.Y), font, average, 50f);
            }

            if (hovering)
            {
                s.Draw(texHover, hoverRect, new Color(255, 255, 166));
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
            else if (selectedEquipSlot && gear[selectedEquipPiece] != null)
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
                    s.Draw(GetCachedAbility(rangedAbilities[selectedTalentSlot / 4, selectedTalentSlot % 4].name).icon, rectMouse, Color.White);
                }
                else if (currentTalentTree == TalentTrees.melee)
                {
                    s.Draw(GetCachedAbility(meleeAbilities[selectedTalentSlot / 4, selectedTalentSlot % 4].name).icon, rectMouse, Color.White);
                }
                else
                {

                }

            }
            else
            {
                rectMouse.Width = 40;
                rectMouse.Height = 40;
                s.Draw(texCursor, rectMouse, Color.White);
            }
            #endregion mouse
        }

        private int zoom = 300;
        private void DrawMiniMap(SpriteBatch s)
        {
            /*
             * Basically we need to find the player's position within a chunk, then we can translate
             * that position in the chunk to a position in the chunk image, and draw that area of the chunk
             */ 
            LevelManager lm = (Game.Services.GetService(typeof(LevelManager)) as LevelManager);

            // Current chunk's image
            Texture2D curChunk = Texture2DUtil.Instance.GetRotatedTexture(lm.GetCurrentChunkImgName(physicalData.Position), lm.GetCurrentChunkRotation(physicalData.Position), Game.GraphicsDevice);

            // The x and y coord of the current chunk (in the 3x3 grid)
            int chunkX = (int)physicalData.Position.X / (LevelManager.CHUNK_SIZE * LevelManager.BLOCK_SIZE);
            int chunkZ = (int)physicalData.Position.Z / (LevelManager.CHUNK_SIZE * LevelManager.BLOCK_SIZE);

            // Where the player's located in the chunk
            float inChunkX = physicalData.Position.X - (chunkX * LevelManager.CHUNK_SIZE * LevelManager.BLOCK_SIZE);
            float inChunkZ = physicalData.Position.Z - (chunkZ * LevelManager.CHUNK_SIZE * LevelManager.BLOCK_SIZE);

            /*
             * Translate to coordinates in the image:
             *  (inChunkX / chunkWidth) = (imgX / imgWidth)
             *  imgX = (imgWidth) * (inChunkX / chunkWidth)
             */
            float imgX = curChunk.Width * (inChunkX / (LevelManager.CHUNK_SIZE * LevelManager.BLOCK_SIZE));
            float imgZ = curChunk.Width * (inChunkZ / (LevelManager.CHUNK_SIZE * LevelManager.BLOCK_SIZE));

            Rectangle src = new Rectangle((int) imgX, (int) imgZ, zoom, zoom);
            // Center it
            src.X -= (int) (zoom / 2);
            src.Y -= (int) (zoom / 2);

            Rectangle drawLoc = mapDict["Total"];
            s.Draw(curChunk, new Rectangle(drawLoc.X + drawLoc.Width / 2, drawLoc.Y + drawLoc.Height / 2, drawLoc.Width, drawLoc.Height), src,
                Color.White, 0, new Vector2(zoom / 2, zoom / 2), SpriteEffects.None, 0);


            Rectangle playerRec = megaMapDict["playerPos"];
            playerRec.X = drawLoc.X + drawLoc.Width / 2;
            playerRec.Y = drawLoc.Y + drawLoc.Height / 2;
            s.Draw(Texture2DUtil.Instance.GetTexture(TextureStrings.WHITE_PIX), playerRec, Color.Blue);
        }

        private void DrawMegaMap(SpriteBatch s)
        {
            float alpha = .5f;
            /*if (Math.Abs(physicalData.LinearVelocity.X) + Math.Abs(physicalData.LinearVelocity.Z) < .01f)
            {
                alpha = .9f;
            }*/
            string currentChunk = (Game.Services.GetService(typeof(LevelManager)) as LevelManager).GetCurrentChunkImgName(physicalData.Position);
            Rectangle chunkRect = guiInsideRects["megaMap"]["megaMap"];

            Rotation rotation = (Game.Services.GetService(typeof(LevelManager)) as LevelManager).GetCurrentChunkRotation(physicalData.Position);

            // We need to rotate the image about its center
            int originX = Texture2DUtil.Instance.GetTexture(currentChunk).Width / 2;
            int originY = Texture2DUtil.Instance.GetTexture(currentChunk).Height / 2;
            s.Draw(Texture2DUtil.Instance.GetTexture(currentChunk), new Rectangle(chunkRect.X + chunkRect.Width/ 2, chunkRect.Y + chunkRect.Height/2, chunkRect.Width, chunkRect.Height), 
                null, Color.White * alpha, -rotation.ToRadians(), new Vector2(originX, originY), SpriteEffects.None, 0);

            // Scale the player location to the map
            Rectangle playerRect = guiInsideRects["megaMap"]["playerPos"];
            playerRect.X = (int) ((physicalData.Position.X % (LevelManager.CHUNK_SIZE * LevelManager.BLOCK_SIZE)) / (LevelManager.CHUNK_SIZE * LevelManager.BLOCK_SIZE) * chunkRect.Width * average) + chunkRect.X;
            playerRect.Y = (int) ((physicalData.Position.Z % (LevelManager.CHUNK_SIZE * LevelManager.BLOCK_SIZE)) / (LevelManager.CHUNK_SIZE * LevelManager.BLOCK_SIZE) * chunkRect.Height * average) + chunkRect.Y;
            
            // Center playerRect
            playerRect.X -= playerRect.Width / 2;
            playerRect.Y -= playerRect.Height / 2;

            s.Draw(Texture2DUtil.Instance.GetTexture(TextureStrings.WHITE_PIX), playerRect, Color.Blue);
        }

        protected override void KillAlive()
        {
            if (looting)
            {
                CloseLoot();
                lootScroll = 0;
            }
            looting = false;
            model.KillAllEmitters();
            currentPower = 0;
            StartSequence("death");

            string attstr = GearSlot.Righthand.ToString();
            if (attached.ContainsKey(attstr) && attached[attstr] != null)
            {
                attached[attstr].Draw = true;
            }
            base.KillAlive();
        }
    }
}
