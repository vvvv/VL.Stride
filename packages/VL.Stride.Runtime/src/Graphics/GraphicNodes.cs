using Stride.Core.Mathematics;
using Stride.Graphics;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;
using VL.Core;

namespace VL.Stride.Graphics
{
    static class GraphicsNodes
    {
        public static IEnumerable<IVLNodeDescription> GetNodeDescriptions(IVLNodeDescriptionFactory factory)
        {
            var graphicsCategory = "Stride.Graphics";

            yield return new CustomNodeDesc<MutablePipelineState>(factory,
                ctor: nodeContext =>
                {
                    var deviceHandle = nodeContext.GetDeviceHandle();
                    return (new MutablePipelineState(deviceHandle.Resource), () => deviceHandle.Dispose());
                },
                name: "PipelineState",
                category: graphicsCategory,
                copyOnWrite: false,
                hasStateOutput: false)
                .AddInput(nameof(PipelineStateDescription.RootSignature), x => x.State.RootSignature, (x, v) => x.State.RootSignature = v)
                .AddInput(nameof(PipelineStateDescription.EffectBytecode), x => x.State.EffectBytecode, (x, v) => x.State.EffectBytecode = v)
                .AddInput(nameof(PipelineStateDescription.BlendState), x => x.State.BlendState.ToEnum(), (x, v) => x.State.BlendState = v.ToDescription())
                .AddInput(nameof(PipelineStateDescription.SampleMask), x => x.State.SampleMask, (x, v) => x.State.SampleMask = v, 0xFFFFFFFF)
                .AddInput(nameof(PipelineStateDescription.RasterizerState), x => x.State.RasterizerState.ToEnum(), (x, v) => x.State.RasterizerState = v.ToDescription())
                .AddInput(nameof(PipelineStateDescription.DepthStencilState), x => x.State.DepthStencilState.ToEnum(), (x, v) => x.State.DepthStencilState = v.ToDescription())
                .AddListInput(nameof(PipelineStateDescription.InputElements), x => x.State.InputElements, (x, v) => x.State.InputElements = v)
                .AddInput(nameof(PipelineStateDescription.PrimitiveType), x => x.State.PrimitiveType, (x, v) => x.State.PrimitiveType = v)
                .AddInput(nameof(PipelineStateDescription.Output), x => x.State.Output, (x, v) => x.State.Output = v)
                .AddCachedOutput("Output", x =>
                {
                    x.Update();
                    return x.CurrentState;
                });

            yield return factory.NewNode<InputElementDescription>(category: graphicsCategory, copyOnWrite: false)
                .AddInput(nameof(InputElementDescription.SemanticName), x => x.SemanticName, (x, v) => x.SemanticName = v)
                .AddInput(nameof(InputElementDescription.SemanticIndex), x => x.SemanticIndex, (x, v) => x.SemanticIndex = v)
                .AddInput(nameof(InputElementDescription.Format), x => x.Format, (x, v) => x.Format = v)
                .AddInput(nameof(InputElementDescription.InputSlot), x => x.InputSlot, (x, v) => x.InputSlot = v)
                .AddInput(nameof(InputElementDescription.AlignedByteOffset), x => x.AlignedByteOffset, (x, v) => x.AlignedByteOffset = v)
                .AddInput(nameof(InputElementDescription.InputSlotClass), x => x.InputSlotClass, (x, v) => x.InputSlotClass = v)
                .AddInput(nameof(InputElementDescription.InstanceDataStepRate), x => x.InstanceDataStepRate, (x, v) => x.InstanceDataStepRate = v);

            yield return factory.NewNode<RenderOutputDescription>(category: graphicsCategory, copyOnWrite: false)
                .AddInput(nameof(RenderOutputDescription.RenderTargetCount), x => x.RenderTargetCount, (x, v) => x.RenderTargetCount = v)
                .AddInput(nameof(RenderOutputDescription.RenderTargetFormat0), x => x.RenderTargetFormat0, (x, v) => x.RenderTargetFormat0 = v)
                .AddInput(nameof(RenderOutputDescription.RenderTargetFormat1), x => x.RenderTargetFormat1, (x, v) => x.RenderTargetFormat1 = v)
                .AddInput(nameof(RenderOutputDescription.RenderTargetFormat2), x => x.RenderTargetFormat2, (x, v) => x.RenderTargetFormat2 = v)
                .AddInput(nameof(RenderOutputDescription.RenderTargetFormat3), x => x.RenderTargetFormat3, (x, v) => x.RenderTargetFormat3 = v)
                .AddInput(nameof(RenderOutputDescription.RenderTargetFormat4), x => x.RenderTargetFormat4, (x, v) => x.RenderTargetFormat4 = v)
                .AddInput(nameof(RenderOutputDescription.RenderTargetFormat5), x => x.RenderTargetFormat5, (x, v) => x.RenderTargetFormat5 = v)
                .AddInput(nameof(RenderOutputDescription.RenderTargetFormat6), x => x.RenderTargetFormat6, (x, v) => x.RenderTargetFormat6 = v)
                .AddInput(nameof(RenderOutputDescription.RenderTargetFormat7), x => x.RenderTargetFormat7, (x, v) => x.RenderTargetFormat7 = v)
                .AddInput(nameof(RenderOutputDescription.DepthStencilFormat), x => x.DepthStencilFormat, (x, v) => x.DepthStencilFormat = v)
                .AddInput(nameof(RenderOutputDescription.MultisampleCount), x => x.MultisampleCount, (x, v) => x.MultisampleCount = v)
                .AddInput(nameof(RenderOutputDescription.ScissorTestEnable), x => x.ScissorTestEnable, (x, v) => x.ScissorTestEnable = v);

            yield return factory.NewNode(name: "SamplerState", category: graphicsCategory, copyOnWrite: false, hasStateOutput: false, fragmented: true, ctor: _ => new StructRef<SamplerStateDescription>(SamplerStateDescription.Default))
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
        }

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
