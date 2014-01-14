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
    class ShadowParticleSystem : ParticleSystem
    {
        public ShadowParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "blob";

            settings.MaxParticles = 2400;

            settings.Duration = TimeSpan.FromSeconds(2);

            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = -5;
            settings.MaxHorizontalVelocity = 5;

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 0;

            settings.Gravity = new Vector3(0, 0, 0);

            settings.MinColor = new Color(0, 0, 0);
            settings.MaxColor = new Color(0, 0, 0);
            
            settings.MinStartSize = 5;
            settings.MaxStartSize = 5;

            settings.MinEndSize = 5;
            settings.MaxEndSize = 5;
        }
    }
}
