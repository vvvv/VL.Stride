using System;
using Xenko.Engine;
using VL.Xenko;

namespace WorkshopGame.Windows
{
    class WorkshopGameApp
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                var vlMainDoc = @"..\..\..\vl\Main.vl";
                game.AttachVL(vlMainDoc, goFullscreen: true);
                game.Run();
            }
        }
    }
}
