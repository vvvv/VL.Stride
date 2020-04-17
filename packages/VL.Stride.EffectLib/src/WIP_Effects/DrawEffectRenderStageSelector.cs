using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xenko.Engine;
using Xenko.Rendering;

namespace VL.Xenko.Rendering
{
    public class DrawEffectRenderStageSelector : RenderStageSelector
    {
        [DefaultValue(RenderGroupMask.All)]
        public RenderGroupMask RenderGroup { get; set; } = RenderGroupMask.All;

        public RenderStage RenderStage { get; set; }

        public override void Process(RenderObject renderObject)
        {
            if (RenderStage != null && ((RenderGroupMask)(1U << (int)renderObject.RenderGroup) & RenderGroup) != 0)
            {
                var renderDrawEffect = (RenderDrawEffect)renderObject;
                renderObject.ActiveRenderStages[RenderStage.Index] = new ActiveRenderStage(renderDrawEffect.EffectName);
            }
        }
    }
}
