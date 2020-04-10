using System;
using System.Collections.Generic;
using System.Text;
using Xenko.Rendering;
using Xenko.Shaders;

namespace VL.Xenko.Effects.ComputeFX
{
    public class ComputeFXKeys
    {
        public static readonly PermutationParameterKey<ShaderSource> ComputeFXRoot = ParameterKeys.NewPermutation<ShaderSource>();
    }
}
