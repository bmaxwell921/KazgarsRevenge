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
    class FrostChargeSystem : ParticleSystem
    {
        public FrostChargeSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "cast_charge";

            settings.MaxParticles = 500;

            settings.Duration = TimeSpan.FromSeconds(.25f);

            settings.DurationRandomness = 1.5f;

            settings.EmitterVelocitySensitivity = 0.05f;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 0;

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 0;

            settings.MinColor = Color.LightBlue;
            settings.MaxColor = Color.LightBlue;

            settings.MinRotateSpeed = 4;
            settings.MaxRotateSpeed = 4;

            settings.MinStartSize = 2;
            settings.MaxStartSize = 2;

            settings.MinEndSize = 2;
            settings.MaxEndSize = 2;

            settings.BlendState = BlendState.Additive;
        }
    }
}
