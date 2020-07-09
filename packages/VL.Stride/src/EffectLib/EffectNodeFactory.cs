using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using VL.Core;
using VL.Stride.Games;
using Stride.Core;
using Stride.Core.Diagnostics;
using Stride.Core.IO;
using Stride.Core.Mathematics;
using Stride.Core.Serialization.Contents;
using Stride.Core.Storage;
using Stride.Engine;
using Stride.Engine.Processors;
using Stride.Games;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Shaders;
using Stride.Shaders.Compiler;
using ServiceRegistry = Stride.Core.ServiceRegistry;
using VLServiceRegistry = VL.Core.ServiceRegistry;

[assembly: NodeFactory(typeof(VL.Stride.EffectLib.EffectNodeFactory))]

namespace VL.Stride.EffectLib
{
    class NodeComparer : IEqualityComparer<IVLNodeDescription>
    {
        public static readonly IEqualityComparer<IVLNodeDescription> Instance = new NodeComparer();

        public bool Equals(IVLNodeDescription x, IVLNodeDescription y)
        {
            if (x == y)
                return true;

            if (x.Name != y.Name)
                return false;

            if (x.Category != y.Category)
                return false;

            //if (!x.Outputs.SequenceEqual(y.Outputs, PinComparer.Instance))
            //    return false;

            //if (!x.Inputs.SequenceEqual(y.Inputs, PinComparer.Instance))
            //    return false;

            return true;
        }

        public int GetHashCode(IVLNodeDescription obj)
        {
            return obj.Name.GetHashCode();
        }
    }

    public class EffectNodeFactory : IVLNodeDescriptionFactory
    {
        // Taken from Stride/Game
        static DatabaseFileProvider InitializeAssetDatabase()
        {
            // Create and mount database file system
            var objDatabase = ObjectDatabase.CreateDefaultDatabase();

            // Only set a mount path if not mounted already
            var mountPath = VirtualFileSystem.ResolveProviderUnsafe("/asset", true).Provider == null ? "/asset" : null;
            return new DatabaseFileProvider(objDatabase, mountPath);
        }

        // Taken from Stride/SkyboxGeneratorContext
        private void Init()
        {
            Services = new ServiceRegistry();

            var fileProvider = InitializeAssetDatabase();
            var fileProviderService = new DatabaseFileProviderService(fileProvider);
            Services.AddService<IDatabaseFileProviderService>(fileProviderService);

            Content = new ContentManager(Services);
            Services.AddService<IContentManager>(Content);
            Services.AddService(Content);

            GraphicsDevice = GraphicsDevice.New();
            GraphicsDeviceService = new GraphicsDeviceServiceLocal(Services, GraphicsDevice);
            Services.AddService<IGraphicsDeviceService>(GraphicsDeviceService);

            var graphicsContext = new GraphicsContext(GraphicsDevice);
            Services.AddService(graphicsContext);

            EffectSystem = new EffectSystem(Services);
            EffectSystem.Compiler = EffectCompilerFactory.CreateEffectCompiler(Content.FileProvider, EffectSystem);

            Services.AddService(EffectSystem);
            EffectSystem.Initialize();
            ((IContentable)EffectSystem).LoadContent();
            ((EffectCompilerCache)EffectSystem.Compiler).CompileEffectAsynchronously = false;
        }

        const string sdslFileFilter = "*.sdsl";
        const string CompiledShadersKey = "__shaders_bytecode__"; // Taken from EffectCompilerCache.cs

        private EffectCompilerParameters effectCompilerParameters = EffectCompilerParameters.Default;

        readonly ObservableCollection<IVLNodeDescription> nodeDescriptions;
        readonly DirectoryWatcher directoryWatcher;
        readonly SynchronizationContext mainContext;
        Timer timer;

        public EffectNodeFactory()
        {
            Init();

            mainContext = Services.GetService<SynchronizationContext>() ?? SynchronizationContext.Current;

            nodeDescriptions = new ObservableCollection<IVLNodeDescription>(GetNodeDescriptions(Content));
            NodeDescriptions = new ReadOnlyObservableCollection<IVLNodeDescription>(nodeDescriptions);

            var effectSystem = EffectSystem;
            // Ensure the effect system tracks the same files as we do
            var fieldInfo = typeof(EffectSystem).GetField("directoryWatcher", BindingFlags.NonPublic | BindingFlags.Instance);
            directoryWatcher = fieldInfo.GetValue(effectSystem) as DirectoryWatcher;
            directoryWatcher.Modified += DirectoryWatcher_Modified;
        }

