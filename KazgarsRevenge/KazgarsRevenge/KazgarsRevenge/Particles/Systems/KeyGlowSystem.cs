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
    class KeyGlowSystem : ParticleSystem
    {
        public KeyGlowSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "mist_white";

            settings.MaxParticles = 200;

            settings.Duration = TimeSpan.FromSeconds(1.25);

            settings.StartColor = Color.Gold * .65f;
            settings.EndColor = Color.Gold * .65f;

            settings.MinVerticalVelocity = -20;
            settings.MaxVerticalVelocity = -35;

            settings.MinStartSize = 20;
            settings.MaxStartSize = 20;

            settings.MinEndSize = 20;
            settings.MaxEndSize = 20;

            settings.BlendState = BlendState.Additive;
        }
    }
}
