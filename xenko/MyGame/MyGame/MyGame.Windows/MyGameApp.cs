using System;
using Xenko.Engine;
using VL.Applications;
using VL.Xenko;

namespace MyGame
{
    class MyGameApp
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
            HDE.Main(new string[0], startupDoc);
        }

        static void HDEUpdate()
        {
            var runtimeHost = HDE.HDEContext.Session.RuntimeHost;
            if(runtimeHost.Mode == VL.Lang.Symbols.RunMode.Running)
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
            timer.Start();
            timer.Elapsed += (s, a) => {
                var game = (Game)sender;
                NuGetAssemblyLoader.AssemblyLoader.AddPackageRepositories(VLPackageRepositories);
                var script = new VLHDE(HDEStart, HDEUpdate);
                VLHDE.GameInstance = game;
                game.Script.Add(script);
            };
        }
    }
}
