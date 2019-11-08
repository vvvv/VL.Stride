using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Xenko.Core;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using DataMemberAttribute = Xenko.Core.DataMemberAttribute;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;


namespace VL.Xenko.Shaders.ShaderFX
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
            var shaderSource = GetShaderSourceForType<T>(ShaderName);

            var leftShaderSource = Left?.GenerateShaderSource(context, baseKeys);
            var rightShaderSource = Right?.GenerateShaderSource(context, baseKeys);

            var mixin = shaderSource.CreateMixin();

            if (leftShaderSource != null)
                mixin.AddComposition("Left", leftShaderSource);
            if (rightShaderSource != null)
                mixin.AddComposition("Right", rightShaderSource);

            return mixin;
        }

        public override string ToString()
        {
            return string.Format("Op {0} {1} {2}", Left, ShaderName, Right);
        }
    }
}
