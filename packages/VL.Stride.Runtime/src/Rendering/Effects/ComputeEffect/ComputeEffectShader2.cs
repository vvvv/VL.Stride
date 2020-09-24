using Stride.Core.Mathematics;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.ComputeEffect;
using Stride.Shaders;

namespace VL.Stride.Rendering.ComputeEffect
{
    /// <summary>
    /// A compute effect allowing to customize the dispatch method through <see cref="IComputeEffectDispatcher"/>.
    /// </summary>
    class ComputeEffectShader2 : DrawEffect
    {
        private MutablePipelineState pipelineState;
        private bool pipelineStateDirty = true;
        private EffectBytecode previousBytecode;

        public ComputeEffectShader2(RenderContext context)
            : base(context, null)
        {
            pipelineState = new MutablePipelineState(context.GraphicsDevice);

            // Setup the effect compiler
            EffectInstance = new DynamicEffectInstance("ComputeEffectShader", Parameters);
            EffectInstance.Initialize(context.Services);

            // We give ComputeEffectShader a higher priority, since they are usually executed serially and blocking
            EffectInstance.EffectCompilerParameters.TaskPriority = -1;

            SetDefaultParameters();
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
        /// Gets or sets the name of the input compute shader file (.sdsl)
        /// </summary>
        public string ShaderSourceName { get; set; }

        /// <summary>
        /// Gets or sets the dispatcher.
        /// </summary>
        public IComputeEffectDispatcher Dispatcher { get; set; }

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
            UpdateParameters();
        }

        /// <summary>
        /// Updates the effect <see cref="DrawEffect.Parameters"/> from properties defined in this instance.
        /// </summary>
        protected virtual void UpdateParameters()
        {
            Parameters.Set(ComputeEffectShaderKeys.ThreadNumbers, ThreadNumbers);
            Parameters.Set(ComputeEffectShaderKeys.ComputeShaderName, ShaderSourceName);
            Dispatcher?.UpdateParameters(Parameters, ThreadNumbers);
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            if (string.IsNullOrEmpty(ShaderSourceName))
                return;

            if (EffectInstance.UpdateEffect(GraphicsDevice) || pipelineStateDirty || previousBytecode != EffectInstance.Effect.Bytecode)
            {
                // The EffectInstance might have been updated from outside
                previousBytecode = EffectInstance.Effect.Bytecode;

                pipelineState.State.SetDefaults();
                pipelineState.State.RootSignature = EffectInstance.RootSignature;
                pipelineState.State.EffectBytecode = EffectInstance.Effect.Bytecode;
                pipelineState.Update();
                pipelineStateDirty = false;
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
