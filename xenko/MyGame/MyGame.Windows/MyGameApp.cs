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
                game.AttachVL();
                game.Run();
            }
        }
    }
}
