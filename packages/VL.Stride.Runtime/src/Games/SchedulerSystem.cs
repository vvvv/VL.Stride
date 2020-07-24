using Stride.Core;
using Stride.Core.Annotations;
using Stride.Games;
using Stride.Graphics;
using Stride.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using VL.Stride.Rendering;

namespace VL.Stride.Games
{
    class SchedulerSystem : GameSystemBase, IRendererScheduler, IGameSystemScheduler
    {
        static readonly PropertyKey<bool> contentLoaded = new PropertyKey<bool>("ContentLoaded", typeof(SchedulerSystem));

        readonly List<GameSystemBase> queue = new List<GameSystemBase>();
        readonly Stack<MiniSystem> pool = new Stack<MiniSystem>();

        public SchedulerSystem([NotNull] IServiceRegistry registry) : base(registry)
        {
            Enabled = true;
            Visible = true;
        }

        public void Schedule(IGraphicsRendererBase renderer)
        {
            var current = queue.ElementAtOrDefault(queue.Count - 1) as MiniSystem;
            if (current is null)
            {
                current = pool.Count > 0 ? pool.Pop() : new MiniSystem(Services);
                Schedule(current);
            }
            current.Schedule(renderer);
        }

        public void Remove(IGraphicsRendererBase graphicsRenderer)
        {
            var mss = queue.OfType<MiniSystem>().ToList();
            foreach(var ms in mss)
            {
                ms.Remove(graphicsRenderer);
            }
        }

        public void Schedule(GameSystemBase gameSystem)
        {
            queue.Add(gameSystem);
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var system in queue)
            {
                if (!system.Tags.Get(contentLoaded) && system is IContentable c)
                {
                    c.LoadContent();
                    system.Tags.Set(contentLoaded, true);
                }
                if (system.Enabled)
                    system.Update(gameTime);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            try
            {
                foreach (var s in queue)
                {
                    if (s.Visible)
                        s.Draw(gameTime);

                    // Put back in the pool
                    if (s is MiniSystem m)
                        pool.Push(m);
                }
            }
            finally
            {
                queue.Clear();
            }
        }

        class MiniSystem : GameSystemBase, IRendererScheduler
        {
            private readonly List<IGraphicsRendererBase> Renderers = new List<IGraphicsRendererBase>();

            private RenderView renderView;
            private RenderContext renderContext;
            private RenderDrawContext renderDrawContext;

            public MiniSystem([NotNull] IServiceRegistry registry) : base(registry)
            {
                Enabled = true;
                Visible = true;
            }

            protected override void LoadContent()
            {
                // Default render view
                renderView = new RenderView()
                {
                    NearClipPlane = 0.05f,
                    FarClipPlane = 1000,
                };

                // Create the drawing context
                var graphicsContext = Services.GetSafeServiceAs<GraphicsContext>();
                renderContext = RenderContext.GetShared(Services);
                renderDrawContext = new RenderDrawContext(Services, renderContext, graphicsContext);
            }

            public override void Draw(GameTime gameTime)
            {
                try
                {
                    using (renderContext.PushRenderViewAndRestore(renderView))
                    using (renderDrawContext.PushRenderTargetsAndRestore())
                    {
                        foreach (var renderer in Renderers)
                            renderer.Draw(renderDrawContext);
                    }
                }
                finally
                {
                    Renderers.Clear();
                }
            }

            public void Schedule(IGraphicsRendererBase graphicsRenderer)
            {
                Renderers.Add(graphicsRenderer);
            }

            public void Remove(IGraphicsRendererBase graphicsRenderer)
            {
                Renderers.Remove(graphicsRenderer);
            }
        }
    }
}
