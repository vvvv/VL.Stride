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
    public class Transform<TMatrix, TValue> : ComputeValue<TValue>
    {
        public Transform(string operatorName, IComputeValue<TValue> left, IComputeValue<TMatrix> right)
        {
            ShaderName = operatorName;
            Left = left;
            Right = right;
        }

        public IComputeValue<TValue> Left { get; }

        public IComputeValue<TMatrix> Right { get; }

        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return ReturnIfNotNull(Left, Right);
        }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderSource = GetShaderSourceForType<TValue>(ShaderName);

            var mixin = shaderSource.CreateMixin();

            mixin.AddComposition(Left,"Left", context, baseKeys);
            mixin.AddComposition(Right, "Right", context, baseKeys);

            return mixin;
        }

        public override string ToString()
        {
            return string.Format("Op {0} {1} {2}", Left, ShaderName, Right);
        }
    }
}
