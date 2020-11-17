using System.Collections.Generic;
using Stride.Core;
using Stride.Rendering.Materials;
using Stride.Shaders;
using static VL.Stride.Shaders.ShaderFX.ShaderFXUtils;


namespace VL.Stride.Shaders.ShaderFX
{
    /// <summary>
    /// Defines a variable and assigns a value to it. Can also re-assign an existing Var.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="ComputeNode{T}" />
    /// <seealso cref="IComputeVoid" />
    public class Var<T> : ComputeNode<T>, IComputeVoid
    {
        static ulong VarIDCounter;

        public Var(IComputeValue<T> value, string name)
            : this(value, null, name)
        { }

        public Var(IComputeValue<T> value, Var<T> var)
            : this(value, var, "Var")
        { }

        public Var(IComputeValue<T> value, Var<T> var = null, string varName = "Var", string semanticName = null, bool appendID = true)
        {
            if (var != null) //re-assign existing var
            {
                VarName = var.VarName;
                SemanticName = var.SemanticName;
            }
            else
            {
                createVar = true;
                VarName = appendID ? varName + "_" + (++VarIDCounter) : varName;
                SemanticName = semanticName;
            }

            Value = value;
        }

        public IComputeValue<T> Value { get; }

        public string VarName { get; }

        public string SemanticName { get; }

        bool createVar;
         
        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return ReturnIfNotNull(Value);
        }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderSource = SemanticName != null ? 
                GetShaderSourceForType<T>("AssignSemantic", VarName, SemanticName) 
                : GetShaderSourceForType<T>("AssignVar", VarName);

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
