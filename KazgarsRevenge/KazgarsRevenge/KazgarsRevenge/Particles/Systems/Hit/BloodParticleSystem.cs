using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    /// <summary>
    /// Custom particle system for creating a giant plume of long lasting smoke.
    /// </summary>
    class BloodParticleSystem : ParticleSystem
    {
        public BloodParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "blood";

            settings.MaxParticles = 600;

            settings.Duration = TimeSpan.FromSeconds(1);

            settings.MinHorizontalVelocity = -15;
            settings.MaxHorizontalVelocity = 15;

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 60;

            settings.Gravity = new Vector3(0, -140, 0);

            settings.MinStartSize = 10;
            settings.MaxStartSize = 10;

            settings.MinEndSize = 20;
            settings.MaxEndSize = 20;
        }
    }
}
