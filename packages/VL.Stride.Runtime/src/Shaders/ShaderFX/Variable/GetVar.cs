using System.Collections.Generic;
using Stride.Core;
using Stride.Rendering.Materials;
using Stride.Shaders;
using static VL.Stride.Shaders.ShaderFX.ShaderFXUtils;

namespace VL.Stride.Shaders.ShaderFX
{
    public class GetVar<T> : ComputeValue<T>
    {

        public GetVar(Var<T> var, bool evaluateChildren = true)
        {
            EvaluateChildren = evaluateChildren;
            Var = var;
        }

        bool EvaluateChildren { get; }

        public Var<T> Var { get; }

        string VarName => Var?.VarName ?? "NoVar";

        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return EvaluateChildren ? ReturnIfNotNull(Var) : ReturnIfNotNull();
        }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            ShaderClassSource shaderSource;

            if (Var is Constant<T> constant)
                shaderSource = GetShaderSourceForType<T>("Constant", GetAsShaderString((dynamic)constant.ConstantValue));
            else if (Var is Semantic<T> semantic)
                shaderSource = GetShaderSourceForType<T>("GetSemantic", semantic.VarName, semantic.SemanticName);
            else
                shaderSource = Var != null ? GetShaderSourceForType<T>("GetVar", VarName) : GetShaderSourceForType<T>("Compute");

            return shaderSource;
        }

        public override string ToString()
        {
            if (Var is Semantic<T> semantic)
                return string.Format("Semantic {0}", semantic.SemanticName);
            else
                return string.Format("Get {0}", VarName);
        }
    }
}
