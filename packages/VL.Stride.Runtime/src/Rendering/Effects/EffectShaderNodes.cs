using Microsoft.Build.Construction;
using Stride.Core;
using Stride.Core.Extensions;
using Stride.Core.IO;
using Stride.Core.Mathematics;
using Stride.Core.Serialization.Contents;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.ComputeEffect;
using Stride.Rendering.Images;
using Stride.Shaders;
using Stride.Shaders.Compiler;
using Stride.Shaders.Parser;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using VL.Core;
using VL.Core.Diagnostics;
using VL.Model;
using VL.Stride.Core;
using VL.Stride.Engine;
using VL.Stride.Rendering.ComputeEffect;
using VL.Stride.Shaders;

namespace VL.Stride.Rendering
{
    static class EffectShaderNodes
    {
        public static void Register(IVLFactory services)
        {
            services.RegisterNodeFactory("VL.Stride.Rendering.EffectShaderNodes",
                init: factory =>
                {
                    var nodes = GetNodeDescriptions(factory).ToImmutableArray();
                    return NodeBuilding.NewFactoryImpl(nodes, forPath: path => factory =>
                    {
                        // In case "shaders" directory gets added or deleted invalidate the whole factory
                        var invalidated = NodeBuilding.WatchDir(path)
                            .Where(e => (e.ChangeType == WatcherChangeTypes.Created || e.ChangeType == WatcherChangeTypes.Deleted || e.ChangeType == WatcherChangeTypes.Renamed) && e.Name == EffectCompilerBase.DefaultSourceShaderFolder);

                        // File provider crashes if directory doesn't exist :/
                        var shadersPath = Path.Combine(path, EffectCompilerBase.DefaultSourceShaderFolder);
                        if (Directory.Exists(shadersPath))
                        {
                            try
                            {
                                var nodes = GetNodeDescriptions(factory, path, shadersPath);
                                // Additionaly watch out for new/deleted/renamed files
                                invalidated = invalidated.Merge(NodeBuilding.WatchDir(shadersPath)
                                    .Where(e => e.ChangeType == WatcherChangeTypes.Created || e.ChangeType == WatcherChangeTypes.Deleted || e.ChangeType == WatcherChangeTypes.Renamed)
                                    // Check for shader files only. Editor (like VS) create lot's of other temporary files.
                                    .Where(e => string.Equals(Path.GetExtension(e.Name), ".sdsl", StringComparison.OrdinalIgnoreCase) || string.Equals(Path.GetExtension(e.Name), ".sdfx", StringComparison.OrdinalIgnoreCase)));
                                return NodeBuilding.NewFactoryImpl(nodes.ToImmutableArray(), invalidated,
                                    export: c =>
                                    {
                                        var project = c.ProjectRootElement as ProjectRootElement;
                                        // Copy all shaders to the project directory
                                        var assetsFolder = Path.Combine(project.DirectoryPath, "Assets");
                                        Directory.CreateDirectory(assetsFolder);
                                        foreach (var f in Directory.EnumerateFiles(shadersPath))
                                        {
                                            if (string.Equals(Path.GetExtension(f), ".sdsl", StringComparison.OrdinalIgnoreCase) || string.Equals(Path.GetExtension(f), ".sdfx", StringComparison.OrdinalIgnoreCase))
                                                File.Copy(f, Path.Combine(assetsFolder, Path.GetFileName(f)), overwrite: true);
                                        }
                                    });
                            }
                            catch (UnauthorizedAccessException)
                            {
                                // When deleting a folder we can run into this one
                            }
                        }

                        // Just watch for changes
                        return NodeBuilding.NewFactoryImpl(invalidated: invalidated);
                    });
                });
        }

