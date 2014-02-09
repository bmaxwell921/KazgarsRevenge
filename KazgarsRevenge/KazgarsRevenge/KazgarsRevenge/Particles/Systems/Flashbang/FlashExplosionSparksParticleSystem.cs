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
    class FlashExplosionSparksParticleSystem : ParticleSystem
    {
        public FlashExplosionSparksParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "spark";

            settings.MaxParticles = 200;

            settings.Duration = TimeSpan.FromSeconds(1);
            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = -100;
            settings.MaxHorizontalVelocity = 100;

            settings.MinVerticalVelocity = 100;
            settings.MaxVerticalVelocity = 160;

            settings.Gravity = new Vector3(0, -160, 0);

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 3;
            settings.MaxStartSize = 5;

            settings.MinEndSize = 0;
            settings.MaxEndSize = 1;

        }
    }
}
