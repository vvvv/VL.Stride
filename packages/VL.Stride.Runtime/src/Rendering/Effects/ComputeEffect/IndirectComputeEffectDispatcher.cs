using Stride.Core.Mathematics;
using Stride.Rendering;
using Buffer = Stride.Graphics.Buffer;

namespace VL.Stride.Rendering.ComputeEffect
{
    /// <summary>
    /// A compute effect dispatcher doing an indirect dispatch using the given argument buffer containing the thread group counts at the given byte offset.
    /// </summary>
    class IndirectComputeEffectDispatcher : IComputeEffectDispatcher
    {
        /// <summary>
        /// The argument buffer containing the thread group counts the shader should be dispatched to.
        /// </summary>
        public Buffer ArgumentBuffer { get; set; }

        /// <summary>
        /// The offset in bytes into the argument buffer.
        /// </summary>
        public int OffsetInBytes { get; set; }

        public void UpdateParameters(ParameterCollection parameters, Int3 threadNumbers)
        {
        }

        public void Dispatch(RenderDrawContext context)
        {
            if (ArgumentBuffer != null)
                context.CommandList.Dispatch(ArgumentBuffer, OffsetInBytes);
        }
    }
}
