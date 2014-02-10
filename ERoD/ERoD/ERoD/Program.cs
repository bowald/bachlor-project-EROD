using System;

namespace ERoD
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (ERoDGame game = new ERoDGame())
            {
                game.Run();
            }
        }
    }
#endif
}

