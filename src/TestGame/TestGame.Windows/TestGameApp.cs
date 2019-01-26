using System;
using Xenko.Engine;
using VL.Xenko;

namespace TestGame.Windows
{
    class TestGameApp
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                var vlMainDoc = @"..\..\..\vl\Main.vl";
                game.AttachVL(vlMainDoc);
                game.Run();
            }
        }
    }
}
