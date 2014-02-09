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
    class FlashExplosionSmokeSmall : ParticleSystem
    {
        public FlashExplosionSmokeSmall(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoke";

            settings.MaxParticles = 200;

            settings.Duration = TimeSpan.FromSeconds(5);
            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = -35;
            settings.MaxHorizontalVelocity = 35;

            settings.EndVelocity = 0;

            settings.MinVerticalVelocity = 00;
            settings.MaxVerticalVelocity = 35;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 48;
            settings.MaxStartSize = 48;

            settings.MinEndSize = 48;
            settings.MaxEndSize = 48;

            settings.BlendState = BlendState.AlphaBlend;
        }
    }
}
