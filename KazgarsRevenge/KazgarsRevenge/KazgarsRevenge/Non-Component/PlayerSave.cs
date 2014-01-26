using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    public class PlayerSave
    {
        public int CharacterLevel { get; private set; }
        public PlayerSave()
        {
            CharacterLevel = 10;
        }
    }
}
