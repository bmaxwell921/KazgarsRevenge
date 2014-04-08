using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Custom particle system for creating the smokey part of the explosions.
    /// </summary>
    class FrostDragonHeadSystem : ParticleSystem
    {
        public FrostDragonHeadSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "frost_trail";

            settings.MaxParticles = 500;

            settings.EmitterVelocitySensitivity = 0;

            settings.Duration = TimeSpan.FromSeconds(.5);

            settings.MinHorizontalVelocity = 60;
            settings.MaxHorizontalVelocity = 60;

            settings.MinVerticalVelocity = 60;
            settings.MaxVerticalVelocity = 60;

            settings.MinStartSize = 25;
            settings.MaxStartSize = 35;

            settings.MinEndSize = 7;
            settings.MaxEndSize = 15;
        }
    }
}
