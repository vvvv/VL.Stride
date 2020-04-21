using System;
using System.Linq;
using VL.Core;
using VL.Xenko.Games;
using VL.Xenko.Rendering;
using VL.Xenko.Shaders;
using Xenko.Core;
using Xenko.Core.Diagnostics;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Graphics;
using Xenko.Rendering;
using Xenko.Rendering.ComputeEffect;

namespace VL.Xenko.EffectLib
{
    class ComputeEffectNode : EffectNodeBase, IVLNode, ILowLevelAPIRender
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
            instance.Parameters.Set(ComputeEffectShaderKeys.ComputeShaderName, description.Name);
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
            Inputs = description.CreateNodeInputs(this, parameters);
            Outputs = description.CreateNodeOutputs(this, parameters);

            profilingKey = new ProfilingKey(description.Name);
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
            instance.Parameters.Set(ComputeEffectShaderKeys.ComputeShaderName, description.Name);
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
            Inputs.OfType<ParameterPin>().Do(p => p.Update(parameters));
            Outputs.OfType<ParameterPin>().Do(p => p.Update(parameters));
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
                profilerName = description.Name;

            if (profilingKey.Name != profilerName)
                profilingKey = new ProfilingKey(profilerName);
        }

        protected override void Destroy()
        {
            instance.Dispose();
        }

        void ILowLevelAPIRender.Initialize()
        {
        }

        void ILowLevelAPIRender.Collect(RenderContext context)
        {
        }

        void ILowLevelAPIRender.Extract()
        {
        }

        void ILowLevelAPIRender.Prepare(RenderDrawContext context)
        {
        }

        void ILowLevelAPIRender.Draw(RenderContext renderContext, RenderDrawContext drawContext, RenderView renderView, RenderViewStage renderViewStage, CommandList commandList)
        {
            if (!initialized)
                return;

            using (drawContext.QueryManager.BeginProfile(Color.LightGreen, profilingKey))
            {
                if (!enabledPin.Value || description.HasCompilerErrors)
                    return;

                try
                {
                    var pipelineState = this.pipelineState ?? (this.pipelineState = new MutablePipelineState(renderContext.GraphicsDevice));
                    var permutationCounter = parameters.PermutationCounter;

                    // TODO1: PerFrame could be done in Update if we'd have access to frame time
                    // TODO2: This code can be optimized by using parameter accessors and not parameter keys

                    parameters.SetPerFrameParameters(perFrameParams, drawContext.RenderContext);
                    var world = ComputeWorldMatrix();
                    parameters.SetPerDrawParameters(perDrawParams, renderView, ref world);
                    parameters.SetPerViewParameters(perViewParams, renderView);

                    // Set permutation parameters before updating the effect (needed by compiler)
                    parameters.Set(ComputeEffectShaderKeys.ThreadNumbers, threadNumbersPin.Value);

                    // Give user chance to override
                    parameterSetterPin?.Value.Invoke(parameters, renderView, drawContext);

                    var upstreamVersion = description.Version;
                    try
                    {
                        if (upstreamVersion > version && instance.UpdateEffect(renderContext.GraphicsDevice))
                        {
                            threadGroupCountAccessor = parameters.GetAccessor(ComputeShaderBaseKeys.ThreadGroupCountGlobal);
                            foreach (var p in Inputs.OfType<ParameterPin>())
                                p.Update(parameters);
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
                        foreach (var p in Inputs.OfType<ParameterPin>())
                            p.Update(parameters);
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
                        iterationParameterSetterPin.Value?.Invoke(parameters, renderView, drawContext, i);

                        // The thread group count can be set for each dispatch
                        threadGroupCount = parameters.Get(threadGroupCountAccessor);

                        // Upload the parameters
                        instance.Apply(drawContext.GraphicsContext);

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
