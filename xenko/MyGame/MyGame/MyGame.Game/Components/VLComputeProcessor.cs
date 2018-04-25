using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;

namespace MyGame.Components
{
    public class VLComputeRenderProcessor : EntityProcessor<VLComputeComponent, VLComputeRenderObject>, IEntityComponentRenderProcessor
    {
        public VisibilityGroup VisibilityGroup { get; set; }

        public VLComputeRenderProcessor()
            : base(typeof(TransformComponent))
        {
        }

        protected override VLComputeRenderObject GenerateComponentData([NotNull] Entity entity, [NotNull] VLComputeComponent component)
        {
            return component.GetRenderObject();
        }

        protected override bool IsAssociatedDataValid([NotNull] Entity entity, [NotNull] VLComputeComponent component, [NotNull] VLComputeRenderObject associatedData)
        {
            return base.IsAssociatedDataValid(entity, component, associatedData);
        }
    }
}