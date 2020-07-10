using System;
using System.Collections.Generic;
using System.Text;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Materials;
using Stride.Shaders;
using Buffer = Stride.Graphics.Buffer;
using static VL.Stride.Shaders.ShaderFX.ShaderFXUtils;


namespace VL.Stride.Shaders.ShaderFX
{
    public class GetItemBuffer<T> : ComputeValue<T>
    {

        public GetItemBuffer(DeclBuffer buffer, IComputeValue<uint> index, bool isRW = false, bool isStructured = false)
        {
            TextureDecl = buffer;
            Index = index;
            IsRW = isRW;
            IsStructured = isStructured;

            var prefix = IsRW ? "GetItemRW" : "GetItem";
            var bufferType = IsStructured ? "StructuredBuffer" : "Buffer";

            ShaderName = prefix + bufferType;
        }

        public DeclBuffer TextureDecl { get; }

        public IComputeValue<uint> Index { get; }

        public bool IsRW { get; }

        public bool IsStructured { get; }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            if (TextureDecl == null)
                return GetShaderSourceForType<T>("Compute");

            TextureDecl.GenerateShaderSource(context, baseKeys);

            var shaderClassSource = GetShaderSourceForType<T>(ShaderName, TextureDecl.BufferKey);

            var mixin = shaderClassSource.CreateMixin();

            mixin.AddComposition(Index, "Index", context, baseKeys);
            return mixin;
        }

        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return ReturnIfNotNull(TextureDecl, Index);
        }
    }
}
