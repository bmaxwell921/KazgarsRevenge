using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Custom particle system for leaving smoke trails behind the rocket projectiles.
    /// </summary>
    class GopherDigPoofSystem : ParticleSystem
    {
        public GopherDigPoofSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoketoon";

            settings.MaxParticles = 150;

            settings.Duration = TimeSpan.FromSeconds(1);

            settings.MinHorizontalVelocity = -185;
            settings.MaxHorizontalVelocity = 185;

            settings.StartColor = Color.SaddleBrown;
            settings.EndColor = Color.SaddleBrown;

            settings.MinVerticalVelocity = 100;
            settings.MaxVerticalVelocity = 150;

            settings.Gravity = new Vector3(0, -50, 0);

            settings.EndVelocity = 0;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 15;
            settings.MaxStartSize = 21;

            settings.MinEndSize = 5;
            settings.MaxEndSize = 10;

        }
    }
}
