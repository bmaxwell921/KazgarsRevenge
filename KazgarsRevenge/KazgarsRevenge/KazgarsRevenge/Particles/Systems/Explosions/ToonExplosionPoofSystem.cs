using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Custom particle system for leaving smoke trails behind the rocket projectiles.
    /// </summary>
    class ToonExplosionPoofSystem : ParticleSystem
    {
        public ToonExplosionPoofSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoketoon";

            settings.MaxParticles = 500;

            settings.Duration = TimeSpan.FromSeconds(1f);

            settings.MinHorizontalVelocity = -175;
            settings.MaxHorizontalVelocity = 175;

            settings.StartColor = Color.Orange;
            settings.EndColor = new Color(80, 0, 0);

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 2;

            settings.EndVelocity = 0;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 10;
            settings.MaxStartSize = 10;

            settings.MinEndSize = 10;
            settings.MaxEndSize = 10;

        }
    }
}
