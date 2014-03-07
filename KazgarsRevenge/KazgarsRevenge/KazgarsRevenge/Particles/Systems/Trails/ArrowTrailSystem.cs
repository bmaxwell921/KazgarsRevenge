#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace KazgarsRevenge
{
    /// <summary>
    /// Custom particle system for creating a giant plume of long lasting smoke.
    /// </summary>
    class ArrowTrailSystem : ParticleSystem
    {
        public ArrowTrailSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "arrow_particle";

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(2);

            settings.DurationRandomness = 1.5f;

            settings.EmitterVelocitySensitivity = 0.1f;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 0;

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 0;

            settings.Gravity = new Vector3(0, -140, 0);

            settings.MinStartSize = 15;
            settings.MaxStartSize = 10;

            settings.MinEndSize = 1;
            settings.MaxEndSize = 5;
        }
    }
}
