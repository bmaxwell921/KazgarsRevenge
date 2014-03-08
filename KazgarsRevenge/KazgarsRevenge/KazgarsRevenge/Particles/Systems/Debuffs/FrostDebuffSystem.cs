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
    class FrostDebuffParticleSystem : ParticleSystem
    {
        public FrostDebuffParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "mist_white";

            settings.MaxParticles = 3000;

            settings.Duration = TimeSpan.FromSeconds(3);

            settings.DurationRandomness = 3f;

            settings.EmitterVelocitySensitivity = 0.01f;

            settings.MinHorizontalVelocity = 5;
            settings.MaxHorizontalVelocity = 15;

            settings.MinVerticalVelocity = -10;
            settings.MaxVerticalVelocity = 10;

            settings.StartColor = Color.DodgerBlue;
            settings.EndColor = Color.DodgerBlue;

            settings.MinRotateSpeed = -4;
            settings.MaxRotateSpeed = 4;

            settings.MinStartSize = 15;
            settings.MaxStartSize = 25;

            settings.MinEndSize = 50;
            settings.MaxEndSize = 95;

            settings.BlendState = BlendState.Additive;
        }
    }
}
