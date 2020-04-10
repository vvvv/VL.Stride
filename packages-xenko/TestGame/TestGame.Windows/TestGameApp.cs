using System;
using Xenko.Engine;
using VL.Xenko;
using VL.Xenko.Games;

namespace TestGame.Windows
{
    class TestGameApp
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (var game = new VLGame())
            {
#if DEBUG
                game.GraphicsDeviceManager.DeviceCreationFlags |= Xenko.Graphics.DeviceCreationFlags.Debug;
#endif
                game.AttachVL(VLScript.MainVLDocSrc);
                game.Run();
            }
        }
    }
}
