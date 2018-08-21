using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xenko.Graphics;
using Xenko.Rendering;

namespace VL.Xenko.RootRenderFeatures
{
    public interface ILowLevelAPIRender
    {
        void Initialize();

        void Collect(RenderContext context);

        void Extract();

        void Prepare(RenderDrawContext context);

        void Draw(RenderContext renderContext, RenderDrawContext drawContext, RenderView renderView, RenderViewStage renderViewStage, CommandList commandList);
    }

    public class VLLowLevelAPIRenderFeature : RootRenderFeature
    {
        public override Type SupportedRenderObjectType => typeof(VLLowLevelAPIRenderObject);

        public ILowLevelAPIRender Renderer { get; set; }

        VisibilityGroup VisibilityGroup;

        public VLLowLevelAPIRenderFeature()
        {
            //pre adjust render priority, low numer is early, high number is late (advantage of backbuffer culling)
            SortKey = 128;
        }

        protected override void InitializeCore()
        {
            Renderer?.Initialize();
        }

        public override void Collect()
        {
            Renderer?.Collect(Context);
        }

        public override void Extract()
        {
            Renderer?.Extract();
        }

        public override void Prepare(RenderDrawContext context)
        {
            // Wait until visibility group is available
            if (VisibilityGroup == null)
            {
                VisibilityGroup = Context.SceneInstance.VisibilityGroups.FirstOrDefault();
                if(VisibilityGroup != null)
                {
                    //add one render object to the visibility group
                    var renderObject = new VLLowLevelAPIRenderObject();
                    VisibilityGroup.RenderObjects.Add(renderObject);
                }
            }

            Renderer?.Prepare(context);
        }

        public override void Draw(RenderDrawContext context, RenderView renderView, RenderViewStage renderViewStage)
        {
            try
            {
                Renderer?.Draw(Context, context, renderView, renderViewStage, context.CommandList);
            }
            catch (Exception e)
            {
                var inner = e.InnerException;
                while (inner != null)
                {
                    e = inner;
                    inner = e.InnerException;
                }

                System.Diagnostics.Debug.WriteLine(e.Message);
                //
            }
        }

    }
        
}
