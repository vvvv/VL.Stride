﻿// <auto-generated>
// Do not edit this file yourself!
//
// This code was generated by Stride Shader Mixin Code Generator.
// To generate it yourself, please install Stride.VisualStudio.Package .vsix
// and re-save the associated .sdfx.
// </auto-generated>

using System;
using Stride.Core;
using Stride.Rendering;
using Stride.Graphics;
using Stride.Shaders;
using Stride.Core.Mathematics;
using Buffer = Stride.Graphics.Buffer;

namespace Stride.Rendering
{
    public static partial class Lomograph_TextureFXKeys
    {
        public static readonly ValueParameterKey<float> VignetteStart = ParameterKeys.NewValue<float>(0.1f);
        public static readonly ValueParameterKey<float> VignetteAmount = ParameterKeys.NewValue<float>(0.25f);
        public static readonly ValueParameterKey<float> VignetteDodge = ParameterKeys.NewValue<float>(0.1f);
        public static readonly ValueParameterKey<float> Color = ParameterKeys.NewValue<float>(0.6f);
        public static readonly ValueParameterKey<float> Contrast = ParameterKeys.NewValue<float>(0.5f);
        public static readonly ValueParameterKey<float> Level = ParameterKeys.NewValue<float>(0.5f);
        public static readonly ValueParameterKey<float> Effect = ParameterKeys.NewValue<float>(1.0f);
        public static readonly ValueParameterKey<int> Type = ParameterKeys.NewValue<int>(0);
        public static readonly ValueParameterKey<int> Iterations = ParameterKeys.NewValue<int>(4);
        public static readonly ObjectParameterKey<SamplerState> s0 = ParameterKeys.NewObject<SamplerState>();
    }
}