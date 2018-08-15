using NuGetAssemblyLoader;
using System;
using Xenko.Engine;
using Xenko.Graphics;
using VL.Applications;

namespace VL.Xenko
{
    public class VLScript : SyncScript
    {
        public static Game GameInstance;
        public static new GraphicsDevice GraphicsDevice => GameInstance.GraphicsDevice;

        static readonly string VLDefaultDirectory = Environment.CurrentDirectory + @"\VL\";
        readonly string FStartupDoc;
        readonly string[] VLPackageRepositories;

        // Declared public member fields and properties will show in the game studio
        public VLScript(string startupDoc = null, string[] packageRepositories = null)
        {
            FStartupDoc = string.IsNullOrWhiteSpace(startupDoc) ? VLDefaultDirectory + "Main.vl" : startupDoc;
            VLPackageRepositories = packageRepositories == null || packageRepositories.Length == 0 ? new string[] { VLDefaultDirectory + "packs" } : packageRepositories;
        }

        public override void Start()
        {
            GameInstance = this.Services.GetService<Game>();

            AssemblyLoader.AddPackageRepositories(VLPackageRepositories);

            //open VL editor only in debug build
#if DEBUG
            var openEditor = true;
#else            
            var openEditor = false; 
#endif

            //setup context
            HDE.Main(new string[0], FStartupDoc, openEditor);

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
            var runtimeHost = HDE.HDEContext.Session.RuntimeHost;
            if (runtimeHost.Mode == VL.Lang.Symbols.RunMode.Running)
                runtimeHost.Step();
        }

        private static void Game_WindowCreated(object sender, System.EventArgs e)
        {
            var game = (Game)sender;
            game.Window.AllowUserResizing = true;

            var timer = new System.Timers.Timer(3000);
            timer.AutoReset = false;
            timer.Elapsed += (s, a) => {

                //setup assembly loader
                var script = new VLScript();

                //attach VLHDE script
                game.Script.Add(script);
            };
            timer.Start();
        }
    }
}
