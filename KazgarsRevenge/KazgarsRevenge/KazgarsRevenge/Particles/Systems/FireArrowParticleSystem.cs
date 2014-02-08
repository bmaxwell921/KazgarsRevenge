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
    class FireArrowParticleSystem : ParticleSystem
    {
        public FireArrowParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "fire";

            settings.MaxParticles = 2400;

            settings.Duration = TimeSpan.FromSeconds(.5);

            settings.DurationRandomness = 1;

            settings.EmitterVelocitySensitivity = .2f;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 25;

            settings.Gravity = new Vector3(0, 15, 0);

            settings.MinColor = new Color(255, 255, 255, 10);
            settings.MaxColor = new Color(255, 255, 255, 40);

            settings.MinVerticalVelocity = -10;
            settings.MaxVerticalVelocity = 10;

            settings.MinStartSize = 10;
            settings.MaxStartSize = 20;

            settings.MinEndSize = 35;
            settings.MaxEndSize = 60;

            settings.BlendState = BlendState.Additive;
        }
    }
}
