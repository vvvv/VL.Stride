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
        public GetItemBuffer(DeclBuffer buffer, IComputeValue<uint> index)
        {
            BufferDecl = buffer;
            Index = index;
        }

        public DeclBuffer BufferDecl { get; }

        public IComputeValue<uint> Index { get; }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            BufferDecl.GenerateShaderSource(context, baseKeys);

            var shaderClassSource = GetShaderSourceForType<T>("GetItemBuffer", BufferDecl.BufferKey);

            var mixin = shaderClassSource.CreateMixin();

            mixin.AddComposition(Index, "Index", context, baseKeys);

            return mixin;
        }

        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return ReturnIfNotNull(Index);
        }
    }
}
