using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Custom particle system for creating the smokey part of the explosions.
    /// </summary>
    class FrostAOESystem : ParticleSystem
    {
        public FrostAOESystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "frost_trail";

            settings.MaxParticles = 200;

            settings.Duration = TimeSpan.FromSeconds(2);

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 0;

            settings.MinVerticalVelocity = 40;
            settings.MaxVerticalVelocity = 50;

            settings.MinStartSize = 1;
            settings.MaxStartSize = 5;

            settings.MinEndSize = 15;
            settings.MaxEndSize = 25;
        }
    }
}
