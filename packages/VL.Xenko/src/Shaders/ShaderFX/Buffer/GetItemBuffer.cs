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
    public class GetItemBuffer<T> : ComputeValue<T> where T : struct
    {
        public GetItemBuffer(Buffer buffer, IComputeValue<uint> index)
        {
            DynamicBuffer = buffer;
            Index = index;
        }

        public Buffer DynamicBuffer { get; }

        public IComputeValue<uint> Index { get; }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var bufferKey = ContextKeyMap<Buffer>.GetParameterKey(context, DynamicBuffer);

            context.Parameters.Set(bufferKey, DynamicBuffer);

            var shaderClassSource = GetShaderSourceForType<T>("GetItemBuffer", bufferKey);

            var mixin = shaderClassSource.CreateMixin();

            mixin.AddComposition(Index, "Index", context, baseKeys);

            return mixin;
        }
    }
}
