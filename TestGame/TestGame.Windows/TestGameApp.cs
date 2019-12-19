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
#if DEBUG
                game.GraphicsDeviceManager.DeviceCreationFlags |= Xenko.Graphics.DeviceCreationFlags.Debug;
#endif
                game.AttachVL();
                game.Run();
            }
        }
    }
}
