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
            systems.Add(typeof(FlashExplosionSmokeBig), new FlashExplosionSmokeBig(Game, Game.Content));
            systems.Add(typeof(FlashExplosionSmokeSmall), new FlashExplosionSmokeSmall(Game, Game.Content));
            systems.Add(typeof(FlashExplosionSparksParticleSystem), new FlashExplosionSparksParticleSystem(Game, Game.Content));
            systems.Add(typeof(TarExplosionParticleSystem), new TarExplosionParticleSystem(Game, Game.Content));
            systems.Add(typeof(FrostboltTrailParticleSystem), new FrostboltTrailParticleSystem(Game, Game.Content));
            systems.Add(typeof(FrostMistParticleSystem), new FrostMistParticleSystem(Game, Game.Content));
            systems.Add(typeof(FrostCharge4System), new FrostCharge4System(Game, Game.Content));
            systems.Add(typeof(FrostCharge3System), new FrostCharge3System(Game, Game.Content));
            systems.Add(typeof(FrostCharge2System), new FrostCharge2System(Game, Game.Content));
            systems.Add(typeof(FrostCharge1System), new FrostCharge1System(Game, Game.Content));


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
        }
    }
}
