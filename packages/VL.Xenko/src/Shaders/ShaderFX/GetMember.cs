using System;
using System.Collections.Generic;
using System.Text;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using Xenko.Core.Shaders.Ast;
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
            //var shaderClassSource = GetShaderSourceForType2<TIn, TOut>("GetMember", MemberName);
            var shaderClassSource = GetMemberStrings.GetShaderSourceStringForType2<TIn, TOut>("GetMember", MemberName);

            if (context is ShaderFXGeneratorContext fxc)
                fxc.CompileShader(shaderClassSource);

            var mixin = shaderClassSource.CreateMixin();

            mixin.AddComposition(Value, "Value", context, baseKeys);

            return mixin;
        }

        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return ReturnIfNotNull(Value);
        }
    }

    public static class GetMemberStrings
    {
        public static string[] FloatVectors = new[] {
            VectorType.Float2.ToNonGenericType().ToString(),
            VectorType.Float3.ToNonGenericType().ToString(),
            VectorType.Float4.ToNonGenericType().ToString()
            };
        const string GetMemberBaseString =
@"shader GetMember_FROM__TO_<MemberName Member> : Compute_TO_
{
    compose Compute_FROM_ Value;

    override _TTO_ Compute()
    {
        return Value.Compute().Member;
    }
};";

        public static ShaderClassString GetShaderSourceStringForType2<T1, T2>(string shaderName, params object[] genericArguments)
        {
            var shaderString = GetMemberBaseString
                .Replace("_FROM_", GetNameForType<T1>())
                .Replace("_TO_", GetNameForType<T2>())
                .Replace("_TFROM_", GetTypeNameForType<T1>())
                .Replace("_TTO_", GetTypeNameForType<T2>());
            return new ShaderClassString(shaderName + GetNameForType<T1>() + GetNameForType<T2>(), shaderString, genericArguments);
        }

    }
}
