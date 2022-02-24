using VL.Core;
using VL.Core.CompilerServices;
using VL.Lib.Basics.Resources;
using VL.Stride.Assets;
using VL.Stride.Games;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using System.Windows.Forms;
using System;
using VL.Lib.Animation;
using StrideApp = Stride.Graphics.SDL.Application;
using SDL = SDL2.SDL;
using System.Runtime.InteropServices;

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
            var services = ServiceRegistry.Current.EnsureService<IResourceProvider<Game>>(() =>
            {
                var clockSubscription = default(IDisposable);
                var assetBuildService = default(AssetBuilderServiceScript);
                var messageFilter = default(MessageFilter);

                return ResourceProvider.New(() =>
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
                    game.Services.AddService(factory);
                    game.Services.AddService(factory.GetService<NodeFactoryRegistry>());

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
                        // SDL_PumpEvents shall not run the message loop (Translate/Dispatch) - already done by windows forms
                        // This calls also needs to be done after the Stride loaded the native SDL library - otherwise crash
                        SDL.SDL_SetHint(SDL.SDL_HINT_WINDOWS_ENABLE_MESSAGELOOP, "0");
                        // Add a message filter which intercepts WM_CHAR messages the Windows Forms loop would otherwise drop because it doesn't know the SDL created windows.
                        Application.AddMessageFilter(messageFilter = new MessageFilter());
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

                    var frameClock = Clocks.FrameClock;
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

                    // Remove the message filter
                    if (messageFilter != null)
                        Application.RemoveMessageFilter(messageFilter);

                    clockSubscription?.Dispose();
                })
                .ShareInParallel();
            });

            services.RegisterProvider(game =>
            {
                game.Window.Visible = true;
                return ResourceProvider.Return(game.Window, disposeAction: (window) =>
                {
                    window.Visible = false;
                }).ShareInParallel();
            });
        }

        sealed class MessageFilter : IMessageFilter
        {
            bool IMessageFilter.PreFilterMessage(ref Message m)
            {
                if (m.Msg >= 256 && m.Msg <= 264)
                {
                    // For these message types the Windows Forms main loop will look for a Control with the given HWND.
                    // If it can't find one it will not do the Translate/Dispatch call -> the SDL windows never receive any text input.
                    if (m.HWnd != null && Control.FromHandle(m.HWnd) is null)
                    {
                        // Is it a SDL window?
                        foreach (var window in StrideApp.Windows)
                        {
                            if (window.Handle == m.HWnd)
                            {
                                TranslateMessage(ref m);
                                DispatchMessage(ref m);

                                StrideApp.ProcessEvents();

                                return true;
                            }
                        }
                    }
                }

                // The WndProc of SDL only creates and enqueues SDL events - those need to be dequed and processed
                StrideApp.ProcessEvents();

                // Let the normal main loop continue to do its work
                return false;
            }

            [DllImport("user32.dll")]
            static extern bool TranslateMessage([In] ref Message m);

            [DllImport("user32.dll")]
            static extern IntPtr DispatchMessage([In] ref Message m);
        }
    }
}