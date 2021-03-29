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
    public static partial class Tunnels_TextureFXKeys
    {
        public static readonly ValueParameterKey<int> Type = ParameterKeys.NewValue<int>();
        public static readonly ValueParameterKey<float> Time = ParameterKeys.NewValue<float>();
        public static readonly ValueParameterKey<float> Rotation = ParameterKeys.NewValue<float>(0);
        public static readonly ValueParameterKey<float> Distance = ParameterKeys.NewValue<float>(0.5f);
        public static readonly ValueParameterKey<float> Offset = ParameterKeys.NewValue<float>(0.0f);
        public static readonly ValueParameterKey<float> FogDistance = ParameterKeys.NewValue<float>(0.5f);
        public static readonly ValueParameterKey<Color4> FogColor = ParameterKeys.NewValue<Color4>(new Color4(0,0,0,1));
    }
}