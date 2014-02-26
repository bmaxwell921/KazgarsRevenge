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
    class SmokeTrailParticleSystem : ParticleSystem
    {
        public SmokeTrailParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoke";

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(3);

            settings.DurationRandomness = 1.5f;

            settings.EmitterVelocitySensitivity = 0.05f;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 1;

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 1;

            settings.MinColor = new Color(64, 96, 128, 255);
            settings.MaxColor = new Color(255, 255, 255, 128);

            settings.MinRotateSpeed = -4;
            settings.MaxRotateSpeed = 4;

            settings.MinStartSize = 10;
            settings.MaxStartSize = 20;

            settings.MinEndSize = 1;
            settings.MaxEndSize = 5;
        }
    }
}
