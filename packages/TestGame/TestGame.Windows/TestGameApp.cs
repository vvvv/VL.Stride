using System;
using Stride.Engine;
using VL.Xenko;
using VL.Stride.Games;

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
                game.GraphicsDeviceManager.DeviceCreationFlags |= Stride.Graphics.DeviceCreationFlags.Debug;
#endif
                game.AttachVL(VLScript.MainVLDocSrc);
                game.Run();
            }
        }
    }
}
