using Xenko.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xenko.Graphics;

namespace ComputeParticlesTest
{
    public partial class VLRootRenderFeature : RootEffectRenderFeature
    {
        public override Type SupportedRenderObjectType => typeof(object);

        public ICanRender Renderer { get; set; }

        public VLRootRenderFeature()
            : base()
        {
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();
        }

        public override void Collect()
        {
            base.Collect();
        }

        public override void Extract()
        {
            base.Extract();
        }

        public override void Prepare(RenderDrawContext context)
        {
            base.Prepare(context);
        }

        public override void Draw(RenderDrawContext context, RenderView renderView, RenderViewStage renderViewStage)
        {
            base.Draw(context, renderView, renderViewStage);
        }

        public override void PrepareEffectPermutations(RenderDrawContext context)
        {
            base.PrepareEffectPermutations(context);
        }

        public override void PrepareEffectPermutationsImpl(RenderDrawContext context)
        {
            base.PrepareEffectPermutationsImpl(context);
        }

        protected override void ProcessPipelineState(RenderContext context, RenderNodeReference renderNodeReference, ref RenderNode renderNode, RenderObject renderObject, PipelineStateDescription pipelineState)
        {
            base.ProcessPipelineState(context, renderNodeReference, ref renderNode, renderObject, pipelineState);
        }

        protected override void InvalidateEffectPermutation(RenderObject renderObject, RenderEffect renderEffect)
        {
            base.InvalidateEffectPermutation(renderObject, renderEffect);
        }

        protected override void OnAddRenderObject(RenderObject renderObject)
        {
            base.OnAddRenderObject(renderObject);
        }

        protected override int ComputeDataArrayExpectedSize(DataType type)
        {
            return base.ComputeDataArrayExpectedSize(type);
        }
    }
}
