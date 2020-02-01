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
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Games;

namespace VL.Xenko
{

    public abstract class GameRegionBase : INotifyHotSwapped
    {
        protected VLGame game;
        protected GameWindow form;
        protected Action runCallback;
        protected SceneLink sceneLink;
        protected EntitySceneLink entitySceneLink;
        protected SerialDisposable locationChangedSubscription = new SerialDisposable();
        protected SerialDisposable orientationChangedSubscription = new SerialDisposable();

        void INotifyHotSwapped.SwappedOut(object newInstance)
        {
            var that = newInstance as GameRegionBase;
            if (that != null)
            {
                that.game = game;
                that.form = form;
                that.locationChangedSubscription = locationChangedSubscription;
                that.orientationChangedSubscription = orientationChangedSubscription;
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
        NodeContext FNodeContext;
        RectangleF FBounds = RectangleF.Empty;
        private Int2 FLastPosition;

        public GameRegion(NodeContext nodeContext, RectangleF bounds)
        {
            FNodeContext = nodeContext;
            FBounds = bounds;
        }

        public TOutput Update(Func<TState> create, Func<TState, Tuple<TState, Entity, Scene, TOutput>> update, out GameWindow window, bool enabled = true, bool reset = false)
        {
            if (FState == null || reset)
            {
                Dispose();
                LibGameExtensions.CreateVLGameWinForms(FBounds, out game, out runCallback, out form);
                SetupBoundsEvents();
                FLastPosition = form.Position;
                RuntimeTreeRegistry<Game>.AddItem(FNodeContext, game);
                var rootScene = game.SceneSystem.SceneInstance.RootScene;
                sceneLink = new SceneLink(rootScene);
                entitySceneLink = new EntitySceneLink(rootScene);
                FState = create();
            }

            if (form != null && form.Position != FLastPosition)
            {
                UpdateBounds(null);
                FLastPosition = form.Position;
            }

            window = form;
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

        void SetupBoundsEvents()
        {
            locationChangedSubscription.Disposable = Observable.FromEventPattern(form, nameof(form.ClientSizeChanged))
                .Subscribe(UpdateBounds);
            orientationChangedSubscription.Disposable = Observable.FromEventPattern(form, nameof(form.OrientationChanged))
                .Subscribe(UpdateBounds);
        }

        private void UpdateBounds(EventPattern<object> obj)
        {
            var b = form.ClientBounds;
            var p = form.Position;
            FBounds = new RectangleF(p.X, p.Y, b.Width, b.Height);
            var solution = VL.Model.VLSession.Instance.CurrentSolution;
            var newSolution = solution.SetPinValue(FNodeContext.Path.Stack.Peek(), "Bounds", FBounds);
            newSolution.Confirm(Model.SolutionUpdateKind.DontCompile);
        }

        public void Dispose()
        {
            DisposeInternalState();
        }

        protected override void DisposeInternalState()
        {
            try
            {
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
