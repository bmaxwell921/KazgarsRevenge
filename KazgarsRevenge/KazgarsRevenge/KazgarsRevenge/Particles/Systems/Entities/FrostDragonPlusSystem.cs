using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Custom particle system for creating the smokey part of the explosions.
    /// </summary>
    class FireDragonPlusSystem : ParticleSystem
    {
        public FireDragonPlusSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "plus";

            settings.MaxParticles = 50;

            settings.Duration = TimeSpan.FromSeconds(1.5);

            settings.StartColor = Color.Orange;
            settings.EndColor = Color.Red;

            settings.MinHorizontalVelocity = 5;
            settings.MaxHorizontalVelocity = 5;

            settings.MinVerticalVelocity = 10;
            settings.MaxVerticalVelocity = 15;

            settings.MinStartSize = 15;
            settings.MaxStartSize = 25;

            settings.MinEndSize = 35;
            settings.MaxEndSize = 55;
        }
    }
}
