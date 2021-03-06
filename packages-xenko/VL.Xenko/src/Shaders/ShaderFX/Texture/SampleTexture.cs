﻿using System;
using System.Collections.Generic;
using System.Text;
using Xenko.Graphics;
using Xenko.Rendering;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using Buffer = Xenko.Graphics.Buffer;
using Xenko.Core.Mathematics;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;


namespace VL.Xenko.Shaders.ShaderFX
{
    public class SampleTexture<T> : ComputeValue<T>
    {

        public SampleTexture(DeclTexture buffer, IComputeValue<Vector2> index, bool isRW = false)
        {
            TextureDecl = buffer;
            TexCd = index;
            IsRW = isRW;

            ShaderName = IsRW ? "SampleTextureRW" : "SampleTexture";
        }

        public DeclTexture TextureDecl { get; }

        public IComputeValue<Vector2> TexCd { get; }

        public bool IsRW { get; }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            if (TextureDecl == null)
                return GetShaderSourceForType<T>("Compute");

            TextureDecl.GenerateShaderSource(context, baseKeys);

            var shaderClassSource = GetShaderSourceForType<T>(ShaderName, TextureDecl.TextureKey);

            if (TexCd != null)
            {
                var mixin = shaderClassSource.CreateMixin();
                mixin.AddComposition(TexCd, "TexCd", context, baseKeys);
                return mixin;
            }
            else
                return shaderClassSource;
        }

        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return ReturnIfNotNull(TextureDecl, TexCd);
        }
    }
}
