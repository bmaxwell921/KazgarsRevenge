using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public enum InteractiveType
    {
        Shopkeeper,
        EssenceGuy,
        Chest,
        Lootsoul,
    }

    public interface IPlayerInteractiveController
    {
        InteractiveType GetType();

        void Target();
        void UnTarget();
    }
}
