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

            settings.Duration = TimeSpan.FromSeconds(1);

            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = -20;
            settings.MaxHorizontalVelocity = 20;

            settings.MinVerticalVelocity = -20;
            settings.MaxVerticalVelocity = 20;

            settings.Gravity = new Vector3(0, 0, 0);

            settings.MinColor = new Color(0, 0, 0);
            settings.MaxColor = new Color(0, 0, 0);
            
            settings.MinStartSize = 5;
            settings.MaxStartSize = 5;

            settings.MinEndSize = 0;
            settings.MaxEndSize = 0;
        }
    }
}
