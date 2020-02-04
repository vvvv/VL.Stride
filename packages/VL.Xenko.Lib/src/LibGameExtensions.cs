using System;
using System.IO;
using System.Threading;
using VL.Core;
using VL.Xenko.Assets;
using VL.Xenko.Games;
using Xenko.Engine;
using Xenko.Games;
using Xenko.Graphics;
using VL.Xenko.EffectLib;
using Xenko.Core.Mathematics;
using Xenko.Engine.Design;

namespace VL.Xenko
{
    public static class LibGameExtensions
    {
        public static void CreateVLGameWinForms(Rectangle bounds, out VLGame output, out Action runCallback, out GameWindow window)
        {
            var game = new VLGame();
#if DEBUG
            game.GraphicsDeviceManager.DeviceCreationFlags |= DeviceCreationFlags.Debug;
#endif

            SetupGameEvents(game);

            var context = new GameContextWinforms(null, 0, 0, isUserManagingRun: true);
            game.Run(context);
            game.AddLayerRenderFeature();
            runCallback = context.RunCallback;

            window = game.Window;

            if (bounds.Width > 1 && bounds.Height > 1)
            {
                game.GraphicsDeviceManager.PreferredBackBufferWidth = bounds.Width;
                game.GraphicsDeviceManager.PreferredBackBufferHeight = bounds.Height;
                game.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
                game.GraphicsDeviceManager.ApplyChanges();

                window.Position = new Int2(bounds.X, bounds.Y);
            }

            game.Window.Visible = true;

            output = game;
        }

        private static void SetupGameEvents(VLGame game)
        {
            var wrStarted = new WeakReference(null);

            EventHandler started = (s, e) =>
            {
                if (s == game)
                {
                    var script = new VLScript(null, game, false);
                    var assetBuildService = new AssetBuildService();
                    game.Services.AddService(assetBuildService);
                    game.Script.Add(assetBuildService);
                    game.Window.AllowUserResizing = true;

                    MultiGameEffectNodeFactory.Instance?.AddGame(game, SynchronizationContext.Current);

                    Game.GameStarted -= wrStarted.Target as EventHandler;
                }
            };

            wrStarted.Target = started;

            Game.GameStarted += started;

            var wrDestroyed = new WeakReference(null);
            EventHandler destroyed = (s, e) =>
            {
                if (s == game)
                {
                    MultiGameEffectNodeFactory.Instance?.RemoveGame(game);
                    Game.GameDestroyed -= wrDestroyed.Target as EventHandler;
                }
            };

            wrDestroyed.Target = destroyed;

            Game.GameDestroyed += destroyed;
        }
    }
}
