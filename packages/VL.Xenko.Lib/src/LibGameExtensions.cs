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

namespace VL.Xenko
{
    public static class LibGameExtensions
    {
        public static void CreateVLGameWinForms(RectangleF bounds, out VLGame output, out Action runCallback, out GameWindow window)
        {
            var game = new VLGame();
#if DEBUG
            game.GraphicsDeviceManager.DeviceCreationFlags |= DeviceCreationFlags.Debug;
#endif

            Game.GameStarted += (s, e) =>
            {
                if (s == game)
                {
                    var script = new VLScript(null, game, false);
                    var assetBuildService = new AssetBuildService();
                    game.Services.AddService(assetBuildService);
                    game.Script.Add(assetBuildService);
                    game.Window.AllowUserResizing = true;

                    MultiGameEffectNodeFactory.Instance?.AddGame(game, SynchronizationContext.Current);

                }
            };

            Game.GameDestroyed += (s, e) =>
            {
                if (s == game)
                {
                    MultiGameEffectNodeFactory.Instance?.RemoveGame(game);
                }
            };

            var context = new GameContextWinforms(null, 0, 0, isUserManagingRun: true);
            game.Run(context);
            game.AddLayerRenderFeature();
            runCallback = context.RunCallback;

            window = game.Window;

            if (bounds.Width > 1 && bounds.Height > 1)
            {
                window.Position = new Int2((int)bounds.X, (int)bounds.Y);
                game.GraphicsDeviceManager.PreferredBackBufferWidth = (int)bounds.Width;
                game.GraphicsDeviceManager.PreferredBackBufferHeight = (int)bounds.Height;
                game.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
                game.GraphicsDeviceManager.ApplyChanges();
            }

            game.Window.Visible = true;

            output = game;
        }
    }
}
