using Stride.Core;
using Stride.Core.Extensions;
using Stride.Core.IO;
using Stride.Core.Mathematics;
using Stride.Core.Serialization.Contents;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.ComputeEffect;
using Stride.Rendering.Images;
using Stride.Rendering.Materials;
using Stride.Shaders;
using Stride.Shaders.Compiler;
using Stride.Shaders.Parser;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using VL.Core;
using VL.Core.Diagnostics;
using VL.Model;
using VL.Stride.Core;
using VL.Stride.Graphics;
using VL.Stride.Effects;
using VL.Stride.Engine;
using VL.Stride.Rendering.ComputeEffect;
using VL.Stride.Shaders;
using VL.Stride.Shaders.ShaderFX;

namespace VL.Stride.Rendering
{
    static class EffectShaderNodes
    {
        public static NodeBuilding.FactoryImpl Init(IVLNodeDescriptionFactory factory)
        {
            ShaderMetadata.RegisterAdditionalShaderAttributes();

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
                            .Where(e => IsNewOrDeletedShaderFile(e)));
                        return NodeBuilding.NewFactoryImpl(nodes.ToImmutableArray(), invalidated,
                            export: c =>
                            {
                                // Copy all shaders to the project directory but do so only once per shader path relying on the assumption that the generated project
                                // containing the Assets folder will be referenced by projects further up in the dependency tree.
                                var pathExportedKey = (typeof(EffectShaderNodes), shadersPath);
                                if (c.SolutionWideStorage.TryAdd(pathExportedKey, pathExportedKey))
                                {
                                    var assetsFolder = Path.Combine(c.DirectoryPath, "Assets");
                                    Directory.CreateDirectory(assetsFolder);
                                    foreach (var f in Directory.EnumerateFiles(shadersPath))
                                    {
                                        if (string.Equals(Path.GetExtension(f), ".sdsl", StringComparison.OrdinalIgnoreCase) || string.Equals(Path.GetExtension(f), ".sdfx", StringComparison.OrdinalIgnoreCase))
                                            File.Copy(f, Path.Combine(assetsFolder, Path.GetFileName(f)), overwrite: true);
                                    }
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
        }

        static bool IsShaderFile(string file) => string.Equals(Path.GetExtension(file), ".sdsl", StringComparison.OrdinalIgnoreCase) || string.Equals(Path.GetExtension(file), ".sdfx", StringComparison.OrdinalIgnoreCase);

        static bool IsNewOrDeletedShaderFile(FileSystemEventArgs e)
        {
            // Check for shader files only. Editor (like VS) create lot's of other temporary files.
            if (e.ChangeType == WatcherChangeTypes.Created || e.ChangeType == WatcherChangeTypes.Deleted)
                return IsShaderFile(e.Name);
            // Also the old name must be a shader file. We're not interested in weired renamings by VS.
            if (e.ChangeType == WatcherChangeTypes.Renamed && e is RenamedEventArgs r)
                return IsShaderFile(e.Name) && IsShaderFile(r.OldName);
            return false;
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
            const string shaderFXSuffix = "_ShaderFX";

            // Traverse either the "shaders" folder in the database or in the given path (if present)
            IVirtualFileProvider fileProvider = default;
            var dbFileProvider = effectSystem.FileProvider; //should include current path
            var sourceManager = dbFileProvider.GetShaderSourceManager();
            if (path != null)
                fileProvider = new FileSystemProvider(null, path);
            else
                fileProvider = contentManager.FileProvider;


            EffectUtils.ResetParserCache();

            foreach (var file in fileProvider.ListFiles(EffectCompilerBase.DefaultSourceShaderFolder, sdslFileFilter, VirtualSearchOption.TopDirectoryOnly))
            {
                var effectName = Path.GetFileNameWithoutExtension(file);
                if (effectName.EndsWith(drawFXSuffix))
                {
                    // Shader only for now
                    var name = GetNodeName(effectName, drawFXSuffix);
                    var shaderNodeName = new NameAndVersion($"{name.NamePart}Shader", name.VersionPart);
                    var shaderMetadata = ShaderMetadata.CreateMetadata(effectName, dbFileProvider, sourceManager);

                    yield return NewDrawEffectShaderNode(shaderNodeName, effectName, shaderMetadata);
                    //DrawFX node
                }
                else if (effectName.EndsWith(textureFXSuffix))
                {
                    var name = GetNodeName(effectName, textureFXSuffix);
                    var shaderNodeName = new NameAndVersion($"{name.NamePart}Shader", name.VersionPart);
                    var shaderMetadata = ShaderMetadata.CreateMetadata(effectName, dbFileProvider, sourceManager);

                    IVLNodeDescription shaderNodeDescription;
                    yield return shaderNodeDescription = NewImageEffectShaderNode(shaderNodeName, effectName, shaderMetadata);
                    yield return NewTextureFXNode(shaderNodeDescription, name, shaderMetadata);
                }
                else if (effectName.EndsWith(computeFXSuffix))
                {
                    // Shader only for now
                    var name = GetNodeName(effectName, computeFXSuffix);
                    var shaderNodeName = new NameAndVersion($"{name.NamePart}Shader", name.VersionPart);
                    var shaderMetadata = ShaderMetadata.CreateMetadata(effectName, dbFileProvider, sourceManager);

                    yield return NewComputeEffectShaderNode(shaderNodeName, effectName, shaderMetadata);
                    //ComputeFX node
                }
                else if (effectName.EndsWith(shaderFXSuffix))
                {
                    // Shader only
                    var name = GetNodeName(effectName, shaderFXSuffix);
                    var shaderNodeName = new NameAndVersion($"{name.NamePart}", name.VersionPart);
                    var shaderMetadata = ShaderMetadata.CreateMetadata(effectName, dbFileProvider, sourceManager);

                    yield return NewShaderFXNode(shaderNodeName, effectName, shaderMetadata);
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

            bool OpenEditor(string effectName)
            {
                var path = EffectUtils.GetPathOfSdslShader(effectName, fileProvider);
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

            (DynamicEffectInstance effect, ImmutableArray<Message> messages, IObservable<object> invalidated) CreateEffectInstance(string effectName, ShaderMetadata shaderMetadata, ParameterCollection parameters = null, string baseShaderName = null)
            {
                var messages = ImmutableArray<Message>.Empty;
                if (baseShaderName is null)
                    baseShaderName = effectName;

                var effect = new DynamicEffectInstance(effectName, parameters);
                if (parameters is null)
                    parameters = effect.Parameters;


                var watchNames = new HashSet<string>() { baseShaderName };

                foreach (var baseClass in shaderMetadata.ParsedShader?.BaseShaders ?? Enumerable.Empty<ParsedShader>())
                {
                    var baseClassPath = baseClass.Shader.Span.Location.FileSource;
                    if (baseClassPath.ToLowerInvariant().Contains("/stride."))
                        continue; //in stride package folder

                    watchNames.Add(Path.GetFileNameWithoutExtension(baseClassPath));
                } 

                IObservable<object> invalidated = modifications.Where(e => watchNames.Contains(Path.GetFileNameWithoutExtension(e.Name)));
                // Setup our own watcher as Stride doesn't track shaders with errors
                if (path != null)
                {
                    invalidated = Observable.Merge(invalidated, NodeBuilding.WatchDir(shadersPath)
                        .Where(e => watchNames.Contains(Path.GetFileNameWithoutExtension(e.Name)))
                        .Do(e =>
                        {
                            ((EffectCompilerBase)effectSystem.Compiler).ResetCache(new HashSet<string>() { Path.GetFileNameWithoutExtension(e.Name) });
                            foreach (var watchName in watchNames)
                            {
                                EffectUtils.ResetParserCache(watchName);
                            }
                        }));
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
                var compositionParameters = parameters.ParameterKeyInfos.Where(pki => pki.Key.PropertyType == typeof(ShaderSource) && pki.Key.Name != "EffectNodeBase.EffectNodeBaseShader");
                foreach (var parameter in parameters.Layout.LayoutParameterKeyInfos.Concat(compositionParameters))
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
                    if (WellKnownParameters.PerFrameMap.ContainsKey(name)
                        || WellKnownParameters.PerViewMap.ContainsKey(name)
                        || WellKnownParameters.TexturingMap.ContainsKey(name))
                        continue;

                    // Skip inputs from ShaderFX graph
                    if (name.StartsWith("ShaderFX.Input"))
                        continue;

                    yield return parameter;
                }
            }

            IVLNodeDescription NewDrawEffectShaderNode(NameAndVersion name, string shaderName, ShaderMetadata shaderMetadata)
            {
                return factory.NewNodeDescription(
                    name: name,
                    category: "Stride.Rendering.DrawShaders",
                    fragmented: true,
                    init: buildContext =>
                    {
                        var mixinParams = BuildBaseMixin(shaderName, shaderMetadata, graphicsDevice, out var shaderMixinSource);
                        var (_effect, _messages, _invalidated) = CreateEffectInstance("DrawFXEffect", shaderMetadata, mixinParams, baseShaderName: shaderName);

                        var _inputs = new List<IVLPinDescription>();
                        var _outputs = new List<IVLPinDescription>() { buildContext.Pin("Output", typeof(IEffect)) };

                        var _parameterSetterInput = new PinDescription<Action<ParameterCollection, RenderView, RenderDrawContext>>("Parameter Setter");

                        var usedNames = new HashSet<string>() { _parameterSetterInput.Name };
                        var needsWorld = false;
                        foreach (var parameter in GetParameters(_effect))
                        {
                            var key = parameter.Key;
                            var name = key.Name;

                            if (WellKnownParameters.PerDrawMap.ContainsKey(name))
                            {
                                // Expose World only - all other world dependent parameters we can compute on our own
                                needsWorld = true;
                                continue;
                            }

                            var typeInPatch = shaderMetadata.GetPinType(key, out var boxedDefaultValue);
                            shaderMetadata.GetPinDocuAndVisibility(key, out var summary, out var remarks, out var isOptional);
                            _inputs.Add(new ParameterPinDescription(usedNames, key, parameter.Count, defaultValue: boxedDefaultValue, typeInPatch: typeInPatch) { IsVisible = !isOptional, Summary = summary, Remarks = remarks });
                        }

                        if (needsWorld)
                            _inputs.Add(new ParameterPinDescription(usedNames, TransformationKeys.World));

                        _inputs.Add(_parameterSetterInput);

                        return buildContext.NewNode(
                            inputs: _inputs,
                            outputs: _outputs,
                            messages: _messages,
                            summary: shaderMetadata.Summary,
                            remarks: shaderMetadata.Remarks,
                            tags: shaderMetadata.Tags,
                            newNode: nodeBuildContext =>
                            {
                                var gameHandle = nodeBuildContext.NodeContext.GetGameHandle();
                                var game = gameHandle.Resource;
                                var mixinParams = BuildBaseMixin(shaderName, shaderMetadata, graphicsDevice, out var shaderMixinSource);
                                var effect = new CustomEffect("DrawFXEffect", game.Services, game.GraphicsDevice, mixinParams);

                                var inputs = new List<IVLPin>();
                                foreach (var _input in _inputs)
                                {
                                    // Handle the predefined pins first
                                    if (_input == _parameterSetterInput)
                                        inputs.Add(nodeBuildContext.Input<Action<ParameterCollection, RenderView, RenderDrawContext>>(v => effect.ParameterSetter = v));
                                    else if (_input is ParameterPinDescription parameterPinDescription)
                                        inputs.Add(parameterPinDescription.CreatePin(game.GraphicsDevice, effect.Parameters));
                                }

                                var compositionPins = inputs.OfType<ShaderFXPin>().ToList();

                                var effectOutput = nodeBuildContext.Output<IEffect>(() =>
                                {
                                    UpdateCompositions(compositionPins, graphicsDevice, effect.Parameters, shaderMixinSource, effect.Subscriptions);

                                    return effect;
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
                            openEditor: () => OpenEditor(shaderName)
                        );
                    });
            }

            IVLNodeDescription NewComputeEffectShaderNode(NameAndVersion name, string shaderName, ShaderMetadata shaderMetadata)
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

                        BuildBaseMixin(shaderName, shaderMetadata, graphicsDevice, out var shaderMixinSource, _parameters);

                        var (_effect, _messages, _invalidated) = CreateEffectInstance("ComputeFXEffect", shaderMetadata, _parameters, baseShaderName: shaderName);

                        var _dispatcherInput = new PinDescription<IComputeEffectDispatcher>("Dispatcher");
                        var _threadNumbersInput = new PinDescription<Int3>("Thread Group Size", Int3.One);
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

                            var typeInPatch = shaderMetadata.GetPinType(key, out var boxedDefaultValue);
                            shaderMetadata.GetPinDocuAndVisibility(key, out var summary, out var remarks, out var isOptional);
                            _inputs.Add(new ParameterPinDescription(usedNames, key, parameter.Count, defaultValue: boxedDefaultValue, typeInPatch: typeInPatch) { IsVisible = !isOptional, Summary = summary, Remarks = remarks });
                        }

                        IVLPinDescription _enabledInput;

                        _inputs.Add(_enabledInput = new PinDescription<bool>("Enabled", defaultValue: true));

                        return buildContext.NewNode(
                            inputs: _inputs,
                            outputs: _outputs,
                            messages: _messages,
                            summary: shaderMetadata.Summary,
                            remarks: shaderMetadata.Remarks,
                            tags: shaderMetadata.Tags,
                            newNode: nodeBuildContext =>
                            {
                                var gameHandle = nodeBuildContext.NodeContext.GetGameHandle();
                                var renderContext = RenderContext.GetShared(gameHandle.Resource.Services);
                                var mixinParams = BuildBaseMixin(shaderName, shaderMetadata, graphicsDevice, out var shaderMixinSource);
                                var effect = new ComputeEffectShader2(renderContext, shaderName, mixinParams);
                                var inputs = new List<IVLPin>();
                                var enabledInput = default(IVLPin);
                                foreach (var _input in _inputs)
                                {
                                    // Handle the predefined pins first
                                    if (_input == _dispatcherInput)
                                        inputs.Add(nodeBuildContext.Input<IComputeEffectDispatcher>(setter: v => effect.Dispatcher = v));
                                    else if (_input == _threadNumbersInput)
                                        inputs.Add(nodeBuildContext.Input<Int3>(setter: v => effect.ThreadGroupSize = v));
                                    else if (_input == _enabledInput)
                                        inputs.Add(enabledInput = nodeBuildContext.Input<bool>(v => effect.Enabled = v, effect.Enabled));
                                    else if (_input is ParameterPinDescription parameterPinDescription)
                                        inputs.Add(parameterPinDescription.CreatePin(graphicsDevice, effect.Parameters));
                                }

                                var compositionPins = inputs.OfType<ShaderFXPin>().ToList();

                                var effectOutput = nodeBuildContext.Output(() =>
                                {
                                    UpdateCompositions(compositionPins, graphicsDevice, effect.Parameters, shaderMixinSource, effect.Subscriptions);

                                    return effect;
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
                            openEditor: () => OpenEditor(shaderName)
                        );
                    });
            }

            IVLNodeDescription NewShaderFXNode(NameAndVersion name, string shaderName, ShaderMetadata shaderMetadata)
            {
                return factory.NewNodeDescription(
                    name: name,
                    category: "Stride.Rendering.Experimental.ShaderFX",
                    fragmented: true,
                    init: buildContext =>
                    {
                        var outputType = shaderMetadata.GetShaderFXOutputType(out var innerType);
                        var mixinParams = BuildBaseMixin(shaderName, shaderMetadata, graphicsDevice, out var shaderMixinSource);
                        var (_effect, _messages, _invalidated) = CreateEffectInstance("ShaderFXEffect", shaderMetadata, mixinParams, baseShaderName: shaderName);

                        var _inputs = new List<IVLPinDescription>();
                        var _outputs = new List<IVLPinDescription>() { buildContext.Pin("Output", outputType) };

                        var usedNames = new HashSet<string>();
                        var needsWorld = false;

                        
                        foreach (var parameter in GetParameters(_effect))
                        {
                            var key = parameter.Key;
                            var name = key.Name;

                            if (WellKnownParameters.PerDrawMap.ContainsKey(name))
                            {
                                // Expose World only - all other world dependent parameters we can compute on our own
                                needsWorld = true;
                                continue;
                            }

                            var typeInPatch = shaderMetadata.GetPinType(key, out var boxedDefaultValue);
                            shaderMetadata.GetPinDocuAndVisibility(key, out var summary, out var remarks, out var isOptional);
                            _inputs.Add(new ParameterPinDescription(usedNames, key, parameter.Count, defaultValue: boxedDefaultValue, typeInPatch: typeInPatch) { IsVisible = !isOptional, Summary = summary, Remarks = remarks });
                        }

                        // local input values
                        foreach (var key in shaderMetadata.ParsedShader?.GetUniformInputs() ?? Enumerable.Empty<ParameterKey>())
                        {
                            var name = key.Name;

                            if (WellKnownParameters.PerDrawMap.ContainsKey(name))
                            {
                                // Expose World only - all other world dependent parameters we can compute on our own
                                needsWorld = true;
                                continue;
                            }

                            var typeInPatch = shaderMetadata.GetPinType(key, out var boxedDefaultValue);
                            if (boxedDefaultValue == null)
                                boxedDefaultValue = key.DefaultValueMetadata.GetDefaultValue();

                            shaderMetadata.GetPinDocuAndVisibility(key, out var summary, out var remarks, out var isOptional);

                            _inputs.Add(new ParameterPinDescription(usedNames, key, 1, defaultValue: boxedDefaultValue, typeInPatch: typeInPatch) { IsVisible = !isOptional, Summary = summary, Remarks = remarks });
                        }

                        if (needsWorld)
                            _inputs.Add(new ParameterPinDescription(usedNames, TransformationKeys.World));

                        return buildContext.NewNode(
                            inputs: _inputs,
                            outputs: _outputs,
                            messages: _messages,
                            summary: shaderMetadata.Summary,
                            remarks: shaderMetadata.Remarks,
                            tags: shaderMetadata.Tags,
                            newNode: nodeBuildContext =>
                            {
                                var gameHandle = nodeBuildContext.NodeContext.GetGameHandle();
                                var game = gameHandle.Resource;

                                var tempParameters = new ParameterCollection(); // only needed for pin construction - parameter updater will later take care of multiple sinks
                                var nodeState = new ShaderFXNodeState(shaderName);

                                var inputs = new List<IVLPin>();
                                foreach (var _input in _inputs)
                                {
                                    if (_input is ParameterPinDescription parameterPinDescription)
                                        inputs.Add(parameterPinDescription.CreatePin(game.GraphicsDevice, tempParameters));
                                }

                                var outputMaker = typeof(EffectShaderNodes).GetMethod(nameof(BuildOutput), BindingFlags.Static | BindingFlags.NonPublic);
                                outputMaker = outputMaker.MakeGenericMethod(outputType, innerType);
                                outputMaker.Invoke(null, new object[] { nodeBuildContext, nodeState, inputs });

                                return nodeBuildContext.Node(
                                    inputs: inputs,
                                    outputs: new[] { nodeState.OutputPin },
                                    update: default,
                                    dispose: () =>
                                    {
                                        gameHandle.Dispose();
                                    });
                            },
                            invalidated: _invalidated,
                            openEditor: () => OpenEditor(shaderName)
                        );
                    });
            }

            const string textureInputName = "Input";
            const string samplerInputName = "Sampler";

            // name = LevelsShader (ClampBoth)
            // effectName = Levels_ClampBoth_TextureFX
            // effectMainName = Levels
            IVLNodeDescription NewImageEffectShaderNode(NameAndVersion name, string shaderName, ShaderMetadata shaderMetadata)
            {
                return factory.NewNodeDescription(
                    name: name,
                    category: "Stride.Rendering.ImageShaders.Experimental.Advanced",
                    fragmented: true,
                    init: buildContext =>
                    {

                        var mixinParams = BuildBaseMixin(shaderName, shaderMetadata, graphicsDevice, out var shaderMixinSource);

                        var (_effect, _messages, _invalidated) = CreateEffectInstance("TextureFXEffect", shaderMetadata, mixinParams, baseShaderName: shaderName);

                        var _inputs = new List<IVLPinDescription>();
                        var _outputs = new List<IVLPinDescription>() { buildContext.Pin("Output", typeof(ImageEffectShader)) };

                        // The pins as specified by https://github.com/devvvvs/vvvv/issues/5756
                        var usedNames = new HashSet<string>()
                        {
                            "Output Size",
                            "Output Format",
                            "Output Texture",
                            "Enabled",
                            "Apply"
                        };

                        var _textureCount = 0;
                        var _samplerCount = 0;
                        var parameters = GetParameters(_effect).OrderBy(p => p.Key.Name.Contains(".Texture") ? 0 : 1).ToList();

                        //order sampler pins after their corresponding texture pins
                        var samplerPins = new Dictionary<ParameterKeyInfo, int>();
                        //find all samplers that have a corresponding texture
                        int insertOffset = 0;
                        foreach (var parameter in parameters)
                        {
                            if (parameter.Key.Name.StartsWith("Texturing.Sampler"))
                            {
                                var texturePinIdx = parameters.IndexOf(p => p.Key.Name == parameter.Key.Name.Replace("Sampler", "Texture"));
                                if (texturePinIdx >= 0)
                                {
                                    samplerPins.Add(parameter, texturePinIdx + insertOffset);
                                    insertOffset++;
                                }
                            }
                        }

                        //move the sampler pins after the corresponding texture pins
                        foreach (var samplerPin in samplerPins)
                        {
                            parameters.Remove(samplerPin.Key);
                            parameters.Insert(samplerPin.Value + 1, samplerPin.Key);
                        }

                        foreach (var parameter in parameters)
                        {
                            var key = parameter.Key;
                            var name = key.Name;

                            // Skip the matrix transform - we're drawing fullscreen
                            if (key == SpriteBaseKeys.MatrixTransform)
                                continue;

                            if (key.PropertyType == typeof(Texture))
                            {
                                var pinName = "";
                                if (shaderMetadata.IsTextureSource)
                                    pinName = key.GetPinName(usedNames);
                                else
                                    pinName = ++_textureCount == 1 ? textureInputName : $"{textureInputName} {_textureCount}";
                                usedNames.Add(pinName);
                                _inputs.Add(new ParameterKeyPinDescription<Texture>(pinName, (ParameterKey<Texture>)key));
                            }
                            else
                            {
                                var pinName = default(string); // Using null the name is based on the parameter name
                                var isOptional = false;
                                if (key.PropertyType == typeof(SamplerState) && key.Name.StartsWith("Texturing.Sampler"))
                                {
                                    pinName = ++_samplerCount == 1 ? samplerInputName : $"{samplerInputName} {_samplerCount}";
                                    usedNames.Add(pinName);
                                    isOptional = true;
                                }

                                var pinTypeInPatch = shaderMetadata.GetPinType(key, out var boxedDefaultValue);
                                shaderMetadata.GetPinDocuAndVisibility(key, out var summary, out var remarks, out var isOptionalAttr);
                                _inputs.Add(new ParameterPinDescription(usedNames, key, parameter.Count, name: pinName, defaultValue: boxedDefaultValue, typeInPatch: pinTypeInPatch) { IsVisible = !(isOptional || isOptionalAttr), Summary = summary, Remarks = remarks });
                            }
                        }

                        IVLPinDescription _outputTextureInput, _enabledInput;

                        _inputs.Add(
                            _outputTextureInput = new PinDescription<Texture>("Output Texture")
                            {
                                Summary = "The texture to render to. If not set, the node creates its own output texture based on the input texture.",
                                Remarks = "The provided texture must be a render target.",
                                IsVisible = false
                            });
                        _inputs.Add(_enabledInput = new PinDescription<bool>("Enabled", defaultValue: true));

                        return buildContext.NewNode(
                            inputs: _inputs,
                            outputs: _outputs,
                            messages: _messages,
                            summary: shaderMetadata.Summary,
                            remarks: shaderMetadata.Remarks,
                            tags: shaderMetadata.Tags,
                            newNode: nodeBuildContext =>
                            {
                                var gameHandle = nodeBuildContext.NodeContext.GetGameHandle();
                                var effect = new TextureFXEffect("TextureFXEffect") { Name = shaderName };

                                BuildBaseMixin(shaderName, shaderMetadata, graphicsDevice, out var textureFXEffectMixin, effect.Parameters);

                                //effect.Parameters.Set
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
                                        inputs.Add(enabledInput = nodeBuildContext.Input<bool>(v => effect.Enabled = v, effect.Enabled));
                                    else if (_input is ParameterPinDescription parameterPinDescription)
                                        inputs.Add(parameterPinDescription.CreatePin(graphicsDevice, effect.Parameters));
                                    else if (_input is ParameterKeyPinDescription<Texture> textureInput)
                                    {
                                        var slot = textureCount++;
                                        inputs.Add(nodeBuildContext.Input<Texture>(setter: t =>
                                        {
                                            effect.SetInput(slot, t);
                                        }));
                                    }
                                }

                                var compositionPins = inputs.OfType<ShaderFXPin>().ToList();

                                var effectOutput = ToOutput(nodeBuildContext, effect, () =>
                                {
                                    UpdateCompositions(compositionPins, graphicsDevice, effect.Parameters, textureFXEffectMixin, effect.Subscriptions);
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
                            openEditor: () => OpenEditor(shaderName)
                        );
                    });
            }

            IVLNodeDescription NewTextureFXNode(IVLNodeDescription shaderDescription, string name, ShaderMetadata shaderMetadata)
            {
                return factory.NewNodeDescription(
                    name: name,
                    category: shaderMetadata.GetCategory("Stride.Textures"),
                    fragmented: true,
                    init: buildContext =>
                    {
                        const string Enabled = "Enabled";

                        var _inputs = shaderDescription.Inputs.ToList();

                        var isFilterOrMixer = !shaderMetadata.IsTextureSource;
                        var defaultSize = isFilterOrMixer ? Int2.Zero : new Int2(512);
                        shaderMetadata.GetPixelFormats(isFilterOrMixer, out var defaultFormat, out var defaultRenderFormat);

                        var _outputSize = new PinDescription<Int2>("Output Size", defaultSize) { IsVisible = shaderMetadata.IsTextureSource };
                        var _outputFormat = new PinDescription<PixelFormat>("Output Format", defaultFormat) { IsVisible = false };
                        var _renderFormat = new PinDescription<PixelFormat>("Render Format", defaultRenderFormat) { IsVisible = false, Summary = "Allows to specify a render format that is differet to the output format" };

                        // Wants mips?
                        var wantsMips = shaderMetadata.WantsMips?.Count > 0;
                        if (wantsMips)
                        {
                            foreach (var textureName in shaderMetadata.WantsMips)
                            {
                                var texDesc = _inputs.FirstOrDefault(p => p is ParameterKeyPinDescription<Texture> ppd && ppd.Key.GetVariableName() == textureName);
                                if (texDesc != null)
                                {
                                    var texIndex = _inputs.IndexOf(texDesc);
                                    _inputs.Insert(texIndex + 1, new PinDescription<bool>("Always Generate Mips for " + texDesc.Name, true)
                                    { 
                                        Summary = "If true, mipmaps will be generated in every frame, if false only on change of the reference. If the texture has mipmaps, nothing will be done." 
                                    });
                                }
                            }
                        }

                        if (isFilterOrMixer)
                        {
                            // Filter or Mixer
                            _inputs.Insert(_inputs.Count - 1, _outputSize);
                            _inputs.Insert(_inputs.Count - 1, _outputFormat);
                            _inputs.Insert(_inputs.Count - 1, _renderFormat);

                            // Replace Enabled with Apply
                            var _enabledPinIndex = _inputs.IndexOf(p => p.Name == Enabled);
                            if (_enabledPinIndex >= 0)
                                _inputs[_enabledPinIndex] = new PinDescription<bool>("Apply", defaultValue: true);
                        }
                        else
                        {
                            // Pure source
                            _inputs.Insert(_inputs.Count - 2, _outputSize);
                            _inputs.Insert(_inputs.Count - 2, _outputFormat);
                            _inputs.Insert(_inputs.Count - 2, _renderFormat);
                        }

                        return buildContext.NewNode(
                            inputs: _inputs,
                            outputs: new[] { buildContext.Pin("Output", typeof(Texture)) },
                            messages: shaderDescription.Messages,
                            invalidated: shaderDescription.Invalidated,
                            summary: shaderMetadata.Summary,
                            remarks: shaderMetadata.Remarks,
                            tags: shaderMetadata.Tags,
                            newNode: nodeBuildContext =>
                            {
                                var nodeContext = nodeBuildContext.NodeContext;
                                var shaderNode = shaderDescription.CreateInstance(nodeContext);
                                var inputs = shaderNode.Inputs.ToList();
                                var mipmapManager = new TextureFXMipmapManager(nodeContext);

                                var shaderNodeInputs = shaderDescription.Inputs.ToList();

                                // synchronize mipmap pins
                                if (wantsMips)
                                {
                                    foreach (var textureName in shaderMetadata.WantsMips)
                                    {
                                        var texDesc = shaderNodeInputs.FirstOrDefault(p => p is ParameterKeyPinDescription<Texture> ppd && ppd.Key.GetVariableName() == textureName);
                                        if (texDesc != null)
                                        {
                                            var texIndex = shaderNodeInputs.IndexOf(texDesc);
                                            var shaderTexturePin = inputs.ElementAtOrDefault(texIndex) as IVLPin<Texture>;
                                            if (shaderTexturePin != null)
                                            {
                                                var newTexturePin = nodeBuildContext.Input<Texture>();
                                                var alwaysGeneratePin = nodeBuildContext.Input(true);

                                                // Replace this texture input with the new one
                                                inputs[texIndex] = newTexturePin;

                                                // Setup mipmap manager
                                                mipmapManager.AddInput(newTexturePin, shaderTexturePin, alwaysGeneratePin, profilerName: name + " " + texDesc.Name + " Mipmap Generator");

                                                // Insert generate pin
                                                inputs.Insert(texIndex + 1, alwaysGeneratePin);
                                                shaderNodeInputs.Insert(texIndex + 1, new PinDescription<bool>("Always Generate Mips for " + texDesc.Name, true)); //keep shader pin indices in sync
                                            } 
                                        }
                                    }
                                }

                                var textureInput = inputs.ElementAtOrDefault(shaderNodeInputs.IndexOf(p => p.Name == textureInputName));
                                var outputTextureInput = inputs.ElementAtOrDefault(shaderNodeInputs.IndexOf(p => p.Name == "Output Texture"));
                                var enabledInput = (IVLPin<bool>)inputs.ElementAt(shaderNodeInputs.IndexOf(p => p.Name == Enabled));

                                var outputSize = nodeBuildContext.Input(defaultSize);
                                var outputFormat = nodeBuildContext.Input(defaultFormat);
                                var renderFormat = nodeBuildContext.Input(defaultRenderFormat);
                                if (isFilterOrMixer)
                                {
                                    inputs.Insert(inputs.Count - 1, outputSize);
                                    inputs.Insert(inputs.Count - 1, outputFormat);
                                    inputs.Insert(inputs.Count - 1, renderFormat);
                                }
                                else
                                {
                                    inputs.Insert(inputs.Count - 2, outputSize);
                                    inputs.Insert(inputs.Count - 2, outputFormat);
                                    inputs.Insert(inputs.Count - 2, renderFormat);
                                }

                                var gameHandle = nodeContext.GetGameHandle();
                                var game = gameHandle.Resource;
                                var scheduler = game.Services.GetService<SchedulerSystem>();
                                var graphicsDevice = game.GraphicsDevice;

                                // Remove this once FrameDelay can deal with textures properly
                                var output1 = default(((Int2 size, PixelFormat format, PixelFormat renderFormat) desc, Texture texture, Texture view));
                                var output2 = default(((Int2 size, PixelFormat format, PixelFormat renderFormat) desc, Texture texture, Texture view));
                                var mainOutput = nodeBuildContext.Output(getter: () =>
                                {
                                    var inputTexture = textureInput?.Value as Texture;

                                    if (!enabledInput.Value)
                                    {
                                        if (isFilterOrMixer)
                                            return inputTexture; // By pass
                                        else
                                            return output1.texture; // Last result
                                    }

                                    var outputTexture = outputTextureInput.Value as Texture;
                                    if (outputTexture is null)
                                    {
                                        // No output texture is provided, generate one
                                        const TextureFlags textureFlags = TextureFlags.ShaderResource | TextureFlags.RenderTarget;
                                        var desc = (size: Int2.One, format: defaultFormat, renderFormat: defaultRenderFormat);
                                        if (inputTexture != null)
                                        {
                                            // Base it on the input texture
                                            desc = (new Int2(inputTexture.ViewWidth, inputTexture.ViewHeight), inputTexture.ViewFormat, PixelFormat.None);

                                            // Watch out for feedback loops
                                            if (inputTexture == output1.texture)
                                            {
                                                Utilities.Swap(ref output1, ref output2);
                                            }
                                        }

                                        // Overwrite with user settings
                                        if (outputSize.Value.X > 0)
                                            desc.size.X = outputSize.Value.X;
                                        if (outputSize.Value.Y > 0)
                                            desc.size.Y = outputSize.Value.Y;
                                        if (outputFormat.Value != PixelFormat.None)
                                            desc.format = outputFormat.Value;
                                        if (renderFormat.Value != PixelFormat.None)
                                            desc.renderFormat = renderFormat.Value;

                                        // Ensure we have an output of proper size
                                        if (desc != output1.desc)
                                        {
                                            output1.view?.Dispose();
                                            output1.texture?.Dispose();
                                            output1.desc = desc;

                                            if (desc.format != PixelFormat.None && desc.size.X > 0 && desc.size.Y > 0)
                                            {
                                                if (desc.renderFormat != PixelFormat.None 
                                                && desc.renderFormat != desc.format 
                                                && desc.renderFormat.BlockSize() == desc.format.BlockSize()
                                                && desc.format.TryToTypeless(out var typelessFormat))
                                                {
                                                    var td = TextureDescription.New2D(desc.size.X, desc.size.Y, typelessFormat, textureFlags);
                                                    var tvd = new TextureViewDescription() { Format = desc.format, Flags = textureFlags };
                                                    var rvd = new TextureViewDescription() { Format = desc.renderFormat, Flags = textureFlags };

                                                    output1.texture = Texture.New(graphicsDevice, td, tvd);
                                                    output1.view = output1.texture.ToTextureView(rvd);
                                                }
                                                else
                                                {
                                                    output1.texture = Texture.New2D(graphicsDevice, desc.size.X, desc.size.Y, desc.format, textureFlags);
                                                    output1.view = output1.texture;
                                                }
                                            }
                                            else
                                            {
                                                output1.texture = null;
                                                output1.view = null;
                                            }
                                        }
                                    }
                                    else //output texture set by patch
                                    {
                                        output1.texture = outputTexture;
                                        output1.view = outputTexture;
                                    }

                                    var effect = shaderNode.Outputs[0].Value as TextureFXEffect;
                                    if (scheduler != null && effect != null && output1.texture != null)
                                    {
                                        effect.SetOutput(output1.view);
                                        if (wantsMips)
                                        {
                                            mipmapManager.Update();
                                            scheduler.Schedule(mipmapManager);
                                        }
                                        scheduler.Schedule(effect);
                                        return output1.texture;
                                    }

                                    return null;
                                });
                                return nodeBuildContext.Node(
                                    inputs: inputs,
                                    outputs: new[] { mainOutput },
                                    dispose: () =>
                                    {
                                        output1.view?.Dispose();
                                        output1.texture?.Dispose();
                                        output2.view?.Dispose();
                                        output2.texture?.Dispose();
                                        mipmapManager?.Dispose();
                                        gameHandle.Dispose();
                                        shaderNode.Dispose();
                                    });
                            },
                            openEditor: () => shaderDescription.OpenEditor()
                        );
                    });
            }
        }

        private static ParameterCollection BuildBaseMixin(string shaderName, ShaderMetadata shaderMetadata, GraphicsDevice graphicsDevice, out ShaderMixinSource effectInstanceMixin, ParameterCollection parameters = null)
        {
            effectInstanceMixin = new ShaderMixinSource();
            effectInstanceMixin.Mixins.Add(new ShaderClassSource(shaderName));

            var mixinParams = parameters ?? new ParameterCollection();
            mixinParams.Set(EffectNodeBaseKeys.EffectNodeBaseShader, effectInstanceMixin);

            var context = new ShaderGeneratorContext(graphicsDevice)
            {
                Parameters = mixinParams,
            };

            //add composition parameters to parameters
            if (shaderMetadata.ParsedShader != null)
            {
                foreach (var compKey in shaderMetadata.ParsedShader.CompositionsWithBaseShaders)
                {
                    var comp = compKey.Value;
                    var shaderSource = comp.GetDefaultShaderSource(context, baseKeys);
                    effectInstanceMixin.AddComposition(comp.Name, shaderSource);
                    mixinParams.Set(comp.Key, shaderSource);
                } 
            }

            return mixinParams;
        }

        //used for shader source generation
        static MaterialComputeColorKeys baseKeys = new MaterialComputeColorKeys(MaterialKeys.DiffuseMap, MaterialKeys.DiffuseValue, Color.White);

        private static bool UpdateCompositions(IReadOnlyList<ShaderFXPin> compositionPins, GraphicsDevice graphicsDevice, ParameterCollection parameters, ShaderMixinSource mixin, CompositeDisposable subscriptions)
        {
            var anyChanged = false;
            for (int i = 0; i < compositionPins.Count; i++)
            {
                anyChanged |= compositionPins[i].ShaderSourceChanged;
            }

            if (anyChanged)
            {
                // Disposes all current subscriptions. So for example all data bindings between the sources and our parameter collection
                // gets removed.
                subscriptions.Clear();

                var context = ShaderGraph.NewShaderGeneratorContext(graphicsDevice, parameters, subscriptions);

                var updatedMixin = new ShaderMixinSource();
                updatedMixin.DeepCloneFrom(mixin);
                for (int i = 0; i < compositionPins.Count; i++)
                {
                    var cp = compositionPins[i];
                    cp.GenerateAndSetShaderSource(updatedMixin, context, baseKeys);
                }
                parameters.Set(EffectNodeBaseKeys.EffectNodeBaseShader, updatedMixin);

                return true;
            }

            return false;
        }

        // For example T = SetVar<Vector3> and TInner = Vector3
        static void BuildOutput<T, TInner>(NodeBuilding.NodeInstanceBuildContext context, ShaderFXNodeState nodeState, IReadOnlyList<IVLPin> inputPins)
        {
            var compositionPins = inputPins.OfType<ShaderFXPin>().ToList();
            var inputs = inputPins.OfType<ParameterPin>().ToList();

            Func<T> getOutput = () =>
            {
                //check shader fx inputs
                var shaderChanged = nodeState.CurrentComputeNode == null;
                for (int i = 0; i < compositionPins.Count; i++)
                {
                    shaderChanged |= compositionPins[i].ShaderSourceChanged;
                    compositionPins[i].ShaderSourceChanged = false; //change seen
                }

                if (shaderChanged)
                {
                    var comps = compositionPins.Select(p => new KeyValuePair<string, IComputeNode>(p.Key.Name, p.GetValueOrDefault()));

                    var newComputeNode = new GenericComputeNode<TInner>(
                        getShaderSource: (c, k) =>
                        {
                            //let the pins subscribe to the parameter collection of the sink
                            foreach (var pin in inputs)
                                pin.SubscribeTo(c);

                            return new ShaderClassSource(nodeState.ShaderName);
                        },
                        inputs: comps);

                    nodeState.CurrentComputeNode = newComputeNode;
                    nodeState.CurrentOutputValue = ShaderFXUtils.DeclAndSetVar(nodeState.ShaderName + "Result", newComputeNode);
                }

                return (T)nodeState.CurrentOutputValue;
            };

            nodeState.OutputPin = context.Output(getOutput);
        }

        class ShaderFXNodeState
        {
            public readonly string ShaderName;
            public IVLPin OutputPin;
            public object CurrentOutputValue;
            public object CurrentComputeNode;

            public ShaderFXNodeState(string shaderName)
            {
                ShaderName = shaderName;
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
            readonly TexturingParameters[] texturingParams;

            public CustomEffect(string effectName, IServiceRegistry serviceRegistry, GraphicsDevice graphicsDevice, ParameterCollection parameters = default)
            {
                EffectInstance = new DynamicEffectInstance(effectName, parameters);
                EffectInstance.Initialize(serviceRegistry);
                EffectInstance.UpdateEffect(graphicsDevice);

                perFrameParams = EffectInstance.Parameters.GetWellKnownParameters(WellKnownParameters.PerFrameMap).ToArray();
                perViewParams = EffectInstance.Parameters.GetWellKnownParameters(WellKnownParameters.PerViewMap).ToArray();
                perDrawParams = EffectInstance.Parameters.GetWellKnownParameters(WellKnownParameters.PerDrawMap).ToArray();
                texturingParams = EffectInstance.Parameters.GetTexturingParameters().ToArray();
            }

            public ParameterCollection Parameters => EffectInstance.Parameters;

            internal readonly CompositeDisposable Subscriptions = new CompositeDisposable();

            public Action<ParameterCollection, RenderView, RenderDrawContext> ParameterSetter { get; set; }

            public void Dispose()
            {
                Subscriptions.Dispose();
                EffectInstance.Dispose();
            }

            public EffectInstance SetParameters(RenderView renderView, RenderDrawContext renderDrawContext)
            {
                EffectInstance.UpdateEffect(renderDrawContext.GraphicsDevice);

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

                    parameters.SetTexturingParameters(texturingParams);

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
