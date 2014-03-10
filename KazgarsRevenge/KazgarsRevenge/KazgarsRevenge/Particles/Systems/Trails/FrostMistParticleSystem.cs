using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Custom particle system for leaving smoke trails behind the rocket projectiles.
    /// </summary>
    class FrostMistParticleSystem : ParticleSystem
    {
        public FrostMistParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "mist_white";

            settings.MaxParticles = 500;

            settings.Duration = TimeSpan.FromSeconds(1);

            settings.DurationRandomness = 3f;

            settings.EmitterVelocitySensitivity = 0.01f;

            settings.MinHorizontalVelocity = 1;
            settings.MaxHorizontalVelocity = 10;

            settings.MinVerticalVelocity = -10;
            settings.MaxVerticalVelocity = 10;

            settings.StartColor = Color.DodgerBlue;
            settings.EndColor = Color.DodgerBlue;

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
