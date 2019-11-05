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

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderSource = GetShaderSourceForType("ComputeAssignVar", VarName);

            var mixin = new ShaderMixinSource();
            mixin.Mixins.Add(shaderSource);

            var valueShaderSource = Value?.GenerateShaderSource(context, baseKeys);
            if (valueShaderSource != null)
                mixin.AddComposition("Value", valueShaderSource);

            return mixin;
        }
    }
}
