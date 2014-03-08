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
    class FireArrowParticleSystem : ParticleSystem
    {
        public FireArrowParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "fire";

            settings.MaxParticles = 600;

            settings.Duration = TimeSpan.FromSeconds(.5);

            settings.DurationRandomness = 1;

            settings.EmitterVelocitySensitivity = .2f;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 25;

            settings.MaxRotateSpeed = 4f;
            settings.MinRotateSpeed = -4f;

            settings.Gravity = new Vector3(0, 15, 0);

            settings.MinVerticalVelocity = -10;
            settings.MaxVerticalVelocity = 10;

            settings.MinStartSize = 5;
            settings.MaxStartSize = 10;

            settings.MinEndSize = 15;
            settings.MaxEndSize = 20;

        }
    }
}
