using System;
using System.Collections.Generic;
using System.Text;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;


namespace VL.Xenko.Shaders.ShaderFX
{
    public class GetMember<TIn, TOut> : ComputeValue<TOut>
    {
        public GetMember(IComputeValue<TIn> value, string memberName)
        {
            Value = value;
            MemberName = memberName;
        }

        public IComputeValue<TIn> Value { get; }
        public string MemberName { get; }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderClassSource = GetShaderSourceForType2<TIn, TOut>("GetMember", MemberName);

            var mixin = shaderClassSource.CreateMixin();

            mixin.AddComposition(Value, "Value", context, baseKeys);

            return mixin;
        }

        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return ReturnIfNotNull(Value);
        }
    }
}
