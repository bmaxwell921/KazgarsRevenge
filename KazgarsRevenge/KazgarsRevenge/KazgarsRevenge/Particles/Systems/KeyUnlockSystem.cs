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
    class KeyUnlockSystem : ParticleSystem
    {
        public KeyUnlockSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoketoon";

            settings.MaxParticles = 200;

            settings.Duration = TimeSpan.FromSeconds(.75f);
            settings.DurationRandomness = .5f;

            settings.MinHorizontalVelocity = -200;
            settings.MaxHorizontalVelocity = 200;
            settings.EndVelocity = .2f;

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 30;

            settings.MinRotateSpeed = 4;
            settings.MaxRotateSpeed = 8;

            settings.MinStartSize = 10;
            settings.MaxStartSize = 10;

            settings.MinEndSize = 45;
            settings.MaxEndSize = 55;
        }
    }
}
