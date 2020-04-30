using System;
using System.Collections.Generic;
using System.Text;
using Stride.Rendering;
using Stride.Shaders;

namespace VL.Stride.Effects.ComputeFX
{
    public class ComputeFXKeys
    {
        public static readonly PermutationParameterKey<ShaderSource> ComputeFXRoot = ParameterKeys.NewPermutation<ShaderSource>();
    }
}
