using Stride.Core.Extensions;
using Stride.Core.IO;
using Stride.Core.Serialization.Contents;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Images;
using Stride.Shaders;
using Stride.Shaders.Compiler;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using VL.Core;
using VL.Core.Diagnostics;
using VL.Model;
using VL.Stride.Core;
using VL.Stride.Engine;
using VL.Stride.Rendering;

[assembly:NodeFactory(typeof(VL.Stride.EffectLib.TextureFXNodeFactory))]

namespace VL.Stride.EffectLib
{
    public class TextureFXNodeFactory : IVLNodeDescriptionFactory
    {
        readonly DirectoryWatcher directoryWatcher;

        public TextureFXNodeFactory()
        {
            var services = SharedServices.GetRegistry();
            var effectSystem = services.GetService<EffectSystem>();

            // Ensure the effect system tracks the same files as we do
            var timer = default(Timer);
            var fieldInfo = typeof(EffectSystem).GetField("directoryWatcher", BindingFlags.NonPublic | BindingFlags.Instance);
            directoryWatcher = fieldInfo.GetValue(effectSystem) as DirectoryWatcher;
            directoryWatcher.Modified += (s, e) =>
            {
                if (e.ChangeType == FileEventChangeType.Changed || e.ChangeType == FileEventChangeType.Renamed)
                {
                    timer?.Dispose();
                    timer = new Timer(_ => ReloadNodeDescriptions(), null, 50, Timeout.Infinite);
                }
            };
        }

        public ImmutableArray<IVLNodeDescription> NodeDescriptions
        {
            get
            {
                if (nodeDescriptions.IsDefault)
                    nodeDescriptions = GetNodeDescriptions(this).ToImmutableArray();
                return nodeDescriptions;
            }
        }
        ImmutableArray<IVLNodeDescription> nodeDescriptions;

        public event PropertyChangedEventHandler PropertyChanged;

