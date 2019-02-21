using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VL.Xenko.RootRenderFeatures;
using Xenko.Core;
using Xenko.Engine;
using Xenko.Engine.Design;

namespace VL.Xenko.Rendering
{
    /// <summary>
    /// Darw effect components get picked up by the <see cref="DrawEffectProcessor"/> for low level rendering.
    /// </summary>
    [DataContract(nameof(DrawEffectComponent))]
    [DefaultEntityComponentRenderer(typeof(DrawEffectProcessor))]
    public class DrawEffectComponent : ActivableEntityComponent
    {
        /// <summary>
        /// The effect to use for drawing.
        /// </summary>
        public string EffectName { get; set; }

        /// <summary>
        /// The layer which does the drawing using the specified effect.
        /// </summary>
        [DataMemberIgnore]
        public ILowLevelAPIRender Layer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this layer should only render once per frame.
        /// i.e. not for each eye in a VR rendering setup.
        /// </summary>
        public bool SingleCallPerFrame { get; set; }
    }
}
