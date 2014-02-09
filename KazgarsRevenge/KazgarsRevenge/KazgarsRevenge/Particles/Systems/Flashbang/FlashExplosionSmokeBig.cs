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
    class FlashExplosionSmokeBig: ParticleSystem
    {
        public FlashExplosionSmokeBig(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoke";

            settings.MaxParticles = 200;
            
            settings.Duration = TimeSpan.FromSeconds(.25);
            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = -100;
            settings.MaxHorizontalVelocity = 100;

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 30;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 0;
            settings.MaxStartSize = 0;

            settings.MinEndSize = 80;
            settings.MaxEndSize = 120;

            settings.BlendState = BlendState.AlphaBlend;
        }
    }
}
