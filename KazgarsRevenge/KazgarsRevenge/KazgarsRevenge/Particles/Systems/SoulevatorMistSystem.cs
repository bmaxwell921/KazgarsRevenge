using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Custom particle system for leaving smoke trails behind the rocket projectiles.
    /// </summary>
    class SoulevatorMistSystem : ParticleSystem
    {
        public SoulevatorMistSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "mist_white";

            settings.MaxParticles = 700;

            settings.Duration = TimeSpan.FromSeconds(10);

            settings.MinHorizontalVelocity = -4;
            settings.MaxHorizontalVelocity = 4;

            settings.MinVerticalVelocity = 25;
            settings.MaxVerticalVelocity = 35;

            settings.StartColor = Color.DodgerBlue;
            settings.EndColor = Color.DodgerBlue;

            settings.MinStartSize = 15;
            settings.MaxStartSize = 15;

            settings.MinEndSize = 15;
            settings.MaxEndSize = 15;

            settings.BlendState = BlendState.Additive;
        }
    }
}
