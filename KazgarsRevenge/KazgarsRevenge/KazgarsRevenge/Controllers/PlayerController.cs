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
using KazgarsRevenge.Libraries;

namespace KazgarsRevenge
{
    public enum StatType
    {
        RunSpeed,
        AttackSpeed,
        Strength,
        Agility,
        Intellect,
        CooldownReduction,
        CritChance,
        Health,
    }
    public enum AttackType
    {
        None,
        Melle,
        Ranged,
        Magic,
    }
    public enum GearSlot
    {
        None,
        Head,
        Chest,
        Legs,
        Righthand,
        Lefthand,
    }
    
    public class PlayerController : DrawableComponent2D
    {

        #region member variables
        //services
        Space physics;
        CameraComponent camera;
        SoundEffectLibrary soundEffects;
        AttackManager attacks;
        LootManager gearGenerator;

        //data
        AnimationPlayer animations;
        Dictionary<string, AttachableModel> attached;
        Entity physicalData;
        Random rand;

        //variables for target
        Entity targetedPhysicalData;
        Entity targetedLootData;
        GameEntity mouseHoveredEntity;
        HealthData mouseHoveredHealth;

        //variables for movement
        const float stopRadius = 10;
        const float targetResetDistance = 1000;
        Vector3 mouseHoveredLocation = Vector3.Zero;
        Vector3 groundTargetLocation = Vector3.Zero;
        double millisRunningCounter = 2000;
        double millisRunTime = 2000;
        double millisCombatLength = 4000;
        double millisCombatCounter = 0;
        bool inCombat;

        #endregion

        #region Abilities
        const float melleRange = 50;
        const float bowRange = 1000;
        Dictionary<string, Ability> allAbilities = new Dictionary<string, Ability>();
        Ability[] boundAbilities = new Ability[9];
        #endregion

        #region UI Frames
        Texture2D icon_selected;
        #endregion

        #region Ability Icons
        Texture2D melee;
        Texture2D range;
        Texture2D magic;
        Texture2D placeHolder;
        Texture2D heartStrikeIcon;
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
        #endregion

        #region Variables for UI
        bool[] UISlotUsed = new bool[14];     //0-7 abilities, 8 primary, 9 secondary, 10-13 items
        #endregion

        #region stats
        //array with one position per type of stat
        //                                move    attspeed   str  agi  intel  cd   crit   hp
        float[] baseStats = new float[] { 120,    .05f,      1,   1,   1,     0,   0,     100 };

        Dictionary<StatType, float> stats = new Dictionary<StatType, float>();
        #endregion

