using System;

namespace KazgarsRevengeServer
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Server game = new Server())
            {
                game.Run();
            }
        }
    }
#endif
}

