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
    class FrostSwipeSystem : ParticleSystem
    {
        public FrostSwipeSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "mist_white";

            settings.MaxParticles = 500;

            settings.Duration = TimeSpan.FromSeconds(.5);

            settings.EmitterVelocitySensitivity = 0.1f;

            settings.StartColor = Color.Blue;
            settings.EndColor = Color.White;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 0;

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 0;

            settings.MinStartSize = 65;
            settings.MaxStartSize = 65;

            settings.MinEndSize = 20;
            settings.MaxEndSize = 20;

            settings.BlendState = BlendState.Additive;
        }
    }
}
