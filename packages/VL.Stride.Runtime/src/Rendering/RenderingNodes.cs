using Stride.Rendering;
using System.Collections.Generic;
using VL.Core;

namespace VL.Stride.Rendering
{
    static class RenderingNodes
    {
        public static IEnumerable<IVLNodeDescription> GetNodeDescriptions(IVLNodeDescriptionFactory factory)
        {
            var renderingCategory = "Stride.Rendering";
            var renderingAdvancedCategory = $"{renderingCategory}.Advanced";

            yield return NewInputRenderBaseNode<WithRenderTargetAndViewPort>(factory, category: renderingCategory)
                .AddInput(nameof(WithRenderTargetAndViewPort.RenderTarget), x => x.RenderTarget, (x, v) => x.RenderTarget = v)
                .AddInput(nameof(WithRenderTargetAndViewPort.DepthBuffer), x => x.DepthBuffer, (x, v) => x.DepthBuffer = v)
                ;

            yield return NewInputRenderBaseNode<WithRenderView>(factory, category: renderingAdvancedCategory)
                .AddInput(nameof(WithRenderView.RenderView), x => x.RenderView, (x, v) => x.RenderView = v)
                ;

            yield return NewInputRenderBaseNode<WithWindowInputSource>(factory, category: renderingAdvancedCategory)
                .AddInput(nameof(WithWindowInputSource.InputSource), x => x.InputSource, (x, v) => x.InputSource = v)
                ;

            yield return factory.NewNode<GetWindowInputSource>(name: nameof(GetWindowInputSource), category: renderingAdvancedCategory, copyOnWrite: false, fragmented: true)
                .AddInput(nameof(RendererBase.Input), x => x.Input, (x, v) => x.Input = v)
                .AddOutput(nameof(GetWindowInputSource.InputSource), x => x.InputSource)
            ;

            //yield return factory.NewNode<RenderView>(name: "RenderView", category: renderingAdvancedCategory, copyOnWrite: false, fragmented: true)
            //    .AddInput(nameof(RenderView.View), x => x.View, (x, v) => x.View = v)
            //    .AddInput(nameof(RenderView.Projection), x => x.Projection, (x, v) => x.Projection = v)
            //    .AddInput(nameof(RenderView.NearClipPlane), x => x.NearClipPlane, (x, v) => x.NearClipPlane = v)
            //    .AddInput(nameof(RenderView.FarClipPlane), x => x.FarClipPlane, (x, v) => x.FarClipPlane = v)
            //    // TODO: add more
            //    ;

        }

        static CustomNodeDesc<TInputRenderBase> NewInputRenderBaseNode<TInputRenderBase>(IVLNodeDescriptionFactory factory, string category, string name = null)
            where TInputRenderBase : RendererBase, new()
        {
            return factory.NewNode<TInputRenderBase>(name: name, category: category, copyOnWrite: false, fragmented: true)
                .AddInput(nameof(RendererBase.Input), x => x.Input, (x, v) => x.Input = v)
                ;
        }
    }
}
