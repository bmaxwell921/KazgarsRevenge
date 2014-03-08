#region File Description
//-----------------------------------------------------------------------------
// SmokePlumeParticleSystem.cs
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
    /// Custom particle system for creating a giant plume of long lasting smoke.
    /// </summary>
    class LifestealParticleSystem : ParticleSystem
    {
        public LifestealParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "plus";

            settings.MaxParticles = 100;

            settings.Duration = TimeSpan.FromSeconds(1);

            settings.EmitterVelocitySensitivity = .5f;

            settings.DurationRandomness = 1.5f;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 0;

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 0;

            settings.Gravity = new Vector3(0, 100, 0);

            settings.StartColor = Color.LightGreen;
            settings.EndColor = Color.LightGreen;

            settings.MinStartSize = 7;
            settings.MaxStartSize = 9;

            settings.MinEndSize = 1;
            settings.MaxEndSize = 3;
        }
    }
}
