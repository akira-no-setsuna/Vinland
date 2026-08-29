using Game;
using Game.Core;
using Game.Core.Application;
using Game.Core.Main;


namespace Game.Core
{
    static class Program
    {
        static void Main(string[] args)
        {
            using var game = new GameManager();
            game.Run();
        }
    }
}

