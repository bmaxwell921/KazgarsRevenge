using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Custom particle system for creating the smokey part of the explosions.
    /// </summary>
    class FireAOESystem : ParticleSystem
    {
        public FireAOESystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "fire";

            settings.MaxParticles = 100;

            settings.Duration = TimeSpan.FromSeconds(1.5);

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 0;

            settings.MinVerticalVelocity = 8;
            settings.MaxVerticalVelocity = 15;

            settings.MinStartSize = 1;
            settings.MaxStartSize = 5;

            settings.MinEndSize = 55;
            settings.MaxEndSize = 65;

            settings.BlendState = BlendState.Additive;
        }
    }
}
