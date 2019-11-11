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
    public class GetItemBuffer<T> : ComputeValue<T>
    {

        public GetItemBuffer(DeclBuffer buffer, IComputeValue<uint> index, bool isRW = false, bool isStructured = false)
        {
            BufferDecl = buffer;
            Index = index;
            IsRW = isRW;
            IsStructured = isStructured;

            var prefix = IsRW ? "GetItemRW" : "GetItem";
            var bufferType = IsStructured ? "StructuredBuffer" : "Buffer";

            ShaderName = prefix + bufferType;
        }

        public DeclBuffer BufferDecl { get; }

        public IComputeValue<uint> Index { get; }

        public bool IsRW { get; }

        public bool IsStructured { get; }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            if (BufferDecl == null)
                return GetShaderSourceForType<T>("Compute");

            BufferDecl.GenerateShaderSource(context, baseKeys);

            var shaderClassSource = GetShaderSourceForType<T>(ShaderName, BufferDecl.BufferKey);

            var mixin = shaderClassSource.CreateMixin();

            mixin.AddComposition(Index, "Index", context, baseKeys);
            return mixin;
        }

        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return ReturnIfNotNull(BufferDecl, Index);
        }
    }
}
