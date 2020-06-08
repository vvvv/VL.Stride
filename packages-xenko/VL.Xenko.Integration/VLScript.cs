using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using VL.Xenko.Core;
using VL.Xenko.Games;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Graphics;

namespace VL.Xenko
{
    // TODO: VL script should talk to one IVLNode so we can also feed it data properly from Xenko as well as exposing its pins to Xenko editor
    public class VLScript : AsyncScript
    {
        /// <summary>
        /// Recommended default doc while developing.
        /// </summary>
        public static string MainVLDocSrc => Path.Combine(Application.StartupPath,"..", "..", "..", "vl", "Main.vl");

        private readonly Action VLUpdate;
        readonly VLContext FContext;
        readonly bool FGoFullscreen;
        readonly Int2 FFullscreenSize;

        // Declared public member fields and properties will show in the game studio
        public VLScript(VLContext context, Game game, bool goFullscreen, Int2 fullscreenSize)
        {
            FContext = context;
            FGoFullscreen = goFullscreen;
            FFullscreenSize = fullscreenSize;
            Game = game;
            VLGame.GameInstance = game;

            if (FContext != null)   
                VLUpdate = FContext.Update;
        }

        public new Game Game { get; }

        public override async Task Execute()
        {
            VLGame.GameInstance = Game;

            if (FGoFullscreen)
            {
                //if (FContext.Runtime is RuntimeHost rth)
                //    rth.Mode = Lang.Symbols.RunMode.Paused;

                var gfxOutput = GraphicsAdapterFactory.Adapters[0].Outputs;
                var displayMode = gfxOutput[0].CurrentDisplayMode;

                var screenWidth = FFullscreenSize.X > 0 ? FFullscreenSize.X : 1920;

                var maxHeight = displayMode.AspectRatio < 1.7f ? 1200 : 1080;
                var screenHeight = FFullscreenSize.Y > 0 ? FFullscreenSize.Y : maxHeight;

                Game.GraphicsDeviceManager.PreferredBackBufferWidth = screenWidth;
                Game.GraphicsDeviceManager.PreferredBackBufferHeight = screenHeight;
                Game.GraphicsDeviceManager.IsFullScreen = true;
                Game.GraphicsDeviceManager.ApplyChanges();
            }

            if (Game is VLGame vlGame)
                vlGame.AddLayerRenderFeature();

            while (true)
            {
                await Script.NextFrame();
                // Update all VL root nodes
                //await Task.Run(VLUpdate);
                FContext?.Update();
            }
        }
    }
}
