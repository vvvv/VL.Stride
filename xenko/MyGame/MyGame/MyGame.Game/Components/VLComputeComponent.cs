using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame.Components
{
    [DataContract("VLComputeComponent")]
    [Display("VLCompute", Expand = ExpandRule.Once)]
    [DefaultEntityComponentProcessor(typeof(VLComputeRenderProcessor))]
    [DefaultEntityComponentRenderer(typeof(VLComputeRenderProcessor))]
    [ComponentOrder(10220)]
    public class VLComputeComponent : ActivableEntityComponent
    {
        VLComputeRenderObject renderObject = new VLComputeRenderObject();

        internal VLComputeRenderObject GetRenderObject()
        {
            return renderObject;
        }
    }
}
