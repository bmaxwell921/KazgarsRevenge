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

            settings.MaxParticles = 150;

            settings.Duration = TimeSpan.FromSeconds(.5);

            settings.EmitterVelocitySensitivity = 0.1f;

            settings.StartColor = Color.Yellow;
            settings.EndColor = Color.Red;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 0;

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 0;

            settings.MinStartSize = 40;
            settings.MaxStartSize = 40;

            settings.MinEndSize = 20;
            settings.MaxEndSize = 20;

            settings.BlendState = BlendState.Additive;
        }
    }
}
