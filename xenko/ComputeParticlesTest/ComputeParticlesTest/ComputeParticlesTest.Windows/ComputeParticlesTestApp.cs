using Xenko.Engine;
using Xenko.Graphics;
using System;
using VL.Applications;
using VL.Xenko;

namespace ComputeParticlesTest
{
    class ComputeParticlesTesteApp
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.WindowCreated += Game_WindowCreated;              
                game.Run();
            }
        }

        static void HDEStart()
        {
            var startupDoc = VLDirectory + "Main.vl";

            //open VL editor only in debug build
#if DEBUG
            var openEditor = true;
#else            
            var openEditor = false; 
#endif

            //setup context
            HDE.Main(new string[0], startupDoc, openEditor);

            //disable VL mainloop
            var runtimeHost = HDE.HDEContext.Session.RuntimeHost as VL.Lang.Platforms.CIL.RuntimeHost;
            //runtimeHost.Mode = VL.Lang.Symbols.RunMode.Stopped;
            if (runtimeHost != null)
                runtimeHost.UseInternalTimer = false;

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

        static void HDEUpdate()
        {
            var runtimeHost = HDE.HDEContext.Session.RuntimeHost;
            if (runtimeHost.Mode == VL.Lang.Symbols.RunMode.Running)
                runtimeHost.Step();
        }

        static readonly string VLDirectory = Environment.CurrentDirectory + @"\VL\";

        static readonly string[] VLPackageRepositories = new string[]
        {
            VLDirectory + "packs"
        };

        private static void Game_WindowCreated(object sender, System.EventArgs e)
        {
            var timer = new System.Timers.Timer(3000);
            timer.AutoReset = false;
            timer.Elapsed += (s, a) => {

                //setup assembly loader
                NuGetAssemblyLoader.AssemblyLoader.AddPackageRepositories(VLPackageRepositories);
                var script = new VLHDE(HDEStart, HDEUpdate);

                //assign game root
                var game = (Game)sender;
                VLHDE.GameInstance = game;

                //attach VLHDE script
                game.Script.Add(script);
            };
            timer.Start();

        }
    }
}