        #region equipped gear
        Dictionary<GearSlot, Equippable> gear = new Dictionary<GearSlot, Equippable>();
        //inventory
        Item[,] inventory = new Item[15, 15];
        public void EquipGear(Equippable equipMe, GearSlot slot)
        {
            //if the player is trying to equip a two-handed weapon to the offhand, unequip both current weapons and equip it to the main hand
            Weapon possWep = equipMe as Weapon;
            if (possWep != null)
            {
                if (possWep.TwoHanded)
                {
                    if (slot == GearSlot.Lefthand)
                    {
                        EquipGear(equipMe, GearSlot.Righthand);
                        return;
                    }
                    UnequipGear(GearSlot.Lefthand);
                }
            }

            //otherwise, carry on
            UnequipGear(slot);
            gear[slot] = equipMe;
            RecalculateStats();

            attached.Add(slot.ToString(), new AttachableModel(equipMe.GearModel, GearSlotToBoneName(slot)));
        }
        /// <summary>
        /// tries to put equipped item into inventory. If there was no inventory space, returns false.
        /// </summary>
        public bool UnequipGear(GearSlot slot)
        {
            Equippable oldEquipped = gear[slot];
            if (oldEquipped != null)
            {
                if (AddToInventory(oldEquipped))
                {
                    attached.Remove(slot.ToString());
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                //if there was nothing in there to start with, return true
                return true;
            }
        }
        /// <summary>
        /// tries to add item to inventory if there is space. returns false if no space left
        /// </summary>
        public bool AddToInventory(Item toAdd)
        {
            if (toAdd == null)
            {
                return true;
            }
            for (int i = 0; i < inventory.GetLength(0); ++i)
            {
                for (int j = 0; j < inventory.GetLength(1); ++j)
                {
                    if (inventory[i, j] == null)
                    {
                        inventory[i, j] = toAdd;
                        return true;
                    }
                }
            }
            return false;
        }
        public void RecalculateStats()
        {
            //add base stats, accounting for level
            for (int i = 0; i < Enum.GetNames(typeof(StatType)).Length; ++i)
            {
                stats[(StatType)i] = baseStats[i];
            }

            //add stats for each piece of gear
            foreach (KeyValuePair<GearSlot, Equippable> k in gear)
            {
                if (k.Value != null)
                {
                    AddStats(k.Value.StatEffects);
                }
            }
        }
        public void AddStats(Dictionary<StatType, float> statsToAdd)
        {
            foreach (KeyValuePair<StatType, float> k in statsToAdd)
            {
                stats[k.Key] += k.Value;
            }
        }
        public AttackType GetMainhandType()
        {
            Equippable e = gear[GearSlot.Righthand];
            if (e != null)
            {
                return (e as Weapon).Type;
            }
            else
            {
                return AttackType.None;
            }
        }
        public AttackType GetOffhandType()
        {
            Equippable e = gear[GearSlot.Lefthand];
            if (e != null)
            {
                return (e as Weapon).Type;
            }
            else
            {
                return AttackType.None;
            }
        }
        #endregion



        public PlayerController(KazgarsRevengeGame game, GameEntity entity)
            : base(game, entity)
        {
            InitGeneralFields();

            InitNewPlayer();

            //adding sword and bow for demo
            EquipGear(gearGenerator.GenerateSword(), GearSlot.Righthand);
            EquipGear(gearGenerator.GenerateBow(), GearSlot.Lefthand);
        }

        private void InitNewPlayer()
        {
            #region stats and inventory
            for (int i = 0; i < Enum.GetNames(typeof(StatType)).Length; ++i)
            {
                stats.Add((StatType)i, 0);
            }

            RecalculateStats();

            for (int i = 0; i < Enum.GetNames(typeof(GearSlot)).Length; ++i)
            {
                gear.Add((GearSlot)i, null);
            }
            #endregion

            #region ability initialization
            //create initial abilities
            allAbilities.Add("heartstrike", new Ability(1, 1, heartStrikeIcon, 10f, AttackType.Melle));
            boundAbilities[0] = allAbilities["heartstrike"];
            allAbilities.Add("I1", new Ability(1, 1, I1, 5f, AttackType.Melle));
            boundAbilities[1] = allAbilities["I1"];
            allAbilities.Add("I2", new Ability(1, 1, I2, 30f, AttackType.Melle));
            boundAbilities[2] = allAbilities["I2"];
            allAbilities.Add("I3", new Ability(1, 1, I3, 60f, AttackType.Melle));
            boundAbilities[3] = allAbilities["I3"];
            allAbilities.Add("I4", new Ability(1, 1, I4, 120f, AttackType.Melle));
            boundAbilities[4] = allAbilities["I4"];
            allAbilities.Add("I5", new Ability(1, 1, I5, 120f, AttackType.Melle));
            boundAbilities[5] = allAbilities["I5"];
            allAbilities.Add("I6", new Ability(1, 1, I6, 90f, AttackType.Melle));
            boundAbilities[6] = allAbilities["I6"];
            allAbilities.Add("I7", new Ability(1, 1, I7, 300f, AttackType.Melle));
            boundAbilities[7] = allAbilities["I7"];
            #endregion
        }

        private void InitPlayerFromFile()
        {

        }

        private void InitGeneralFields()
        {
            #region members
            //shared data
            this.physicalData = entity.GetSharedData(typeof(Entity)) as Entity;
            this.animations = entity.GetSharedData(typeof(AnimationPlayer)) as AnimationPlayer;
            this.attached = entity.GetSharedData(typeof(Dictionary<string, AttachableModel>)) as Dictionary<string, AttachableModel>;

            //misc
            rand = new Random();
            rayCastFilter = RayCastFilter;

            //services
            physics = Game.Services.GetService(typeof(Space)) as Space;
            camera = Game.Services.GetService(typeof(CameraComponent)) as CameraComponent;
            attacks = Game.Services.GetService(typeof(AttackManager)) as AttackManager;
            soundEffects = Game.Services.GetService(typeof(SoundEffectLibrary)) as SoundEffectLibrary;
            gearGenerator = Game.Services.GetService(typeof(LootManager)) as LootManager;

            //required content
            texWhitePixel = Game.Content.Load<Texture2D>("Textures\\whitePixel");
            font = Game.Content.Load<SpriteFont>("Verdana");
            InitDrawingParams();
            #endregion

            #region UI Frame Load
            icon_selected = Game.Content.Load<Texture2D>("Textures\\UI\\Frames\\icon_selected");
            #endregion

            #region Ability Image Load
            melee = Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\DB");
            range = Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\LW");
            magic = Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\BR");
            placeHolder = Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\BN");
            heartStrikeIcon = Game.Content.Load<Texture2D>("Textures\\UI\\Abilities\\HS");
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
            #endregion

            #region animation setup
            aniDurations = new Dictionary<string, double>();
            aniDurations["shotRelease"] = animations.skinningDataValue.AnimationClips["k_fire_arrow"].Duration.TotalMilliseconds / 2;
            aniDurations["melleDamage"] = animations.skinningDataValue.AnimationClips["k_onehanded_swing"].Duration.TotalMilliseconds / 2;
            foreach (KeyValuePair<string, AnimationClip> k in animations.skinningDataValue.AnimationClips)
            {
                string key = k.Key;
                double length = k.Value.Duration.TotalMilliseconds;

                if (key == "k_fighting_stance")
                {
                    length *= 2;
                }

                aniDurations.Add(key, length - 10);
            }

            PlayAnimation("k_idle1");
            #endregion
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
            Looting,
        }


        Dictionary<string, double> aniDurations;

        private double millisAniCounter;
        private double millisAniDuration;

        private AttackState attState = AttackState.None;
        private double millisShotAniCounter;
        private double millisShotArrowAttachLength = 200;
        private double millisShotArrowAttachCounter;

        private double millisMelleCounter;

        private string currentAniName;
        public void PlayAnimation(string animationName)
        {
            animations.StartClip(animationName);
            if (animationName == "k_idle1")
            {
                millisAniDuration = rand.Next(2000, 4000);
            }
            else
            {
                millisAniDuration = aniDurations[animationName];
            }

            currentAniName = animationName;
            millisAniCounter = 0;
            millisShotAniCounter = 0;

            //TODO: send network signal

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
            double currentTime = gameTime.TotalGameTime.TotalSeconds;
            millisAniCounter += elapsed;
            millisShotAniCounter += elapsed;
            millisShotArrowAttachCounter += elapsed;
            millisMelleCounter += elapsed;
            millisRunningCounter += elapsed;

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
            for (int i = 0; i < 8; i++)
            {
                boundAbilities[i].update((float)currentTime);
            }
            #endregion

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
                        targetedPhysicalData = null;
                    }
                }
                bool newTarget = curMouse.LeftButton == ButtonState.Released || prevMouse.LeftButton == ButtonState.Released || (curMouse.RightButton == ButtonState.Pressed && prevMouse.RightButton == ButtonState.Released);
                
