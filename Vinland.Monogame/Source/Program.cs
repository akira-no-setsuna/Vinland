using Vinland.Monogame.Main;

namespace Vinland.Monogame;

internal static class Program
{
    private static void Main(string[] args)
    {
        using var game = new Manager();
        game.Run();
    }
}