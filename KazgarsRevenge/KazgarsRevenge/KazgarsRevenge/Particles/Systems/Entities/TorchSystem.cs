using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Custom particle system for leaving smoke trails behind the rocket projectiles.
    /// </summary>
    class TorchSystem : ParticleSystem
    {
        public TorchSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "fire";

            settings.MaxParticles = 500;

            settings.Duration = TimeSpan.FromSeconds(5);

            settings.DurationRandomness = 1.5f;

            settings.MinVerticalVelocity = 2;
            settings.MaxVerticalVelocity = 5;

            settings.MinRotateSpeed = 0;
            settings.MaxRotateSpeed = 0;

            settings.MinStartSize = 15;
            settings.MaxStartSize = 10;

            settings.MinEndSize = 1;
            settings.MaxEndSize = 2;
        }
    }
}
