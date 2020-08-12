using Stride.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using VL.Core;

namespace VL.Stride.Graphics
{
    static class GraphicsNodes
    {
        public static IEnumerable<IVLNodeDescription> GetNodeDescriptions(IVLNodeDescriptionFactory factory)
        {
            var graphicsCategory = "Stride.API.Graphics.Advanced";

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

            yield return factory.NewNode<InputElementDescription>(category: graphicsCategory, copyOnWrite: false, fragmented: true)
                .AddInput(nameof(InputElementDescription.SemanticName), x => x.SemanticName, (x, v) => x.SemanticName = v)
                .AddInput(nameof(InputElementDescription.SemanticIndex), x => x.SemanticIndex, (x, v) => x.SemanticIndex = v)
                .AddInput(nameof(InputElementDescription.Format), x => x.Format, (x, v) => x.Format = v)
                .AddInput(nameof(InputElementDescription.InputSlot), x => x.InputSlot, (x, v) => x.InputSlot = v)
                .AddInput(nameof(InputElementDescription.AlignedByteOffset), x => x.AlignedByteOffset, (x, v) => x.AlignedByteOffset = v)
                .AddInput(nameof(InputElementDescription.InputSlotClass), x => x.InputSlotClass, (x, v) => x.InputSlotClass = v)
                .AddInput(nameof(InputElementDescription.InstanceDataStepRate), x => x.InstanceDataStepRate, (x, v) => x.InstanceDataStepRate = v);

            yield return factory.NewNode<RenderOutputDescription>(category: graphicsCategory, copyOnWrite: false, fragmented: true)
                .AddInput<CommandList>("Input", x => default, (x, v) => x.CaptureState(v), equals: (a, b) => false /* Always need to capture */);

            yield return factory.NewNode<RenderOutputDescription>(name: "RenderOutputDescription (Manually)", category: graphicsCategory, copyOnWrite: false, fragmented: true)
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

            yield return factory.NewNode(category: graphicsCategory, copyOnWrite: false, fragmented: true, ctor: () => BlendStateDescription.Default)
                .AddInput(nameof(BlendStateDescription.AlphaToCoverageEnable), x => x.AlphaToCoverageEnable, (x, v) => x.AlphaToCoverageEnable = v, false)
                .AddInput(nameof(BlendStateDescription.IndependentBlendEnable), x => x.IndependentBlendEnable, (x, v) => x.IndependentBlendEnable = v, false)
                .AddInput(nameof(BlendStateDescription.RenderTarget0), x => x.RenderTarget0, (x, v) => x.RenderTarget0 = v)
                .AddInput(nameof(BlendStateDescription.RenderTarget1), x => x.RenderTarget1, (x, v) => x.RenderTarget1 = v)
                .AddInput(nameof(BlendStateDescription.RenderTarget2), x => x.RenderTarget2, (x, v) => x.RenderTarget2 = v)
                .AddInput(nameof(BlendStateDescription.RenderTarget3), x => x.RenderTarget3, (x, v) => x.RenderTarget3 = v)
                .AddInput(nameof(BlendStateDescription.RenderTarget4), x => x.RenderTarget4, (x, v) => x.RenderTarget4 = v)
                .AddInput(nameof(BlendStateDescription.RenderTarget5), x => x.RenderTarget5, (x, v) => x.RenderTarget5 = v)
                .AddInput(nameof(BlendStateDescription.RenderTarget6), x => x.RenderTarget6, (x, v) => x.RenderTarget6 = v)
                .AddInput(nameof(BlendStateDescription.RenderTarget7), x => x.RenderTarget7, (x, v) => x.RenderTarget7 = v);


            yield return factory.NewNode(category: graphicsCategory, copyOnWrite: false, fragmented: true,
                ctor: () => new BlendStateRenderTargetDescription() 
                { 
                    ColorSourceBlend = Blend.One,
                    ColorDestinationBlend = Blend.Zero,
                    ColorBlendFunction = BlendFunction.Add,
                    AlphaSourceBlend = Blend.One,
                    AlphaDestinationBlend = Blend.Zero,
                    AlphaBlendFunction = BlendFunction.Add,
                    ColorWriteChannels = ColorWriteChannels.All
                })
                .AddInput(nameof(BlendStateRenderTargetDescription.BlendEnable), x => x.BlendEnable, (x, v) => x.BlendEnable = v, false)
                .AddInput(nameof(BlendStateRenderTargetDescription.ColorSourceBlend), x => x.ColorSourceBlend, (x, v) => x.ColorSourceBlend = v, Blend.One)
                .AddInput(nameof(BlendStateRenderTargetDescription.ColorDestinationBlend), x => x.ColorDestinationBlend, (x, v) => x.ColorDestinationBlend = v, Blend.Zero)
                .AddInput(nameof(BlendStateRenderTargetDescription.ColorBlendFunction), x => x.ColorBlendFunction, (x, v) => x.ColorBlendFunction = v, BlendFunction.Add)
                .AddInput(nameof(BlendStateRenderTargetDescription.AlphaSourceBlend), x => x.AlphaSourceBlend, (x, v) => x.AlphaSourceBlend = v, Blend.One)
                .AddInput(nameof(BlendStateRenderTargetDescription.AlphaDestinationBlend), x => x.AlphaDestinationBlend, (x, v) => x.AlphaDestinationBlend = v, Blend.Zero)
                .AddInput(nameof(BlendStateRenderTargetDescription.AlphaBlendFunction), x => x.AlphaBlendFunction, (x, v) => x.AlphaBlendFunction = v, BlendFunction.Add)
                .AddInput(nameof(BlendStateRenderTargetDescription.ColorWriteChannels), x => x.ColorWriteChannels, (x, v) => x.ColorWriteChannels = v, ColorWriteChannels.All);

            yield return factory.NewNode(category: graphicsCategory, copyOnWrite: false, fragmented: true, ctor: () => RasterizerStateDescription.Default)
                .AddInput(nameof(RasterizerStateDescription.FillMode), x => x.FillMode, (x, v) => x.FillMode = v, FillMode.Solid)
                .AddInput(nameof(RasterizerStateDescription.CullMode), x => x.CullMode, (x, v) => x.CullMode = v, CullMode.Back)
                .AddInput(nameof(RasterizerStateDescription.DepthClipEnable), x => x.DepthClipEnable, (x, v) => x.DepthClipEnable = v, true)
                .AddInput(nameof(RasterizerStateDescription.FrontFaceCounterClockwise), x => x.FrontFaceCounterClockwise, (x, v) => x.FrontFaceCounterClockwise = v, false)
                .AddInput(nameof(RasterizerStateDescription.ScissorTestEnable), x => x.ScissorTestEnable, (x, v) => x.ScissorTestEnable = v, false)
                .AddInput(nameof(RasterizerStateDescription.MultisampleCount), x => x.MultisampleCount, (x, v) => x.MultisampleCount = v, MultisampleCount.None)
                .AddInput(nameof(RasterizerStateDescription.MultisampleAntiAliasLine), x => x.MultisampleAntiAliasLine, (x, v) => x.MultisampleAntiAliasLine = v, false)
                .AddInput(nameof(RasterizerStateDescription.DepthBias), x => x.DepthBias, (x, v) => x.DepthBias = v, 0)
                .AddInput(nameof(RasterizerStateDescription.DepthBiasClamp), x => x.DepthBiasClamp, (x, v) => x.DepthBiasClamp = v, 0f)
                .AddInput(nameof(RasterizerStateDescription.SlopeScaleDepthBias), x => x.SlopeScaleDepthBias, (x, v) => x.SlopeScaleDepthBias = v, 0f);

            yield return factory.NewNode(category: graphicsCategory, copyOnWrite: false, fragmented: true, ctor: () => DepthStencilStateDescription.Default)
                .AddInput(nameof(DepthStencilStateDescription.DepthBufferEnable), x => x.DepthBufferEnable, (x, v) => x.DepthBufferEnable = v, true)
                .AddInput(nameof(DepthStencilStateDescription.DepthBufferWriteEnable), x => x.DepthBufferWriteEnable, (x, v) => x.DepthBufferWriteEnable = v, true)
                .AddInput(nameof(DepthStencilStateDescription.DepthBufferFunction), x => x.DepthBufferFunction, (x, v) => x.DepthBufferFunction = v, CompareFunction.LessEqual)
                .AddInput(nameof(DepthStencilStateDescription.StencilEnable), x => x.StencilEnable, (x, v) => x.StencilEnable = v, false)
                .AddInput(nameof(DepthStencilStateDescription.FrontFace), x => x.FrontFace, (x, v) => x.FrontFace = v)
                .AddInput(nameof(DepthStencilStateDescription.BackFace), x => x.BackFace, (x, v) => x.BackFace = v)
                .AddInput(nameof(DepthStencilStateDescription.StencilMask), x => x.StencilMask, (x, v) => x.StencilMask = v, byte.MaxValue)
                .AddInput(nameof(DepthStencilStateDescription.StencilWriteMask), x => x.StencilWriteMask, (x, v) => x.StencilWriteMask = v, byte.MaxValue);

            yield return factory.NewNode(category: graphicsCategory, copyOnWrite: false, fragmented: true,
                ctor: () => new DepthStencilStencilOpDescription()
                {
                    StencilFunction = CompareFunction.Always,
                    StencilPass = StencilOperation.Keep,
                    StencilFail = StencilOperation.Keep,
                    StencilDepthBufferFail = StencilOperation.Keep
                })
                .AddInput(nameof(DepthStencilStencilOpDescription.StencilFunction), x => x.StencilFunction, (x, v) => x.StencilFunction = v, CompareFunction.Always)
                .AddInput(nameof(DepthStencilStencilOpDescription.StencilPass), x => x.StencilPass, (x, v) => x.StencilPass = v, StencilOperation.Keep)
                .AddInput(nameof(DepthStencilStencilOpDescription.StencilFail), x => x.StencilFail, (x, v) => x.StencilFail = v, StencilOperation.Keep)
                .AddInput(nameof(DepthStencilStencilOpDescription.StencilDepthBufferFail), x => x.StencilDepthBufferFail, (x, v) => x.StencilDepthBufferFail = v, StencilOperation.Keep);
        }
    }
}
