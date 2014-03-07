using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Custom particle system for leaving smoke trails behind the rocket projectiles.
    /// </summary>
    class FireMistTrailSystem : ParticleSystem
    {
        public FireMistTrailSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "mist";

            settings.MaxParticles = 3000;

            settings.Duration = TimeSpan.FromSeconds(1);

            settings.DurationRandomness = 3f;

            settings.EmitterVelocitySensitivity = 0.01f;

            settings.MinHorizontalVelocity = 1;
            settings.MaxHorizontalVelocity = 10;

            settings.MinVerticalVelocity = -10;
            settings.MaxVerticalVelocity = 10;

            settings.StartColor = Color.OrangeRed;
            settings.EndColor = Color.OrangeRed;

            settings.MinRotateSpeed = -4;
            settings.MaxRotateSpeed = 4;

            settings.MinStartSize = 10;
            settings.MaxStartSize = 20;

            settings.MinEndSize = 40;
            settings.MaxEndSize = 80;

            settings.BlendState = BlendState.Additive;
        }
    }
}
