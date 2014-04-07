#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace KazgarsRevenge
{
    /// <summary>
    /// Custom particle system for creating a flame effect.
    /// </summary>
    class LevelUpExplosionSystem : ParticleSystem
    {
        public LevelUpExplosionSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoketoon";

            settings.MaxParticles = 100;

            settings.Duration = TimeSpan.FromSeconds(1);

            settings.DurationRandomness = 1;

            settings.StartColor = Color.Gold;
            settings.EndColor = Color.Gold;

            settings.MinHorizontalVelocity = -150;
            settings.MaxHorizontalVelocity = 150;

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 150;

            settings.Gravity = new Vector3(0, 0, 0);

            settings.MinStartSize = 25;
            settings.MaxStartSize = 35;

            settings.MinEndSize = 1;
            settings.MaxEndSize = 5;
        }
    }
}
