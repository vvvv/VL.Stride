using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Xenko.Core;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using DataMemberAttribute = Xenko.Core.DataMemberAttribute;

namespace VL.Xenko.Shaders.ShaderFX.Operations
{
    public class BinaryOperation<T> : ComputeValue<T>
    {
        public BinaryOperation(string operatorName)
        {
            ShaderName = operatorName;
        }

        /// <summary>
        /// The left (background) child node.
        /// </summary>
        /// <userdoc>
        /// The map used for the left (background) node.
        /// </userdoc>
        [DataMember(20)]
        [Display("Left")]
        public IComputeValue<T> Left { get; set; }

        /// <summary>
        /// The right (foreground) child node.
        /// </summary>
        /// <userdoc>
        /// The map used for the right (foreground) node.
        /// </userdoc>
        [DataMember(30)]
        [Display("Right")]
        public IComputeValue<T> Right { get; set; }

        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return ReturnIfNotNull(Left, Right);
        }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderSource = GetShaderSourceForType(ShaderName);

            var leftShaderSource = Left?.GenerateShaderSource(context, baseKeys);
            var rightShaderSource = Right?.GenerateShaderSource(context, baseKeys);

            var mixin = CreateMixin(shaderSource);

            if (leftShaderSource != null)
                mixin.AddComposition("Left", leftShaderSource);
            if (rightShaderSource != null)
                mixin.AddComposition("Right", rightShaderSource);

            return mixin;
        }

    }
}
