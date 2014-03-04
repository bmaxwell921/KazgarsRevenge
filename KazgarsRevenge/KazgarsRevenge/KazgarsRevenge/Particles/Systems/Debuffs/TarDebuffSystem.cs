using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Custom particle system for leaving smoke trails behind the rocket projectiles.
    /// </summary>
    class TarDebuffParticleSystem : ParticleSystem
    {
        public TarDebuffParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoke";

            settings.MaxParticles = 200;

            settings.Duration = TimeSpan.FromSeconds(1);
            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = -100;
            settings.MaxHorizontalVelocity = 100;

            settings.MinVerticalVelocity = 100;
            settings.MaxVerticalVelocity = 130;

            settings.Gravity = new Vector3(0, -130, 0);

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 50;
            settings.MaxStartSize = 50;

            settings.MinEndSize = 6;
            settings.MaxEndSize = 15;

        }
    }
}
