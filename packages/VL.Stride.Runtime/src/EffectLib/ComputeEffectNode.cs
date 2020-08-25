using System;
using System.Linq;
using VL.Core;
using VL.Stride.Games;
using VL.Stride.Rendering;
using VL.Stride.Shaders;
using Stride.Core;
using Stride.Core.Diagnostics;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.ComputeEffect;

namespace VL.Stride.EffectLib
{
    class ComputeEffectNode : EffectNodeBase, IVLNode, IGraphicsRendererBase
    {
        DynamicEffectInstance instance;
        GraphicsDevice graphicsDevice;
        PerFrameParameters[] perFrameParams;
        PerViewParameters[] perViewParams;
        PerDrawParameters[] perDrawParams;
        ParameterCollection parameters;
        Pin<Int3> dispatchCountPin, threadNumbersPin;
        Pin<int> iterationCountPin;
        Pin<Action<ParameterCollection, RenderView, RenderDrawContext>> parameterSetterPin;
        Pin<Action<ParameterCollection, RenderView, RenderDrawContext, int>> iterationParameterSetterPin;
        Pin<string> profilerNameInput;
        Pin<bool> enabledPin;
        ValueParameter<Int3> threadGroupCountAccessor;
        MutablePipelineState pipelineState;
        bool pipelineStateDirty = true;
        ProfilingKey profilingKey;
        private bool initialized;

        public ComputeEffectNode(NodeContext nodeContext, EffectNodeDescription description) : base(nodeContext, description)
        {
            graphicsDevice = Game.GraphicsDevice;
            instance = new DynamicEffectInstance("ComputeEffectShader");
            // TODO: Same code as in description
            instance.Parameters.Set(ComputeEffectShaderKeys.ComputeShaderName, description.EffectName);
            instance.Parameters.Set(ComputeEffectShaderKeys.ThreadNumbers, new Int3(1));
            try
            {
                instance.Initialize(Game.Services);
                instance.UpdateEffect(graphicsDevice);
            }
            catch (Exception e)
            {
                ReportException(e);
            }
            parameters = instance.Parameters;
            Inputs = description.CreateNodeInputs(this, graphicsDevice, parameters: parameters);
            Outputs = description.CreateNodeOutputs(this, graphicsDevice, parameters: parameters);

            profilingKey = new ProfilingKey(description.EffectName);
        }

        void Initialize()
        {
            var game = Game;
            instance?.Dispose();
            instance = null;

            if (game == null)
                return;

            instance = new DynamicEffectInstance("ComputeEffectShader");
            // TODO: Same code as in description
            instance.Parameters.Set(ComputeEffectShaderKeys.ComputeShaderName, description.EffectName);
            instance.Parameters.Set(ComputeEffectShaderKeys.ThreadNumbers, new Int3(1));
            try
            {
                instance.Initialize(game.Services);
                instance.UpdateEffect(game.GraphicsDevice);
            }
            catch (Exception e)
            {
                ReportException(e);
            }
            parameters = instance.Parameters;
            perFrameParams = parameters.GetWellKnownParameters(WellKnownParameters.PerFrameMap).ToArray();
            perViewParams = parameters.GetWellKnownParameters(WellKnownParameters.PerViewMap).ToArray();
            perDrawParams = parameters.GetWellKnownParameters(WellKnownParameters.PerDrawMap).ToArray();
            if (perDrawParams.Length > 0)
                worldPin = Inputs.OfType<ValueParameterPin<Matrix>>().FirstOrDefault(p => p.Key == TransformationKeys.World);

            Inputs.SelectPin(EffectNodeDescription.ComputeDispatchCountInput, ref dispatchCountPin);
            Inputs.SelectPin(EffectNodeDescription.ComputeThreadNumbersInput, ref threadNumbersPin);
            Inputs.SelectPin(EffectNodeDescription.ComputeIterationCountInput, ref iterationCountPin);
            Inputs.SelectPin(EffectNodeDescription.ParameterSetterInput, ref parameterSetterPin);
            Inputs.SelectPin(EffectNodeDescription.ComputeIterationParameterSetterInput, ref iterationParameterSetterPin);
            Inputs.SelectPin(EffectNodeDescription.ProfilerNameInput, ref profilerNameInput);
            Inputs.SelectPin(EffectNodeDescription.ComputeEnabledInput, ref enabledPin);

            initialized = true;
        }

