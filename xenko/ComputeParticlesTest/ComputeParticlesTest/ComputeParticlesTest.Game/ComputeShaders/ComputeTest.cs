﻿// <auto-generated>
// Do not edit this file yourself!
//
// This code was generated by Xenko Shader Mixin Code Generator.
// To generate it yourself, please install SiliconStudio.Xenko.VisualStudio.Package .vsix
// and re-save the associated .xkfx.
// </auto-generated>

using System;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;
using SiliconStudio.Core.Mathematics;
using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace ComputeParticlesTest
{
    public static partial class ComputeTestKeys
    {
        public static readonly ObjectParameterKey<Buffer> Random = ParameterKeys.NewObject<Buffer>();
        public static readonly ValueParameterKey<int> RandomCount = ParameterKeys.NewValue<int>(1);
        public static readonly ObjectParameterKey<Buffer> output = ParameterKeys.NewObject<Buffer>();
        public static readonly ValueParameterKey<Vector3> Gravity = ParameterKeys.NewValue<Vector3>(new Vector3(0.01f,0,0));
        public static readonly ValueParameterKey<float> VelMult = ParameterKeys.NewValue<float>(0.95f);
    }
}
