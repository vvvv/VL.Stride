using System.Collections.Generic;
using Xenko.Core;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using DataMemberAttribute = Xenko.Core.DataMemberAttribute;

namespace VL.Xenko.Shaders
{
    public class GetVar<T> : ComputeValue<T>, IComputeVoid
    {
        public AssignVar<T> Var { get; set; }

        public string VarName { get; set; }

        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return ReturnIfNotNull(Var);
        }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderSource = GetShaderSourceForType("GetVar", Var?.VarName ?? VarName);

            return shaderSource;
        }

    }
}
