using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    class NetworkMessageManager : GameComponent
    {
        AttackManager attacks;
        PlayerManager players;
        LevelManager levels;
        EnemyManager enemies;

        public NetworkMessageManager(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            attacks = Game.Services.GetService(typeof(AttackManager)) as AttackManager;
            players = Game.Services.GetService(typeof(PlayerManager)) as PlayerManager;
            levels = Game.Services.GetService(typeof(LevelManager)) as LevelManager;
            enemies = Game.Services.GetService(typeof(EnemyManager)) as EnemyManager;
        }

        public void ReceiveMessage()
        {

        }
    }
}
