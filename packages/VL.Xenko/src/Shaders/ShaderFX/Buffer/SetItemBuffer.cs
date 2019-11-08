using System;
using System.Collections.Generic;
using System.Text;
using Xenko.Graphics;
using Xenko.Rendering;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using Buffer = Xenko.Graphics.Buffer;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;

namespace VL.Xenko.Shaders.ShaderFX
{
    public class SetItemBuffer<T> : ComputeVoid
    {
        public SetItemBuffer(Buffer buffer, IComputeValue<T> value, IComputeValue<uint> index)
        {
            DynamicBuffer = buffer;
            Value = value;
            Index = index;
        }

        public Buffer DynamicBuffer { get; }

        public IComputeValue<T> Value { get; }

        public IComputeValue<uint> Index { get; }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var bufferKey = ContextKeyMap<Buffer>.GetParameterKey(context, DynamicBuffer);

            context.Parameters.Set(bufferKey, DynamicBuffer);

            var shaderClassSource = GetShaderSourceForType<T>("SetItemRWBuffer", bufferKey);

            var mixin = shaderClassSource.CreateMixin();

            mixin.AddComposition(Value, "Value", context, baseKeys);
            mixin.AddComposition(Index, "Index", context, baseKeys);

            return mixin;
        }
    }
}
