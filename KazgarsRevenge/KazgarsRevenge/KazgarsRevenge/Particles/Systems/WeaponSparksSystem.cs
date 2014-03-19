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

            settings.Duration = TimeSpan.FromSeconds(.5);
            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = -60;
            settings.MaxHorizontalVelocity = 60;

            settings.MinVerticalVelocity = 60;
            settings.MaxVerticalVelocity = 80;

            settings.Gravity = new Vector3(0, -45, 0);
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