        public IVLPin[] Inputs { get; }

        public IVLPin[] Outputs { get; }

        public void Update()
        {
            if (!initialized)
                Initialize();

            if (!initialized)
                return;

            var profilerName = profilerNameInput.Value;

            if (string.IsNullOrWhiteSpace(profilerName))
                profilerName = description.EffectName;

            if (profilingKey.Name != profilerName)
                profilingKey = new ProfilingKey(profilerName);
        }

        protected override void Destroy()
        {
            instance.Dispose();
        }

        void IGraphicsRendererBase.Draw(RenderDrawContext context)
        {
            if (!initialized)
                return;

            using (context.QueryManager.BeginProfile(Color.LightGreen, profilingKey))
            {
                if (!enabledPin.Value || description.HasCompilerErrors)
                    return;

                try
                {
                    var renderContext = context.RenderContext;
                    var renderView = renderContext.RenderView;
                    var commandList = context.CommandList;
                    var pipelineState = this.pipelineState ?? (this.pipelineState = new MutablePipelineState(renderContext.GraphicsDevice));
                    var permutationCounter = parameters.PermutationCounter;

                    // TODO1: PerFrame could be done in Update if we'd have access to frame time
                    // TODO2: This code can be optimized by using parameter accessors and not parameter keys

                    parameters.SetPerFrameParameters(perFrameParams, context.RenderContext);

                    var parentTransformation = renderContext.Tags.Get(EntityRendererRenderFeature.CurrentParentTransformation);
                    if (worldPin != null)
                    {
                        var world = worldPin.Value;
                        Matrix.Multiply(ref world, ref parentTransformation, out var result);
                        worldPin.Value = result;
                        parameters.SetPerDrawParameters(perDrawParams, renderView, ref result);
                    }
                    else
                    {
                        parameters.SetPerDrawParameters(perDrawParams, renderView, ref parentTransformation);
                    }

                    parameters.SetPerViewParameters(perViewParams, renderView);

                    // Set permutation parameters before updating the effect (needed by compiler)
                    parameters.Set(ComputeEffectShaderKeys.ThreadNumbers, threadNumbersPin.Value);

                    // Give user chance to override
                    parameterSetterPin?.Value.Invoke(parameters, renderView, context);

                    var upstreamVersion = description.Version;
                    try
                    {
                        if (upstreamVersion > version && instance.UpdateEffect(renderContext.GraphicsDevice))
                        {
                            threadGroupCountAccessor = parameters.GetAccessor(ComputeShaderBaseKeys.ThreadGroupCountGlobal);
                            pipelineStateDirty = true;
                        }
                    }
                    finally
                    {
                        version = upstreamVersion;
                    }

                    if (pipelineStateDirty || permutationCounter != parameters.PermutationCounter)
                    {
                        instance.UpdateEffect(renderContext.GraphicsDevice);
                        threadGroupCountAccessor = parameters.GetAccessor(ComputeShaderBaseKeys.ThreadGroupCountGlobal);
                        pipelineState.State.SetDefaults();
                        pipelineState.State.RootSignature = instance.RootSignature;
                        pipelineState.State.EffectBytecode = instance.Effect.Bytecode;
                        pipelineState.Update();
                        pipelineStateDirty = false;
                    }

                    // Apply pipeline state
                    commandList.SetPipelineState(pipelineState.CurrentState);

                    // Set thread group count as provided by input pin
                    var threadGroupCount = dispatchCountPin.Value;
                    parameters.Set(ComputeShaderBaseKeys.ThreadGroupCountGlobal, threadGroupCount);

                    // TODO: This can be optimized by uploading only parameters from the PerDispatch groups - look in Xenkos RootEffectRenderFeature
                    var iterationCount = Math.Max(iterationCountPin.Value, 1);
                    for (int i = 0; i < iterationCount; i++)
                    {
                        // Give user chance to override
                        iterationParameterSetterPin.Value?.Invoke(parameters, renderView, context, i);

                        // The thread group count can be set for each dispatch
                        threadGroupCount = parameters.Get(threadGroupCountAccessor);

                        // Upload the parameters
                        instance.Apply(context.GraphicsContext);

                        // Draw a full screen quad
                        commandList.Dispatch(threadGroupCount.X, threadGroupCount.Y, threadGroupCount.Z);
                    }
                }
                catch (Exception e)
                {
                    ReportException(e);
                } 
            }
        }
    }
}
