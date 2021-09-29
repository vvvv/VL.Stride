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
using System.Diagnostics;

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
                        var assetBuildService = default(AssetBuilderServiceScript);
                        EventHandler processSdlEventsHandler = (s, e) => global::Stride.Graphics.SDL.Application.ProcessEvents();

                        return ResourceProvider.New(
                            factory: () =>
                            {
                                var game = new VLGame();
#if DEBUG
                                game.GraphicsDeviceManager.DeviceCreationFlags |= DeviceCreationFlags.Debug;
#endif
                                // for now we don't let the user decide upon the colorspace
                                // as we'd need to either recreate all textures and swapchains in that moment or make sure that these weren't created yet.
                                game.GraphicsDeviceManager.PreferredColorSpace = ColorSpace.Linear;

                                assetBuildService = new AssetBuilderServiceScript();
                                game.Services.AddService(assetBuildService);
                                game.Services.AddService(nodeContext.Factory);

                                var gameStartedHandler = default(EventHandler);
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

                                // Clear the default scene system. We use a frame based system where scene systems as well as graphics renderer get enqueued.
                                game.SceneSystem.SceneInstance = null;
                                game.SceneSystem.GraphicsCompositor = null;

                                // Make sure the main window doesn't block the main loop
                                game.GraphicsDevice.Presenter.PresentInterval = PresentInterval.Immediate;

                                var frameClock = nodeContext.FrameClock;
                                clockSubscription = frameClock.GetFrameFinished().Subscribe(ffm =>
                                {
                                    try
                                    {
                                        game.ElapsedUserTime = ffm.LastInterval;
                                        gameContext.RunCallback();
                                    }
                                    catch (Exception e)
                                    {
                                        RuntimeGraph.ReportException(e);
                                    }
                                });

                                return game;
                            },
                            beforeDispose: game =>
                            {
                                // Stride doesn't seem to remove the script on it's own. Do it manually to ensure the cancellation token gets set.
                                if (assetBuildService != null)
                                {
                                    game.Script.Remove(assetBuildService);
                                    // And run the scheduler one last time to actually trigger the cancellation
                                    game.Script.Scheduler.Run();
                                }

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