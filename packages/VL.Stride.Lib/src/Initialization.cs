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
using System.Windows.Forms;
using System;
using VL.Lib.Animation;

[assembly: AssemblyInitializer(typeof(VL.Stride.Lib.Initialization))]

namespace VL.Stride.Lib
{
    public sealed class Initialization : AssemblyInitializer<Initialization>
    {
        public Initialization()
        {
        }

        // Remove once tested enough
        bool UseSDL = true;

        protected override void RegisterServices(IVLFactory factory)
        {
            factory.RegisterService<NodeContext, IResourceProvider<Game>>(nodeContext =>
            {
                return ResourceProvider.NewPooledPerApp(nodeContext,
                    factory: () =>
                    {
                        var clockSubscription = default(IDisposable);
                        EventHandler processSdlEventsHandler = (s, e) => global::Stride.Graphics.SDL.Application.ProcessEvents();

                        return ResourceProvider.New(
                            factory: () =>
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

                                GameContext gameContext;
                                if (UseSDL)
                                {
                                    gameContext = new GameContextSDL(null, 0, 0, isUserManagingRun: true);
                                    Application.Idle += processSdlEventsHandler;
                                }
                                else
                                {
                                    gameContext = new GameContextWinforms(null, 0, 0, isUserManagingRun: true);
                                }

                                game.Run(gameContext); // Creates the window

                                // Clear the default scene system (our renderers setup their own)
                                game.SceneSystem.SceneInstance = null;
                                game.SceneSystem.GraphicsCompositor = null;

                                var frameClock = factory.CreateService<IFrameClock>(nodeContext);
                                clockSubscription = frameClock.GetTicks().Subscribe(ftm => gameContext.RunCallback());

                                return game;
                            },
                            beforeDispose: game =>
                            {
                                // Remove the event handlers
                                if (UseSDL)
                                {
                                    Application.Idle -= processSdlEventsHandler;
                                }
                                clockSubscription?.Dispose();
                            });
                    }
                    , delayDisposalInMilliseconds: 0);
            });

            factory.RegisterService<NodeContext, IResourceProvider<GameWindow>>(nodeContext =>
            {
                return ResourceProvider.NewPooledPerApp(nodeContext, () =>
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