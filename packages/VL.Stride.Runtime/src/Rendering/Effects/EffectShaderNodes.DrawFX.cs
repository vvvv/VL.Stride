using Stride.Core;
using Stride.Graphics;
using Stride.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using VL.Core;
using VL.Model;

namespace VL.Stride.Rendering
{
    static partial class EffectShaderNodes
    {
        static IVLNodeDescription NewDrawEffectShaderNode(this IVLNodeDescriptionFactory factory, NameAndVersion name, string shaderName, ShaderMetadata shaderMetadata, IObservable<object> changes, Func<bool> openEditor, IServiceRegistry serviceRegistry, GraphicsDevice graphicsDevice)
        {
            return factory.NewNodeDescription(
                name: name,
                category: "Stride.Rendering.DrawShaders",
                fragmented: true,
                invalidated: changes,
                init: buildContext =>
                {
                    var mixinParams = BuildBaseMixin(shaderName, shaderMetadata, graphicsDevice, out var shaderMixinSource);
                    var (_effect, _messages) = CreateEffectInstance("DrawFXEffect", shaderMetadata, serviceRegistry, graphicsDevice, mixinParams, baseShaderName: shaderName);

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
                            var effect = new CustomDrawEffect("DrawFXEffect", game.Services, game.GraphicsDevice, mixinParams);

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
                        openEditor: openEditor
                    );
                });
        }
    }
}
