using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using VL.Core;
using VL.Lang.PublicAPI;
using VL.Xenko.Games;
using VL.Xenko.Layer;
using VL.Xenko.Rendering;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Games;
using Xenko.Rendering;

namespace VL.Xenko
{

    public abstract class GameRegionBase : INotifyHotSwapped
    {
        protected VLGame game;
        protected GameWindow gameWindow;
        protected Action runCallback;
        protected SceneLink sceneLink;
        protected EntitySceneLink entitySceneLink;
        protected SerialDisposable sizeChangedSubscription = new SerialDisposable();

        void INotifyHotSwapped.SwappedOut(object newInstance)
        {
            var that = newInstance as GameRegionBase;
            if (that != null)
            {
                that.game = game;
                that.gameWindow = gameWindow;
                that.sizeChangedSubscription = sizeChangedSubscription;
                that.runCallback = runCallback;
                that.sceneLink = sceneLink;
                that.entitySceneLink = entitySceneLink;
            }
            else
                DisposeInternalState();
        }

        void INotifyHotSwapped.SwappedIn(object oldInstance) { }

        protected abstract void DisposeInternalState();
    }

    public class GameRegion<TState, TOutput> : GameRegionBase, IDisposable
    {
        TState FState;
        TOutput FLastOutput;
        private readonly NodeContext FNodeContext;
        private RectangleF FBounds = RectangleF.Empty;
        private readonly bool FSaveBounds;
        private readonly bool FBoundToDocument;
        private readonly bool FShowDialogIfDocumentChanged;
        private Int2 FLastPosition;

        public GameRegion(NodeContext nodeContext, RectangleF bounds, bool saveBounds = true, bool boundToDocument = false, bool dialogIfDocumentChanged = false)
        {
            FNodeContext = nodeContext;
            FBounds = bounds;
            FSaveBounds = saveBounds;
            FBoundToDocument = boundToDocument;
            FShowDialogIfDocumentChanged = dialogIfDocumentChanged;
        }

        public TOutput Update(Func<TState> create, Func<TState, Tuple<TState, Entity, Scene, TOutput>> update, out GameWindow window, Color4 color, bool clear = true, bool verticalSync = false, bool enabled = true, bool reset = false, float depth = 1, byte stencilValue = 0, ClearRendererFlags clearFlags = ClearRendererFlags.ColorAndDepth)
        {
            if (FState == null || reset)
            {
                Dispose();
                LibGameExtensions.CreateVLGameWinForms((Rectangle)FBounds, out game, out runCallback, out gameWindow);
                SetupEvents();
                FLastPosition = gameWindow.Position;
                RuntimeTreeRegistry<Game>.AddItem(FNodeContext, game);
                var rootScene = game.SceneSystem.SceneInstance.RootScene;
                sceneLink = new SceneLink(rootScene);
                entitySceneLink = new EntitySceneLink(rootScene);
                FState = create();
            }

            if (verticalSync != game.GraphicsDeviceManager.SynchronizeWithVerticalRetrace)
            {
                game.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = verticalSync;
                game.GraphicsDeviceManager.ApplyChanges();
            }

            if (gameWindow != null && gameWindow.Position != FLastPosition)
            {
                UpdateBounds(null);
                FLastPosition = gameWindow.Position;
            }

            game.SceneSystem.GraphicsCompositor.GetFirstForwardRenderer(out var forwardRenderer);
            forwardRenderer?.SetClearOptions(color, depth, stencilValue, clearFlags, clear);

            window = gameWindow;
            if (enabled)
            {
                var t = update(FState);
                runCallback?.Invoke();
                FState = t.Item1;
                entitySceneLink?.Update(t.Item2);
                sceneLink?.Update(t.Item3);
                FLastOutput = t.Item4;           
                return FLastOutput;
            }
            else
                return FLastOutput;
        }

        void SetupEvents()
        {
            //register events handlers
            sizeChangedSubscription.Disposable = Observable.Merge(
                Observable.FromEventPattern(gameWindow, nameof(gameWindow.ClientSizeChanged)), 
                Observable.FromEventPattern(gameWindow, nameof(gameWindow.OrientationChanged)))
                .Throttle(TimeSpan.FromSeconds(0.5))
                .Subscribe(UpdateBounds);

            if (gameWindow != null)
                gameWindow.Closing += Window_Closing;
        }

        private void UpdateBounds(EventPattern<object> obj)
        {
            //write bounds into pin
            if (FSaveBounds)
            {
                var b = gameWindow.ClientBounds;
                var p = gameWindow.Position;
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

            if (gameWindow != null)
                gameWindow.Closing -= Window_Closing;
        }

        public void Dispose()
        {
            DisposeInternalState();
        }

        protected override void DisposeInternalState()
        {
            try
            {
                if (game != null)
                    RuntimeTreeRegistry<Game>.RemoveItem(FNodeContext);
                (FState as IDisposable)?.Dispose();
                game?.Dispose();
            }
            finally
            {
                FState = default;
            }
        }
    }
}
