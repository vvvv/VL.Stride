using System;
using System.Collections.Generic;
using System.Text;
using Stride.Rendering;
using Stride.Shaders;

namespace VL.Stride.Effects.ShaderFX
{
    class ShaderFXKeys
    {
        public static readonly PermutationParameterKey<ShaderSource> ShaderFXRoot = ParameterKeys.NewPermutation<ShaderSource>();
    }
}
