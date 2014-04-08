#region File Description
//-----------------------------------------------------------------------------
// ExplosionParticleSystem.cs
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
    /// Custom particle system for creating the fiery part of the explosions.
    /// </summary>
    class WeaponSparksSystem : ParticleSystem
    {
        public WeaponSparksSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "spark";

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(.35);
            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = -100;
            settings.MaxHorizontalVelocity = 100;

            settings.MinVerticalVelocity = 70;
            settings.MaxVerticalVelocity = 100;

            settings.EndVelocity = 0;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 5;
            settings.MaxStartSize = 5;

            settings.MinEndSize = 0;
            settings.MaxEndSize = .5f;
        }
    }
}
