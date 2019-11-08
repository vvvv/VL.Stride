using System.Collections.Generic;
using Xenko.Core;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;

namespace VL.Xenko.Shaders.ShaderFX
{
    public class GetVar<T> : ComputeValue<T>
    {
        public GetVar(AssignVar<T> var)
        {
            Var = var;
            VarName = var.VarName;
        }

        public AssignVar<T> Var { get; set; }

        public string VarName { get; set; }

        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return ReturnIfNotNull(Var);
        }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderSource = GetShaderSourceForType<T>("GetVar", Var?.VarName ?? VarName);

            return shaderSource;
        }

        public override string ToString()
        {
            return string.Format("Get {0}", VarName);
        }
    }
}
