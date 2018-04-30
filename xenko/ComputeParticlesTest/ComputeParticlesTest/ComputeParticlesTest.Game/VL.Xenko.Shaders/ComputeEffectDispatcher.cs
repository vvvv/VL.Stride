using System;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Shaders;
using SiliconStudio.Xenko.Rendering.ComputeEffect;
using System.Reflection;
using System.Linq;

namespace VL.Xenko.Shaders
{
    public class ComputeEffectDispatcher : DrawEffect
    {
        private MutablePipelineState pipelineState;
        private bool pipelineStateDirty = true;
        private EffectBytecode previousBytecode;
        private FieldInfo nativeDeviceContextFi;
        private FieldInfo unorderedAccessViewsFi;

        public ComputeEffectDispatcher(RenderContext context)
            : base(context, null)
        {
            pipelineState = new MutablePipelineState(context.GraphicsDevice);

            // Setup the effect compiler
            EffectInstance = new DynamicEffectInstance("ComputeEffectShader", Parameters);
            EffectInstance.Initialize(context.Services);

            // We give ComputeEffectShader a higher priority, since they are usually executed serially and blocking
            EffectInstance.EffectCompilerParameters.TaskPriority = -1;

            ThreadNumbers = new Int3(1);
            ThreadGroupCounts = new Int3(1);

            SetDefaultParameters();

            //get filedinfos
            var type = VLHDE.GameInstance.GraphicsContext.CommandList.GetType();
            var fieldInfos = type.GetRuntimeFields();
            nativeDeviceContextFi = fieldInfos.Where(fi => fi.Name == "nativeDeviceContext").First();
            unorderedAccessViewsFi = fieldInfos.Where(fi => fi.Name == "unorderedAccessViews").First();
        }

        /// <summary>
        /// The current effect instance.
        /// </summary>
        public DynamicEffectInstance EffectInstance { get; private set; }

        /// <summary>
        /// Gets or sets the number of group counts the shader should be dispatched to.
        /// </summary>
        public Int3 ThreadGroupCounts { get; set; }

        /// <summary>
        /// Gets or sets the number of threads desired by thread group.
        /// </summary>
        public Int3 ThreadNumbers { get; set; }

        /// <summary>
        /// Gets or sets the name of the input compute shader file (.xksl)
        /// </summary>
        public string ShaderSourceName { get; set; }

        public bool ResetCounter { get; set; }
        public int CounterValue { get; set; }

        /// <summary>
        /// Sets the default parameters (called at constructor time and if <see cref="DrawEffect.Reset"/> is called)
        /// </summary>
        protected override void SetDefaultParameters()
        {
        }

        protected override void PreDrawCore(RenderDrawContext context)
        {
            base.PreDrawCore(context);

            // Default handler for parameters
            UpdateParameters();
        }

        /// <summary>
        /// Updates the effect <see cref="ComputeEffectDispatcher.Parameters" /> from properties defined in this instance. See remarks.
        /// </summary>
        protected virtual void UpdateParameters()
        {
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            if (string.IsNullOrEmpty(ShaderSourceName))
                return;

            Parameters.Set(ComputeEffectShaderKeys.ThreadNumbers, ThreadNumbers);
            Parameters.Set(ComputeEffectShaderKeys.ComputeShaderName, ShaderSourceName);
            Parameters.Set(ComputeShaderBaseKeys.ThreadGroupCountGlobal, ThreadGroupCounts);

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
            var commandList = context.CommandList;
            commandList.SetPipelineState(pipelineState.CurrentState);

            // Apply the effect
            EffectInstance.Apply(context.GraphicsContext);
            if (ResetCounter)
            {
                var uavs = (SharpDX.Direct3D11.UnorderedAccessView[])unorderedAccessViewsFi.GetValue(commandList);
                commandList.ComputeShaderSetUnorderedAccessView(0, uavs[0], CounterValue);
            }

            // Dispatch compute shader
            commandList.Dispatch(ThreadGroupCounts.X, ThreadGroupCounts.Y, ThreadGroupCounts.Z);

            // Un-apply
            //throw new InvalidOperationException();
            //EffectInstance.Effect.UnbindResources(GraphicsDevice);
        }
    }
}
