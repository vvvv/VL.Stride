using System;
using System.Linq;
using VL.Core;
using VL.Xenko.Rendering;
using VL.Xenko.Shaders;
using Xenko.Core.Mathematics;
using Xenko.Graphics;
using Xenko.Rendering;
using Xenko.Rendering.ComputeEffect;

namespace VL.Xenko.EffectLib
{
    class ComputeEffectNode : EffectNodeBase, IVLNode, ILowLevelAPIRender
    {
        readonly DynamicEffectInstance instance;
        readonly GraphicsDevice graphicsDevice;
        readonly PerFrameParameters[] perFrameParams;
        readonly PerViewParameters[] perViewParams;
        readonly PerDrawParameters[] perDrawParams;
        readonly ParameterCollection parameters;
        ConvertedValueParameterPin<Matrix, SharpDX.Matrix> worldPin;
        Pin<Int3> dispatchCountPin, threadNumbersPin;
        Pin<int> iterationCountPin;
        Pin<Action<ParameterCollection, RenderView, RenderDrawContext>> parameterSetterPin;
        Pin<Action<ParameterCollection, RenderView, RenderDrawContext, int>> iterationParameterSetterPin;
        Pin<bool> enabledPin;
        ValueParameter<Int3> threadGroupCountAccessor;
        MutablePipelineState pipelineState;
        bool pipelineStateDirty = true;

        public ComputeEffectNode(NodeContext nodeContext, EffectNodeDescription description) : base(nodeContext, description)
        {
            graphicsDevice = description.Factory.DeviceManager.GraphicsDevice;
            instance = new DynamicEffectInstance("ComputeEffectShader");
            // TODO: Same code as in description
            instance.Parameters.Set(ComputeEffectShaderKeys.ComputeShaderName, description.Name);
            instance.Parameters.Set(ComputeEffectShaderKeys.ThreadNumbers, new Int3(1));
            try
            {
                instance.Initialize(description.Factory.ServiceRegistry);
                instance.UpdateEffect(graphicsDevice);
            }
            catch (Exception e)
            {
                ReportException(e);
            }
            parameters = instance.Parameters;
            Inputs = description.CreateNodeInputs(this, parameters);
            Outputs = description.CreateNodeOutputs(this, parameters);
            perFrameParams = parameters.GetWellKnownParameters(WellKnownParameters.PerFrameMap).ToArray();
            perViewParams = parameters.GetWellKnownParameters(WellKnownParameters.PerViewMap).ToArray();
            perDrawParams = parameters.GetWellKnownParameters(WellKnownParameters.PerDrawMap).ToArray();
            if (perDrawParams.Length > 0)
                worldPin = Inputs.OfType<ConvertedValueParameterPin<Matrix, SharpDX.Matrix>>().FirstOrDefault(p => p.Key == TransformationKeys.World);

            Inputs.SelectPin(EffectNodeDescription.ComputeDispatchCountInput, ref dispatchCountPin);
            Inputs.SelectPin(EffectNodeDescription.ComputeThreadNumbersInput, ref threadNumbersPin);
            Inputs.SelectPin(EffectNodeDescription.ComputeIterationCountInput, ref iterationCountPin);
            Inputs.SelectPin(EffectNodeDescription.ParameterSetterInput, ref parameterSetterPin);
            Inputs.SelectPin(EffectNodeDescription.ComputeIterationParameterSetterInput, ref iterationParameterSetterPin);
            Inputs.SelectPin(EffectNodeDescription.ComputeEnabledInput, ref enabledPin);
        }

        public IVLPin[] Inputs { get; }

        public IVLPin[] Outputs { get; }

        public void Update()
        {
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
            if (!enabledPin.Value || description.HasCompilerErrors)
                return;

            try
            {
                var pipelineState = this.pipelineState ?? (this.pipelineState = new MutablePipelineState(renderContext.GraphicsDevice));

                // TODO1: PerFrame could be done in Update if we'd have access to frame time
                // TODO2: This code can be optimized by using parameter accessors and not parameter keys
                parameters.SetPerFrameParameters(perFrameParams, drawContext.RenderContext);
                parameters.SetPerViewParameters(perViewParams, renderView);

                if (worldPin != null)
                {
                    var world = worldPin.ShaderValue;
                    parameters.SetPerDrawParameters(perDrawParams, renderView, ref world);
                }

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

                if (pipelineStateDirty)
                {
                    instance.UpdateEffect(renderContext.GraphicsDevice);
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
