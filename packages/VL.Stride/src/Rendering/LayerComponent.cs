using Stride.Core;
using Stride.Engine;
using Stride.Engine.Design;

namespace VL.Stride.Rendering
{
    public enum LayerRenderStage
    {
        BeforeScene,
        InScene,
        AfterScene
    }

    /// <summary>
    /// Layer components get picked up by the <see cref="LayerProcessor"/> for low level rendering.
    /// </summary>
    [DataContract(nameof(LayerComponent))]
    [DefaultEntityComponentRenderer(typeof(LayerProcessor))]
    public sealed class LayerComponent : ActivableEntityComponent
    {
        /// <summary>
        /// Gets or sets a value indicating whether this layer should only render once per frame.
        /// i.e. not for each eye in a VR rendering setup.
        /// </summary>
        public bool SingleCallPerFrame { get; set; }

        /// <summary>
        /// Gets or sets a value indicating on which render stage this layer should be rendered.
        /// </summary>
        public LayerRenderStage RenderStage { get; set; } = LayerRenderStage.InScene;

        public LayerComponent()
        {

        }

        /// <summary>
        /// The layer which does the rendering.
        /// </summary>
        [DataMemberIgnore]
        public ILowLevelAPIRender Layer { get; set; }
    }
}
