#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace KazgarsRevenge
{
    /// <summary>
    /// Custom particle system for leaving smoke trails behind the rocket projectiles.
    /// </summary>
    class FrostMistParticleSystem : ParticleSystem
    {
        public FrostMistParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "mist";

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(2);

            settings.DurationRandomness = 3f;

            settings.EmitterVelocitySensitivity = 0.01f;

            settings.MinHorizontalVelocity = 1;
            settings.MaxHorizontalVelocity = 10;

            settings.MinVerticalVelocity = -10;
            settings.MaxVerticalVelocity = 10;

            settings.MinColor = Color.LightBlue;
            settings.MaxColor = Color.LightBlue;

            settings.MinRotateSpeed = -4;
            settings.MaxRotateSpeed = 4;

            settings.MinStartSize = 20;
            settings.MaxStartSize = 40;

            settings.MinEndSize = 80;
            settings.MaxEndSize = 150;

            settings.BlendState = BlendState.Additive;
        }
    }
}
