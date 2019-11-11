using System;
using System.Collections.Generic;
using System.Text;
using Xenko.Rendering;
using Xenko.Shaders;

namespace VL.Xenko.Effects.ShaderFX
{
    class ShaderFXKeys
    {
        public static readonly PermutationParameterKey<ShaderSource> ShaderFXRoot = ParameterKeys.NewPermutation<ShaderSource>();
    }
}
