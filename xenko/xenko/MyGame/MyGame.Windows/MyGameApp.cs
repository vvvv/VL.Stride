using System;
using VL.Xenko;
using VL.Xenko.Games;

namespace MyGame.Windows
{
    class MyGameApp
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (var game = new VLGame())
            {
                game.AttachVL(VLScript.MainVLDocSrc);
                game.Run();
            }
        }
    }
}
