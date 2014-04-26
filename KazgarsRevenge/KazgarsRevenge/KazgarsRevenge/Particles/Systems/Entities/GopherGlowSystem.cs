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
    class GopherGlowSystem : ParticleSystem
    {
        public GopherGlowSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "mist_white";

            settings.MaxParticles = 200;

            settings.EmitterVelocitySensitivity = .01f;
            settings.Duration = TimeSpan.FromSeconds(.5);

            settings.StartColor = Color.Gold * .65f;
            settings.EndColor = Color.Gold * .65f;

            settings.MinVerticalVelocity = -10;
            settings.MaxVerticalVelocity = -15;

            settings.MinStartSize = 10;
            settings.MaxStartSize = 10;

            settings.MinEndSize = 10;
            settings.MaxEndSize = 10;

            settings.BlendState = BlendState.Additive;
        }
    }
}
