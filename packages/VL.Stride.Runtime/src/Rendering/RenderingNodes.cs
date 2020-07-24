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

            yield return NewInputRenderBaseNode<SetRenderTargetsAndViewPort>(factory, category: renderingCategory)
                .AddInput(nameof(SetRenderTargetsAndViewPort.RenderTarget), x => x.RenderTarget, (x, v) => x.RenderTarget = v)
                .AddInput(nameof(SetRenderTargetsAndViewPort.DepthBuffer), x => x.DepthBuffer, (x, v) => x.DepthBuffer = v)
                ;

            yield return NewInputRenderBaseNode<SetRenderView>(factory, category: renderingAdvancedCategory)
                .AddInput(nameof(SetRenderView.RenderView), x => x.RenderView, (x, v) => x.RenderView = v)
                ;
        }

        static CustomNodeDesc<TInputRenderBase> NewInputRenderBaseNode<TInputRenderBase>(IVLNodeDescriptionFactory factory, string category, string name = null)
            where TInputRenderBase : InputRenderBase, new()
        {
            return factory.NewNode<TInputRenderBase>(name: name, category: category, copyOnWrite: false, fragmented: true)
                .AddInput(nameof(InputRenderBase.Input), x => x.Input, (x, v) => x.Input = v)
                ;
        }
    }
}
