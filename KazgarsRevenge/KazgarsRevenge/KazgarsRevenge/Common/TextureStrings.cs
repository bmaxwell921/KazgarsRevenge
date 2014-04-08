using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    /// <summary>
    /// While this may look weird, I really like this model. When you need the
    /// LOTS of coins string you do: TextureStrings.UI.Items.Coins.LOTS. Makes everything nice and organized
    /// </summary>
    public class TextureStrings
    {
        public static readonly string WHITE = @"white";
        public static readonly string WHITE_PIX = @"Textures\whitePixel";
        public class UI
        {
            public static readonly string WHITE_CURSOR = @"Textures\UI\cursor";
            public static readonly string HOVER = @"Textures\UI\hover";
            public static readonly string INPUT_TEXTURE = @"Textures\UI\TextBox";
            public static readonly string CHARGE_BAR_FRONT = @"Textures\UI\chargeBarFront";
            public static readonly string Place_Holder = @"Textures\UI\Abilities\BN";
            public static readonly string Talent_Arrow_U = @"Textures\UI\Frames\TTArrow";
            public static readonly string Talent_Arrow_UL = @"Textures\UI\Frames\TTArrow1";
            public static readonly string Talent_Arrow_L = @"Textures\UI\Frames\TTArrow2";
            public static readonly string Talent_Arrow_DL = @"Textures\UI\Frames\TTArrow3";
            public static readonly string Talent_Arrow_D = @"Textures\UI\\Frames\TTArrow4";
            public static readonly string Talent_Arrow_DR = @"Textures\UI\Frames\TTArrow5";
            public static readonly string Talent_Arrow_R = @"Textures\UI\Frames\TTArrow6";
            public static readonly string Talent_Arrow_UR = @"Textures\UI\Frames\TTArrow7";

            public static readonly string MeleeBanner = @"Textures\UI\Frames\meleeBanner";
            public static readonly string RangedBanner = @"Textures\UI\Frames\rangedBanner";
            public static readonly string MagicBanner = @"Textures\UI\Frames\magicBanner";


            public class Items
            {
                public class Coins
                {
                    public static readonly string FEW = @"Textures\UI\Items\gold1";
                    public static readonly string SOME = @"Textures\UI\Items\gold2";
                    public static readonly string LOTS = @"Textures\UI\Items\gold3";
                }

                public class Potions
                {
                    public static readonly string HEALTH = @"Textures\UI\Items\HP";
                    public static readonly string SUPER_HEALTH = @"Textures\UI\Items\UNIMPLEMENTED";
                    public static readonly string INSTA_HEALTH = @"Textures\UI\Items\UNIMPLEMENTED";
                    public static readonly string LUCK = @"Textures\UI\Items\UNIMPLEMENTED";
                    public static readonly string INVISIBILITY = @"Textures\UI\Items\UNIMPLEMENTED";
                }

                public class Armor
                {
                    public static readonly string MELEE_HEAD_RHINO = @"Textures\UI\Items\Armor\helmetIcon";
                    public static readonly string MELEE_CHEST_RHINO = @"Textures\UI\Items\Armor\I2";
                    public static readonly string MELEE_LEGS_RHINO = @"Textures\UI\Items\Armor\I2";
                    public static readonly string MELEE_FEET_RHINO = @"Textures\UI\Items\Armor\I2";
                    public static readonly string MELEE_SHOULDERS_RHINO = @"Textures\UI\Items\Armor\I2";
                    public static readonly string MELEE_WRIST_RHINO = @"Textures\UI\Items\Armor\I2";
                    public static readonly string MELEE_BLING = @"Textures\UI\Items\Armor\I2";

                    public static readonly string RANGED_HEAD_SKELETON = @"Textures\UI\Items\Armor\helmetIcon";
                    public static readonly string RANGED_CHEST_SKELETON = @"Textures\UI\Items\Armor\I2";
                    public static readonly string RANGED_LEGS_SKELETON = @"Textures\UI\Items\Armor\I2";
                    public static readonly string RANGED_FEET_SKELETON = @"Textures\UI\Items\Armor\I2";
                    public static readonly string RANGED_SHOULDERS_SKELETON = @"Textures\UI\Items\Armor\I2";
                    public static readonly string RANGED_WRIST_SKELETON = @"Textures\UI\Items\Armor\I2";
                    public static readonly string RANGED_BLING = @"Textures\UI\Items\Armor\I2";

                    public static readonly string MAGIC_HEAD_SMART = @"Textures\UI\Items\Armor\helmetIcon";
                    public static readonly string MAGIC_CHEST_SMART = @"Textures\UI\Items\Armor\I2";
                    public static readonly string MAGIC_LEGS_SMART = @"Textures\UI\Items\Armor\I2";
                    public static readonly string MAGIC_FEET_SMART = @"Textures\UI\Items\Armor\I2";
                    public static readonly string MAGIC_SHOULDERS_SMART = @"Textures\UI\Items\Armor\I2";
                    public static readonly string MAGIC_WRIST_SMART = @"Textures\UI\Items\Armor\I2";
                    public static readonly string MAGIC_BLING = @"Textures\UI\Items\Armor\I2";
                }
                public class Weapons
                {
                    public static readonly string BRUTISH_AXE = @"Textures\UI\Items\Armor\I3";
                    public static readonly string GENERIC_SWORD = @"Textures\UI\Items\Armor\I1";
                    public static readonly string GENERIC_STAFF = @"Textures\UI\Items\Armor\BR";

                    public static readonly string GENERIC_BOW = @"Textures\UI\Items\Armor\LW";
                    public static readonly string GENERIC_CROSSBOW = @"Textures\UI\Items\Armor\speedy Graple";
                }
            }

            public class Abilities
            {
                public class Range
                {
                    public static readonly string SNIPE = @"Textures\UI\Abilities\snipe";
                    public static readonly string HEART_STRIKE = @"Textures\UI\Abilities\HS";
                    public static readonly string ICE_CLAW_PRI = @"Textures\whitePixel";
                    public static readonly string OMNI_SHOT = @"Textures\UI\Abilities\OmniShot";
                    public static readonly string ADREN_RUSH = @"Textures\UI\Abilities\AdrenR";
                    public static readonly string LEECH_ARROWS = @"Textures\UI\Abilities\leechingArrow";
                    public static readonly string LOOSE_CANNON = @"Textures\UI\Abilities\looseCan";
                    public static readonly string MAKE_IT_RAIN = @"Textures\UI\Abilities\makeItRain";
                    public static readonly string FLASH_BOMB = @"Textures\UI\Abilities\flashBomb";
                    public static readonly string TAR_BOMB = @"Textures\UI\Abilities\tarBomb";
                    public static readonly string GRAP_HOOK = @"Textures\UI\Abilities\graple";
                    public static readonly string CHAIN_SPEAR = @"Textures\UI\Abilities\I4";
                    public static readonly string MOLT_BOLT = @"Textures\UI\Abilities\moltenBolt";
                    public static readonly string PENETRATING = @"Textures\UI\Abilities\penetratingShot";
                    public static readonly string HOMING = @"Textures\UI\Abilities\homingM";
                    public static readonly string SERRATED = @"Textures\UI\Abilities\seratedA";
                    public static readonly string HEADSHOT = @"Textures\UI\Abilities\headshot";
                    public static readonly string MAGNETIC_IMPLANT = @"Textures\UI\Abilities\magI";
                    public static readonly string MAKE_IT_HAIL = @"Textures\UI\Abilities\makeItHail";
                    public static readonly string STRONG_WINDS = @"Textures\UI\Abilities\strongWinds";
                    public static readonly string SPEEDY_GRAPPLE = @"Textures\UI\Abilities\speedy Graple";
                    public static readonly string BIGGER_BOMBS = @"Textures\UI\Abilities\biggerBombs";
                    public static readonly string TUMBLE = @"Textures\UI\Abilities\tumble";
                    public static readonly string ELUSIVENESS = @"Textures\UI\Abilities\evasion";
                }

                public class Melee
                {
                    public static readonly string CLEAVE = @"Textures\UI\Abilities\Melee\cleave";
                    public static readonly string Decapitation = @"Textures\UI\Abilities\Melee\decapitation";
                    public static readonly string Invigoration = @"Textures\UI\Abilities\Melee\headbutt";
                    public static readonly string ObsidianCoagulation = @"Textures\UI\Abilities\Melee\obsidian";
                    public static readonly string DevastatingStrike = @"Textures\UI\Abilities\Melee\devastate";
                    public static readonly string DevastatingReach = @"Textures\UI\Abilities\Melee\reach";
                    public static readonly string Reflect = @"Textures\UI\Abilities\Melee\reflect";
                    public static readonly string Charge = @"Textures\UI\Abilities\Melee\charge";
                    public static readonly string Garrote = @"Textures\UI\Abilities\Melee\garrote";
                    public static readonly string ExcruciatingTwist = @"Textures\UI\Abilities\Melee\twist";
                    public static readonly string SadisticFrenzy = @"Textures\UI\Abilities\Melee\sadistic";
                    public static readonly string Bash = @"Textures\UI\Abilities\Melee\bash";
                    public static readonly string Berserk = @"Textures\UI\Abilities\Melee\berserk";
                    public static readonly string SecondWind = @"Textures\UI\Abilities\Melee\secondwind";
                    public static readonly string RiskyRegeneration = @"Textures\UI\Abilities\Melee\regen";
                    public static readonly string RejuvenatingStrikes = @"Textures\UI\Abilities\Melee\rejuv";
                    public static readonly string Headbutt = @"Textures\UI\Abilities\Melee\headbutt";
                    public static readonly string ChainSpear = @"Textures\UI\Abilities\Melee\chainspear";
                    public static readonly string ForcefulThrow = @"Textures\UI\Abilities\Melee\throw";
                    public static readonly string Execute = @"Textures\UI\Abilities\Melee\execute";
                    public static readonly string Swordnado = @"Textures\UI\Abilities\Melee\swordnado";
                    public static readonly string regen = @"Textures\UI\Abilities\Melee\headbutt";

                }
            }

            public class Frames
            {
                public static readonly string ICON_SEL = @"Textures\UI\Frames\icon_selected";
                public static readonly string HEALTH_BAR = @"Textures\UI\Frames\health_bar";
                public static readonly string RIGHT_ARROW = @"Textures\UI\Frames\rightArrow";
                public static readonly string LEFT_ARROW = @"Textures\UI\Frames\leftArrow";
                public static readonly string HELMET = @"Textures\UI\Frames\helmetIcon";
            }

            public class Buffs
            {
                public static readonly string AdrenalineRush = @"Textures\UI\Abilities\AdrenR";
                public static readonly string Berserk = @"Textures\UI\Abilities\BN";
                public static readonly string Berserk2 = @"Textures\UI\Abilities\BN";
                public static readonly string Berserk3 = @"Textures\UI\Abilities\BN";
                public static readonly string Elusiveness = @"Textures\UI\Abilities\BN";
                public static readonly string HealthPotion = @"Textures\UI\Abilities\BN";
                public static readonly string Homing = @"Textures\UI\Abilities\BN";
                public static readonly string Invincibility = @"Textures\UI\Abilities\BN";
                public static readonly string LuckPotion = @"Textures\UI\Abilities\BN";
                public static readonly string None = @"Textures\UI\Abilities\BN";
                public static readonly string SadistivFrenzy = @"Textures\UI\Abilities\BN";
                public static readonly string SerratedBleeding = @"Textures\UI\Abilities\BN";
                public static readonly string SuperHealthPotion = @"Textures\UI\Abilities\BN";
                public static readonly string Swordnado = @"Textures\UI\Abilities\BN";
                public static readonly string Unstoppable = @"Textures\UI\Abilities\BN";
            }

            public class DeBuffs
            {
                public static readonly string Burning = @"Textures\UI\Abilities\BN";
                public static readonly string Charge = @"Textures\UI\Abilities\BN";
                public static readonly string Execute = @"Textures\UI\Abilities\BN";
                public static readonly string FlashBomb = @"Textures\UI\Abilities\BN";
                public static readonly string ForcefulThrow = @"Textures\UI\Abilities\BN";
                public static readonly string Frost = @"Textures\UI\Abilities\BN";
                public static readonly string Frozen = @"Textures\UI\Abilities\BN";
                public static readonly string Garrote = @"Textures\UI\Abilities\BN";
                public static readonly string Headbutt = @"Textures\UI\Abilities\BN";
                public static readonly string MagneticImplant = @"Textures\UI\Abilities\BN";
                public static readonly string None = @"Textures\UI\Abilities\BN";
                public static readonly string SerratedBleeding = @"Textures\UI\Abilities\BN";
                public static readonly string Stunned = @"Textures\UI\Abilities\BN";
                public static readonly string Tar = @"Textures\UI\Abilities\BN";

            }
        }

        public class BillBoards
        {
            public static readonly string Beam = @"Textures\Billboards\laserbillboard";
            public static readonly string BLOB = @"Textures\Billboards\blob";
            public static readonly string GRND_IND = @"Textures\Billboards\groundIndicator";
            public static readonly string CHAIN = @"Textures\Billboards\chain";
            public static readonly string ROPE = @"Textures\Billboards\rope";
            public static readonly string ARROWV = @"Textures\Billboards\arrowtrail";
            public static readonly string CIRCLE = @"Textures\Billboards\circle";
            public static readonly string Rings = @"Textures\Billboards\rings";
            public static readonly string Saiyan = @"Textures\Billboards\saiyan";
            public static readonly string LevelUpBeam = @"Textures\Billboards\level_up";
        }

        public class Menu
        {
            public static readonly string BACKGRND = @"Textures\Menu\menuBackground";
        }
    }
}
