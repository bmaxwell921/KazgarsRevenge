using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Custom particle system for creating the smokey part of the explosions.
    /// </summary>
    class FirePillarSystem : ParticleSystem
    {
        public FirePillarSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "fire";

            settings.MaxParticles = 200;

            settings.Duration = TimeSpan.FromSeconds(1.5);

            settings.MinHorizontalVelocity = 5;
            settings.MaxHorizontalVelocity = 5;

            settings.MinVerticalVelocity = 30;
            settings.MaxVerticalVelocity = 60;

            settings.MinStartSize = 15;
            settings.MaxStartSize = 25;

            settings.MinEndSize = 35;
            settings.MaxEndSize = 55;

            settings.BlendState = BlendState.Additive;
        }
    }
}
