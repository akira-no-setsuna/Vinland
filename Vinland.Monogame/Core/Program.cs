using Vinland.Monogame.Core.Main;

namespace Vinland.Monogame.Core;

internal static class Program
{
    private static void Main(string[] args)
    {
        using var game = new Manager();
        game.Run();
    }
}