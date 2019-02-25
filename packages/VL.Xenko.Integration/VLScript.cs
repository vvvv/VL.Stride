using System;
using System.Windows.Forms;
using Xenko.Engine;
using Xenko.Graphics;

namespace VL.Xenko
{
    // TODO: VL script should talk to one IVLNode so we can also feed it data properly from Xenko as well as exposing its pins to Xenko editor
    public class VLScript : SyncScript
    {
        /// <summary>
        /// Recommended default doc while developing.
        /// </summary>
        public const string MainVLDocSrc = @"..\..\..\vl\Main.vl";

        // TODO: Get rid of me
        public static Game GameInstance { get; private set; }

        readonly VLContext FContext;
        readonly bool FGoFullscreen;

        // Declared public member fields and properties will show in the game studio
        public VLScript(VLContext context, Game game, bool goFullscreen)
        {
            FContext = context;
            FGoFullscreen = goFullscreen;
            Game = game;
            GameInstance = game;
        }

        public new Game Game { get; }

        public override void Start()
        {
            if (FGoFullscreen)
            {
                var gfxOutput = GraphicsAdapterFactory.Adapters[0].Outputs;
                var displayMode = gfxOutput[0].CurrentDisplayMode;

                var screenWidth = Math.Min(displayMode.Width, 1920);
                var maxHeight = displayMode.AspectRatio < 1.7f ? 1200 : 1080;
                var screenHeight = Math.Min(displayMode.Height, maxHeight);

                Game.GraphicsDeviceManager.PreferredBackBufferWidth = screenWidth;
                Game.GraphicsDeviceManager.PreferredBackBufferHeight = screenHeight;
                Game.GraphicsDeviceManager.IsFullScreen = true;
                Game.GraphicsDeviceManager.ApplyChanges();
            }
        }

        public override void Update()
        {
            // Update all VL root nodes
            FContext.Update();
        }
    }
}
