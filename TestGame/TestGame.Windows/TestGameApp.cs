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
                //game.GraphicsDeviceManager.DeviceCreationFlags |= Xenko.Graphics.DeviceCreationFlags.Debug;
                game.AttachVL(VLScript.MainVLDocSrc);
                game.Run();
            }
        }
    }
}
