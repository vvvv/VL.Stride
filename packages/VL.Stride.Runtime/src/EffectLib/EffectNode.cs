using System;
using System.Linq;
using VL.Core;
using VL.Lib.Basics.Resources;
using VL.Stride.Rendering;
using VL.Stride.Shaders;
using Stride.Core.Mathematics;
using Stride.Games;
using Stride.Graphics;
using Stride.Rendering;

namespace VL.Stride.EffectLib
{
    class EffectNode : EffectNodeBase, IVLNode, IEffect
    {
        DynamicEffectInstance instance;
        GraphicsDevice graphicsDevice;
        PerFrameParameters[] perFrameParams;
        PerViewParameters[] perViewParams;
        PerDrawParameters[] perDrawParams;
        ParameterCollection parameters;
        bool pipelineStateDirty = true;
        Pin<Action<ParameterCollection, RenderView, RenderDrawContext>> customParameterSetterPin;
        private bool initialized;

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

            //just create the pins, actual setup will be in initalize
            Inputs = description.CreateNodeInputs(this, parameters);
            Outputs = description.CreateNodeOutputs(this, parameters);
        }

        public void Initialize()
        {
            var game = Game;
            instance?.Dispose();
            instance = null;

            if (game == null)
                return;

            graphicsDevice = game.GraphicsDevice;

            instance = new DynamicEffectInstance(description.EffectName);
            try
            {
                instance.Initialize(game.Services);
                instance.UpdateEffect(graphicsDevice);
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
            if (Inputs.Length > 0)
                customParameterSetterPin = Inputs[Inputs.Length - 1] as Pin<Action<ParameterCollection, RenderView, RenderDrawContext>>;

            initialized = true;
        }

        public IVLPin[] Inputs { get; }

        public IVLPin[] Outputs { get; }

        public void Update()
        {
            if (!initialized)
            {
                Initialize();
            }

            if (instance == null)
                return;

            var upstreamVersion = description.Version;
            try
            {
                if (pipelineStateDirty || (upstreamVersion > version && instance.UpdateEffect(graphicsDevice)))
                {
                    foreach (var p in Inputs.OfType<ParameterPin>())
                        p.Update(parameters);
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
            instance?.Dispose();
        }

        EffectInstance IEffect.SetParameters(RenderView renderView, RenderDrawContext renderDrawContext)
        {
            if (instance == null)
                return instance;

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
