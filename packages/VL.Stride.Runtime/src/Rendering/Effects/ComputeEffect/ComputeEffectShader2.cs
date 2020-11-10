using Stride.Core.Diagnostics;
using Stride.Core.Mathematics;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.ComputeEffect;
using Stride.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using VL.Lib.Control;

namespace VL.Stride.Rendering.ComputeEffect
{
    /// <summary>
    /// A compute effect allowing to customize the dispatch method through <see cref="IComputeEffectDispatcher"/>.
    /// </summary>
    class ComputeEffectShader2 : DrawEffect
    {
        PerFrameParameters[] perFrameParams;
        PerViewParameters[] perViewParams;
        PerDrawParameters[] perDrawParams;
        static Dictionary<string, ProfilingKey> profilingKeys = new Dictionary<string, ProfilingKey>();
        ProfilingKey profilingKey;

        private MutablePipelineState pipelineState;
        private bool pipelineStateDirty = true;
        private EffectBytecode previousBytecode;
        private TimeSpan FRefreshTime;
        private bool FCompiled;

        public string LastError { get; private set; }

        public ComputeEffectShader2(RenderContext context, string name)
            : base(context, name)
        {
        }

        /// <summary>
        /// The current effect instance.
        /// </summary>
        public DynamicEffectInstance EffectInstance { get; private set; }

        /// <summary>
        /// Gets or sets the number of threads desired by thread group.
        /// </summary>
        public Int3 ThreadNumbers { get; set; }

        /// <summary>
        /// Gets or sets the dispatcher.
        /// </summary>
        public IComputeEffectDispatcher Dispatcher { get; set; }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            pipelineState = new MutablePipelineState(Context.GraphicsDevice);

            // Setup the effect compiler
            EffectInstance = new DynamicEffectInstance("ComputeEffectShader", Parameters);

            // We give ComputeEffectShader a higher priority, since they are usually executed serially and blocking
            EffectInstance.EffectCompilerParameters.TaskPriority = -1;

            Parameters.Set(ComputeEffectShaderKeys.ComputeShaderName, Name);
            Parameters.Set(ComputeEffectShaderKeys.ThreadNumbers, new Int3(1));

            EffectInstance.Initialize(Context.Services);
            EffectInstance.UpdateEffect(Context.GraphicsDevice);

            perFrameParams = EffectInstance.Parameters.GetWellKnownParameters(WellKnownParameters.PerFrameMap).ToArray();
            perViewParams = EffectInstance.Parameters.GetWellKnownParameters(WellKnownParameters.PerViewMap).ToArray();
            perDrawParams = EffectInstance.Parameters.GetWellKnownParameters(WellKnownParameters.PerDrawMap).ToArray();

            if (!profilingKeys.TryGetValue(Name, out profilingKey))
            {
                profilingKey = new ProfilingKey(Name);
                profilingKeys[Name] = profilingKey;
            }
            
        }

        /// <summary>
        /// Sets the default parameters (called at constructor time and if <see cref="DrawEffect.Reset"/> is called)
        /// </summary>
        protected override void SetDefaultParameters()
        {
            ThreadNumbers = new Int3(1);
        }

        protected override void PreDrawCore(RenderDrawContext context)
        {
            base.PreDrawCore(context);

            // Default handler for parameters
            UpdateParameters(context);
        }

        /// <summary>
        /// Updates the effect <see cref="DrawEffect.Parameters"/> from properties defined in this instance.
        /// </summary>
        protected virtual void UpdateParameters(RenderDrawContext context)
        {
            Parameters.Set(ComputeEffectShaderKeys.ThreadNumbers, ThreadNumbers);

            Parameters.SetPerFrameParameters(perFrameParams, context.RenderContext);

            var renderView = context.RenderContext.RenderView;
            var parentTransformation = context.RenderContext.Tags.Get(EntityRendererRenderFeature.CurrentParentTransformation);
            if (Parameters.ContainsKey(TransformationKeys.World))
            {
                var world = Parameters.Get(TransformationKeys.World);
                Matrix.Multiply(ref world, ref parentTransformation, out var result);
                Parameters.SetPerDrawParameters(perDrawParams, renderView, ref result);
            }
            else
            {
                Parameters.SetPerDrawParameters(perDrawParams, renderView, ref parentTransformation);
            }

            Parameters.SetPerViewParameters(perViewParams, renderView);

            Dispatcher?.UpdateParameters(Parameters, ThreadNumbers);
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            if (string.IsNullOrEmpty(Name) || FRefreshTime > context.RenderContext.Time.Total)
                return;

            

            using (Profiler.Begin(profilingKey))
            using (context.QueryManager.BeginProfile(Color.Green, profilingKey))
            {
                var effectUpdated = false;
                try
                {
                    effectUpdated = EffectInstance.UpdateEffect(GraphicsDevice);
                    FCompiled = true;
                    LastError = string.Empty;

                }
                catch (Exception e)
                {
                    LastError = e.InnermostException().Message;
                    FCompiled = false;
                    FRefreshTime = context.RenderContext.Time.Total + TimeSpan.FromSeconds(3);
                }

                if (!FCompiled)
                    return;

                try
                {
                    if (effectUpdated || pipelineStateDirty || previousBytecode != EffectInstance.Effect.Bytecode)
                    {
                        // The EffectInstance might have been updated from outside
                        previousBytecode = EffectInstance.Effect.Bytecode;

                        pipelineState.State.SetDefaults();
                        pipelineState.State.RootSignature = EffectInstance.RootSignature;
                        pipelineState.State.EffectBytecode = EffectInstance.Effect.Bytecode;
                        pipelineState.Update();
                        pipelineStateDirty = false;
                    }
                }
                catch (Exception e)
                {

                    LastError = e.InnermostException().Message;
                    FCompiled = false;
                    FRefreshTime = context.RenderContext.Time.Total + TimeSpan.FromSeconds(3);
                    return;
                }

                // Apply pipeline state
                context.CommandList.SetPipelineState(pipelineState.CurrentState);

                // Apply the effect
                EffectInstance.Apply(context.GraphicsContext);

                // Dispatch
                Dispatcher?.Dispatch(context);

                // Un-apply
                //throw new InvalidOperationException();
                //EffectInstance.Effect.UnbindResources(GraphicsDevice);
            }
        }
    }
}
