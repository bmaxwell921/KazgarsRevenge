using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Custom particle system for creating the smokey part of the explosions.
    /// </summary>
    class SmokeAOESystem : ParticleSystem
    {
        public SmokeAOESystem(Game game, ContentManager content)
            : base(game, content)
        { }



        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoketoon";

            settings.MaxParticles = 100;

            settings.Duration = TimeSpan.FromSeconds(2);

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 0;

            settings.StartColor = Color.DarkGray;
            settings.EndColor = Color.Black;


            settings.MinVerticalVelocity = 15;
            settings.MaxVerticalVelocity = 25;

            settings.MinStartSize = 1;
            settings.MaxStartSize = 5;

            settings.MinEndSize = 55;
            settings.MaxEndSize = 75;
        }
    }
}
