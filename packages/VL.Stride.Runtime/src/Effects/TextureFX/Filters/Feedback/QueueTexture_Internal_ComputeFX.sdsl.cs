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
    public static partial class QueueTexture_Internal_ComputeFXKeys
    {
        public static readonly ObjectParameterKey<Texture> RWArray = ParameterKeys.NewObject<Texture>();
        public static readonly ValueParameterKey<Vector2> Reso = ParameterKeys.NewValue<Vector2>();
        public static readonly ValueParameterKey<int> counter = ParameterKeys.NewValue<int>();
        public static readonly ValueParameterKey<int> frameCount = ParameterKeys.NewValue<int>();
    }
}
