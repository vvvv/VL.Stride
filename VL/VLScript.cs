using System;
using Xenko.Engine;
using Xenko.Graphics;
using VL;
using System.Windows.Forms;
using System.IO;

namespace VL.Xenko
{
    public class VLScript : SyncScript
    {
        public static Game GameInstance;
        public static new GraphicsDevice GraphicsDevice => GameInstance.GraphicsDevice;

        readonly string FStartupDoc;
        VLContext FContext;

        // Declared public member fields and properties will show in the game studio
        public VLScript(string startupDoc = null, string[] packageRepositories = null)
        {
            FStartupDoc = string.IsNullOrWhiteSpace(startupDoc) ? Path.Combine(Application.StartupPath, "vl", "Main.vl") : startupDoc;
        }

        public override void Start()
        {
            //open VL editor only in debug build
#if DEBUG
            var openEditor = true;
#else            
            var openEditor = false; 
#endif

            //setup context
            FContext = VLSetup.CreateContext(FStartupDoc, openEditor, false);

            //go fullscreen in release build
#if !DEBUG
            var gfxOutput = GraphicsAdapterFactory.Adapters[0].Outputs;
            var displayMode = gfxOutput[0].CurrentDisplayMode;
            
            var screenWidth = Math.Min(displayMode.Width, 1920);
            var maxHeight = displayMode.AspectRatio < 1.7f ? 1200 : 1080;
            var screenHeight = Math.Min(displayMode.Height, maxHeight);

            var game = VLHDE.GameInstance;
            game.GraphicsDeviceManager.PreferredBackBufferWidth = screenWidth;
            game.GraphicsDeviceManager.PreferredBackBufferHeight = screenHeight;
            game.GraphicsDeviceManager.IsFullScreen = true;
            game.GraphicsDeviceManager.ApplyChanges(); 
#endif
        }

        public override void Update()
        {
            FContext.Update();
        }
    }
}
