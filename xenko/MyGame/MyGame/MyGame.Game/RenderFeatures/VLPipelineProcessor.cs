
using Xenko.Graphics;
using Xenko.Rendering;

namespace MyGame
{
    public class VLPipelineProcessor : PipelineProcessor
    {
        public RenderStage RenderStage { get; set; }

        public override void Process(RenderNodeReference renderNodeReference, ref RenderNode renderNode, RenderObject renderObject, PipelineStateDescription pipelineState)
        {
            if (renderNode.RenderStage == RenderStage)
            {
                pipelineState.RasterizerState = RasterizerStates.Wireframe;
            }
        }
    }
}