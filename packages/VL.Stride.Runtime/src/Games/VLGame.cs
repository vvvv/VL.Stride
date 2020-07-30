using Stride.Engine;
using Stride.Engine.Design;
using VL.Stride.Engine;

namespace VL.Stride.Games
{
    public class VLGame : Game
    {
        internal readonly SchedulerSystem SchedulerSystem;

        public VLGame()
            : base()
        {
            SchedulerSystem = new SchedulerSystem(Services);
            Services.AddService(SchedulerSystem);
        }

        protected override void PrepareContext()
        {
            base.PrepareContext();
        }

        protected override void OnWindowCreated()
        {
            Window.AllowUserResizing = true;
            base.OnWindowCreated();
        }

        protected override void Initialize()
        {
            Settings.EffectCompilation = EffectCompilationMode.Local;
            Settings.RecordUsedEffects = false;

            base.Initialize();

            GameSystems.Add(SchedulerSystem);
        }
    }
}
