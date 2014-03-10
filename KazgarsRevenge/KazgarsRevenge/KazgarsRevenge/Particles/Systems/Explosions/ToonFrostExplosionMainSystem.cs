using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Custom particle system for leaving smoke trails behind the rocket projectiles.
    /// </summary>
    class ToonFrostExplosionMainSystem : ParticleSystem
    {
        public ToonFrostExplosionMainSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoketoon";

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(2f);

            settings.MinHorizontalVelocity = -100;
            settings.MaxHorizontalVelocity = 100;

            settings.StartColor = Color.LightBlue;
            settings.EndColor = Color.DarkBlue;

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 60;
            settings.EndVelocity = 0;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 20;
            settings.MaxStartSize = 20;

            settings.MinEndSize = 40;
            settings.MaxEndSize = 40;

        }
    }
}
