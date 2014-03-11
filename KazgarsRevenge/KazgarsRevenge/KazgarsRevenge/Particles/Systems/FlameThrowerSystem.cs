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
    class FlameThrowerSystem : ParticleSystem
    {
        public FlameThrowerSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "flamethrower";
            settings.framesPerSecond = 30;
            settings.totalFrames = 23;
            settings.SpriteDimensions = new Vector2(5, 5);

            settings.MaxParticles = 200;

            settings.Duration = TimeSpan.FromSeconds(1.5);

            settings.DurationRandomness = 1.5f;

            settings.EmitterVelocitySensitivity = 0.03f;

            settings.MinHorizontalVelocity = -8;
            settings.MaxHorizontalVelocity = 8;

            settings.MinVerticalVelocity = -8;
            settings.MaxVerticalVelocity = 8;

            settings.MinRotateSpeed = -4;
            settings.MaxRotateSpeed = 4;

            settings.MinStartSize = 1;
            settings.MaxStartSize = 5;

            settings.MinEndSize = 80;
            settings.MaxEndSize = 80;
        }
    }
}
