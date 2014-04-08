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
    class DevastateTrailSystem : ParticleSystem
    {
        public DevastateTrailSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "mist_white";

            settings.MaxParticles = 300;

            settings.Duration = TimeSpan.FromSeconds(.5);

            settings.EmitterVelocitySensitivity = 0;

            settings.StartColor = Color.Orange;
            settings.EndColor = Color.Red;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 0;

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 0;

            settings.MinStartSize = 55;
            settings.MaxStartSize = 55;

            settings.MinEndSize = 15;
            settings.MaxEndSize = 15;

            settings.BlendState = BlendState.Additive;
        }
    }
}
