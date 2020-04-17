using System;
using System.Collections.Generic;
using System.Text;
using Xenko.Rendering;
using Xenko.Shaders;

namespace VL.Xenko.Effects.TextureFX
{
    class TextureFXKeys
    {
        public static readonly PermutationParameterKey<ShaderSource> TextureFXRoot = ParameterKeys.NewPermutation<ShaderSource>();
    }
}
