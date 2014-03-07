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
    class ToonExplosionDebrisSystem : ParticleSystem
    {
        public ToonExplosionDebrisSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoketoon";

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(1f);

            settings.DurationRandomness = 1.5f;

            settings.EmitterVelocitySensitivity = 0.03f;

            settings.MinHorizontalVelocity = -8;
            settings.MaxHorizontalVelocity = 8;

            settings.MinVerticalVelocity = 8;
            settings.MaxVerticalVelocity = 20;

            settings.StartColor = Color.Orange;
            settings.EndColor = new Color(100, 0, 0);

            settings.MinRotateSpeed = -4;
            settings.MaxRotateSpeed = 4;

            settings.MinStartSize = 20;
            settings.MaxStartSize = 20;

            settings.MinEndSize = 10;
            settings.MaxEndSize = 10;
        }
    }
}
