using System;
using VL.Xenko;
using Xenko.Engine;

namespace MyGame.Windows
{
    class MyGameApp
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
