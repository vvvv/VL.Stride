﻿using Stride.Core.Mathematics;
using Stride.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using VL.Core;
using VL.Lib.Basics.Resources;

namespace VL.Stride.Graphics
{
    static class GraphicsNodes
    {
        public static IEnumerable<IVLNodeDescription> GetNodeDescriptions(IVLNodeDescriptionFactory factory)
        {
            var graphicsCategory = "Stride.Graphics.Advanced";

            yield return new CustomNodeDesc<MutablePipelineState>(factory,
                ctor: nodeContext =>
                {
                    var deviceHandle = nodeContext.GetDeviceHandle();
                    return (CreateInitialPipelineState(deviceHandle), () => deviceHandle.Dispose());
                },
                name: "PipelineState",
                category: graphicsCategory,
                copyOnWrite: false,
                fragmented: true,
                hasStateOutput: false)
                .AddCachedInput(nameof(PipelineStateDescription.RootSignature), x => x.State.RootSignature, (x, v) => x.State.RootSignature = v)
                .AddCachedInput(nameof(PipelineStateDescription.EffectBytecode), x => x.State.EffectBytecode, (x, v) => x.State.EffectBytecode = v)
                .AddCachedInput(nameof(PipelineStateDescription.BlendState), x => x.State.BlendState, (x, v) => x.State.BlendState = v)
                .AddCachedInput(nameof(PipelineStateDescription.SampleMask), x => x.State.SampleMask, (x, v) => x.State.SampleMask = v, 0xFFFFFFFF)
                .AddCachedInput(nameof(PipelineStateDescription.RasterizerState), x => x.State.RasterizerState, (x, v) => x.State.RasterizerState = v)
                .AddCachedInput(nameof(PipelineStateDescription.DepthStencilState), x => x.State.DepthStencilState, (x, v) => x.State.DepthStencilState = v)
                .AddCachedListInput(nameof(PipelineStateDescription.InputElements), x => x.State.InputElements, (x, v) => x.State.InputElements = v)
                .AddCachedInput(nameof(PipelineStateDescription.PrimitiveType), x => x.State.PrimitiveType, (x, v) => x.State.PrimitiveType = v)
                .AddCachedInput(nameof(PipelineStateDescription.Output), x => x.State.Output, (x, v) => x.State.Output = v)
                .AddCachedOutput("Output", x =>
                {
                    x.Update();
                    return x.CurrentState;
                });

            yield return factory.NewDescriptionNode(graphicsCategory, new InputElementDescription())
                .AddCachedInput(nameof(InputElementDescription.SemanticName), x => x.v.SemanticName, (x, v) => x.v.SemanticName = v)
                .AddCachedInput(nameof(InputElementDescription.SemanticIndex), x => x.v.SemanticIndex, (x, v) => x.v.SemanticIndex = v)
                .AddCachedInput(nameof(InputElementDescription.Format), x => x.v.Format, (x, v) => x.v.Format = v)
                .AddCachedInput(nameof(InputElementDescription.InputSlot), x => x.v.InputSlot, (x, v) => x.v.InputSlot = v)
                .AddCachedInput(nameof(InputElementDescription.AlignedByteOffset), x => x.v.AlignedByteOffset, (x, v) => x.v.AlignedByteOffset = v)
                .AddCachedInput(nameof(InputElementDescription.InputSlotClass), x => x.v.InputSlotClass, (x, v) => x.v.InputSlotClass = v)
                .AddCachedInput(nameof(InputElementDescription.InstanceDataStepRate), x => x.v.InstanceDataStepRate, (x, v) => x.v.InstanceDataStepRate = v)
                .AddStateOutput();

            yield return factory.NewDescriptionNode(graphicsCategory, new RenderOutputDescription())
                .AddCachedInput<CommandList>("Input", x => default, (x, v) => { if (v != null) { x.v.CaptureState(v); } }, equals: (a, b) => false /* Always need to capture */)
                .AddStateOutput();

            yield return factory.NewDescriptionNode(name: "RenderOutputDescription (Manually)", category: graphicsCategory, initial: new RenderOutputDescription())
                .AddCachedInput(nameof(RenderOutputDescription.RenderTargetCount), x => x.v.RenderTargetCount, (x, v) => x.v.RenderTargetCount = v)
                .AddCachedInput(nameof(RenderOutputDescription.RenderTargetFormat0), x => x.v.RenderTargetFormat0, (x, v) => x.v.RenderTargetFormat0 = v)
                .AddCachedInput(nameof(RenderOutputDescription.RenderTargetFormat1), x => x.v.RenderTargetFormat1, (x, v) => x.v.RenderTargetFormat1 = v)
                .AddCachedInput(nameof(RenderOutputDescription.RenderTargetFormat2), x => x.v.RenderTargetFormat2, (x, v) => x.v.RenderTargetFormat2 = v)
                .AddCachedInput(nameof(RenderOutputDescription.RenderTargetFormat3), x => x.v.RenderTargetFormat3, (x, v) => x.v.RenderTargetFormat3 = v)
                .AddCachedInput(nameof(RenderOutputDescription.RenderTargetFormat4), x => x.v.RenderTargetFormat4, (x, v) => x.v.RenderTargetFormat4 = v)
                .AddCachedInput(nameof(RenderOutputDescription.RenderTargetFormat5), x => x.v.RenderTargetFormat5, (x, v) => x.v.RenderTargetFormat5 = v)
                .AddCachedInput(nameof(RenderOutputDescription.RenderTargetFormat6), x => x.v.RenderTargetFormat6, (x, v) => x.v.RenderTargetFormat6 = v)
                .AddCachedInput(nameof(RenderOutputDescription.RenderTargetFormat7), x => x.v.RenderTargetFormat7, (x, v) => x.v.RenderTargetFormat7 = v)
                .AddCachedInput(nameof(RenderOutputDescription.DepthStencilFormat), x => x.v.DepthStencilFormat, (x, v) => x.v.DepthStencilFormat = v)
                .AddCachedInput(nameof(RenderOutputDescription.MultisampleCount), x => x.v.MultisampleCount, (x, v) => x.v.MultisampleCount = v)
                .AddCachedInput(nameof(RenderOutputDescription.ScissorTestEnable), x => x.v.ScissorTestEnable, (x, v) => x.v.ScissorTestEnable = v)
                .AddStateOutput();

            yield return factory.NewDescriptionNode(name: "SamplerState", category: graphicsCategory, initial: SamplerStateDescription.Default)
                .AddCachedInput(nameof(SamplerStateDescription.Filter), x => x.v.Filter, (x, v) => x.v.Filter = v, TextureFilter.Linear)
                .AddCachedInput(nameof(SamplerStateDescription.AddressU), x => x.v.AddressU, (x, v) => x.v.AddressU = v, TextureAddressMode.Clamp)
                .AddCachedInput(nameof(SamplerStateDescription.AddressV), x => x.v.AddressV, (x, v) => x.v.AddressV = v, TextureAddressMode.Clamp)
                .AddCachedInput(nameof(SamplerStateDescription.AddressW), x => x.v.AddressW, (x, v) => x.v.AddressW = v, TextureAddressMode.Clamp)
                .AddCachedInput(nameof(SamplerStateDescription.BorderColor), x => x.v.BorderColor, (x, v) => x.v.BorderColor = v, Color4.Black)
                .AddCachedInput(nameof(SamplerStateDescription.MaxAnisotropy), x => x.v.MaxAnisotropy, (x, v) => x.v.MaxAnisotropy = v, 16)
                .AddCachedInput(nameof(SamplerStateDescription.MinMipLevel), x => x.v.MinMipLevel, (x, v) => x.v.MinMipLevel = v, 0f)
                .AddCachedInput(nameof(SamplerStateDescription.MaxMipLevel), x => x.v.MaxMipLevel, (x, v) => x.v.MaxMipLevel = v, float.MaxValue)
                .AddCachedInput(nameof(SamplerStateDescription.MipMapLevelOfDetailBias), x => x.v.MipMapLevelOfDetailBias, (x, v) => x.v.MipMapLevelOfDetailBias = v, 0f)
                .AddCachedInput(nameof(SamplerStateDescription.CompareFunction), x => x.v.CompareFunction, (x, v) => x.v.CompareFunction = v, CompareFunction.Never)
                .AddCachedOutput("Output", nodeContext =>
                {
                    var disposable = new SerialDisposable();
                    Func<StructRef<SamplerStateDescription>, SamplerState> getter = generator =>
                    {
                        var gdh = nodeContext.GetDeviceHandle();
                        var st = SamplerState.New(gdh.Resource, generator.v);
                        gdh.Dispose();
                        disposable.Disposable = st;
                        return st;
                    };
                    return (getter, disposable);
                });
            ;

            yield return factory.NewDescriptionNode(graphicsCategory, BlendStates.Default)
               .AddCachedInput(nameof(BlendStateDescription.AlphaToCoverageEnable), x => x.v.AlphaToCoverageEnable, (x, v) => x.v.AlphaToCoverageEnable = v, false)
               .AddCachedInput(nameof(BlendStateDescription.IndependentBlendEnable), x => x.v.IndependentBlendEnable, (x, v) => x.v.IndependentBlendEnable = v, false)
               .AddCachedInput(nameof(BlendStateDescription.RenderTarget0), x => x.v.RenderTarget0, (x, v) => x.v.RenderTarget0 = v)
               .AddCachedInput(nameof(BlendStateDescription.RenderTarget1), x => x.v.RenderTarget1, (x, v) => x.v.RenderTarget1 = v)
               .AddCachedInput(nameof(BlendStateDescription.RenderTarget2), x => x.v.RenderTarget2, (x, v) => x.v.RenderTarget2 = v)
               .AddCachedInput(nameof(BlendStateDescription.RenderTarget3), x => x.v.RenderTarget3, (x, v) => x.v.RenderTarget3 = v)
               .AddCachedInput(nameof(BlendStateDescription.RenderTarget4), x => x.v.RenderTarget4, (x, v) => x.v.RenderTarget4 = v)
               .AddCachedInput(nameof(BlendStateDescription.RenderTarget5), x => x.v.RenderTarget5, (x, v) => x.v.RenderTarget5 = v)
               .AddCachedInput(nameof(BlendStateDescription.RenderTarget6), x => x.v.RenderTarget6, (x, v) => x.v.RenderTarget6 = v)
               .AddCachedInput(nameof(BlendStateDescription.RenderTarget7), x => x.v.RenderTarget7, (x, v) => x.v.RenderTarget7 = v)
               .AddStateOutput();


            yield return factory.NewDescriptionNode(graphicsCategory,
                new BlendStateRenderTargetDescription()
                {
                    ColorSourceBlend = Blend.One,
                    ColorDestinationBlend = Blend.Zero,
                    ColorBlendFunction = BlendFunction.Add,
                    AlphaSourceBlend = Blend.One,
                    AlphaDestinationBlend = Blend.Zero,
                    AlphaBlendFunction = BlendFunction.Add,
                    ColorWriteChannels = ColorWriteChannels.All
                })
                .AddCachedInput(nameof(BlendStateRenderTargetDescription.BlendEnable), x => x.v.BlendEnable, (x, v) => x.v.BlendEnable = v, false)
                .AddCachedInput(nameof(BlendStateRenderTargetDescription.ColorSourceBlend), x => x.v.ColorSourceBlend, (x, v) => x.v.ColorSourceBlend = v, Blend.One)
                .AddCachedInput(nameof(BlendStateRenderTargetDescription.ColorDestinationBlend), x => x.v.ColorDestinationBlend, (x, v) => x.v.ColorDestinationBlend = v, Blend.Zero)
                .AddCachedInput(nameof(BlendStateRenderTargetDescription.ColorBlendFunction), x => x.v.ColorBlendFunction, (x, v) => x.v.ColorBlendFunction = v, BlendFunction.Add)
                .AddCachedInput(nameof(BlendStateRenderTargetDescription.AlphaSourceBlend), x => x.v.AlphaSourceBlend, (x, v) => x.v.AlphaSourceBlend = v, Blend.One)
                .AddCachedInput(nameof(BlendStateRenderTargetDescription.AlphaDestinationBlend), x => x.v.AlphaDestinationBlend, (x, v) => x.v.AlphaDestinationBlend = v, Blend.Zero)
                .AddCachedInput(nameof(BlendStateRenderTargetDescription.AlphaBlendFunction), x => x.v.AlphaBlendFunction, (x, v) => x.v.AlphaBlendFunction = v, BlendFunction.Add)
                .AddCachedInput(nameof(BlendStateRenderTargetDescription.ColorWriteChannels), x => x.v.ColorWriteChannels, (x, v) => x.v.ColorWriteChannels = v, ColorWriteChannels.All)
                .AddStateOutput();

            yield return factory.NewDescriptionNode(graphicsCategory, RasterizerStateDescription.Default)
                .AddCachedInput(nameof(RasterizerStateDescription.FillMode), x => x.v.FillMode, (x, v) => x.v.FillMode = v, FillMode.Solid)
                .AddCachedInput(nameof(RasterizerStateDescription.CullMode), x => x.v.CullMode, (x, v) => x.v.CullMode = v, CullMode.Back)
                .AddCachedInput(nameof(RasterizerStateDescription.DepthClipEnable), x => x.v.DepthClipEnable, (x, v) => x.v.DepthClipEnable = v, true)
                .AddCachedInput(nameof(RasterizerStateDescription.FrontFaceCounterClockwise), x => x.v.FrontFaceCounterClockwise, (x, v) => x.v.FrontFaceCounterClockwise = v, false)
                .AddCachedInput(nameof(RasterizerStateDescription.ScissorTestEnable), x => x.v.ScissorTestEnable, (x, v) => x.v.ScissorTestEnable = v, false)
                .AddCachedInput(nameof(RasterizerStateDescription.MultisampleCount), x => x.v.MultisampleCount, (x, v) => x.v.MultisampleCount = v, MultisampleCount.X8)
                .AddCachedInput(nameof(RasterizerStateDescription.MultisampleAntiAliasLine), x => x.v.MultisampleAntiAliasLine, (x, v) => x.v.MultisampleAntiAliasLine = v, true)
                .AddCachedInput(nameof(RasterizerStateDescription.DepthBias), x => x.v.DepthBias, (x, v) => x.v.DepthBias = v, 0)
                .AddCachedInput(nameof(RasterizerStateDescription.DepthBiasClamp), x => x.v.DepthBiasClamp, (x, v) => x.v.DepthBiasClamp = v, 0f)
                .AddCachedInput(nameof(RasterizerStateDescription.SlopeScaleDepthBias), x => x.v.SlopeScaleDepthBias, (x, v) => x.v.SlopeScaleDepthBias = v, 0f)
                .AddStateOutput();

            yield return factory.NewDescriptionNode(graphicsCategory, DepthStencilStates.Default)
                .AddCachedInput(nameof(DepthStencilStateDescription.DepthBufferEnable), x => x.v.DepthBufferEnable, (x, v) => x.v.DepthBufferEnable = v, true)
                .AddCachedInput(nameof(DepthStencilStateDescription.DepthBufferWriteEnable), x => x.v.DepthBufferWriteEnable, (x, v) => x.v.DepthBufferWriteEnable = v, true)
                .AddCachedInput(nameof(DepthStencilStateDescription.DepthBufferFunction), x => x.v.DepthBufferFunction, (x, v) => x.v.DepthBufferFunction = v, CompareFunction.LessEqual)
                .AddCachedInput(nameof(DepthStencilStateDescription.StencilEnable), x => x.v.StencilEnable, (x, v) => x.v.StencilEnable = v, false)
                .AddCachedInput(nameof(DepthStencilStateDescription.FrontFace), x => x.v.FrontFace, (x, v) => x.v.FrontFace = v)
                .AddCachedInput(nameof(DepthStencilStateDescription.BackFace), x => x.v.BackFace, (x, v) => x.v.BackFace = v)
                .AddCachedInput(nameof(DepthStencilStateDescription.StencilMask), x => x.v.StencilMask, (x, v) => x.v.StencilMask = v, byte.MaxValue)
                .AddCachedInput(nameof(DepthStencilStateDescription.StencilWriteMask), x => x.v.StencilWriteMask, (x, v) => x.v.StencilWriteMask = v, byte.MaxValue)
                .AddStateOutput();

            yield return factory.NewDescriptionNode(graphicsCategory,
                new DepthStencilStencilOpDescription()
                {
                    StencilFunction = CompareFunction.Always,
                    StencilPass = StencilOperation.Keep,
                    StencilFail = StencilOperation.Keep,
                    StencilDepthBufferFail = StencilOperation.Keep
                })
                .AddCachedInput(nameof(DepthStencilStencilOpDescription.StencilFunction), x => x.v.StencilFunction, (x, v) => x.v.StencilFunction = v, CompareFunction.Always)
                .AddCachedInput(nameof(DepthStencilStencilOpDescription.StencilPass), x => x.v.StencilPass, (x, v) => x.v.StencilPass = v, StencilOperation.Keep)
                .AddCachedInput(nameof(DepthStencilStencilOpDescription.StencilFail), x => x.v.StencilFail, (x, v) => x.v.StencilFail = v, StencilOperation.Keep)
                .AddCachedInput(nameof(DepthStencilStencilOpDescription.StencilDepthBufferFail), x => x.v.StencilDepthBufferFail, (x, v) => x.v.StencilDepthBufferFail = v, StencilOperation.Keep)
                .AddStateOutput();
        }

        private static MutablePipelineState CreateInitialPipelineState(IResourceHandle<GraphicsDevice> deviceHandle)
        {
            var mps = new MutablePipelineState(deviceHandle.Resource);
            mps.State.RasterizerState.MultisampleAntiAliasLine = true;
            mps.State.RasterizerState.MultisampleCount = MultisampleCount.X8;
            return mps;
        }

        static CustomNodeDesc<StructRef<T>> NewDescriptionNode<T>(this IVLNodeDescriptionFactory factory, string category, T initial, string name = default) where T : struct
        {
            return factory.NewNode(name: name ?? typeof(T).Name, category: category, copyOnWrite: false, fragmented: true, hasStateOutput: false, ctor: _ => S(initial));
        }

        static CustomNodeDesc<StructRef<T>> AddStateOutput<T>(this CustomNodeDesc<StructRef<T>> node) where T : struct
        {
            return node.AddOutput("Output", x => x.v);
        }

        static StructRef<T> S<T>(T value) where T : struct => new StructRef<T>(value);

        class StructRef<T> where T : struct
        {
            public T v;

            public StructRef(T value)
            {
                v = value;
            }
        }
    }
}
