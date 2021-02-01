using GameDev2.src;
using System;

namespace GameDev2
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new Game1())
            {
                GlobalProps.GetGameInstance = game;
                game.Run();
            }
        }
    }
}
