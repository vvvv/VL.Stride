using Stride.Engine;
using Stride.Engine.Design;
using Stride.Shaders.Compiler;
using VL.Stride.Core.IO;
using VL.Stride.Engine;
using VL.Stride.Rendering;

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

            // Setup the effect compiler with our file provider so we can attach shader lookup paths per document
            EffectSystem.InstallEffectCompilerWithCustomPaths();
        }
    }
}
