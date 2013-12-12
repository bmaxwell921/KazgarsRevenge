#region File Description
//-----------------------------------------------------------------------------
// ExplosionSmokeParticleSystem.cs
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
    /// Custom particle system for creating the smokey part of the explosions.
    /// </summary>
    class SoulCreationParticleSystem : ParticleSystem
    {
        public SoulCreationParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "soulTrail";

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(1);

            settings.MinHorizontalVelocity = -50;
            settings.MaxHorizontalVelocity = 50;

            settings.MinVerticalVelocity = -50;
            settings.MaxVerticalVelocity = 50;

            settings.MinStartSize = 10;
            settings.MaxStartSize = 10;

            settings.MinEndSize = 0;
            settings.MaxEndSize = 0;
        }
    }
}
