#region File Description
//-----------------------------------------------------------------------------
// ProjectileTrailParticleSystem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace KazgarsRevenge
{
    /// <summary>
    /// Custom particle system for leaving smoke trails behind the rocket projectiles.
    /// </summary>
    class ArmorSoulSystem : ParticleSystem
    {
        public ArmorSoulSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "soulTrail";

            settings.MaxParticles = 10000;

            settings.Duration = TimeSpan.FromSeconds(1);

            settings.Gravity = new Vector3(0, 20, 0);

            settings.DurationRandomness = 1.5f;

            settings.EmitterVelocitySensitivity = .3f;

            settings.MinHorizontalVelocity = -1;
            settings.MaxHorizontalVelocity = 1;

            settings.MinVerticalVelocity = -1;
            settings.MaxVerticalVelocity = 1;

            settings.MinRotateSpeed = 0;
            settings.MaxRotateSpeed = 0;

            settings.MinStartSize = 0;
            settings.MaxStartSize = 1;

            settings.MinEndSize = 7;
            settings.MaxEndSize = 8;

            settings.BlendState = BlendState.Additive;
        }
    }
}
