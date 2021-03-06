﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using VL.Core;
using VL.Xenko.Assets;
using VL.Xenko.EffectLib;
using Xenko.Core.Mathematics;
using Xenko.Engine;

namespace VL.Xenko
{
    public static class GameExtensions
    {
        /// <summary>
        /// Attaches VL to the game.
        /// </summary>
        /// <param name="game">The game to attach VL to.</param>
        /// <param name="document">The main document VL should open. Defaults to EXE-PATH/vl/Main.vl, so leave blank when distributing the output folder with contained vl docs.</param>
        /// <param name="openEditor">Whether or not to open the VL editor.</param>
        /// <param name="openEditorInOtherThread">Whether or not to start VL editor in separate thread.</param>
        /// <param name="goFullscreen">Whether or not the game window should go fullscreen.</param>
        public static void AttachVL(this Game game, string document = null, bool openEditor = true, bool openEditorInOtherThread = true, bool goFullscreen = false, int fullscreenWidth = -1, int fullscreenHeight = -1)
        {
            if (!CheckBuild(game))
                return;

            // Use lazy initialization so VL will start after the Xenko game is up an running
            var context = VLContext.Create("xenko",
                document: document ?? Path.Combine(Application.StartupPath, "vl", "Main.vl"),
                openEditor: openEditor,
                openEditorInOtherThread: openEditorInOtherThread,
                useInternalTimer: false,
                lazy: true);


            // Register the Xenko game in our registry so node factories can fetch it later
            ServiceRegistry.Default.RegisterService(game);

            //// Game is driving the message loop so tell windows forms about it
            //Application.RegisterMessageLoop(() => game.IsRunning);

            Game.GameStarted += (s, e) =>
            {
                // Raise Idle - needed by VL context to initiailize
                Application.RaiseIdle(System.EventArgs.Empty);

                // Register the main synchronization context
                ServiceRegistry.Default.RegisterService(SynchronizationContext.Current);

                // Register the context and the session in Xenko so we can fetch it later
                game.Services.AddService(context);
                game.Services.AddService(context.Session);
                game.Services.AddService(context.Runtime);

                var script = new VLScript(context, game, goFullscreen, new Int2(fullscreenWidth, fullscreenHeight));
                var assetBuildService = new AssetBuildService();
                game.Services.AddService(assetBuildService);
                game.Script.Add(script);
                game.Script.Add(assetBuildService);
                game.Window.AllowUserResizing = true;

                MultiGameEffectNodeFactory.WaitingGame = game;
                MultiGameEffectNodeFactory.WaitingSyncContext = context.Session.MainContext;

            };

            // Shutdown the game when VL editor shuts down
            context.ThreadExit += (s, e) => game.Exit();
            // Shutdown VL when game shuts down
            game.Exiting += (s, e) => context.Dispose();
        }

        private static bool CheckBuild(Game game)
        {
            var packsFolder = Path.Combine(Application.StartupPath, "vl", "packs");
            var packDirs = Directory.EnumerateDirectories(packsFolder);
            var buildSuccessful = packDirs.Any(f => Path.GetFileName(f).StartsWith("VL.CoreLib"));
            if (!buildSuccessful)
            {
                MessageBox.Show("Please Rebuild (again)", "VL.CoreLib not found", MessageBoxButtons.OK);
                Game.GameStarted += (s, e) =>
                {
                    game.Exit();
                };
            }

            return buildSuccessful;
        }
    }
}
