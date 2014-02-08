using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KazgarsRevenge
{
    class ParticleManager : GameComponent
    {
        Dictionary<Type, ParticleSystem> systems = new Dictionary<Type, ParticleSystem>();
        CameraComponent camera;
        public ParticleManager(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            camera = Game.Services.GetService(typeof(CameraComponent)) as CameraComponent;

            systems.Add(typeof(WeaponSparksSystem), new WeaponSparksSystem(Game, Game.Content));
            systems.Add(typeof(SoulCreationParticleSystem), new SoulCreationParticleSystem(Game, Game.Content));
            systems.Add(typeof(Buff1ParticleSystem), new Buff1ParticleSystem(Game, Game.Content));
            systems.Add(typeof(SoulTrailParticleSystem), new SoulTrailParticleSystem(Game, Game.Content));
            systems.Add(typeof(BloodParticleSystem), new BloodParticleSystem(Game, Game.Content));
            systems.Add(typeof(SnipeTrailParticleSystem), new SnipeTrailParticleSystem(Game, Game.Content));
            systems.Add(typeof(HomingTrailParticleSystem), new HomingTrailParticleSystem(Game, Game.Content));
            systems.Add(typeof(HealTrailParticleSystem), new HealTrailParticleSystem(Game, Game.Content));
            systems.Add(typeof(FireArrowParticleSystem), new FireArrowParticleSystem(Game, Game.Content));
            systems.Add(typeof(SmokeTrailParticleSystem), new SmokeTrailParticleSystem(Game, Game.Content));
            systems.Add(typeof(ExplosionParticleSystem), new ExplosionParticleSystem(Game, Game.Content));
            systems.Add(typeof(LifestealParticleSystem), new LifestealParticleSystem(Game, Game.Content));


            foreach (KeyValuePair<Type, ParticleSystem> k in systems)
            {
                k.Value.Initialize();
            }
        }

        public ParticleSystem GetSystem(Type t)
        {
            return systems[t];
        }

        public override void Update(GameTime gameTime)
        {
            foreach (KeyValuePair<Type, ParticleSystem> k in systems)
            {
                k.Value.Update(gameTime);
                k.Value.SetCamera(camera.View, camera.Projection);
            }
        }

        public void Draw(GameTime gameTime)
        {
            foreach (KeyValuePair<Type, ParticleSystem> k in systems)
            {
                k.Value.Draw(gameTime);
            }
            Game.GraphicsDevice.BlendState = BlendState.Opaque;
            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
    }
}