        static IEnumerable<IVLNodeDescription> GetNodeDescriptions(IVLNodeDescriptionFactory factory, string path = default, string shadersPath = default)
        {
            var serviceRegistry = SharedServices.GetRegistry();
            var graphicsDeviceService = serviceRegistry.GetService<IGraphicsDeviceService>();
            var graphicsDevice = graphicsDeviceService.GraphicsDevice;
            var contentManager = serviceRegistry.GetService<ContentManager>();
            var effectSystem = serviceRegistry.GetService<EffectSystem>();

            // Ensure path is visible to the effect system
            if (path != null)
                effectSystem.EnsurePathIsVisible(path);

            // Ensure the effect system tracks the same files as we do
            var fieldInfo = typeof(EffectSystem).GetField("directoryWatcher", BindingFlags.NonPublic | BindingFlags.Instance);
            var directoryWatcher = fieldInfo.GetValue(effectSystem) as DirectoryWatcher;
            var modifications = Observable.FromEventPattern<FileEvent>(directoryWatcher, nameof(DirectoryWatcher.Modified))
                .Select(e => e.EventArgs)
                .Where(e => e.ChangeType == FileEventChangeType.Changed || e.ChangeType == FileEventChangeType.Renamed);


            // Effect system deals with its internal cache on update, so make sure its called.
            effectSystem.Update(default);

            var compiler = effectSystem.Compiler;

            const string sdslFileFilter = "*.sdsl";
            const string drawFXSuffix = "_DrawFX";
            const string computeFXSuffix = "_ComputeFX";
            const string textureFXSuffix = "_TextureFX";

            // Traverse either the "shaders" folder in the database or in the given path (if present)
            IVirtualFileProvider fileProvider = default;
            if (path != null)
                fileProvider = new FileSystemProvider(null, path);
            else
                fileProvider = contentManager.FileProvider;

            foreach (var file in fileProvider.ListFiles(EffectCompilerBase.DefaultSourceShaderFolder, sdslFileFilter, VirtualSearchOption.TopDirectoryOnly))
            {
                var effectName = Path.GetFileNameWithoutExtension(file);
                if (effectName.EndsWith(drawFXSuffix))
                {
                    // Shader only for now
                    var name = GetNodeName(effectName, drawFXSuffix);
                    var shaderNodeName = new NameAndVersion($"{name.NamePart}Shader", name.VersionPart);

                    yield return NewDrawEffectShaderNode(shaderNodeName, effectName);
                    //DrawFX node
                }
                else if (effectName.EndsWith(textureFXSuffix))
                {
                    var name = GetNodeName(effectName, textureFXSuffix);
                    var shaderNodeName = new NameAndVersion($"{name.NamePart}Shader", name.VersionPart);

                    IVLNodeDescription shaderNodeDescription;
                    yield return shaderNodeDescription = NewImageEffectShaderNode(shaderNodeName, effectName);
                    yield return NewTextureFXNode(shaderNodeDescription, name);
                }
                else if (effectName.EndsWith(computeFXSuffix))
                {
                    // Shader only for now
                    var name = GetNodeName(effectName, computeFXSuffix);
                    var shaderNodeName = new NameAndVersion($"{name.NamePart}Shader", name.VersionPart);

                    yield return NewComputeEffectShaderNode(shaderNodeName, effectName);
                    //ComputeFX node
                }
            }

            static NameAndVersion GetNodeName(string effectName, string suffix)
            {
                // Levels_ClampBoth_TextureFX
                var name = effectName.Substring(0, effectName.Length - suffix.Length);
                // Levels_ClampBoth
                var nameParts = name.Split('_');
                if (nameParts.Length > 0)
                {
                    name = nameParts[0];
                    return new NameAndVersion(name, string.Join(" ", nameParts.Skip(1)));
                }
                return new NameAndVersion(name);
            }

            string GetPathOfSdslShader(string effectName)
            {
                var path = EffectCompilerBase.GetStoragePathFromShaderType(effectName);
                if (fileProvider.TryGetFileLocation(path, out var filePath, out _, out _))
                    return filePath;

                var pathUrl = path + "/path";
                if (fileProvider.FileExists(pathUrl))
                {
                    using (var pathStream = fileProvider.OpenStream(pathUrl, VirtualFileMode.Open, VirtualFileAccess.Read))
                    using (var reader = new StreamReader(pathStream))
                    {
                        return reader.ReadToEnd();
                    }
                }

                return null;
            }

            bool OpenEditor(string effectName)
            {
                var path = GetPathOfSdslShader(effectName);
                try
                {
                    Process.Start(path);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            (DynamicEffectInstance effect, ImmutableArray<Message> messages, IObservable<object> invalidated) CreateEffectInstance(string effectName, ParameterCollection parameters = null, string watchName = null)
            {
                var messages = ImmutableArray<Message>.Empty;
                if (watchName is null)
                    watchName = effectName;

                var effect = new DynamicEffectInstance(effectName, parameters);
                if (parameters is null)
                    parameters = effect.Parameters;

                IObservable<object> invalidated = modifications.Where(e => Path.GetFileNameWithoutExtension(e.Name) == watchName);
                // Setup our own watcher as Stride doesn't track shaders with errors
                if (path != null)
                {
                    invalidated = Observable.Merge(invalidated, NodeBuilding.WatchDir(shadersPath)
                        .Where(e => Path.GetFileNameWithoutExtension(e.Name) == watchName)
                        .Do(_ => ((EffectCompilerBase)effectSystem.Compiler).ResetCache(new HashSet<string>() { watchName })));
                }
                try
                {
                    effect.Initialize(serviceRegistry);
                    effect.UpdateEffect(graphicsDevice);
                }
                catch (InvalidOperationException)
                {
                    try
                    {
                        // Compile manually to get detailed errors
                        var compilerParameters = new CompilerParameters() { EffectParameters = effect.EffectCompilerParameters };
                        foreach (var effectParameterKey in parameters.ParameterKeyInfos)
                            if (effectParameterKey.Key.Type == ParameterKeyType.Permutation)
                                compilerParameters.SetObject(effectParameterKey.Key, parameters.ObjectValues[effectParameterKey.BindingSlot]);
                        var compilerResult = compiler.Compile(
                            shaderSource: GetShaderSource(effectName),
                            compilerParameters: compilerParameters);
                        messages = compilerResult.Messages.Select(m => m.ToMessage()).ToImmutableArray();
                    }
                    catch (Exception e)
                    {
                        messages = messages.Add(new Message(MessageType.Error, $"Shader compiler crashed: {e}"));
                    }
                }

                return (effect, messages, invalidated);
            }

            IEnumerable<ParameterKeyInfo> GetParameters(DynamicEffectInstance effectInstance)
            {
                var byteCode = effectInstance.Effect?.Bytecode;
                if (byteCode is null)
                    yield break;

                var layoutNames = byteCode.Reflection.ResourceBindings.Select(x => x.ResourceGroup ?? "Globals").Distinct().ToList();
                var parameters = effectInstance.Parameters;
                foreach (var parameter in parameters.Layout.LayoutParameterKeyInfos)
                {
                    var key = parameter.Key;
                    var name = key.Name;

                    // Skip constant buffers
                    if (layoutNames.Contains(name))
                        continue;

                    // Skip compiler injected paddings
                    if (name.Contains("_padding_"))
                        continue;

                    // Skip well known parameters
                    if (WellKnownParameters.PerFrameMap.ContainsKey(name) || WellKnownParameters.PerViewMap.ContainsKey(name))
                        continue;

                    yield return parameter;
                }
            }

            IVLNodeDescription NewDrawEffectShaderNode(NameAndVersion name, string effectName)
            {
                return factory.NewNodeDescription(
                    name: name,
                    category: "Stride.Rendering.DrawShaders",
                    fragmented: true,
                    init: buildContext =>
                    {
                        var (_effect, _messages, _invalidated) = CreateEffectInstance(effectName);

                        var _inputs = new List<IVLPinDescription>();
                        var _outputs = new List<IVLPinDescription>() { buildContext.Pin("Output", typeof(IEffect)) };

                        var _parameterSetterInput = new PinDescription<Action<ParameterCollection, RenderView, RenderDrawContext>>("Parameter Setter");

                        var usedNames = new HashSet<string>() { _parameterSetterInput.Name };
                        var needsWorld = false;
                        foreach (var parameter in GetParameters(_effect))
                        {
                            var key = parameter.Key;
                            var name = key.Name;

                            // Skip well known parameters
                            if (WellKnownParameters.PerFrameMap.ContainsKey(name) || WellKnownParameters.PerViewMap.ContainsKey(name))
                                continue;

                            if (WellKnownParameters.PerDrawMap.ContainsKey(name))
                            {
                                // Expose World only - all other world dependent parameters we can compute on our own
                                needsWorld = true;
                                continue;
                            }

                            _inputs.Add(new ParameterPinDescription(usedNames, key, parameter.Count));
                        }

                        if (needsWorld)
                            _inputs.Add(new ParameterPinDescription(usedNames, TransformationKeys.World));

                        _inputs.Add(_parameterSetterInput);

                        return buildContext.Implementation(
                            inputs: _inputs,
                            outputs: _outputs,
                            messages: _messages,
                            newNode: nodeBuildContext =>
                            {
                                var gameHandle = nodeBuildContext.NodeContext.GetGameHandle();
                                var game = gameHandle.Resource;
                                var effect = new CustomEffect(effectName, game.Services, game.GraphicsDevice);

                                var inputs = new List<IVLPin>();
                                foreach (var _input in _inputs)
                                {
                                    // Handle the predefined pins first
                                    if (_input == _parameterSetterInput)
                                        inputs.Add(nodeBuildContext.Input<Action<ParameterCollection, RenderView, RenderDrawContext>>(v => effect.ParameterSetter = v));
                                    else if (_input is ParameterPinDescription parameterPinDescription)
                                        inputs.Add(parameterPinDescription.CreatePin(game.GraphicsDevice, effect.Parameters));
                                }

                                var effectOutput = nodeBuildContext.Output<IEffect>(() => effect);
                                return nodeBuildContext.Node(
                                    inputs: inputs,
                                    outputs: new[] { effectOutput },
                                    update: default,
                                    dispose: () =>
                                    {
                                        effect.Dispose();
                                        gameHandle.Dispose();
                                    });
                            },
                            invalidated: _invalidated,
                            openEditor: () => OpenEditor(effectName)
                        );
                    });
            }

            IVLNodeDescription NewComputeEffectShaderNode(NameAndVersion name, string effectName)
            {
                return factory.NewNodeDescription(
                    name: name,
                    category: "Stride.Rendering.ComputeShaders",
                    fragmented: true,
                    init: buildContext =>
                    {
                        var _parameters = new ParameterCollection();
                        _parameters.Set(ComputeShaderBaseKeys.ThreadGroupCountGlobal, Int3.One);
                        _parameters.Set(ComputeEffectShaderKeys.ThreadNumbers, Int3.One);
                        _parameters.Set(ComputeEffectShaderKeys.ComputeShaderName, effectName);
                        var (_effect, _messages, _invalidated) = CreateEffectInstance("ComputeEffectShader", _parameters, watchName: effectName);

                        var _dispatcherInput = new PinDescription<IComputeEffectDispatcher>("Dispatcher");
                        var _threadNumbersInput = new PinDescription<Int3>("Thread Count", Int3.One);
                        var _inputs = new List<IVLPinDescription>()
                        {
                            _dispatcherInput,
                            _threadNumbersInput
                        };
                        var _outputs = new List<IVLPinDescription>() { buildContext.Pin("Output", typeof(IGraphicsRendererBase)) };

                        var usedNames = new HashSet<string>()
                        {
                            "Enabled"
                        };

                        foreach (var parameter in GetParameters(_effect))
                        {
                            var key = parameter.Key;
                            var name = key.Name;

                            _inputs.Add(new ParameterPinDescription(usedNames, key, parameter.Count));
                        }

                        IVLPinDescription _enabledInput;

                        _inputs.Add(_enabledInput = new PinDescription<bool>("Enabled", defaultValue: true));

                        return buildContext.Implementation(
                            inputs: _inputs,
                            outputs: _outputs,
                            messages: _messages,
                            newNode: nodeBuildContext =>
                            {
                                var gameHandle = nodeBuildContext.NodeContext.GetGameHandle();
                                var renderContext = RenderContext.GetShared(gameHandle.Resource.Services);
                                var shader = new ComputeEffectShader2(renderContext) { ShaderSourceName = effectName };
                                var inputs = new List<IVLPin>();
                                var enabledInput = default(IVLPin);
                                foreach (var _input in _inputs)
                                {
                                    // Handle the predefined pins first
                                    if (_input == _dispatcherInput)
                                        inputs.Add(nodeBuildContext.Input<IComputeEffectDispatcher>(setter: v => shader.Dispatcher = v));
                                    else if (_input == _threadNumbersInput)
                                        inputs.Add(nodeBuildContext.Input<Int3>(setter: v => shader.ThreadNumbers = v));
                                    else if (_input == _enabledInput)
                                        inputs.Add(enabledInput = nodeBuildContext.Input<bool>(v => shader.Enabled = v));
                                    else if (_input is ParameterPinDescription parameterPinDescription)
                                        inputs.Add(parameterPinDescription.CreatePin(graphicsDevice, shader.Parameters));
                                }

                                var effectOutput = nodeBuildContext.Output(() => shader);
                                return nodeBuildContext.Node(
                                    inputs: inputs,
                                    outputs: new[] { effectOutput },
                                    update: default,
                                    dispose: () =>
                                    {
                                        shader.Dispose();
                                        gameHandle.Dispose();
                                    });
                            },
                            invalidated: _invalidated,
                            openEditor: () => OpenEditor(effectName)
                        );
                    });
            }

            // name = LevelsShader (ClampBoth)
            // effectName = Levels_ClampBoth_TextureFX
            // effectMainName = Levels
            IVLNodeDescription NewImageEffectShaderNode(NameAndVersion name, string effectName)
            {
                return factory.NewNodeDescription(
                    name: name,
                    category: "Stride.Rendering.ImageShaders",
                    fragmented: true,
                    init: buildContext =>
                    {
                        var (_effect, _messages, _invalidated) = CreateEffectInstance(effectName);

                        var _inputs = new List<IVLPinDescription>();
                        var _outputs = new List<IVLPinDescription>() { buildContext.Pin("Output", typeof(ImageEffectShader)) };

                        var usedNames = new HashSet<string>()
                        {
                            "Output Texture",
                            "Enabled"
                        };

                        var _textureCount = 0;
                        foreach (var parameter in GetParameters(_effect).OrderBy(p => p.Key.Name.Contains("Texture") ? 0 : 1))
                        {
                            var key = parameter.Key;
                            var name = key.Name;

                            // Skip texel size - gets set by ImageEffectShader
                            if (name.EndsWith("TexelSize"))
                                continue;

                            // Skip the matrix transform - we're drawing fullscreen using a triangle
                            if (key == SpriteBaseKeys.MatrixTransform)
                                continue;

                            if (key.PropertyType == typeof(Texture))
                            {
                                var pinName = ++_textureCount == 1 ? "Texture" : $"Texture {_textureCount}";
                                usedNames.Add(pinName);
                                _inputs.Add(new PinDescription<Texture>(pinName));
                            }
                            else
                                _inputs.Add(new ParameterPinDescription(usedNames, key, parameter.Count));
                        }

                        IVLPinDescription _outputTextureInput, _enabledInput;

                        _inputs.Add(_outputTextureInput = new PinDescription<Texture>("Output Texture"));
                        _inputs.Add(_enabledInput = new PinDescription<bool>("Enabled", defaultValue: true));

                        return buildContext.Implementation(
                            inputs: _inputs,
                            outputs: _outputs,
                            messages: _messages,
                            newNode: nodeBuildContext =>
                            {
                                var gameHandle = nodeBuildContext.NodeContext.GetGameHandle();
                                var effect = new TextureFXEffect(effectName);
                                var inputs = new List<IVLPin>();
                                var enabledInput = default(IVLPin);
                                var textureCount = 0;
                                foreach (var _input in _inputs)
                                {
                                    // Handle the predefined pins first
                                    if (_input == _outputTextureInput)
                                    {
                                        inputs.Add(nodeBuildContext.Input<Texture>(setter: t =>
                                        {
                                            if (t != null)
                                                effect.SetOutput(t);
                                        }));
                                    }
                                    else if (_input == _enabledInput)
                                        inputs.Add(enabledInput = nodeBuildContext.Input<bool>(v => effect.Enabled = v));
                                    else if (_input is ParameterPinDescription parameterPinDescription)
                                        inputs.Add(parameterPinDescription.CreatePin(graphicsDevice, effect.Parameters));
                                    else if (_input is PinDescription<Texture> textureInput)
                                    {
                                        var slot = textureCount++;
                                        inputs.Add(nodeBuildContext.Input<Texture>(setter: t =>
                                        {
                                            effect.SetInput(slot, t);
                                        }));
                                    }
                                }

                                var effectOutput = ToOutput(nodeBuildContext, effect, () =>
                                {
                                    //effect.Enabled = enabledInput.Value && effect.IsInputAssigned && effect.IsOutputAssigned;
                                });
                                return nodeBuildContext.Node(
                                    inputs: inputs,
                                    outputs: new[] { effectOutput },
                                    update: default,
                                    dispose: () =>
                                    {
                                        effect.Dispose();
                                        gameHandle.Dispose();
                                    });
                            },
                            invalidated: _invalidated,
                            openEditor: () => OpenEditor(effectName)
                        );
                    });
            }

            IVLNodeDescription NewTextureFXNode(IVLNodeDescription shaderDescription, string name)
            {
                return factory.NewNodeDescription(
                    name: name,
                    category: "Stride.Textures.TextureFX",
                    fragmented: true,
                    init: buildContext =>
                    {
                        const int defaultSize = 512;
                        const PixelFormat defaultFormat = PixelFormat.R8G8B8A8_UNorm;

                        var _inputs = shaderDescription.Inputs.ToList();
                        var hasTextureInput = _inputs.Any(p => p.Type == typeof(Texture) && p.Name != "Output Texture");
                        if (!hasTextureInput)
                        {
                            _inputs.Insert(0, new PinDescription<int>("Width", defaultSize));
                            _inputs.Insert(1, new PinDescription<int>("Height", defaultSize));
                            _inputs.Insert(2, new PinDescription<PixelFormat>("Format", defaultFormat));
                        }
                        return buildContext.Implementation(
                            inputs: _inputs,
                            outputs: new[] { buildContext.Pin("Output", typeof(Texture)) },
                            messages: shaderDescription.Messages,
                            invalidated: shaderDescription.Invalidated,
                            newNode: nodeBuildContext =>
                            {
                                var nodeContext = nodeBuildContext.NodeContext;
                                var node = shaderDescription.CreateInstance(nodeContext);
                                var inputs = node.Inputs.ToList();
                                var textureInput = node.Inputs.ElementAtOrDefault(shaderDescription.Inputs.IndexOf(p => p.Name == "Texture"));
                                var outputTextureInput = node.Inputs.ElementAtOrDefault(shaderDescription.Inputs.IndexOf(p => p.Name == "Output Texture"));
                                var enabledInput = (IVLPin<bool>)node.Inputs.ElementAt(shaderDescription.Inputs.IndexOf(p => p.Name == "Enabled"));

                                IVLPin<int> outputWidth = default, outputHeight = default;
                                IVLPin<PixelFormat> outputFormat = default;
                                if (!hasTextureInput)
                                {
                                    inputs.Insert(0, outputWidth = nodeBuildContext.Input(defaultSize));
                                    inputs.Insert(1, outputHeight = nodeBuildContext.Input(defaultSize));
                                    inputs.Insert(2, outputFormat = nodeBuildContext.Input(defaultFormat));
                                }

                                var gameHandle = nodeContext.GetGameHandle();
                                var game = gameHandle.Resource;
                                var scheduler = game.Services.GetService<SchedulerSystem>();
                                var graphicsDevice = game.GraphicsDevice;
                                var output1 = default(((int width, int height, PixelFormat format) desc, Texture texture));
                                var output2 = default(((int width, int height, PixelFormat format) desc, Texture texture));
                                var mainOutput = nodeBuildContext.Output<Texture>(getter: () =>
                                {
                                    var inputTexture = textureInput?.Value as Texture;
                                    if (!enabledInput.Value)
                                        return inputTexture;

                                    var outputTexture = outputTextureInput.Value as Texture;
                                    if (outputTexture is null)
                                    {
                                        // No output texture is provided, generate on
                                        const TextureFlags textureFlags = TextureFlags.ShaderResource | TextureFlags.RenderTarget;
                                        var desc = default((int width, int height, PixelFormat format));
                                        if (inputTexture != null)
                                        {
                                            // Based on the input texture
                                            desc = (inputTexture.Width, inputTexture.Height, inputTexture.Format);

                                            // Watch out for feedback loops
                                            if (inputTexture == output1.texture)
                                            {
                                                Utilities.Swap(ref output1, ref output2);
                                            }
                                        }
                                        else if (!hasTextureInput)
                                        {
                                            // Based on the given parameters
                                            desc = (
                                                Math.Max(1, outputWidth.Value), 
                                                Math.Max(1, outputHeight.Value), 
                                                outputFormat.Value != PixelFormat.None ? outputFormat.Value : defaultFormat);
                                        }

                                        // Ensure we have an output of proper size
                                        if (desc != output1.desc)
                                        {
                                            output1.texture?.Dispose();
                                            output1.desc = desc;
                                            if (desc != default)
                                                output1.texture = Texture.New2D(graphicsDevice, desc.width, desc.height, desc.format, textureFlags);
                                            else
                                                output1.texture = null;
                                        }

                                        // Select it
                                        outputTexture = output1.texture;
                                    }

                                    var effect = node.Outputs[0].Value as TextureFXEffect;
                                    if (scheduler != null && effect != null && outputTexture != null)
                                    {
                                        effect.SetOutput(outputTexture);
                                        scheduler.Schedule(effect);
                                        return outputTexture;
                                    }

                                    return null;
                                });
                                return nodeBuildContext.Node(
                                    inputs: inputs,
                                    outputs: new[] { mainOutput },
                                    dispose: () =>
                                    {
                                        output1.texture?.Dispose();
                                        output2.texture?.Dispose();
                                        gameHandle.Dispose();
                                        node.Dispose();
                                    });
                            },
                            openEditor: () => shaderDescription.OpenEditor()
                        );
                    });
            }
        }

        static IVLPin ToOutput<T>(NodeBuilding.NodeInstanceBuildContext c, T value, Action getter)
        {
            return c.Output(() =>
            {
                getter();
                return value;
            });
        }

        static ShaderSource GetShaderSource(string effectName)
        {
            var isMixin = ShaderMixinManager.Contains(effectName);
            if (isMixin)
                return new ShaderMixinGeneratorSource(effectName);
            return new ShaderClassSource(effectName);
        }

        // Not used yet
        static Dictionary<string, Dictionary<string, ParameterKey>> GetCompilerParameters(string filePath, string sdfxEffectName)
        {
            // In .sdfx, shader has been renamed to effect, in order to avoid ambiguities with HLSL and .sdsl
            var macros = new[]
            {
                    new global::Stride.Core.Shaders.Parser.ShaderMacro("shader", "effect")
                };

            // Parse and collect
            var shader = StrideShaderParser.PreProcessAndParse(filePath, macros);
            var builder = new RuntimeShaderMixinBuilder(shader);
            return builder.CollectParameters();
        }

        class CustomEffect : IEffect, IDisposable
        {
            public readonly DynamicEffectInstance EffectInstance;
            readonly PerFrameParameters[] perFrameParams;
            readonly PerViewParameters[] perViewParams;
            readonly PerDrawParameters[] perDrawParams;

            public CustomEffect(string effectName, IServiceRegistry serviceRegistry, GraphicsDevice graphicsDevice, ParameterCollection parameters = default)
            {
                EffectInstance = new DynamicEffectInstance(effectName, parameters);
                EffectInstance.Initialize(serviceRegistry);
                EffectInstance.UpdateEffect(graphicsDevice);

                perFrameParams = EffectInstance.Parameters.GetWellKnownParameters(WellKnownParameters.PerFrameMap).ToArray();
                perViewParams = EffectInstance.Parameters.GetWellKnownParameters(WellKnownParameters.PerViewMap).ToArray();
                perDrawParams = EffectInstance.Parameters.GetWellKnownParameters(WellKnownParameters.PerDrawMap).ToArray();
            }

            public ParameterCollection Parameters => EffectInstance.Parameters;

            public Action<ParameterCollection, RenderView, RenderDrawContext> ParameterSetter { get; set; }

            public void Dispose()
            {
                EffectInstance.Dispose();
            }

            public EffectInstance SetParameters(RenderView renderView, RenderDrawContext renderDrawContext)
            {
                var parameters = EffectInstance.Parameters;
                try
                {
                    // TODO1: PerFrame could be done in Update if we'd have access to frame time
                    // TODO2: This code can be optimized by using parameter accessors and not parameter keys
                    parameters.SetPerFrameParameters(perFrameParams, renderDrawContext.RenderContext);

                    var parentTransformation = renderDrawContext.RenderContext.Tags.Get(EntityRendererRenderFeature.CurrentParentTransformation);
                    if (parameters.ContainsKey(TransformationKeys.World))
                    {
                        var world = parameters.Get(TransformationKeys.World);
                        Matrix.Multiply(ref world, ref parentTransformation, out var result);
                        parameters.SetPerDrawParameters(perDrawParams, renderView, ref result);
                    }
                    else
                    {
                        parameters.SetPerDrawParameters(perDrawParams, renderView, ref parentTransformation);
                    }

                    parameters.SetPerViewParameters(perViewParams, renderView);

                    ParameterSetter?.Invoke(parameters, renderView, renderDrawContext);
                }
                catch (Exception e)
                {
                    RuntimeGraph.ReportException(e);
                }
                return EffectInstance;
            }
        }
    }
}
