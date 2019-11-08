using System.Collections.Generic;
using Xenko.Core;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using DataMemberAttribute = Xenko.Core.DataMemberAttribute;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;


namespace VL.Xenko.Shaders.ShaderFX
{
    public class AssignVar<T> : ComputeNode<T>, IComputeVoid
    {
        static ulong VarIDCounter;

        public AssignVar(IComputeValue<T> value, string varName = "Var", bool appendID = true)
        {
            VarName = appendID ? varName + "_fx" + (++VarIDCounter) : varName;
            Value = value;
        }

        /// <summary>
        /// The compute node that generates the value to be assigned.
        /// </summary>
        /// <userdoc>
        /// The map used for the left (background) node.
        /// </userdoc>
        [DataMember(20)]
        [Display("Value")]
        public IComputeValue<T> Value { get; }

        [DataMember(30)]
        [Display("Variable Name")]
        public string VarName { get; }

        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return ReturnIfNotNull(Value);
        }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderSource = GetShaderSourceForType<T>("AssignVar", VarName);

            var mixin = shaderSource.CreateMixin();

            mixin.AddComposition(Value, "Value", context, baseKeys);

            return mixin;
        }

        public override string ToString()
        {
            return string.Format("Set {0}", VarName);
        }
    }
}
