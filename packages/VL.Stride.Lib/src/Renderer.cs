using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Rendering;
using Stride.Rendering.Compositing;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using VL.Core;
using VL.Lang.PublicAPI;
using VL.Lib.Basics.Resources;
using VL.Lib.Collections;
using VL.Lib.Collections.TreePatching;
using VL.Stride.Games;
using VL.Stride.Rendering;

namespace VL.Stride
{
    public class Renderer : IDisposable
    {
        private readonly NodeContext FNodeContext;
        private readonly IResourceHandle<Game> FGameHandle;
        private readonly IResourceHandle<GameWindow> FWindowHandle;
        private RectangleF FBounds = RectangleF.Empty;
        private readonly bool FSaveBounds;
        private readonly bool FBoundToDocument;
        private readonly bool FShowDialogIfDocumentChanged;
        private readonly SerialDisposable sizeChangedSubscription = new SerialDisposable();
        private readonly TreeNodeChildrenManager<Scene, Scene> FSceneManager;
        private Int2 FLastPosition;

        public Renderer(NodeContext nodeContext, RectangleF bounds, 
            Func<Scene, TreeNodeParentManager<Scene, Scene>> parentManagerProvider, bool saveBounds = true,
            bool boundToDocument = false, bool dialogIfDocumentChanged = false)
        {
            FNodeContext = nodeContext;
            FBounds = bounds;
            FSaveBounds = saveBounds;
            FBoundToDocument = boundToDocument;
            FShowDialogIfDocumentChanged = dialogIfDocumentChanged;

            FGameHandle = nodeContext.GetGameHandle();
            FWindowHandle = nodeContext.GetGameWindowProvider().GetHandle();

            var game = FGameHandle.Resource;
            if (bounds.Width > 1 && bounds.Height > 1)
            {
                game.GraphicsDeviceManager.PreferredBackBufferWidth = (int)bounds.Width;
                game.GraphicsDeviceManager.PreferredBackBufferHeight = (int)bounds.Height;
                game.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
                game.GraphicsDeviceManager.ApplyChanges();

                Window.Position = new Int2((int)bounds.X, (int)bounds.Y);
            }

            SetupEvents(Window);

            // Init scene graph links 
            //var rootScene = game.SceneSystem.SceneInstance.RootScene;

            //FSceneManager = new TreeNodeChildrenManager<Scene, Scene>(rootScene, childrenOrderingMatters: false, parentManagerProvider);
        }

        public GameWindow Window => FWindowHandle.Resource;

        Spread<Scene> scenes = Spread<Scene>.Empty;

        public void Update(SceneInstance sceneInstance, GraphicsCompositor compositor, bool verticalSync = false, bool enabled = true)
        {
            var game = (VLGame)FGameHandle.Resource;

            //v-sync
            if (verticalSync != game.GraphicsDeviceManager.SynchronizeWithVerticalRetrace)
            {
                game.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = verticalSync;
                game.GraphicsDeviceManager.ApplyChanges();
            }

            //write bounds to patch?
            if (Window.Position != FLastPosition)
            {
                UpdateBounds(null);
                FLastPosition = Window.Position;
            }

            if (enabled)
            {
                game.SceneSystem.GraphicsCompositor = compositor;
                game.SceneSystem.SceneInstance = sceneInstance;

                //var scene = sceneInstance.RootScene;
                //if (scene != scenes.FirstOrDefault())
                //    scenes = scene != null ? Spread.Create(scene) : Spread<Scene>.Empty;

                //FSceneManager?.Update(scenes);

                game.RunCallback?.Invoke(); //calls Game.Tick();
            }
        }

        void SetupEvents(GameWindow window)
        {
            //register events handlers
            sizeChangedSubscription.Disposable = Observable.Merge(
                Observable.FromEventPattern(window, nameof(Window.ClientSizeChanged)), 
                Observable.FromEventPattern(window, nameof(Window.OrientationChanged)))
                .Throttle(TimeSpan.FromSeconds(0.5))
                .Subscribe(UpdateBounds);

            window.Closing += Window_Closing;
        }

        private void UpdateBounds(EventPattern<object> obj)
        {
            //write bounds into pin
            if (FSaveBounds)
            {
                var b = Window.ClientBounds;
                var p = Window.Position;
                FBounds = new RectangleF(p.X, p.Y, b.Width, b.Height);
                var solution = VL.Model.VLSession.Instance.CurrentSolution as ISolution;
                solution = solution?.SetPinValue(FNodeContext.Path.Stack, "Bounds", FBounds);
                solution?.Confirm(Model.SolutionUpdateKind.DontCompile); 
            }
        }
        private void Window_Closing(object sender, EventArgs e)
        {
            //close doument, if requested
            if (FBoundToDocument)
                Session.CloseDocumentOfNode(FNodeContext.Path.Stack.Peek(), FShowDialogIfDocumentChanged);

            Window.Closing -= Window_Closing;
        }

        public void Dispose()
        {
            FSceneManager?.Dispose();
            FWindowHandle?.Dispose();
            FGameHandle?.Dispose();
        }
    }
}
