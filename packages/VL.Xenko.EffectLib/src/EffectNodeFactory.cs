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
using VL.Xenko.Games;
using Xenko.Core;
using Xenko.Core.Diagnostics;
using Xenko.Core.IO;
using Xenko.Core.Mathematics;
using Xenko.Core.Serialization.Contents;
using Xenko.Core.Storage;
using Xenko.Engine;
using Xenko.Engine.Processors;
using Xenko.Games;
using Xenko.Graphics;
using Xenko.Rendering;
using Xenko.Shaders;
using Xenko.Shaders.Compiler;
using VLServiceRegistry = VL.Core.ServiceRegistry;

[assembly: NodeFactory(typeof(VL.Xenko.EffectLib.MultiGameEffectNodeFactory))]

namespace VL.Xenko.EffectLib
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

    public class MultiGameEffectNodeFactory : IVLNodeDescriptionFactory
    {
        public static Game WaitingGame;
        public static Action WaitingRunCallback;
        public static MultiGameEffectNodeFactory Instance { get; private set; }

        Dictionary<Game, EffectNodeFactory> gameFactories = new Dictionary<Game, EffectNodeFactory>();

        ObservableCollection<IVLNodeDescription> nodeDescriptions = new ObservableCollection<IVLNodeDescription>();

        public ReadOnlyObservableCollection<IVLNodeDescription> NodeDescriptions { get; }

        public MultiGameEffectNodeFactory()
        {
            if (VLGame.GameInstance != null)
            {
                CreateTempGame(new Rectangle(), out var game, out var update);
                AddGame(game);
            }

            NodeDescriptions = new ReadOnlyObservableCollection<IVLNodeDescription>(nodeDescriptions);
            Instance = this;
            if (WaitingGame != null)
            {
                AddGame(WaitingGame);
            }
        }

        public void AddGame(Game game, SynchronizationContext mainContext = null)
        {
            if (!gameFactories.ContainsKey(game))
            {
                var factory = new EffectNodeFactory(game, this, mainContext);

                foreach (var nd in factory.NodeDescriptions)
                    if (!nodeDescriptions.Contains(nd, NodeComparer.Instance))
                        nodeDescriptions.Add(nd);

                ((INotifyCollectionChanged)factory.NodeDescriptions).CollectionChanged += GameEffectNodeFactory_CollectionChanged;

                gameFactories[game] = factory;
            }
        }

        public void RemoveGame(Game game)
        {
            if (gameFactories.TryGetValue(game, out var factory))
            {
                ((INotifyCollectionChanged)factory.NodeDescriptions).CollectionChanged -= GameEffectNodeFactory_CollectionChanged;

                //foreach (var item in factory.NodeDescriptions)
                //    nodeDescriptions.Remove(item);

                gameFactories.Remove(game);
            }
        }

        private void GameEffectNodeFactory_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                    {
                        var nd = (IVLNodeDescription)item;
                        if (!nodeDescriptions.Contains(nd, NodeComparer.Instance))
                            nodeDescriptions.Add(nd);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                        nodeDescriptions.Remove((IVLNodeDescription)item);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    nodeDescriptions[e.OldStartingIndex] = ((IEnumerable<IVLNodeDescription>)e.NewItems).FirstOrDefault();
                    break;
                case NotifyCollectionChangedAction.Move:
                    nodeDescriptions.Move(e.OldStartingIndex, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var item in e.OldItems)
                        nodeDescriptions.Remove((IVLNodeDescription)item);
                    break;
                default:
                    break;
            }
        }

        public static void CreateTempGame(Rectangle bounds, out VLGame output, out Action runCallback)
        {
            var game = new VLGame();
#if DEBUG
            game.GraphicsDeviceManager.DeviceCreationFlags |= DeviceCreationFlags.Debug;
#endif

            SetupGameEvents(game);



            var context = new GameContextWinforms(null, 0, 0, isUserManagingRun: true);
            game.Run(context);
            game.AddLayerRenderFeature();
            runCallback = context.RunCallback;


            //game.Window.Visible = true;

            output = game;
        }

        private static void SetupGameEvents(VLGame game)
        {
            var wrStarted = new WeakReference(null);

            EventHandler started = (s, e) =>
            {
                if (s == game)
                {
                    game.Window.AllowUserResizing = true;

                    MultiGameEffectNodeFactory.Instance?.AddGame(game, SynchronizationContext.Current);

                    Game.GameStarted -= wrStarted.Target as EventHandler;
                }
            };

            wrStarted.Target = started;

            Game.GameStarted += started;

            var wrDestroyed = new WeakReference(null);
            EventHandler destroyed = (s, e) =>
            {
                if (s == game)
                {
                    MultiGameEffectNodeFactory.Instance?.RemoveGame(game);
                    Game.GameDestroyed -= wrDestroyed.Target as EventHandler;
                }
            };

            wrDestroyed.Target = destroyed;

            Game.GameDestroyed += destroyed;
        }

    }

    public class EffectNodeFactory : IVLNodeDescriptionFactory
    {
        const string xkslFileFilter = "*.xksl";
        const string CompiledShadersKey = "__shaders_bytecode__"; // Taken from EffectCompilerCache.cs

        private EffectCompilerParameters effectCompilerParameters = EffectCompilerParameters.Default;

        public readonly ContentManager ContentManger;
        public readonly EffectSystem EffectSystem;
        public readonly IServiceRegistry ServiceRegistry;
        public readonly GraphicsDeviceManager DeviceManager;
        public readonly ScriptSystem ScriptSystem;

        readonly ObservableCollection<IVLNodeDescription> nodeDescriptions;
        readonly DirectoryWatcher directoryWatcher;
        readonly SynchronizationContext mainContext;
        readonly MultiGameEffectNodeFactory parentFactory;
        Timer timer;

        public EffectNodeFactory(Game game, MultiGameEffectNodeFactory parentFactory, SynchronizationContext syncContext = null)
        {
            this.parentFactory = parentFactory;
            mainContext = syncContext ?? SynchronizationContext.Current;

            ServiceRegistry = game?.Services;
            ContentManger = game?.Content;
            EffectSystem = game?.EffectSystem;
            DeviceManager = game?.GraphicsDeviceManager;
            ScriptSystem = game?.Script;
            nodeDescriptions = new ObservableCollection<IVLNodeDescription>(GetNodeDescriptions());
            NodeDescriptions = new ReadOnlyObservableCollection<IVLNodeDescription>(nodeDescriptions);

            if (EffectSystem != null)
            {
                // Leads to deadlocks
                ((EffectCompilerCache)EffectSystem.Compiler).CompileEffectAsynchronously = false;

                // Ensure the effect system tracks the same files as we do
                var fieldInfo = typeof(EffectSystem).GetField("directoryWatcher", BindingFlags.NonPublic | BindingFlags.Instance);
                directoryWatcher = fieldInfo.GetValue(EffectSystem) as DirectoryWatcher;
            }
            else
            {
                directoryWatcher = new DirectoryWatcher(xkslFileFilter);
            }
            directoryWatcher.Modified += DirectoryWatcher_Modified;
        }

        public ReadOnlyObservableCollection<IVLNodeDescription> NodeDescriptions { get; }

        public readonly object SyncRoot = new object();

        protected GraphicsDevice GraphicsDevice => DeviceManager?.GraphicsDevice;
        protected DatabaseFileProvider FileProvider => ContentManger?.FileProvider;

        public CompilerResults GetCompilerResults(string effectName)
        {
            return GetCompilerResults(effectName, new CompilerParameters());
        }

        public CompilerResults GetCompilerResults(string effectName, CompilerParameters compilerParameters)
        {
            if (effectName == null) throw new ArgumentNullException("effectName");
            if (compilerParameters == null) throw new ArgumentNullException("compilerParameters");

            // Setup compilation parameters
            compilerParameters.EffectParameters.Platform = GraphicsDevice.Platform;
            compilerParameters.EffectParameters.Profile = DeviceManager?.ShaderProfile ?? GraphicsDevice.Features.RequestedProfile;
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
                    using (var pathStream = FileProvider.OpenStream(EffectCompilerBase.GetStoragePathFromShaderType(type) + "/path", VirtualFileMode.Open, VirtualFileAccess.Read))
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
            var isXkfx = ShaderMixinManager.Contains(effectName);
            if (isXkfx)
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
                    throw new ArgumentException("Unsupported ShaderSource type [{0}]. Supporting only ShaderMixinSource/xkfx, ShaderClassSource", "shaderSource");
                }
                if (string.IsNullOrEmpty(mixinToCompile.Name))
                {
                    throw new ArgumentException("ShaderMixinSource must have a name", "shaderSource");
                }
            }

            return mixinToCompile;
        }

        public string GetPathOfXkslShader(string effectName)
        {
            using (var pathStream = FileProvider.OpenStream(EffectCompilerBase.GetStoragePathFromShaderType(effectName) + "/path", VirtualFileMode.Open, VirtualFileAccess.Read))
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

        IEnumerable<IVLNodeDescription> GetNodeDescriptions()
        {
            var nodeDescriptions = new Dictionary<string, IVLNodeDescription>();
            if (ContentManger != null)
            {
                var files = ContentManger.FileProvider.ListFiles(EffectCompilerBase.DefaultSourceShaderFolder, xkslFileFilter, VirtualSearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var effectName = Path.GetFileNameWithoutExtension(file);
                    yield return new EffectNodeDescription(this, parentFactory, effectName);
                }
            }
        }

        void DirectoryWatcher_Modified(object sender, FileEvent e)
        {
            if (e.ChangeType == FileEventChangeType.Changed || e.ChangeType == FileEventChangeType.Renamed)
            {
                timer?.Dispose();
                timer = new Timer(_ => mainContext.Post(s => UpdateNodeDescriptions(), null), null, 500, Timeout.Infinite);
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

                var updatedDescription = new EffectNodeDescription(this, parentFactory, description.Name);
                if (updatedDescription.HasCompilerErrors != description.HasCompilerErrors ||
                    !updatedDescription.Inputs.SequenceEqual(description.Inputs, PinComparer.Instance) ||
                    !updatedDescription.Outputs.SequenceEqual(description.Outputs, PinComparer.Instance))
                {
                    if (updatedDescription.HasCompilerErrors)
                    {
                        // Keep the signature of previous description but show errors and since we have errors kill all living instances so they don't have to deal with it
                        nodeDescriptions[i] = new EffectNodeDescription(description, parentFactory, updatedDescription.CompilerResults);
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
