using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Custom particle system for creating the smokey part of the explosions.
    /// </summary>
    class FireDragonHeadSystem : ParticleSystem
    {
        public FireDragonHeadSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "fire";

            settings.MaxParticles = 100;

            settings.EmitterVelocitySensitivity = 0;

            settings.Duration = TimeSpan.FromSeconds(.5);

            settings.MinHorizontalVelocity = 60;
            settings.MaxHorizontalVelocity = 60;

            settings.MinVerticalVelocity = 60;
            settings.MaxVerticalVelocity = 60;

            settings.MinStartSize = 5;
            settings.MaxStartSize = 8;

            settings.MinEndSize = 25;
            settings.MaxEndSize = 35;
        }
    }
}
