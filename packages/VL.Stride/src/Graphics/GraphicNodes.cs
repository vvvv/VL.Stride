using Stride.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using VL.Core;

namespace VL.Stride.Graphics
{
    static class GraphicNodes
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
                update: (nc, x) => x.Update(),
                name: "PipelineState",
                category: graphicsCategory,
                copyOnWrite: false,
                hasStateOutput: false)
                .AddInput(nameof(PipelineStateDescription.RootSignature), x => x.State.RootSignature, (x, v) => x.State.RootSignature = v)
                .AddInput(nameof(PipelineStateDescription.EffectBytecode), x => x.State.EffectBytecode, (x, v) => x.State.EffectBytecode = v)
                .AddInput(nameof(PipelineStateDescription.BlendState), x => x.State.BlendState.ToEnum(), (x, v) => x.State.BlendState = v.ToDescription())
                //.AddInput<BlendStateDescription?>("Custom Blend State", x => x.State.BlendState, (x, v) => { if (v.HasValue) x.State.BlendState = v.Value; })
                .AddInput(nameof(PipelineStateDescription.SampleMask), x => x.State.SampleMask, (x, v) => x.State.SampleMask = v, 0xFFFFFFFF)
                .AddInput(nameof(PipelineStateDescription.RasterizerState), x => x.State.RasterizerState.ToEnum(), (x, v) => x.State.RasterizerState = v.ToDescription())
                .AddInput(nameof(PipelineStateDescription.DepthStencilState), x => x.State.DepthStencilState.ToEnum(), (x, v) => x.State.DepthStencilState = v.ToDescription())
                .AddListInput(nameof(PipelineStateDescription.InputElements), x => x.State.InputElements, (x, v) => x.State.InputElements = v)
                .AddInput(nameof(PipelineStateDescription.PrimitiveType), x => x.State.PrimitiveType, (x, v) => x.State.PrimitiveType = v)
                .AddInput(nameof(PipelineStateDescription.Output), x => x.State.Output, (x, v) => x.State.Output = v)
                .AddOutput("Output", x => x.CurrentState);

            yield return factory.Create<InputElementDescription>(category: graphicsCategory, copyOnWrite: false)
                .AddInput(nameof(InputElementDescription.SemanticName), x => x.SemanticName, (x, v) => x.SemanticName = v)
                .AddInput(nameof(InputElementDescription.SemanticIndex), x => x.SemanticIndex, (x, v) => x.SemanticIndex = v)
                .AddInput(nameof(InputElementDescription.Format), x => x.Format, (x, v) => x.Format = v)
                .AddInput(nameof(InputElementDescription.InputSlot), x => x.InputSlot, (x, v) => x.InputSlot = v)
                .AddInput(nameof(InputElementDescription.AlignedByteOffset), x => x.AlignedByteOffset, (x, v) => x.AlignedByteOffset = v)
                .AddInput(nameof(InputElementDescription.InputSlotClass), x => x.InputSlotClass, (x, v) => x.InputSlotClass = v)
                .AddInput(nameof(InputElementDescription.InstanceDataStepRate), x => x.InstanceDataStepRate, (x, v) => x.InstanceDataStepRate = v);

            yield return factory.Create<RenderOutputDescription>(category: graphicsCategory, copyOnWrite: false)
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
        }
    }
}
