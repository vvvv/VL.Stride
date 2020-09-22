using Stride.Core.Mathematics;
using Stride.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using VL.Core;

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
                    return (new MutablePipelineState(deviceHandle.Resource), () => deviceHandle.Dispose());
                },
                name: "PipelineState",
                category: graphicsCategory,
                copyOnWrite: false,
                fragmented: true,
                hasStateOutput: false)
                .AddInput(nameof(PipelineStateDescription.RootSignature), x => x.State.RootSignature, (x, v) => x.State.RootSignature = v)
                .AddInput(nameof(PipelineStateDescription.EffectBytecode), x => x.State.EffectBytecode, (x, v) => x.State.EffectBytecode = v)
                .AddInput(nameof(PipelineStateDescription.BlendState), x => x.State.BlendState, (x, v) => x.State.BlendState = v)
                .AddInput(nameof(PipelineStateDescription.SampleMask), x => x.State.SampleMask, (x, v) => x.State.SampleMask = v, 0xFFFFFFFF)
                .AddInput(nameof(PipelineStateDescription.RasterizerState), x => x.State.RasterizerState, (x, v) => x.State.RasterizerState = v)
                .AddInput(nameof(PipelineStateDescription.DepthStencilState), x => x.State.DepthStencilState, (x, v) => x.State.DepthStencilState = v)
                .AddListInput(nameof(PipelineStateDescription.InputElements), x => x.State.InputElements, (x, v) => x.State.InputElements = v)
                .AddInput(nameof(PipelineStateDescription.PrimitiveType), x => x.State.PrimitiveType, (x, v) => x.State.PrimitiveType = v)
                .AddInput(nameof(PipelineStateDescription.Output), x => x.State.Output, (x, v) => x.State.Output = v)
                .AddCachedOutput("Output", x =>
                {
                    x.Update();
                    return x.CurrentState;
                });

            yield return factory.NewDescriptionNode(graphicsCategory, new InputElementDescription())
                .AddInput(nameof(InputElementDescription.SemanticName), x => x.v.SemanticName, (x, v) => x.v.SemanticName = v)
                .AddInput(nameof(InputElementDescription.SemanticIndex), x => x.v.SemanticIndex, (x, v) => x.v.SemanticIndex = v)
                .AddInput(nameof(InputElementDescription.Format), x => x.v.Format, (x, v) => x.v.Format = v)
                .AddInput(nameof(InputElementDescription.InputSlot), x => x.v.InputSlot, (x, v) => x.v.InputSlot = v)
                .AddInput(nameof(InputElementDescription.AlignedByteOffset), x => x.v.AlignedByteOffset, (x, v) => x.v.AlignedByteOffset = v)
                .AddInput(nameof(InputElementDescription.InputSlotClass), x => x.v.InputSlotClass, (x, v) => x.v.InputSlotClass = v)
                .AddInput(nameof(InputElementDescription.InstanceDataStepRate), x => x.v.InstanceDataStepRate, (x, v) => x.v.InstanceDataStepRate = v)
                .AddStateOutput();

            yield return factory.NewDescriptionNode(graphicsCategory, new RenderOutputDescription())
                .AddInput<CommandList>("Input", x => default, (x, v) => { if (v != null) { x.v.CaptureState(v); } }, equals: (a, b) => false /* Always need to capture */)
                .AddStateOutput();

            yield return factory.NewDescriptionNode(name: "RenderOutputDescription (Manually)", category: graphicsCategory, initial: new RenderOutputDescription())
                .AddInput(nameof(RenderOutputDescription.RenderTargetCount), x => x.v.RenderTargetCount, (x, v) => x.v.RenderTargetCount = v)
                .AddInput(nameof(RenderOutputDescription.RenderTargetFormat0), x => x.v.RenderTargetFormat0, (x, v) => x.v.RenderTargetFormat0 = v)
                .AddInput(nameof(RenderOutputDescription.RenderTargetFormat1), x => x.v.RenderTargetFormat1, (x, v) => x.v.RenderTargetFormat1 = v)
                .AddInput(nameof(RenderOutputDescription.RenderTargetFormat2), x => x.v.RenderTargetFormat2, (x, v) => x.v.RenderTargetFormat2 = v)
                .AddInput(nameof(RenderOutputDescription.RenderTargetFormat3), x => x.v.RenderTargetFormat3, (x, v) => x.v.RenderTargetFormat3 = v)
                .AddInput(nameof(RenderOutputDescription.RenderTargetFormat4), x => x.v.RenderTargetFormat4, (x, v) => x.v.RenderTargetFormat4 = v)
                .AddInput(nameof(RenderOutputDescription.RenderTargetFormat5), x => x.v.RenderTargetFormat5, (x, v) => x.v.RenderTargetFormat5 = v)
                .AddInput(nameof(RenderOutputDescription.RenderTargetFormat6), x => x.v.RenderTargetFormat6, (x, v) => x.v.RenderTargetFormat6 = v)
                .AddInput(nameof(RenderOutputDescription.RenderTargetFormat7), x => x.v.RenderTargetFormat7, (x, v) => x.v.RenderTargetFormat7 = v)
                .AddInput(nameof(RenderOutputDescription.DepthStencilFormat), x => x.v.DepthStencilFormat, (x, v) => x.v.DepthStencilFormat = v)
                .AddInput(nameof(RenderOutputDescription.MultisampleCount), x => x.v.MultisampleCount, (x, v) => x.v.MultisampleCount = v)
                .AddInput(nameof(RenderOutputDescription.ScissorTestEnable), x => x.v.ScissorTestEnable, (x, v) => x.v.ScissorTestEnable = v)
                .AddStateOutput();

            yield return factory.NewDescriptionNode(name: "SamplerState", category: graphicsCategory, initial: SamplerStateDescription.Default)
                .AddInput(nameof(SamplerStateDescription.Filter), x => x.v.Filter, (x, v) => x.v.Filter = v, TextureFilter.Linear)
                .AddInput(nameof(SamplerStateDescription.AddressU), x => x.v.AddressU, (x, v) => x.v.AddressU = v, TextureAddressMode.Clamp)
                .AddInput(nameof(SamplerStateDescription.AddressV), x => x.v.AddressV, (x, v) => x.v.AddressV = v, TextureAddressMode.Clamp)
                .AddInput(nameof(SamplerStateDescription.AddressW), x => x.v.AddressW, (x, v) => x.v.AddressW = v, TextureAddressMode.Clamp)
                .AddInput(nameof(SamplerStateDescription.BorderColor), x => x.v.BorderColor, (x, v) => x.v.BorderColor = v, Color4.Black)
                .AddInput(nameof(SamplerStateDescription.MaxAnisotropy), x => x.v.MaxAnisotropy, (x, v) => x.v.MaxAnisotropy = v, 16)
                .AddInput(nameof(SamplerStateDescription.MinMipLevel), x => x.v.MinMipLevel, (x, v) => x.v.MinMipLevel = v, 0f)
                .AddInput(nameof(SamplerStateDescription.MaxMipLevel), x => x.v.MaxMipLevel, (x, v) => x.v.MaxMipLevel = v, float.MaxValue)
                .AddInput(nameof(SamplerStateDescription.MipMapLevelOfDetailBias), x => x.v.MipMapLevelOfDetailBias, (x, v) => x.v.MipMapLevelOfDetailBias = v, 0f)
                .AddInput(nameof(SamplerStateDescription.CompareFunction), x => x.v.CompareFunction, (x, v) => x.v.CompareFunction = v, CompareFunction.Never)
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
               .AddInput(nameof(BlendStateDescription.AlphaToCoverageEnable), x => x.v.AlphaToCoverageEnable, (x, v) => x.v.AlphaToCoverageEnable = v, false)
               .AddInput(nameof(BlendStateDescription.IndependentBlendEnable), x => x.v.IndependentBlendEnable, (x, v) => x.v.IndependentBlendEnable = v, false)
               .AddInput(nameof(BlendStateDescription.RenderTarget0), x => x.v.RenderTarget0, (x, v) => x.v.RenderTarget0 = v)
               .AddInput(nameof(BlendStateDescription.RenderTarget1), x => x.v.RenderTarget1, (x, v) => x.v.RenderTarget1 = v)
               .AddInput(nameof(BlendStateDescription.RenderTarget2), x => x.v.RenderTarget2, (x, v) => x.v.RenderTarget2 = v)
               .AddInput(nameof(BlendStateDescription.RenderTarget3), x => x.v.RenderTarget3, (x, v) => x.v.RenderTarget3 = v)
               .AddInput(nameof(BlendStateDescription.RenderTarget4), x => x.v.RenderTarget4, (x, v) => x.v.RenderTarget4 = v)
               .AddInput(nameof(BlendStateDescription.RenderTarget5), x => x.v.RenderTarget5, (x, v) => x.v.RenderTarget5 = v)
               .AddInput(nameof(BlendStateDescription.RenderTarget6), x => x.v.RenderTarget6, (x, v) => x.v.RenderTarget6 = v)
               .AddInput(nameof(BlendStateDescription.RenderTarget7), x => x.v.RenderTarget7, (x, v) => x.v.RenderTarget7 = v)
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
                .AddInput(nameof(BlendStateRenderTargetDescription.BlendEnable), x => x.v.BlendEnable, (x, v) => x.v.BlendEnable = v, false)
                .AddInput(nameof(BlendStateRenderTargetDescription.ColorSourceBlend), x => x.v.ColorSourceBlend, (x, v) => x.v.ColorSourceBlend = v, Blend.One)
                .AddInput(nameof(BlendStateRenderTargetDescription.ColorDestinationBlend), x => x.v.ColorDestinationBlend, (x, v) => x.v.ColorDestinationBlend = v, Blend.Zero)
                .AddInput(nameof(BlendStateRenderTargetDescription.ColorBlendFunction), x => x.v.ColorBlendFunction, (x, v) => x.v.ColorBlendFunction = v, BlendFunction.Add)
                .AddInput(nameof(BlendStateRenderTargetDescription.AlphaSourceBlend), x => x.v.AlphaSourceBlend, (x, v) => x.v.AlphaSourceBlend = v, Blend.One)
                .AddInput(nameof(BlendStateRenderTargetDescription.AlphaDestinationBlend), x => x.v.AlphaDestinationBlend, (x, v) => x.v.AlphaDestinationBlend = v, Blend.Zero)
                .AddInput(nameof(BlendStateRenderTargetDescription.AlphaBlendFunction), x => x.v.AlphaBlendFunction, (x, v) => x.v.AlphaBlendFunction = v, BlendFunction.Add)
                .AddInput(nameof(BlendStateRenderTargetDescription.ColorWriteChannels), x => x.v.ColorWriteChannels, (x, v) => x.v.ColorWriteChannels = v, ColorWriteChannels.All)
                .AddStateOutput();

            yield return factory.NewDescriptionNode(graphicsCategory, RasterizerStateDescription.Default)
                .AddInput(nameof(RasterizerStateDescription.FillMode), x => x.v.FillMode, (x, v) => x.v.FillMode = v, FillMode.Solid)
                .AddInput(nameof(RasterizerStateDescription.CullMode), x => x.v.CullMode, (x, v) => x.v.CullMode = v, CullMode.Back)
                .AddInput(nameof(RasterizerStateDescription.DepthClipEnable), x => x.v.DepthClipEnable, (x, v) => x.v.DepthClipEnable = v, true)
                .AddInput(nameof(RasterizerStateDescription.FrontFaceCounterClockwise), x => x.v.FrontFaceCounterClockwise, (x, v) => x.v.FrontFaceCounterClockwise = v, false)
                .AddInput(nameof(RasterizerStateDescription.ScissorTestEnable), x => x.v.ScissorTestEnable, (x, v) => x.v.ScissorTestEnable = v, false)
                .AddInput(nameof(RasterizerStateDescription.MultisampleCount), x => x.v.MultisampleCount, (x, v) => x.v.MultisampleCount = v, MultisampleCount.None)
                .AddInput(nameof(RasterizerStateDescription.MultisampleAntiAliasLine), x => x.v.MultisampleAntiAliasLine, (x, v) => x.v.MultisampleAntiAliasLine = v, false)
                .AddInput(nameof(RasterizerStateDescription.DepthBias), x => x.v.DepthBias, (x, v) => x.v.DepthBias = v, 0)
                .AddInput(nameof(RasterizerStateDescription.DepthBiasClamp), x => x.v.DepthBiasClamp, (x, v) => x.v.DepthBiasClamp = v, 0f)
                .AddInput(nameof(RasterizerStateDescription.SlopeScaleDepthBias), x => x.v.SlopeScaleDepthBias, (x, v) => x.v.SlopeScaleDepthBias = v, 0f)
                .AddStateOutput();

            yield return factory.NewDescriptionNode(graphicsCategory, DepthStencilStates.Default)
                .AddInput(nameof(DepthStencilStateDescription.DepthBufferEnable), x => x.v.DepthBufferEnable, (x, v) => x.v.DepthBufferEnable = v, true)
                .AddInput(nameof(DepthStencilStateDescription.DepthBufferWriteEnable), x => x.v.DepthBufferWriteEnable, (x, v) => x.v.DepthBufferWriteEnable = v, true)
                .AddInput(nameof(DepthStencilStateDescription.DepthBufferFunction), x => x.v.DepthBufferFunction, (x, v) => x.v.DepthBufferFunction = v, CompareFunction.LessEqual)
                .AddInput(nameof(DepthStencilStateDescription.StencilEnable), x => x.v.StencilEnable, (x, v) => x.v.StencilEnable = v, false)
                .AddInput(nameof(DepthStencilStateDescription.FrontFace), x => x.v.FrontFace, (x, v) => x.v.FrontFace = v)
                .AddInput(nameof(DepthStencilStateDescription.BackFace), x => x.v.BackFace, (x, v) => x.v.BackFace = v)
                .AddInput(nameof(DepthStencilStateDescription.StencilMask), x => x.v.StencilMask, (x, v) => x.v.StencilMask = v, byte.MaxValue)
                .AddInput(nameof(DepthStencilStateDescription.StencilWriteMask), x => x.v.StencilWriteMask, (x, v) => x.v.StencilWriteMask = v, byte.MaxValue)
                .AddStateOutput();

            yield return factory.NewDescriptionNode(graphicsCategory,
                new DepthStencilStencilOpDescription()
                {
                    StencilFunction = CompareFunction.Always,
                    StencilPass = StencilOperation.Keep,
                    StencilFail = StencilOperation.Keep,
                    StencilDepthBufferFail = StencilOperation.Keep
                })
                .AddInput(nameof(DepthStencilStencilOpDescription.StencilFunction), x => x.v.StencilFunction, (x, v) => x.v.StencilFunction = v, CompareFunction.Always)
                .AddInput(nameof(DepthStencilStencilOpDescription.StencilPass), x => x.v.StencilPass, (x, v) => x.v.StencilPass = v, StencilOperation.Keep)
                .AddInput(nameof(DepthStencilStencilOpDescription.StencilFail), x => x.v.StencilFail, (x, v) => x.v.StencilFail = v, StencilOperation.Keep)
                .AddInput(nameof(DepthStencilStencilOpDescription.StencilDepthBufferFail), x => x.v.StencilDepthBufferFail, (x, v) => x.v.StencilDepthBufferFail = v, StencilOperation.Keep)
                .AddStateOutput();
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
