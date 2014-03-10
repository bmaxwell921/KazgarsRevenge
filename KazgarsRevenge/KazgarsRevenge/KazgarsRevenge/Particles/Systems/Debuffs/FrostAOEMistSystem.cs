using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Custom particle system for creating the smokey part of the explosions.
    /// </summary>
    class FrostAOEMistSystem : ParticleSystem
    {
        public FrostAOEMistSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "mist_white";

            settings.MaxParticles = 500;

            settings.Duration = TimeSpan.FromSeconds(1.25);

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 0;

            settings.StartColor = Color.DodgerBlue;
            settings.EndColor = Color.DodgerBlue;

            settings.MinVerticalVelocity = 90;
            settings.MaxVerticalVelocity = 120;

            settings.MinStartSize = 115;
            settings.MaxStartSize = 120;

            settings.MinEndSize = 20;
            settings.MaxEndSize = 25;

            settings.BlendState = BlendState.Additive;
        }
    }
}
