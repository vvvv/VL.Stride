using System.Collections.Generic;
using Xenko.Core;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;


namespace VL.Xenko.Shaders.ShaderFX
{
    public class Var<T> : ComputeNode<T>, IComputeVoid
    {
        static ulong VarIDCounter;

        public Var(IComputeValue<T> value, string name)
            : this(value, null, name)
        { }

        public Var(IComputeValue<T> value, Var<T> var)
            : this(value, var, "")
        { }

        public Var(IComputeValue<T> value, Var<T> var = null, string varName = "Var", bool appendID = true)
        {
            if (var != null) //re-assign existing var
            {
                VarName = var.VarName;
                Parent = var;
            }
            else
            {
                createVar = true;
                VarName = appendID ? varName + "_" + (++VarIDCounter) : varName;
            }

            Value = value;
        }

        public IComputeValue<T> Value { get; }

        public string VarName { get; }

        bool createVar;
        Var<T> Parent;
        ShaderClassSource ShaderClassSource;
         

        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return ReturnIfNotNull(Value);
        }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderSource = GetShaderSourceForType<T>("AssignVar", VarName);
            ShaderClassSource = shaderSource;

            var mixin = shaderSource.CreateMixin();

            mixin.AddComposition(Value, "Value", context, baseKeys);

            return mixin;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", createVar ? "": "Assign", VarName);
        }
    }
}
