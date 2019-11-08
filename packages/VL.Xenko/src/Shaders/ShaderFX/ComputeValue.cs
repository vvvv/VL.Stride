using System.Collections.Generic;
using System.Text;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;


namespace VL.Xenko.Shaders.ShaderFX
{
    public interface IComputeValue<T> : IComputeNode
    {

    }

    public class ComputeValue<T> : ComputeNode<T>, IComputeValue<T>
    {
        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            return GetShaderSourceForType<T>(ShaderName);
        }
    }
}
