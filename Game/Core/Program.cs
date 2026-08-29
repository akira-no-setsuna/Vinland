using Game.Core.Main;

namespace Game.Core;

internal static class Program
{
    private static void Main(string[] args)
    {
        using var game = new GameManager();
        game.Run();
    }
}