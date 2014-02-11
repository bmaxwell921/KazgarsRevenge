using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazgarsRevenge
{
    /// <summary>
    /// Class used to represent an action of transitioning to a new menu
    /// </summary>
    public class MenuTransitionAction : IAction
    {
        // MenuManager to use
        private MenuManager mm;

        // The menu to transition to
        private IMenu next;

        public MenuTransitionAction(MenuManager mm, IMenu next)
        {
            this.mm = mm;
            this.next = next;
        }
        public void Perform()
        {
            mm.TransitionTo(next);
        }
    }
}
