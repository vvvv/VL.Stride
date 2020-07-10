using System;
using System.Collections.Generic;
using System.Text;
using Stride.Rendering;
using Stride.Shaders;

namespace VL.Stride.Effects.TextureFX
{
    class TextureFXKeys
    {
        public static readonly PermutationParameterKey<ShaderSource> TextureFXRoot = ParameterKeys.NewPermutation<ShaderSource>();
    }
}
