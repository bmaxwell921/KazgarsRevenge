using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

using KazgarsRevenge;

namespace KazgarsRevengeServer
{
    public abstract class SEntityManager : GameComponent
    {
        protected GeneralComponentManager gcm;
        protected KazgarsRevengeGame game;

        public SEntityManager(KazgarsRevengeGame game)
            : base(game)
        {
            this.game = game;
        }

        public override void Initialize()
        {
            base.Initialize();
            gcm = Game.Services.GetService(typeof(GeneralComponentManager)) as GeneralComponentManager;
        }

    }
}
