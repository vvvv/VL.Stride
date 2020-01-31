using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VL.Core;
using VL.Xenko.Games;
using VL.Xenko.Layer;
using Xenko.Engine;

namespace VL.Xenko
{

    public abstract class GameRegionBase : INotifyHotSwapped
    {
        protected VLGame game;
        protected Action runCallback;
        protected SceneLink sceneLink;
        protected EntitySceneLink entitySceneLink;

        void INotifyHotSwapped.SwappedOut(object newInstance)
        {
            var that = newInstance as GameRegionBase;
            if (that != null)
            {
                that.game = game;
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
        NodeContext context;

        public GameRegion(NodeContext nodeContext)
        {
            context = nodeContext;
        }

        public TOutput Update(Func<TState> create, Func<TState, Tuple<TState, Entity, Scene, TOutput>> update, bool enabled = true, bool reset = false)
        {
            if (FState == null || reset)
            {
                Dispose();
                LibGameExtensions.CreateVLGame(out game, out runCallback);
                RuntimeTreeRegistry<Game>.AddItem(context, game);
                var rootScene = game.SceneSystem.SceneInstance.RootScene;
                sceneLink = new SceneLink(rootScene);
                entitySceneLink = new EntitySceneLink(rootScene);
                FState = create();
            }

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

        public void Dispose()
        {
            DisposeInternalState();
        }

        protected override void DisposeInternalState()
        {
            try
            {
                RuntimeTreeRegistry<Game>.RemoveItem(context);
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
