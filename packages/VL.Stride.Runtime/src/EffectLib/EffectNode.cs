using System;
using System.Linq;
using VL.Core;
using VL.Stride.Rendering;
using VL.Stride.Shaders;
using Stride.Core.Mathematics;
using Stride.Graphics;
using Stride.Rendering;
using System.Collections.Immutable;

namespace VL.Stride.EffectLib
{
    class EffectNode : EffectNodeBase, IVLNode, IEffect
    {
        readonly DynamicEffectInstance instance;
        readonly GraphicsDevice graphicsDevice;
        readonly PerFrameParameters[] perFrameParams;
        readonly PerViewParameters[] perViewParams;
        readonly PerDrawParameters[] perDrawParams;
        readonly ParameterCollection parameters;
        bool pipelineStateDirty = true;
        Pin<Action<ParameterCollection, RenderView, RenderDrawContext>> customParameterSetterPin;

        public EffectNode(NodeContext nodeContext, EffectNodeDescription description) : base(nodeContext, description)
        {
            graphicsDevice = Game.GraphicsDevice;
            instance = new DynamicEffectInstance(description.EffectName);
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

            perFrameParams = parameters.GetWellKnownParameters(WellKnownParameters.PerFrameMap).ToArray();
            perViewParams = parameters.GetWellKnownParameters(WellKnownParameters.PerViewMap).ToArray();
            perDrawParams = parameters.GetWellKnownParameters(WellKnownParameters.PerDrawMap).ToArray();
            if (perDrawParams.Length > 0)
                worldPin = Inputs.OfType<ValueParameterPin<Matrix>>().FirstOrDefault(p => p.Key == TransformationKeys.World);
            if (Inputs.Length > 0)
                customParameterSetterPin = Inputs[Inputs.Length - 1] as Pin<Action<ParameterCollection, RenderView, RenderDrawContext>>;
        }

        public ImmutableArray<IVLPin> Inputs { get; }

        public ImmutableArray<IVLPin> Outputs { get; }

        public void Update()
        {
            var upstreamVersion = description.Version;
            try
            {
                if (pipelineStateDirty || (upstreamVersion > version && instance.UpdateEffect(graphicsDevice)))
                {
                    pipelineStateDirty = false;
                }
            }
            finally
            {
                version = upstreamVersion;
            }
        }

        protected override void Destroy()
        {
            instance.Dispose();
        }

        EffectInstance IEffect.SetParameters(RenderView renderView, RenderDrawContext renderDrawContext)
        {
            try
            {
                // TODO1: PerFrame could be done in Update if we'd have access to frame time
                // TODO2: This code can be optimized by using parameter accessors and not parameter keys
                parameters.SetPerFrameParameters(perFrameParams, renderDrawContext.RenderContext);

                var parentTransformation = renderDrawContext.RenderContext.Tags.Get(EntityRendererRenderFeature.CurrentParentTransformation);
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

                customParameterSetterPin?.Value?.Invoke(parameters, renderView, renderDrawContext);
            }
            catch (Exception e)
            {
                var re = new RuntimeException(e.InnermostException(), this);
                RuntimeGraph.ReportException(re);
            }
            return instance;
        }
    }
}
