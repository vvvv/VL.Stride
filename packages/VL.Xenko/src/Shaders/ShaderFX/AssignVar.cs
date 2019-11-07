using System.Collections.Generic;
using Xenko.Core;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using DataMemberAttribute = Xenko.Core.DataMemberAttribute;

namespace VL.Xenko.Shaders
{
    public class AssignVar<T> : ComputeNode<T>, IComputeVoid
    {
        /// <summary>
        /// The compute node that generates the value to be assigned.
        /// </summary>
        /// <userdoc>
        /// The map used for the left (background) node.
        /// </userdoc>
        [DataMember(20)]
        [Display("Value")]
        public IComputeValue<T> Value { get; set; }

        [DataMember(30)]
        [Display("Variable Name")]
        public string VarName { get; set; }

        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return ReturnIfNotNull(Value);
        }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderSource = GetShaderSourceForType("AssignVar", VarName);

            var mixin = CreateMixin(shaderSource);

            var valueShaderSource = Value?.GenerateShaderSource(context, baseKeys);
            if (valueShaderSource != null)
                mixin.AddComposition("Value", valueShaderSource);

            return mixin;
        }
    }
}
