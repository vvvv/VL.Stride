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
using System.Text.RegularExpressions;
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
                                        // Copy all shaders to the project directory
                                        var assetsFolder = Path.Combine(c.DirectoryPath, "Assets");
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
            const string drawFXType = "DrawFX";
            const string computeFXType = "ComputeFX";
            const string textureFXType = "TextureFX";

            // Traverse either the "shaders" folder in the database or in the given path (if present)
            IVirtualFileProvider fileProvider = default;
            if (path != null)
                fileProvider = new FileSystemProvider(null, path);
            else
                fileProvider = contentManager.FileProvider;

            /* 
             * Blood
             * Blood_TextureFX
             * Blend_Internal_TextureFX
             * Blend_Internal_Experimental_TextureFX
             * Noise_TextureFX_Source
             * Noise_TextureFX_Source_Advanced
             */
            var regex = new Regex("^(?'name'[a-zA-Z0-9]+)(_(?'version'[a-zA-Z0-9_]+))*?_(?'type'[A-Z][a-z]+FX)(_(?'category'[a-zA-Z0-9_]+))?$");
            foreach (var file in fileProvider.ListFiles(EffectCompilerBase.DefaultSourceShaderFolder, sdslFileFilter, VirtualSearchOption.TopDirectoryOnly))
            {
                var effectName = Path.GetFileNameWithoutExtension(file);
                var match = regex.Match(effectName);
                if (!match.Success)
                    continue;

                var (type, name, subCategory) = SplitMatch(match);
                if (type == drawFXType)
                {
                    // Shader only for now
                    var shaderNodeName = new NameAndVersion($"{name.NamePart}Shader", name.VersionPart);

                    yield return NewDrawEffectShaderNode(shaderNodeName, effectName, subCategory);
                    //DrawFX node
                }
                else if (type == textureFXType)
                {
                    var shaderNodeName = new NameAndVersion($"{name.NamePart}Shader", name.VersionPart);

                    IVLNodeDescription shaderNodeDescription;
                    yield return shaderNodeDescription = NewImageEffectShaderNode(shaderNodeName, effectName);
                    yield return NewTextureFXNode(shaderNodeDescription, name, subCategory);
                }
                else if (type == computeFXType)
                {
                    // Shader only for now
                    var shaderNodeName = new NameAndVersion($"{name.NamePart}Shader", name.VersionPart);

                    yield return NewComputeEffectShaderNode(shaderNodeName, effectName);
                    //ComputeFX node
                }
            }

            static (string type, NameAndVersion name, string subCategory) SplitMatch(Match match)
            {
                var name = match.Groups["name"];
                var version = match.Groups["version"];
                var type = match.Groups["type"];
                var category = match.Groups["category"];
                var nameAndVersion = version?.Value != null ? new NameAndVersion(name.Value, version.Value.Replace('_', ' ')) : new NameAndVersion(name.Value);
                return (type.Value, nameAndVersion, category?.Value?.Replace('_', '.'));
            }

            static string GetCategory(string rootCategory, string subCategory)
            {
                if (string.IsNullOrEmpty(rootCategory))
                    return subCategory;
                if (!string.IsNullOrEmpty(subCategory))
                    return $"{rootCategory}.{subCategory}";
                return rootCategory;
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
                    if (WellKnownParameters.PerFrameMap.ContainsKey(name) 
                        || WellKnownParameters.PerViewMap.ContainsKey(name) 
                        || WellKnownParameters.TexturingMap.ContainsKey(name))
                        continue;

                    yield return parameter;
                }
            }

            IVLNodeDescription NewDrawEffectShaderNode(NameAndVersion name, string effectName, string subCategory)
            {
                return factory.NewNodeDescription(
                    name: name,
                    category: GetCategory("Stride.Rendering.DrawShaders", subCategory),
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
                                var shader = new ComputeEffectShader2(renderContext, effectName);
                                var inputs = new List<IVLPin>();
                                var enabledInput = default(IVLPin);
                                foreach (var _input in _inputs)
                                {
                                    // Handle the predefined pins first
                                    if (_input == _dispatcherInput)
                                        inputs.Add(nodeBuildContext.Input<IComputeEffectDispatcher>(setter: v => shader.Dispatcher = v));
                                    else if (_input == _threadNumbersInput)
                                        inputs.Add(nodeBuildContext.Input<Int3>(setter: v => shader.ThreadGroupSize = v));
                                    else if (_input == _enabledInput)
                                        inputs.Add(enabledInput = nodeBuildContext.Input<bool>(v => shader.Enabled = v, shader.Enabled));
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

            const string textureInputName = "Input";
            const string samplerInputName = "Sampler";

            // name = LevelsShader (ClampBoth)
            // effectName = Levels_ClampBoth_TextureFX
            // effectMainName = Levels
            IVLNodeDescription NewImageEffectShaderNode(NameAndVersion name, string effectName)
            {
                return factory.NewNodeDescription(
                    name: name,
                    category: "Stride.Rendering.ImageShaders.Experimental.Advanced",
                    fragmented: true,
                    init: buildContext =>
                    {
                        var (_effect, _messages, _invalidated) = CreateEffectInstance(effectName);

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
                        foreach (var parameter in GetParameters(_effect).OrderBy(p => p.Key.Name.Contains("Texture") ? 0 : 1))
                        {
                            var key = parameter.Key;
                            var name = key.Name;

                            // Skip the matrix transform - we're drawing fullscreen using a triangle
                            if (key == SpriteBaseKeys.MatrixTransform)
                                continue;

                            if (key.PropertyType == typeof(Texture))
                            {
                                var pinName = ++_textureCount == 1 ? textureInputName : $"{textureInputName} {_textureCount}";
                                usedNames.Add(pinName);
                                _inputs.Add(new PinDescription<Texture>(pinName));
                            }
                            else
                            {
                                var pinName = default(string); // Using null the name is based on the parameter name
                                var isVisible = true;
                                if (key.PropertyType == typeof(SamplerState))
                                {
                                    pinName = ++_samplerCount == 1 ? samplerInputName : $"{samplerInputName} {_samplerCount}";
                                    usedNames.Add(pinName);
                                    isVisible = false;
                                }
                                _inputs.Add(new ParameterPinDescription(usedNames, key, parameter.Count, name: pinName) { IsVisible = isVisible });
                            }
                        }

                        IVLPinDescription _outputTextureInput, _enabledInput;

                        _inputs.Add(
                            _outputTextureInput = new PinDescription<Texture>("Output Texture") 
                            { 
                                Summary = "The texture to render to. If not set the node creates its own output texture based on the input texture.",
                                Remarks = "The provided texture must be a render target.",
                                IsVisible = false
                            });
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
                                        inputs.Add(enabledInput = nodeBuildContext.Input<bool>(v => effect.Enabled = v, effect.Enabled));
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

            IVLNodeDescription NewTextureFXNode(IVLNodeDescription shaderDescription, string name, string subCategory)
            {
                return factory.NewNodeDescription(
                    name: name,
                    category: GetCategory("Stride.Textures", subCategory),
                    fragmented: true,
                    init: buildContext =>
                    {
                        const string Enabled = "Enabled";

                        var _inputs = shaderDescription.Inputs.ToList();

                        var hasTextureInput = _inputs.Any(p => p.Type == typeof(Texture) && p.Name != "Output Texture");

                        var defaultSize = new Int2(512);
                        var defaultFormat = PixelFormat.R8G8B8A8_UNorm;
                        if (hasTextureInput)
                        {
                            defaultSize = Int2.Zero;
                            defaultFormat = PixelFormat.None;
                        }

                        var _outputSize = new PinDescription<Int2>("Output Size", defaultSize) { IsVisible = !hasTextureInput };
                        var _outputFormat = new PinDescription<PixelFormat>("Output Format", defaultFormat) { IsVisible = !hasTextureInput };
                        if (hasTextureInput)
                        {
                            // Filter or Mixer
                            _inputs.Insert(_inputs.Count - 1, _outputSize);
                            _inputs.Insert(_inputs.Count - 1, _outputFormat);

                            // Replace Enabled with Apply
                            var _enabledPinIndex = _inputs.IndexOf(p => p.Name == Enabled);
                            if (_enabledPinIndex >= 0)
                                _inputs[_enabledPinIndex] = new PinDescription<bool>("Apply", defaultValue: true);
                        }
                        else
                        {
                            // Pure source
                            _inputs.Insert(0, _outputSize);
                            _inputs.Insert(1, _outputFormat);
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
                                var textureInput = node.Inputs.ElementAtOrDefault(shaderDescription.Inputs.IndexOf(p => p.Name == textureInputName));
                                var outputTextureInput = node.Inputs.ElementAtOrDefault(shaderDescription.Inputs.IndexOf(p => p.Name == "Output Texture"));
                                var enabledInput = (IVLPin<bool>)node.Inputs.ElementAt(shaderDescription.Inputs.IndexOf(p => p.Name == Enabled));

                                var outputSize = nodeBuildContext.Input(defaultSize);
                                var outputFormat = nodeBuildContext.Input(defaultFormat);
                                if (hasTextureInput)
                                {
                                    inputs.Insert(inputs.Count - 1, outputSize);
                                    inputs.Insert(inputs.Count - 1, outputFormat);
                                }
                                else
                                {
                                    inputs.Insert(0, outputSize);
                                    inputs.Insert(1, outputFormat);
                                }

                                var gameHandle = nodeContext.GetGameHandle();
                                var game = gameHandle.Resource;
                                var scheduler = game.Services.GetService<SchedulerSystem>();
                                var graphicsDevice = game.GraphicsDevice;
                                // Remove this once FrameDelay can deal with textures properly
                                var output1 = default(((Int2 size, PixelFormat format) desc, Texture texture));
                                var output2 = default(((Int2 size, PixelFormat format) desc, Texture texture));
                                var mainOutput = nodeBuildContext.Output<Texture>(getter: () =>
                                {
                                    var inputTexture = textureInput?.Value as Texture;

                                    if (!enabledInput.Value)
                                    {
                                        if (hasTextureInput)
                                            return inputTexture; // By pass
                                        else
                                            return output1.texture; // Last result
                                    }

                                    var outputTexture = outputTextureInput.Value as Texture;
                                    if (outputTexture is null)
                                    {
                                        // No output texture is provided, generate one
                                        const TextureFlags textureFlags = TextureFlags.ShaderResource | TextureFlags.RenderTarget;
                                        var desc = (size: Int2.One, format: PixelFormat.R8G8B8A8_UNorm);
                                        if (inputTexture != null)
                                        {
                                            // Base it on the input texture
                                            desc = (new Int2(inputTexture.Width, inputTexture.Height), inputTexture.Format);

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

                                        // Ensure we have an output of proper size
                                        if (desc != output1.desc)
                                        {
                                            output1.texture?.Dispose();
                                            output1.desc = desc;
                                            if (desc != default)
                                                output1.texture = Texture.New2D(graphicsDevice, desc.size.X, desc.size.Y, desc.format, textureFlags);
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
