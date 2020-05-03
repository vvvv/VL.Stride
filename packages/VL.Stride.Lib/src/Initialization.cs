using System.Linq;
using System.Threading;
using VL.Core;
using VL.Core.CompilerServices;
using VL.Lib.Basics.Resources;
using VL.Stride;
using VL.Stride.Assets;
using VL.Stride.Games;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;

[assembly: AssemblyInitializer(typeof(VL.Stride.Lib.Initialization))]

namespace VL.Stride.Lib
{
    public sealed class Initialization : AssemblyInitializer<Initialization>
    {
        public Initialization()
        {

        }

        protected override void RegisterServices(IVLFactory factory)
        {
            factory.RegisterService<NodeContext, IResourceProvider<Game>>(nodeContext =>
            {
                var key = nodeContext.GetResourceKey();
                return ResourceProvider.NewPooled(key,
                    factory: _ =>
                    {
                        var game = new VLGame();
#if DEBUG
                        game.GraphicsDeviceManager.DeviceCreationFlags |= DeviceCreationFlags.Debug;
#endif

                        var assetBuildService = new AssetBuildService();
                        game.Services.AddService(assetBuildService);

                        var gameStartedHandler = default(System.EventHandler);
                        gameStartedHandler = (s, e) =>
                        {
                            game.Script.Add(assetBuildService);
                            Game.GameStarted -= gameStartedHandler;
                        };
                        Game.GameStarted += gameStartedHandler;

                        var gameContext = new GameContextWinforms(null, 0, 0, isUserManagingRun: true);
                        game.Run(gameContext); // Creates the window
                        game.RunCallback = gameContext.RunCallback;

                        game.AddLayerRenderFeature();

                        return game;
                    }
                    , delayDisposalInMilliseconds: 0);
            });

            factory.RegisterService<NodeContext, IResourceProvider<GameWindow>>(nodeContext =>
            {
                var key = nodeContext.GetResourceKey();
                return ResourceProvider.NewPooled(key, k =>
                {
                    var gameProvider = nodeContext.GetGameProvider();
                    return gameProvider
                        .Bind(game =>
                        {
                            game.Window.Visible = true;
                            return ResourceProvider.Return(game.Window, disposeAction: (window) =>
                            {
                                window.Visible = false;
                            });
                        });
                });
            });
        }
    }
}