using Stride.Core.Mathematics;
using Stride.Rendering;
using Stride.Rendering.ComputeEffect;

namespace VL.Stride.Rendering.ComputeEffect
{
    /// <summary>
    /// A compute effect dispatcher doing a direct dispatch with the given thread group counts.
    /// </summary>
    class DirectComputeEffectDispatcher : IComputeEffectDispatcher
    {
        /// <summary>
        /// Gets or sets the number of group counts the shader should be dispatched to.
        /// </summary>
        public Int3 ThreadGroupCounts { get; set; } = Int3.One;

        public void UpdateParameters(ParameterCollection parameters, Int3 threadNumbers)
        {
            parameters.Set(ComputeShaderBaseKeys.ThreadGroupCountGlobal, ThreadGroupCounts);
        }

        public void Dispatch(RenderDrawContext context)
        {
            context.CommandList.Dispatch(ThreadGroupCounts.X, ThreadGroupCounts.Y, ThreadGroupCounts.Z);
        }
    }
}
