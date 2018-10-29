using System;
using Xenko.Engine;
using VL.Xenko;
using System.IO;

namespace MyVLGame
{
    class MyVLGameApp
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                var startupPath = AppDomain.CurrentDomain.BaseDirectory;
                var mainDocument = Path.Combine(startupPath, "..", "..", "..", "vl", "Main.vl");
                game.AttachVL(mainDocument, openEditor: true, goFullscreen: false);
                game.Run();
            }
        }
    }
}
