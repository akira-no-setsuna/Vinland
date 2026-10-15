using Vinland.Core.Main;

namespace Vinland.Core;

internal static class Program
{
    private static void Main(string[] args)
    {
        using var game = new Manager();
        game.Run();
    }
}