                newTarget = CheckButtons() || newTarget;
                
                CheckMouseRay(newTarget);

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
                    targetedPhysicalData = null;
                }
                if (attState == AttackState.None)
                {
                    CheckAbilities(move, (float)currentTime);
                }

                //targets ground location to start running to if holding mouse button
                if (curMouse.LeftButton == ButtonState.Pressed && attState == AttackState.None)
                {
                    groundTargetLocation = mouseHoveredLocation;
                    if (prevMouse.LeftButton == ButtonState.Released && targetedPhysicalData == null)
                    {
                        attacks.CreateMouseSpikes(groundTargetLocation);
                    }
                }
                Vector3 groundMove = move;
                if (targetedPhysicalData == null)
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
                    ChangeVelocity(Vector3.Zero);
                }


                CheckAnimations();
            }
            else
            {
                ChangeVelocity(Vector3.Zero);
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



                //check if ray hits GameEntity if not holding down mouse button
                List<RayCastResult> results = new List<RayCastResult>();
                physics.RayCast(r, 10000, rayCastFilter, results);

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
                            if (mouseHoveredEntity.Name == "loot")
                            {
                                targetedLootData = mouseHoveredEntity.GetSharedData(typeof(Entity)) as Entity;
                            }
                            //if the left mouse button was either just clicked or not pressed down at all, or if any other ability input was just clicked
                            else if(newTarget)
                            {
                                mouseHoveredHealth = mouseHoveredEntity.GetHealth();
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

        /// <summary>
        /// Checks which buttons are pressed down
        /// </summary>
        /// <returns></returns>
        private bool CheckButtons()
        {
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

            //switch weapon hands (for demo)
            if (curKeys.IsKeyDown(Keys.Tab) && prevKeys.IsKeyUp(Keys.Tab))
            {
                Equippable r = gear[GearSlot.Righthand];
                Equippable l = gear[GearSlot.Lefthand];

                EquipGear(r, GearSlot.Lefthand);
                EquipGear(l, GearSlot.Righthand);
            }

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
                string aniSuffix = "right";

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
                    else if (mainHandType == offHandType)
                    {
                        aniSuffix = "dual";
                    }

                }

                //remove this once we get more animations
                aniSuffix = "";

                bool inRange = false;
                //attack if in range
                switch (mainHandType)
                {
                    case AttackType.None:
                        if (distance < melleRange)
                        {
                            PlayAnimation("k_onehanded_swing" + aniSuffix);
                            attState = AttackState.InitialSwing;
                            inRange = true;
                        }
                        break;
                    case AttackType.Melle:
                        if (distance < melleRange)
                        {
                            PlayAnimation("k_onehanded_swing" + aniSuffix);
                            attState = AttackState.InitialSwing;
                            inRange = true;
                        }
                        break;
                    case AttackType.Ranged:
                        if (distance < bowRange)
                        {
                            PlayAnimation("k_fire_arrow" + aniSuffix);
                            attState = AttackState.GrabbingArrow;
                            inRange = true;
                        }
                        break;
                    case AttackType.Magic:
                        if (distance < bowRange)
                        {
                            //need magic item animation here
                            PlayAnimation("k_fire_arrow" + aniSuffix);
                            attState = AttackState.GrabbingArrow;
                            inRange = true;
                        }
                        break;
                }

                if (inRange)
                {
                    UpdateRotation(dir);
                    millisMelleCounter = 0;
                    millisShotAniCounter = 0;
                    millisShotArrowAttachCounter = 0;
                    mouseHoveredLocation = physicalData.Position;
                    groundTargetLocation = physicalData.Position;
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
            //Q
            if (curKeys.IsKeyDown(Keys.Q) && boundAbilities[0].tryUse(currentTime))
            {
                PlayAnimation("k_flip");
                attState = AttackState.InitialSwing;
                UpdateRotation(dir);
                millisMelleCounter = 0;
                mouseHoveredLocation = physicalData.Position;
                groundTargetLocation = physicalData.Position;
                targettedPhysicalData = null;
            }
            //W
            if (curKeys.IsKeyDown(Keys.W) && boundAbilities[1].tryUse(currentTime))
            {
                PlayAnimation("k_flip");
                attState = AttackState.InitialSwing;
                UpdateRotation(dir);
                millisMelleCounter = 0;
                mouseHoveredLocation = physicalData.Position;
                groundTargetLocation = physicalData.Position;
                targettedPhysicalData = null;
            }
            //E
            if (curKeys.IsKeyDown(Keys.E) && boundAbilities[2].tryUse(currentTime))
            {
                PlayAnimation("k_flip");
                attState = AttackState.InitialSwing;
                UpdateRotation(dir);
                millisMelleCounter = 0;
                mouseHoveredLocation = physicalData.Position;
                groundTargetLocation = physicalData.Position;
                targettedPhysicalData = null;
            }
            //R
            if (curKeys.IsKeyDown(Keys.R) && boundAbilities[3].tryUse(currentTime))
            {
                PlayAnimation("k_flip");
                attState = AttackState.InitialSwing;
                UpdateRotation(dir);
                millisMelleCounter = 0;
                mouseHoveredLocation = physicalData.Position;
                groundTargetLocation = physicalData.Position;
                targettedPhysicalData = null;
            }
            //A
            if (curKeys.IsKeyDown(Keys.A) && boundAbilities[4].tryUse(currentTime))
            {
                PlayAnimation("k_flip");
                attState = AttackState.InitialSwing;
                UpdateRotation(dir);
                millisMelleCounter = 0;
                mouseHoveredLocation = physicalData.Position;
                groundTargetLocation = physicalData.Position;
                targettedPhysicalData = null;
            }
            //S
            if (curKeys.IsKeyDown(Keys.S) && boundAbilities[5].tryUse(currentTime))
            {
                PlayAnimation("k_flip");
                attState = AttackState.InitialSwing;
                UpdateRotation(dir);
                millisMelleCounter = 0;
                mouseHoveredLocation = physicalData.Position;
                groundTargetLocation = physicalData.Position;
                targettedPhysicalData = null;
            }
            //D
            if (curKeys.IsKeyDown(Keys.D) && boundAbilities[6].tryUse(currentTime))
            {
                PlayAnimation("k_flip");
                attState = AttackState.InitialSwing;
                UpdateRotation(dir);
                millisMelleCounter = 0;
                mouseHoveredLocation = physicalData.Position;
                groundTargetLocation = physicalData.Position;
                targettedPhysicalData = null;
            }
            //F
            if (curKeys.IsKeyDown(Keys.F) && boundAbilities[7].tryUse(currentTime))
            {
                PlayAnimation("k_flip");
                attState = AttackState.InitialSwing;
                UpdateRotation(dir);
                millisMelleCounter = 0;
                mouseHoveredLocation = physicalData.Position;
                groundTargetLocation = physicalData.Position;
                targetedPhysicalData = null;
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

            Vector3 moveVec = move * stats[(int)StatType.RunSpeed];
            moveVec.Y = 0;
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
                ChangeVelocity(new Vector3(0, 0, 0));
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
            //(mainly to handle idle animations and fighting stance to idle)
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
                                    soundEffects.playScratch();
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
                    attached.Add("arrow", ((MainGame)Game).GetAttachable("arrow", "sword", "Bone_001_R_004"));
                    attState = AttackState.DrawingString;
                    millisShotArrowAttachCounter = 0;
                }
                else if (attState == AttackState.DrawingString && millisShotAniCounter >= aniDurations["shotRelease"])
                {
                    Vector3 forward = GetForward();
                    attached.Remove("arrow");
                    attacks.CreateArrow(physicalData.Position + forward * 10, forward * 450, 25, FactionType.Players);
                    attState = AttackState.LettingGo;
                    millisShotAniCounter = 0;
                }
                else if (attState == AttackState.LettingGo && millisShotAniCounter >= aniDurations["shotRelease"] * stats[StatType.AttackSpeed])
                {
                    attState = AttackState.None;
                    millisShotAniCounter = 0;
                }
            }

            //swinging animation
            if (currentAniName == "k_onehanded_swing" || currentAniName == "k_flip")
            {
                if (attState == AttackState.InitialSwing && millisMelleCounter >= aniDurations["melleDamage"])
                {
                    Vector3 forward = GetForward();
                    attacks.CreateMelleAttack(physicalData.Position + forward * 35, 25, FactionType.Players);
                    attState = AttackState.FinishSwing;
                    millisMelleCounter = 0;
                }
            }
        }

        #region helpers
        public string GearSlotToBoneName(GearSlot s)
        {
            switch (s)
            {
                case GearSlot.Lefthand:
                    return "Bone_001_L_004";
                case GearSlot.Righthand:
                    return "Bone_001_R_004";
                default:
                    return "RootNode";
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
        }

        public override void Draw(SpriteBatch s)
        {
            if (mouseHoveredEntity != null)
            {
                s.DrawString(font, mouseHoveredEntity.Name, vecName, Color.Red, 0, Vector2.Zero, average, SpriteEffects.None, 0);
            }
            if(mouseHoveredHealth != null)
            {
                s.Draw(texWhitePixel, RectEnemyHealthBar, Color.Red);
                s.Draw(texWhitePixel, new Rectangle(RectEnemyHealthBar.X, RectEnemyHealthBar.Y, (int)(RectEnemyHealthBar.Width * mouseHoveredHealth.HealthPercent), RectEnemyHealthBar.Height), Color.Green);
            }

            #region UI
            #region UIBase
            //Chat Pane
            s.Draw(texWhitePixel, new Rectangle(0, (int) ((maxY-444*average)), (int) (362*average), (int)(444*average)) , Color.Black*0.5f);

            #region Ability Bar
            //Ability Bar
            s.Draw(texWhitePixel, new Rectangle((int)((maxX/2 -311 * average)), (int)((maxY - 158 * average)), (int)(622 * average), (int)(158 * average)), Color.Red * 0.5f);
            //Q
            s.Draw(boundAbilities[0].icon, new Rectangle((int)((maxX / 2 - 301 * average)), (int)((maxY - 148 * average)), (int)(64 * average), (int)(64 * average)), Color.White);
            //W
            s.Draw(boundAbilities[1].icon, new Rectangle((int)((maxX / 2 - 227 * average)), (int)((maxY - 148 * average)), (int)(64 * average), (int)(64 * average)), Color.White);
            //E
            s.Draw(boundAbilities[2].icon, new Rectangle((int)((maxX / 2 - 153 * average)), (int)((maxY - 148 * average)), (int)(64 * average), (int)(64 * average)), Color.White);
            //R
            s.Draw(boundAbilities[3].icon, new Rectangle((int)((maxX / 2 - 79 * average)), (int)((maxY - 148 * average)), (int)(64 * average), (int)(64 * average)), Color.White);
            //A
            s.Draw(boundAbilities[4].icon, new Rectangle((int)((maxX / 2 - 301 * average)), (int)((maxY - 74 * average)), (int)(64 * average), (int)(64 * average)), Color.White);
            //S
            s.Draw(boundAbilities[5].icon, new Rectangle((int)((maxX / 2 - 227 * average)), (int)((maxY - 74 * average)), (int)(64 * average), (int)(64 * average)), Color.White);
            //D
            s.Draw(boundAbilities[6].icon, new Rectangle((int)((maxX / 2 - 153 * average)), (int)((maxY - 74 * average)), (int)(64 * average), (int)(64 * average)), Color.White);
            //F
            s.Draw(boundAbilities[7].icon, new Rectangle((int)((maxX / 2 - 79 * average)), (int)((maxY - 74 * average)), (int)(64 * average), (int)(64 * average)), Color.White);

            //LM
            switch (GetMainhandType())
            {
                case AttackType.None:
                case AttackType.Melle:
                    s.Draw(melee, new Rectangle((int)((maxX / 2 + 5 * average)), (int)((maxY - 111 * average)), (int)(64 * average), (int)(64 * average)), Color.White);
                    break;
                case AttackType.Magic:
                    s.Draw(magic, new Rectangle((int)((maxX / 2 + 5 * average)), (int)((maxY - 111 * average)), (int)(64 * average), (int)(64 * average)), Color.White);
                    break;
                case AttackType.Ranged:
                    s.Draw(range, new Rectangle((int)((maxX / 2 + 5 * average)), (int)((maxY - 111 * average)), (int)(64 * average), (int)(64 * average)), Color.White);
                    break;
            }
                
            //RM
            s.Draw(placeHolder, new Rectangle((int)((maxX / 2 + 79 * average)), (int)((maxY - 111 * average)), (int)(64 * average), (int)(64 * average)), Color.White);

            //Item 1
            s.Draw(healthPot, new Rectangle((int)((maxX / 2 + 163 * average)), (int)((maxY - 148 * average)), (int)(64 * average), (int)(64 * average)), Color.White);
            //Item 2
            s.Draw(healthPot, new Rectangle((int)((maxX / 2 + 237 * average)), (int)((maxY - 148 * average)), (int)(64 * average), (int)(64 * average)), Color.White);
            //Item 3
            s.Draw(healthPot, new Rectangle((int)((maxX / 2 + 163 * average)), (int)((maxY - 74 * average)), (int)(64 * average), (int)(64 * average)), Color.White);
            //Item 4
            s.Draw(healthPot, new Rectangle((int)((maxX / 2 + 237 * average)), (int)((maxY - 74 * average)), (int)(64 * average), (int)(64 * average)), Color.White);

            #endregion
            
            #region Ability Bar Mods
            #region Cooldown Mod
            //TODO make into for loop for all bound abilities / items
            //s.Draw(texWhitePixel, new Rectangle((int)((maxX / 2 - (301 - 74 * 0) * average)), (int)((maxY - (84 + 64 * (boundAbilities[0].timeRemaining / boundAbilities[0].cooldownSeconds)) * average)), (int)(64 * average), (int)(64 * (boundAbilities[0].timeRemaining / boundAbilities[0].cooldownSeconds) * average) + 1), Color.Black * 0.5f);

            for (int i = 0; i < 4; i++)
            {
                if (boundAbilities[i].onCooldown)
                {
                    s.Draw(texWhitePixel, new Rectangle((int)((maxX / 2 - (301 - 74 * i) * average)), (int)((maxY - (84 + 64 * (boundAbilities[i].timeRemaining / boundAbilities[i].cooldownSeconds)) * average)), (int)(64 * average), (int)(64 * (boundAbilities[i].timeRemaining / boundAbilities[i].cooldownSeconds) * average) + 1), Color.Black * 0.5f);
                }
                if (boundAbilities[i + 4].onCooldown)
                {
                    s.Draw(texWhitePixel, new Rectangle((int)((maxX / 2 - (301 - 74 * i) * average)), (int)((maxY - (10  + 64 * (boundAbilities[i + 4].timeRemaining / boundAbilities[i + 4].cooldownSeconds)) * average)), (int)(64 * average), (int)(64 * (boundAbilities[i + 4].timeRemaining / boundAbilities[i + 4].cooldownSeconds) * average) + 1), Color.Black * 0.5f);
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
            s.Draw(texWhitePixel, new Rectangle((int)((maxX / 2 - 311 * average)), (int)((maxY - 178 * average)), (int)(622 * average), (int)(20 * average)), Color.Brown * 0.5f);
            //Damage Tracker
            s.Draw(texWhitePixel, new Rectangle((int)((maxX - 300 * average)), (int)((maxY - 230 * average)), (int)(300 * average), (int)(230 * average)), Color.Green * 0.5f);
            //Mini Map (square for now)
            s.Draw(texWhitePixel, new Rectangle((int)((maxX - 344 * average)), 0, (int)(344 * average), (int)(344 * average)), Color.Orange * 0.5f);
            //Main Player Frame Pic
            s.Draw(texWhitePixel, new Rectangle(0, 0, (int)(160 * average), (int)(160 * average)), Color.Blue * 0.5f);
            //Main Player Frame Health
            s.Draw(texWhitePixel, new Rectangle((int)(160 * average), 0, (int)(310 * average), (int)(52 * average)), Color.Blue * 0.5f);
            //Second Player Frame Pic
            s.Draw(texWhitePixel, new Rectangle((int)(20 * average), (int)(180 * average), (int)(54 * average), (int)(54 * average)), Color.Blue * 0.5f);
            //Second Player Frame Health
            s.Draw(texWhitePixel, new Rectangle((int)(74 * average), (int)(180 * average), (int)(74 * average), (int)(30 * average)), Color.Blue * 0.5f);
            //Third Player Frame Pic
            s.Draw(texWhitePixel, new Rectangle((int)(20 * average), (int)(254 * average), (int)(54 * average), (int)(54 * average)), Color.Blue * 0.5f);
            //Third Player Frame Health
            s.Draw(texWhitePixel, new Rectangle((int)(74 * average), (int)(254 * average), (int)(74 * average), (int)(30 * average)), Color.Blue * 0.5f);
            //Fourth Player Frame Pic
            s.Draw(texWhitePixel, new Rectangle((int)(20 * average), (int)(328 * average), (int)(54 * average), (int)(54 * average)), Color.Blue * 0.5f);
            //Fourth Player Frame Health
            s.Draw(texWhitePixel, new Rectangle((int)(74 * average), (int)(328 * average), (int)(74 * average), (int)(30 * average)), Color.Blue * 0.5f);
            //Fifth Player Frame Pic
            s.Draw(texWhitePixel, new Rectangle((int)(20 * average), (int)(402 * average), (int)(54 * average), (int)(54 * average)), Color.Blue * 0.5f);
            //Fifth Player Frame Health
            s.Draw(texWhitePixel, new Rectangle((int)(74 * average), (int)(402 * average), (int)(74 * average), (int)(30 * average)), Color.Blue * 0.5f);

            #endregion
            #endregion

        }
    }
}
