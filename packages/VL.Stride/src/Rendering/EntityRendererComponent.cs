using Stride.Core;
using Stride.Engine;
using Stride.Engine.Design;
using Stride.Rendering;

namespace VL.Stride.Rendering
{
    public enum DrawerRenderStage
    {
        BeforeScene,
        Opaque,
        Transparent,
        AfterScene
    }

    /// <summary>
    /// Renderer components get picked up by the <see cref="EntityRendererProcessor"/> for low level rendering.
    /// </summary>
    [DataContract(nameof(EntityRendererComponent))]
    [DefaultEntityComponentRenderer(typeof(EntityRendererProcessor))]
    public sealed class EntityRendererComponent : ActivableEntityComponent
    {
        /// <summary>
        /// Gets or sets a value indicating whether this renderer should only render once per frame.
        /// i.e. not for each eye in a VR rendering setup.
        /// </summary>
        public bool SingleCallPerFrame { get; set; }

        /// <summary>
        /// Gets or sets a value indicating on which render stage this renderer should be rendered.
        /// </summary>
        public DrawerRenderStage RenderStage { get; set; } = DrawerRenderStage.Opaque;

        public EntityRendererComponent()
        {

        }

        /// <summary>
        /// The renderer which does the rendering.
        /// </summary>
        [DataMemberIgnore]
        public IGraphicsRendererBase Renderer { get; set; }
    }
}
