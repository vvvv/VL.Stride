using Stride.Core;
using Stride.Core.Annotations;
using Stride.Games;
using System.Collections.Generic;

namespace VL.Stride.Engine
{
    /// <summary>
    /// Allows to schedule game systems (e.g. a SceneSystem or a LayerSystem).
    /// </summary>
    class SchedulerSystem : GameSystemBase, IGameSystemScheduler
    {
        static readonly PropertyKey<bool> contentLoaded = new PropertyKey<bool>("ContentLoaded", typeof(SchedulerSystem));

        readonly List<GameSystemBase> queue = new List<GameSystemBase>();

        public SchedulerSystem([NotNull] IServiceRegistry registry) : base(registry)
        {
            Enabled = true;
            Visible = true;
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
                }
            }
            finally
            {
                queue.Clear();
            }
        }
    }
}
