using Stride.Core.Annotations;
using Stride.Engine;
using Stride.Rendering;

namespace VL.Stride.Rendering
{
    /// <summary>
    /// The entity renderer processor installs for each <see cref="EntityRendererComponent"/> a <see cref="RenderRenderer"/> object in its visibility group.
    /// </summary>
    public class EntityRendererProcessor : EntityProcessor<EntityRendererComponent, EntityRendererProcessor.RendererData>, IEntityComponentRenderProcessor
    {
        public class RendererData
        {
            public DrawerRenderStage RenderStage;
            public RenderRenderer RenderRenderer;
        }

        public VisibilityGroup VisibilityGroup { get; set; }

        protected override RendererData GenerateComponentData([NotNull] Entity entity, [NotNull] EntityRendererComponent component)
        {
            var data = new RendererData() { RenderStage = component.RenderStage };
            return data;
        }

        protected override bool IsAssociatedDataValid([NotNull] Entity entity, [NotNull] EntityRendererComponent component, [NotNull] RendererData associatedData)
        {
            return associatedData.RenderStage == component.RenderStage;
        }

        public override void Draw(RenderContext context)
        {
            // Go thru components
            foreach (var entityKeyPair in ComponentDatas)
            {
                var component = entityKeyPair.Key;
                var rendererData = entityKeyPair.Value;

                // Component was just added
                if (rendererData.RenderRenderer == null)
                {
                    CreateAndAddRenderObject(rendererData);
                }

                // Stage has changed
                if (rendererData.RenderStage != component.RenderStage)
                {
                    VisibilityGroup.RenderObjects.Remove(rendererData.RenderRenderer);
                    rendererData.RenderStage = component.RenderStage;
                    CreateAndAddRenderObject(rendererData);
                }

                var renderRenderer = rendererData.RenderRenderer;
                renderRenderer.Enabled = component.Enabled && component.Renderer != null;
                if (renderRenderer.Enabled)
                {
                    renderRenderer.SingleCallPerFrame = component.SingleCallPerFrame;
                    renderRenderer.Renderer = component.Renderer;
                }
            }

            base.Draw(context);
        }

        private void CreateAndAddRenderObject(RendererData rendererData)
        {
            rendererData.RenderRenderer = new RenderRenderer() { RenderStage = rendererData.RenderStage };
            VisibilityGroup.RenderObjects.Add(rendererData.RenderRenderer);
        }

        protected override void OnEntityComponentAdding(Entity entity, [NotNull] EntityRendererComponent component, [NotNull] RendererData data)
        {
            base.OnEntityComponentAdding(entity, component, data);
        }

        protected override void OnEntityComponentRemoved(Entity entity, [NotNull] EntityRendererComponent component, [NotNull] RendererData data)
        {
            if (data.RenderRenderer != null)
                VisibilityGroup.RenderObjects.Remove(data.RenderRenderer);

            base.OnEntityComponentRemoved(entity, component, data);
        }
    }
}
