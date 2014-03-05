using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Custom particle system for leaving smoke trails behind the rocket projectiles.
    /// </summary>
    class AnimTestSystem : ParticleSystem
    {
        public AnimTestSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "animtest";
            settings.framesPerSecond = 1;
            settings.SpriteDimensions = new Vector2(1, 4);
            settings.totalFrames = 4;

            settings.MaxParticles = 100;

            settings.Duration = TimeSpan.FromSeconds(10);

            settings.DurationRandomness = 1.5f;

            settings.EmitterVelocitySensitivity = 0.1f;

            settings.MinHorizontalVelocity = -1;
            settings.MaxHorizontalVelocity = 1;

            settings.MinVerticalVelocity = -1;
            settings.MaxVerticalVelocity = 1;

            settings.MinRotateSpeed = 0;
            settings.MaxRotateSpeed = 0;

            settings.MinStartSize = 20;
            settings.MaxStartSize = 20;

            settings.MinEndSize = 20;
            settings.MaxEndSize = 20;

            settings.BlendState = BlendState.Additive;
        }
    }
}
