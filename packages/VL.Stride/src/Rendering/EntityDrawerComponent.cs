using Stride.Core;
using Stride.Engine;
using Stride.Engine.Design;

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
    /// Layer components get picked up by the <see cref="LayerProcessor"/> for low level rendering.
    /// </summary>
    [DataContract(nameof(EntityDrawerComponent))]
    [DefaultEntityComponentRenderer(typeof(EntityDrawerProcessor))]
    public sealed class EntityDrawerComponent : ActivableEntityComponent
    {
        /// <summary>
        /// Gets or sets a value indicating whether this layer should only render once per frame.
        /// i.e. not for each eye in a VR rendering setup.
        /// </summary>
        public bool SingleCallPerFrame { get; set; }

        /// <summary>
        /// Gets or sets a value indicating on which render stage this layer should be rendered.
        /// </summary>
        public DrawerRenderStage RenderStage { get; set; } = DrawerRenderStage.Opaque;

        public EntityDrawerComponent()
        {

        }

        /// <summary>
        /// The layer which does the rendering.
        /// </summary>
        [DataMemberIgnore]
        public IEntityDrawer Drawer { get; set; }
    }
}
