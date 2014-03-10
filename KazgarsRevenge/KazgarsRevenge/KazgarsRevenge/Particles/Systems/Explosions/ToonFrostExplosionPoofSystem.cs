using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Custom particle system for leaving smoke trails behind the rocket projectiles.
    /// </summary>
    class ToonFrostExplosionPoofSystem : ParticleSystem
    {
        public ToonFrostExplosionPoofSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoketoon";

            settings.MaxParticles = 150;

            settings.Duration = TimeSpan.FromSeconds(.85);

            settings.MinHorizontalVelocity = -275;
            settings.MaxHorizontalVelocity = 275;

            settings.StartColor = Color.Blue;
            settings.EndColor = Color.DarkBlue;

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
