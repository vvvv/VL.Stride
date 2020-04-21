using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using VL.Core;
using VL.Lang.PublicAPI;
using VL.Lib.Basics.Resources;
using VL.Xenko.Games;
using VL.Xenko.Layer;
using VL.Xenko.Rendering;
using Xenko.Core.Mathematics;
using Xenko.Core.MicroThreading;
using Xenko.Engine;
using Xenko.Games;
using Xenko.Rendering;

namespace VL.Xenko
{
    public class Renderer : IDisposable
    {
        private readonly NodeContext FNodeContext;
        private readonly IResourceHandle<Game> FGameHandle;
        private readonly VLGame FGame;
        private RectangleF FBounds = RectangleF.Empty;
        private readonly bool FSaveBounds;
        private readonly bool FBoundToDocument;
        private readonly bool FShowDialogIfDocumentChanged;
        private readonly SerialDisposable sizeChangedSubscription = new SerialDisposable();
        private Int2 FLastPosition;
        private SceneLink sceneLink;
        private EntitySceneLink entitySceneLink;

        public Renderer(NodeContext nodeContext, RectangleF bounds, bool saveBounds = true, bool boundToDocument = false, bool dialogIfDocumentChanged = false)
        {
            FNodeContext = nodeContext;
            FBounds = bounds;
            FSaveBounds = saveBounds;
            FBoundToDocument = boundToDocument;
            FShowDialogIfDocumentChanged = dialogIfDocumentChanged;

            FGameHandle = nodeContext.GetGameHandle();
            var game = FGame = (VLGame)FGameHandle.Resource;

            Window = game.Window;
            Window.Title = "Game";

            if (bounds.Width > 1 && bounds.Height > 1)
            {
                game.GraphicsDeviceManager.PreferredBackBufferWidth = (int)bounds.Width;
                game.GraphicsDeviceManager.PreferredBackBufferHeight = (int)bounds.Height;
                game.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
                game.GraphicsDeviceManager.ApplyChanges();

                Window.Position = new Int2((int)bounds.X, (int)bounds.Y);
            }

            SetupEvents();

            Window.Visible = true;
        }

        private bool FSceneGraphInitialized;

        public GameWindow Window { get; }

        public void Update(Entity entity, Scene scene, Color4 color, bool clear = true, bool verticalSync = false, bool enabled = true, bool reset = false, float depth = 1, byte stencilValue = 0, ClearRendererFlags clearFlags = ClearRendererFlags.ColorAndDepth)
        {
            //v-sync
            if (verticalSync != FGame.GraphicsDeviceManager.SynchronizeWithVerticalRetrace)
            {
                FGame.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = verticalSync;
                FGame.GraphicsDeviceManager.ApplyChanges();
            }

            //write bounds to patch?
            if (Window.Position != FLastPosition)
            {
                UpdateBounds(null);
                FLastPosition = Window.Position;
            }

            //init scene graph links and add layer renderer
            if (entitySceneLink == null && FGame.SceneSystem.SceneInstance != null)
            {
                var rootScene = FGame.SceneSystem.SceneInstance.RootScene;
                sceneLink = new SceneLink(rootScene);
                entitySceneLink = new EntitySceneLink(rootScene);
                FSceneGraphInitialized = true;
            }

            if (enabled && FSceneGraphInitialized)
            { 
                FGame.RunCallback.Invoke(); //calls Game.Tick();

                FGame.SceneSystem.GraphicsCompositor.GetFirstForwardRenderer(out var forwardRenderer);
                forwardRenderer?.SetClearOptions(color, depth, stencilValue, clearFlags, clear);
                
                entitySceneLink.Update(entity);
                sceneLink.Update(scene);
            }
            else if (enabled) //update game, e.g. splash screen is showing
            {
                FGame.RunCallback.Invoke(); //calls Game.Tick();
            }
            
        }

        void SetupEvents()
        {
            //register events handlers
            sizeChangedSubscription.Disposable = Observable.Merge(
                Observable.FromEventPattern(Window, nameof(Window.ClientSizeChanged)), 
                Observable.FromEventPattern(Window, nameof(Window.OrientationChanged)))
                .Throttle(TimeSpan.FromSeconds(0.5))
                .Subscribe(UpdateBounds);

            Window.Closing += Window_Closing;
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
                solution = solution?.SetPinValue(FNodeContext.Path.Stack.Peek(), "Bounds", FBounds);
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
            FGameHandle.Dispose();
        }
    }
}
