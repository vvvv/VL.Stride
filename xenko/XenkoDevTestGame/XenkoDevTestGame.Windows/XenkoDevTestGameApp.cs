using Xenko.Engine;

namespace XenkoDevTestGame.Windows
{
    class XenkoDevTestGameApp
    {
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