        public ReadOnlyObservableCollection<IVLNodeDescription> NodeDescriptions { get; }
        public ContentManager Content { get; private set; }
        public ServiceRegistry Services { get; private set; }
        public GraphicsDevice GraphicsDevice { get; private set; }
        public GraphicsDeviceServiceLocal GraphicsDeviceService { get; private set; }
        public EffectSystem EffectSystem { get; private set; }

        public readonly object SyncRoot = new object();

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
                    if (TryGetFilePath(m, out string path))
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

        public static bool TryGetFilePath(ILogMessage m, out string path)
        {
            var module = m.Module;
            if (module != null && TryGetFilePath(module, out path))
                return true;
            return TryGetFilePath(m.Text, out path);
        }

        private static bool TryGetFilePath(string s, out string path)
        {
            var index = s.IndexOf('(');
            if (index >= 0)
            {
                path = s.Substring(0, index);
                return true;
            }
            else
            {
                path = null;
                return false;
            }
        }

        private static ShaderSource GetShaderSource(string effectName)
        {
            var isXdfx = ShaderMixinManager.Contains(effectName);
            if (isXdfx)
                return new ShaderMixinGeneratorSource(effectName);
            return new ShaderClassSource(effectName);
        }

        private ShaderMixinSource GetShaderMixinSource(ShaderSource shaderSource, CompilerParameters compilerParameters)
        {
            ShaderMixinSource mixinToCompile;
            var shaderMixinGeneratorSource = shaderSource as ShaderMixinGeneratorSource;

            if (shaderMixinGeneratorSource != null)
            {
                mixinToCompile = ShaderMixinManager.Generate(shaderMixinGeneratorSource.Name, compilerParameters);
            }
            else
            {
                mixinToCompile = shaderSource as ShaderMixinSource;
                var shaderClassSource = shaderSource as ShaderClassSource;

                if (shaderClassSource != null)
                {
                    mixinToCompile = new ShaderMixinSource { Name = shaderClassSource.ClassName };
                    mixinToCompile.Mixins.Add(shaderClassSource);
                }

                if (mixinToCompile == null)
                {
                    throw new ArgumentException("Unsupported ShaderSource type [{0}]. Supporting only ShaderMixinSource/sdfx, ShaderClassSource", "shaderSource");
                }
                if (string.IsNullOrEmpty(mixinToCompile.Name))
                {
                    throw new ArgumentException("ShaderMixinSource must have a name", "shaderSource");
                }
            }

            return mixinToCompile;
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
                timer = new Timer(_ => UpdateNodeDescriptions(), null, 500, Timeout.Infinite);
            }
        }

        void UpdateNodeDescriptions()
        {
            var descriptions = nodeDescriptions.ToArray();
            for (int i = 0; i < descriptions.Length; i++)
            {
                var description = descriptions[i] as EffectNodeDescription;
                if (description == null || !description.IsInUse)
                    continue;

                var updatedDescription = new EffectNodeDescription(this, description.Name, description.EffectName);
                if (updatedDescription.HasCompilerErrors != description.HasCompilerErrors ||
                    !updatedDescription.Inputs.SequenceEqual(description.Inputs, PinComparer.Instance) ||
                    !updatedDescription.Outputs.SequenceEqual(description.Outputs, PinComparer.Instance))
                {
                    if (updatedDescription.HasCompilerErrors)
                    {
                        // Keep the signature of previous description but show errors and since we have errors kill all living instances so they don't have to deal with it
                        nodeDescriptions[i] = new EffectNodeDescription(description, updatedDescription.CompilerResults);
                    }
                    else
                    {
                        // Signature change, force a new compilation
                        nodeDescriptions[i] = updatedDescription;
                    }
                }
                else if (updatedDescription.Bytecode != description.Bytecode)
                {
                    // Just increase the change count so instances can update
                    description.Version++;
                }
            }
        }
    }
    class PinComparer : IEqualityComparer<IVLPinDescription>
    {
        public static readonly PinComparer Instance = new PinComparer();

        public bool Equals(IVLPinDescription x, IVLPinDescription y)
        {
            return x.Name == y.Name && x.Type == y.Type;
        }

        public int GetHashCode(IVLPinDescription obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
