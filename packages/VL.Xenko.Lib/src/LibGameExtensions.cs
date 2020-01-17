using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using VL.Core;
using VL.Xenko.Assets;
using VL.Xenko.Games;
using Xenko.Engine;
using Xenko.Games;
using Xenko.Graphics;
using VL.Xenko.EffectLib;

namespace VL.Xenko
{
    public static class LibGameExtensions
    {
        public static void CreateVLGame(out VLGame output, out Action runCallback)
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

            context.Control.Show();

            output = game;
        }
    }
}
