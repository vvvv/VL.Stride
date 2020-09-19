using Stride.Core.Mathematics;
using Stride.Rendering;
using System;

namespace VL.Stride.Rendering.ComputeEffect
{
    /// <summary>
    /// A commpute effect dispatcher using a delegate to compute the dispatch count.
    /// </summary>
    class CustomComputeEffectDispatcher : IComputeEffectDispatcher
    {
        readonly DirectComputeEffectDispatcher directComputeEffectDispatcher = new DirectComputeEffectDispatcher();

        /// <summary>
        /// The selector function to compute the number of dispatch group counts based on the shader defined thread numbers.
        /// </summary>
        public Func<Int3, Int3> ThreadGroupCountsSelector { get; set; }

        public void UpdateParameters(ParameterCollection parameters, Int3 threadNumbers)
        {
            directComputeEffectDispatcher.ThreadGroupCounts = ThreadGroupCountsSelector?.Invoke(threadNumbers) ?? Int3.Zero;
            directComputeEffectDispatcher.UpdateParameters(parameters, threadNumbers);
        }

        public void Dispatch(RenderDrawContext context)
        {
            directComputeEffectDispatcher.Dispatch(context);
        }
    }
}
