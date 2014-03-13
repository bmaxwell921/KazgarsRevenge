﻿using System;
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
        public static readonly string WHITE_CURSOR = @"Textures\whiteCursor";
        public class UI
        {
            public static readonly string INPUT_TEXTURE = @"Textures\UI\TextBox";
            public static readonly string CHARGE_BAR_FRONT = @"Textures\UI\chargeBarFront";
            public static readonly string Place_Holder = @"Textures\\UI\\Abilities\\BN";

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
        }

        public class BillBoards
        {
            public static readonly string BLOB = @"Textures\Billboards\blob";
            public static readonly string GRND_IND = @"Textures\Billboards\groundIndicator";
            public static readonly string CHAIN = @"Textures\Billboards\chain";
            public static readonly string ROPE = @"Textures\Billboards\rope";
        }

        public class Menu
        {
            public static readonly string BACKGRND = @"Textures\Menu\menuBackground";
        }
    }
}
