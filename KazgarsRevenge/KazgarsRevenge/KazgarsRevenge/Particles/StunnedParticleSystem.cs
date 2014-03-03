using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Custom particle system for leaving smoke trails behind the rocket projectiles.
    /// </summary>
    class StunnedParticleSystem : ParticleSystem
    {
        public StunnedParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "question";

            settings.MaxParticles = 2000;

            settings.Duration = TimeSpan.FromSeconds(1);
            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = -50;
            settings.MaxHorizontalVelocity = 50;

            settings.MinVerticalVelocity = 100;
            settings.MaxVerticalVelocity = 130;

            settings.Gravity = new Vector3(0, -130, 0);

            settings.MinRotateSpeed = -4;
            settings.MaxRotateSpeed = 4;

            settings.MinStartSize = 10;
            settings.MaxStartSize = 15;

            settings.MinEndSize = 2;
            settings.MaxEndSize = 7;

        }
    }
}