        void ReloadNodeDescriptions()
        {
            // Check if someone is even interested
            if (nodeDescriptions.IsDefault)
                return;

            var newDescriptions = GetNodeDescriptions(this).ToImmutableArray();
            if (!newDescriptions.SequenceEqual(nodeDescriptions, NodeDescriptionComparer.Default))
            {
                nodeDescriptions = newDescriptions;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NodeDescriptions)));
            }
        }

        static IEnumerable<IVLNodeDescription> GetNodeDescriptions(IVLNodeDescriptionFactory factory)
        {
            var serviceRegistry = SharedServices.GetRegistry();
            var graphicsDeviceService = serviceRegistry.GetService<IGraphicsDeviceService>();
            var graphicsDevice = graphicsDeviceService.GraphicsDevice;
            var contentManager = serviceRegistry.GetService<ContentManager>();
            var effectSystem = serviceRegistry.GetService<EffectSystem>();

            // Effect system deals with its internal cache on update, so make sure its called.
            effectSystem.Update(default);

            var compiler = effectSystem.Compiler;

            const string sdslFileFilter = "*.sdsl";
            const string suffix = "_TextureFX";

            var files = contentManager.FileProvider.ListFiles(EffectCompilerBase.DefaultSourceShaderFolder, sdslFileFilter, VirtualSearchOption.AllDirectories);
            foreach (var file in files)
            {
                var effectName = Path.GetFileNameWithoutExtension(file);
                if (effectName.EndsWith(suffix))
                {
                    var name = GetNodeName(effectName);
                    var shaderNodeName = new NameAndVersion($"{name.NamePart}Shader", name.VersionPart);

                    IVLNodeDescription shaderNodeDescription;
                    yield return shaderNodeDescription = NewImageEffectShaderNode(shaderNodeName, effectName, name.NamePart);
                    yield return NewImageEffectNode(shaderNodeDescription, name);
                }
            }

            static NameAndVersion GetNodeName(string effectName)
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
                var fileProvider = contentManager.FileProvider;
                using (var pathStream = fileProvider.OpenStream(EffectCompilerBase.GetStoragePathFromShaderType(effectName) + "/path", VirtualFileMode.Open, VirtualFileAccess.Read))
                using (var reader = new StreamReader(pathStream))
                {
                    return reader.ReadToEnd();
                }
            }

            // name = LevelsShader (ClampBoth)
            // effectName = Levels_ClampBoth_TextureFX
            // effectMainName = Levels
            IVLNodeDescription NewImageEffectShaderNode(NameAndVersion name, string effectName, string effectMainName)
            {
                return new DelegateNodeDescription(
                    factory: factory,
                    name: name,
                    category: "Stride.ImageShaders",
                    fragmented: true,
                    init: self =>
                    {
                        var _inputs = new List<IVLPinDescription>();
                        var _outputs = new List<IVLPinDescription>() { DelegatePinDescription.New<ImageEffectShader>("Output") };
                        var _messages = ImmutableArray<Message>.Empty;

                        using var _effect = new DynamicEffectInstance(effectName);
                        try
                        {
                            _effect.Initialize(serviceRegistry);
                            _effect.UpdateEffect(graphicsDevice);
                        }
                        catch (InvalidOperationException)
                        {
                            try
                            {
                                // Compile manually to get detailed errors
                                var compilerResult = compiler.Compile(
                                    shaderSource: new ShaderClassSource(effectName),
                                    compilerParameters: new CompilerParameters() { EffectParameters = _effect.EffectCompilerParameters });
                                _messages = compilerResult.Messages.Select(m => m.ToMessage()).ToImmutableArray();
                            }
                            catch (Exception e)
                            {
                                _messages = _messages.Add(new Message(MessageType.Error, $"Shader compiler crashed: {e}"));
                            }
                        }

                        var byteCode = _effect.Effect?.Bytecode;
                        if (byteCode != null)
                        {
                            var layoutNames = byteCode.Reflection.ResourceBindings.Select(x => x.ResourceGroup ?? "Globals").Distinct().ToList();

                            var usedNames = new HashSet<string>()
                            {
                                "Output Texture",
                                "Enabled"
                            };

                            var _textureCount = 0;
                            var parameters = _effect.Parameters;
                            foreach (var parameter in parameters.Layout.LayoutParameterKeyInfos.OrderBy(p => p.Key.Name.Contains("Texture") ? 0 : 1))
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

                                // Skip unbound parameters
                                if (parameter.BindingSlot < 0 && !name.StartsWith(effectMainName))
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
                        }

                        IVLPinDescription _outputTextureInput, _enabledInput;
                        _inputs.Add(_outputTextureInput = new PinDescription<Texture>("Output Texture"));
                        _inputs.Add(_enabledInput = new PinDescription<bool>("Enabled", defaultValue: true));

                        return (
                            inputs: _inputs,
                            outputs: _outputs,
                            messages: _messages,
                            createInstance: nodeContext =>
                            {
                                var effect = new TextureFXEffect(effectName);
                                var inputs = new List<IVLPin>();
                                var enabledInput = default(DelegatePin<bool>);
                                var textureCount = 0;
                                foreach (var _input in _inputs)
                                {
                                    // Handle the predefined pins first
                                    if (_input == _outputTextureInput)
                                    {
                                        inputs.Add(new DelegatePin<Texture>(setter: t =>
                                        {
                                            if (t != null)
                                                effect.SetOutput(t);
                                        }));
                                    }
                                    else if (_input == _enabledInput)
                                        inputs.Add(enabledInput = new DelegatePin<bool>(() => effect.Enabled, v => effect.Enabled = v));
                                    else if (_input is ParameterPinDescription parameterPinDescription)
                                        inputs.Add(parameterPinDescription.CreatePin(graphicsDevice, effect.Parameters));
                                    else if (_input is PinDescription<Texture> textureInput)
                                    {
                                        var slot = textureCount++;
                                        inputs.Add(new DelegatePin<Texture>(setter: t =>
                                        {
                                            effect.SetInput(slot, t);
                                        }));
                                    }
                                }

                                var effectOutput = ToOutput(effect, () =>
                                {
                                    //effect.Enabled = enabledInput.Value && effect.IsInputAssigned && effect.IsOutputAssigned;
                                });
                                return new DelegateNode(
                                    nodeContext: nodeContext,
                                    nodeDescription: self,
                                    inputs: inputs.ToArray(),
                                    outputs: new[] { effectOutput },
                                    update: default,
                                    dispose: () => effect.Dispose());
                            },
                            openEditor: () =>
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
                        );
                    });
            }

            IVLNodeDescription NewImageEffectNode(IVLNodeDescription shaderDescription, string name)
            {
                return new DelegateNodeDescription(
                    factory: shaderDescription.Factory,
                    name: name,
                    category: "Stride.TextureFX",
                    fragmented: true,
                    init: self =>
                    {
                        return (
                            inputs: shaderDescription.Inputs,
                            outputs: new[] { new PinDescription<Texture>("Output") },
                            messages: shaderDescription.Messages.ToList(),
                            createInstance: nodeContext =>
                            {
                                var node = shaderDescription.CreateInstance(nodeContext);
                                var textureInput = node.Inputs.ElementAtOrDefault(shaderDescription.Inputs.IndexOf(p => p.Name == "Texture"));
                                var outputTextureInput = node.Inputs.ElementAtOrDefault(shaderDescription.Inputs.IndexOf(p => p.Name == "Output Texture"));
                                var gameHandle = nodeContext.GetGameHandle();
                                var game = gameHandle.Resource;
                                var graphicsDevice = game.GraphicsDevice;
                                var current = default((TextureDescription inputDesc, Texture outputTexture));
                                var mainOutput = new DelegatePin<Texture>(getter: () =>
                                {
                                    var inputTexture = textureInput?.Value as Texture;
                                    var outputTexture = outputTextureInput.Value as Texture;
                                    if (inputTexture != null && outputTexture is null)
                                    {
                                        var desc = inputTexture.Description;
                                        if (desc != current.inputDesc)
                                        {
                                            current.outputTexture?.Dispose();
                                            current.inputDesc = desc;
                                            current.outputTexture = Texture.New2D(graphicsDevice, desc.Width, desc.Height, desc.Format, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
                                        }
                                        outputTexture = current.outputTexture;
                                    }

                                    // Set the final output texture
                                    outputTextureInput.Value = outputTexture;

                                    var effect = node.Outputs[0].Value as TextureFXEffect;
                                    var scheduler = game.Services.GetService<SchedulerSystem>();
                                    if (scheduler != null && effect != null && effect.IsOutputAssigned)
                                    {
                                        scheduler.Schedule(effect);
                                    }

                                    return outputTexture;
                                });
                                return new DelegateNode(
                                    nodeContext: nodeContext,
                                    nodeDescription: self,
                                    inputs: node.Inputs,
                                    outputs: new[] { mainOutput },
                                    dispose: () =>
                                    {
                                        current.outputTexture?.Dispose();
                                        gameHandle.Dispose();
                                        node.Dispose();
                                    });
                            },
                            openEditor: () => shaderDescription.OpenEditor()
                        );
                    });
            }
        }

        static DelegatePin<T> ToOutput<T>(T value, Action getter)
        {
            return new DelegatePin<T>(() =>
            {
                getter();
                return value;
            });
        }

        class DelegateNodeDescription : IVLNodeDescription
        {
            readonly Lazy<(IReadOnlyList<IVLPinDescription> inputs, IReadOnlyList<IVLPinDescription> outputs, IReadOnlyList<Message> messages, Func<NodeContext, IVLNode> createInstance, Func<bool> openEditor)> init;

            public DelegateNodeDescription(
                IVLNodeDescriptionFactory factory,
                string name,
                string category,
                bool fragmented,
                Func<IVLNodeDescription, (IReadOnlyList<IVLPinDescription> inputs, IReadOnlyList<IVLPinDescription> outputs, IReadOnlyList<Message> messages, Func<NodeContext, IVLNode> createInstance, Func<bool> openEditor)> init)
            {
                Name = name;
                Category = category;
                Fragmented = fragmented;
                Factory = factory;
                this.init = new Lazy<(IReadOnlyList<IVLPinDescription> inputs, IReadOnlyList<IVLPinDescription> outputs, IReadOnlyList<Message> messages, Func<NodeContext, IVLNode> createInstance, Func<bool> openEditor)>(() => init(this), LazyThreadSafetyMode.ExecutionAndPublication);
            }

            public IVLNodeDescriptionFactory Factory { get; }

            public string Name { get; }

            public string Category { get; }

            public bool Fragmented { get; }

            public IReadOnlyList<IVLPinDescription> Inputs => init.Value.inputs;

            public IReadOnlyList<IVLPinDescription> Outputs => init.Value.outputs;

            public IEnumerable<Message> Messages => init.Value.messages;

            public IVLNode CreateInstance(NodeContext context) => init.Value.createInstance(context);

            public bool OpenEditor() => init.Value.openEditor?.Invoke() ?? false;
        }

        class DelegatePinDescription : IVLPinDescription
        {
            public static DelegatePinDescription New<T>(string name, T defaultValue = default)
            {
                return new DelegatePinDescription(name, typeof(T), defaultValue);
            }

            public DelegatePinDescription(string name, Type type, object defaultValue)
            {
                Name = name;
                Type = type;
                DefaultValue = defaultValue;
            }

            public string Name { get; }

            public Type Type { get; }

            public object DefaultValue { get; }
        }

        class DelegateNode : VLObject, IVLNode
        {
            readonly Action update, dispose;

            public DelegateNode(NodeContext nodeContext, IVLNodeDescription nodeDescription, IVLPin[] inputs, IVLPin[] outputs, Action update = default, Action dispose = default)
                : base(nodeContext)
            {
                this.NodeDescription = nodeDescription;
                this.Inputs = inputs;
                this.Outputs = outputs;
                this.update = update;
                this.dispose = dispose;
            }

            public IVLNodeDescription NodeDescription { get; }

            public IVLPin[] Inputs { get; }

            public IVLPin[] Outputs { get; }

            public void Update() => update?.Invoke();

            public void Dispose() => dispose?.Invoke();
        }

        class Pin<T> : IVLPin
        {
            public Pin(T defaultValue)
            {
                Value = defaultValue;
            }

            public T Value { get; set; }
            object IVLPin.Value { get => Value; set => Value = (T)value; }
        }

        class DelegatePin : IVLPin
        {
            readonly Func<object> getter;
            readonly Action<object> setter;

            public DelegatePin(Func<object> getter, Action<object> setter)
            {
                this.getter = getter;
                this.setter = setter;
            }

            public object Value { get => getter(); set => setter(value); }
        }

        class DelegatePin<T> : IVLPin
        {
            readonly Func<T> getter;
            readonly Action<T> setter;
            T value;

            public DelegatePin(Func<T> getter = default, Action<T> setter = default)
            {
                this.getter = getter;
                this.setter = setter;
            }

            public T Value 
            {
                get => getter != null ? getter.Invoke() : value;
                set 
                { 
                    this.value = value; 
                    setter?.Invoke(value); 
                }
            }

            object IVLPin.Value { get => Value; set => Value = (T)value; }
        }
    }
}
