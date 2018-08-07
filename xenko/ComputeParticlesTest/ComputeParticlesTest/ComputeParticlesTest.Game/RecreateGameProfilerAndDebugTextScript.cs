using System.Linq;
using Xenko.Engine;
using Xenko.Profiling;

namespace ComputeParticlesTest
{
    public class RecreateGameProfilerAndDebugTextScript : StartupScript
    {
        public override void Start()
        {
            // GameProfiler
            GameProfiler.DisableProfiling();
            Game.GameSystems.Remove(GameProfiler);
            Services.RemoveService<GameProfilingSystem>();

            var gameProfiler = new GameProfilingSystem(Services);
            Services.AddService(gameProfiler);
            Game.GameSystems.Add(gameProfiler);

            // DebugText
            DebugText.Enabled = false;
            DebugText.Visible = false;
            Game.GameSystems.Remove(DebugText);
            Services.RemoveService<DebugTextSystem>();

            var debugText = new DebugTextSystem(Services);
            Services.AddService(debugText);
            Game.GameSystems.Add(debugText);

            // Reload current scene

            var result = Content.TryGetAssetUrl(SceneSystem.SceneInstance.RootScene, out var url);
            if (!result)
            {
            }

            Content.Unload(SceneSystem.SceneInstance.RootScene);

            var scene = Content.Load<Scene>(url);
            var script = scene.Entities.SelectMany((e) => e.Components).OfType<RecreateGameProfilerAndDebugTextScript>().First();
            script.Entity.Remove(script);

            SceneSystem.SceneInstance.RootScene = scene;
        }
    }
}
