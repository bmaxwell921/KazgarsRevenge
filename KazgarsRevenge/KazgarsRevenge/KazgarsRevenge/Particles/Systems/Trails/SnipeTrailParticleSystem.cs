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
    class SnipeTrailParticleSystem : ParticleSystem
    {
        public SnipeTrailParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "mist_white";

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(.5);

            settings.EmitterVelocitySensitivity = 0.1f;

            settings.StartColor = Color.Yellow;
            settings.EndColor = Color.Yellow;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 0;

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 0;

            settings.MinStartSize = 76;
            settings.MaxStartSize = 76;

            settings.MinEndSize = 30;
            settings.MaxEndSize = 30;

            settings.BlendState = BlendState.Additive;
        }
    }
}
