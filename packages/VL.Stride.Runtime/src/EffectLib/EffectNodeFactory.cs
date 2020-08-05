using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using VL.Core;
using Stride.Core.IO;
using Stride.Core.Serialization.Contents;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Shaders;
using Stride.Shaders.Compiler;
using ServiceRegistry = Stride.Core.ServiceRegistry;
using VL.Stride.Core;
using System.Collections.Immutable;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace VL.Stride.EffectLib
{
    public class EffectNodeFactory : IVLNodeDescriptionFactory
    {
        const string sdslFileFilter = "*.sdsl";
        const string CompiledShadersKey = "__shaders_bytecode__"; // Taken from EffectCompilerCache.cs

        private EffectCompilerParameters effectCompilerParameters = EffectCompilerParameters.Default;

        readonly DirectoryWatcher directoryWatcher;
        readonly SynchronizationContext mainContext;
        Timer timer;

        public EffectNodeFactory()
        {
            Services = SharedServices.GetRegistry();
            Content = Services.GetService<ContentManager>();
            EffectSystem = Services.GetService<EffectSystem>();
            GraphicsDeviceService = Services.GetService<IGraphicsDeviceService>();
            GraphicsDevice = GraphicsDeviceService.GraphicsDevice;

            mainContext = Services.GetService<SynchronizationContext>() ?? SynchronizationContext.Current;

            NodeDescriptions = GetNodeDescriptions(Content).ToImmutableArray();

            var effectSystem = EffectSystem;
            // Ensure the effect system tracks the same files as we do
            var fieldInfo = typeof(EffectSystem).GetField("directoryWatcher", BindingFlags.NonPublic | BindingFlags.Instance);
            directoryWatcher = fieldInfo.GetValue(effectSystem) as DirectoryWatcher;
            directoryWatcher.Modified += DirectoryWatcher_Modified;
        }

        public ImmutableArray<IVLNodeDescription> NodeDescriptions { get; private set; }

        public string Identifier => "VL.Stride.EffectNodeFactory";
        public ContentManager Content { get; private set; }
        public ServiceRegistry Services { get; private set; }
        public GraphicsDevice GraphicsDevice { get; private set; }
        public IGraphicsDeviceService GraphicsDeviceService { get; private set; }
        public EffectSystem EffectSystem { get; private set; }

        // Would be needed if shaders could be added or deleted
        public IObservable<IVLNodeDescriptionFactory> Invalidated => Observable.Empty<IVLNodeDescriptionFactory>();

        public readonly object SyncRoot = new object();

        public IVLNodeDescriptionFactory ForPath(string path) => null;

        public CompilerResults GetCompilerResults(string effectName)
        {
            return GetCompilerResults(effectName, new CompilerParameters());
        }

        public CompilerResults GetCompilerResults(string effectName, CompilerParameters compilerParameters)
        {
            if (effectName == null) throw new ArgumentNullException("effectName");
            if (compilerParameters == null) throw new ArgumentNullException("compilerParameters");

            // Setup compilation parameters
            var graphicsDevice = this.GraphicsDevice;
            compilerParameters.EffectParameters.Platform = GraphicsDevice.Platform;
            compilerParameters.EffectParameters.Profile = graphicsDevice.Features.RequestedProfile;
            // Copy optimization/debug levels
            compilerParameters.EffectParameters.OptimizationLevel = effectCompilerParameters.OptimizationLevel;
            compilerParameters.EffectParameters.Debug = effectCompilerParameters.Debug;

            var source = GetShaderSource(effectName);
            var compiler = EffectSystem.Compiler;
            var compilerResults = compiler.Compile(source, compilerParameters);

            // Watch the source code files
            var effectBytecodeCompilerResult = compilerResults.Bytecode.WaitForResult();
            if (effectBytecodeCompilerResult.CompilationLog.HasErrors)
            {
                foreach (var m in effectBytecodeCompilerResult.CompilationLog.Messages)
                {
                    if (m.TryGetFilePath(out string path))
                        directoryWatcher.Track(path);
                }
            }
            else
            {
                var bytecode = effectBytecodeCompilerResult.Bytecode;
                foreach (var type in bytecode.HashSources.Keys)
                {
                    // TODO: the "/path" is hardcoded, used in ImportStreamCommand and ShaderSourceManager. Find a place to share this correctly.
                    using (var pathStream = Content.FileProvider.OpenStream(EffectCompilerBase.GetStoragePathFromShaderType(type) + "/path", VirtualFileMode.Open, VirtualFileAccess.Read))
                    using (var reader = new StreamReader(pathStream))
                    {
                        var path = reader.ReadToEnd();
                        directoryWatcher.Track(path);
                    }
                }
            }

            return compilerResults;
        }

        private static ShaderSource GetShaderSource(string effectName)
        {
            var isXdfx = ShaderMixinManager.Contains(effectName);
            if (isXdfx)
                return new ShaderMixinGeneratorSource(effectName);
            return new ShaderClassSource(effectName);
        }

        public string GetPathOfSdslShader(string effectName)
        {
            var fileProvider = this.Content.FileProvider;
            using (var pathStream = fileProvider.OpenStream(EffectCompilerBase.GetStoragePathFromShaderType(effectName) + "/path", VirtualFileMode.Open, VirtualFileAccess.Read))
            using (var reader = new StreamReader(pathStream))
            {
                return reader.ReadToEnd();
            }
        }

        public string GetPathOfHlslShader(string effectName, CompilerResults compilerResults)
        {
            // See EffectCompiler.cs Compile method - hard to do as it depends on AST
            return null;
        }

        const string suffix = "_VLNode";

        IEnumerable<IVLNodeDescription> GetNodeDescriptions(ContentManager contentManager)
        {
            var files = contentManager.FileProvider.ListFiles(EffectCompilerBase.DefaultSourceShaderFolder, sdslFileFilter, VirtualSearchOption.AllDirectories);
            foreach (var file in files)
            {
                var effectName = Path.GetFileNameWithoutExtension(file);
                if (effectName.EndsWith(suffix))
                {
                    var name = effectName.Substring(0, effectName.Length - suffix.Length);
                    yield return new EffectNodeDescription(this, name, effectName);
                }
            }
        }

        void DirectoryWatcher_Modified(object sender, FileEvent e)
        {
            if (e.ChangeType == FileEventChangeType.Changed || e.ChangeType == FileEventChangeType.Renamed)
            {
                timer?.Dispose();
                timer = new Timer(_ => ReloadNodeDescriptions(), null, 50, Timeout.Infinite);
            }
        }

        void ReloadNodeDescriptions()
        {
            // The effect system invalidates its shader cache on update. So make sure to call it.
            EffectSystem.Update(null);

            var newDescriptions = ImmutableArray.CreateBuilder<IVLNodeDescription>();
            foreach (EffectNodeDescription description in NodeDescriptions)
            {
                var updatedDescription = new EffectNodeDescription(this, description.Name, description.EffectName);
                if (updatedDescription.HasCompilerErrors != description.HasCompilerErrors ||
                    !ImmutableArray<IVLPinDescription>.CastUp(updatedDescription.Inputs).SequenceEqual(description.Inputs, PinDescriptionComparer.Default) ||
                    !ImmutableArray<IVLPinDescription>.CastUp(updatedDescription.Outputs).SequenceEqual(description.Outputs, PinDescriptionComparer.Default))
                {
                    description.Invalidate();

                    if (updatedDescription.HasCompilerErrors)
                    {
                        // Keep the signature of previous description but show errors and since we have errors kill all living instances so they don't have to deal with it
                        newDescriptions.Add(new EffectNodeDescription(description, updatedDescription.CompilerResults));
                    }
                    else
                    {
                        // Signature change, force a new compilation
                        newDescriptions.Add(updatedDescription);
                    }
                }
                else if (updatedDescription.Bytecode != description.Bytecode)
                {
                    // Just increase the change count so instances can update
                    description.Version++;
                    newDescriptions.Add(description);
                }
                else
                {
                    // Just the same
                    newDescriptions.Add(description);
                }
            }
            NodeDescriptions = newDescriptions.ToImmutable();
        }
    }
}
