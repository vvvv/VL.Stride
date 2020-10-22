using Stride.Engine;
using Stride.Engine.Design;
using Stride.Games;
using System.Collections.Generic;
using System.IO;
using VL.Core;
using VL.Stride.Engine;
using VL.Stride.Rendering;

namespace VL.Stride.Games
{
    public class VLGame : Game
    {
        internal readonly SchedulerSystem SchedulerSystem;
        private NodeFactoryRegistry NodeFactoryRegistry;

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

            IsUserManagingTime = true;

            base.Initialize();

            GameSystems.Add(SchedulerSystem);

            // Setup the effect compiler with our file provider so we can attach shader lookup paths per document
            EffectSystem.InstallEffectCompilerWithCustomPaths();
        }

        protected override void Update(GameTime gameTime)
        {
            var factory = Services.GetService<IVLFactory>();

            // Ensure all the paths referenced by VL are visible to the effect system
            var nodeFactoryRegistry = factory?.GetService<NodeFactoryRegistry>();
            if (nodeFactoryRegistry != null)
                UpdateShaderPaths(nodeFactoryRegistry);

            base.Update(gameTime);
        }

        void UpdateShaderPaths(NodeFactoryRegistry nodeFactoryRegistry)
        {
            if (nodeFactoryRegistry == NodeFactoryRegistry)
                return;

            NodeFactoryRegistry = nodeFactoryRegistry;

            // TODO: Newer vvvv versions have this exposed
            var pathsField = typeof(NodeFactoryRegistry).GetField("paths", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var paths = (HashSet<string>)pathsField.GetValue(nodeFactoryRegistry);
            foreach (var path in paths)
                if (Directory.Exists(Path.Combine(path, "shaders")))
                    EffectSystem.EnsurePathIsVisible(path);
        }
    }
}
