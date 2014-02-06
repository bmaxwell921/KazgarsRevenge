#region File Description
//-----------------------------------------------------------------------------
// FireParticleSystem.cs
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
    /// Custom particle system for creating a flame effect.
    /// </summary>
    class Buff1ParticleSystem : ParticleSystem
    {
        public Buff1ParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "spark";

            settings.MaxParticles = 500;

            settings.Duration = TimeSpan.FromSeconds(2);

            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = -50;
            settings.MaxHorizontalVelocity = 50;

            settings.MinVerticalVelocity = -50;
            settings.MaxVerticalVelocity = 50;

            settings.Gravity = new Vector3(0, 0, 0);
            
            settings.MinStartSize = 13;
            settings.MaxStartSize = 10;

            settings.MinEndSize = 0;
            settings.MaxEndSize = 1;
        }
    }
}
