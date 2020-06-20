using Stride.Core.Annotations;
using Stride.Engine;
using Stride.Rendering;

namespace VL.Stride.Rendering
{
    /// <summary>
    /// The layer processor installs for each <see cref="LayerComponent"/> a <see cref="RenderInScene"/> object in its visibility group.
    /// </summary>
    public class LayerProcessor : EntityProcessor<LayerComponent, LayerProcessor.LayerData>, IEntityComponentRenderProcessor
    {
        public class LayerData
        {
            public LayerRenderStage RenderStage;
            public RenderLayerBase RenderLayer;
        }

        public VisibilityGroup VisibilityGroup { get; set; }

        protected override LayerData GenerateComponentData([NotNull] Entity entity, [NotNull] LayerComponent component)
        {
            var data = new LayerData() { RenderStage = component.RenderStage };
            return data;
        }

        private static RenderLayerBase CreateRenderObject(LayerRenderStage renderStage)
        {
            switch (renderStage)
            {
                case LayerRenderStage.BeforeScene:
                    return new RenderBeforeScene();
                case LayerRenderStage.InScene:
                    return new RenderInScene();
                case LayerRenderStage.AfterScene:
                    return new RenderAfterScene();
                default:
                    return new RenderInScene();
            }
        }

        protected override bool IsAssociatedDataValid([NotNull] Entity entity, [NotNull] LayerComponent component, [NotNull] LayerData associatedData)
        {
            return associatedData.RenderStage == component.RenderStage;
        }

        public override void Draw(RenderContext context)
        {
            // Go thru components
            foreach (var entityKeyPair in ComponentDatas)
            {
                var component = entityKeyPair.Key;
                var layerData = entityKeyPair.Value;

                // Component was just added
                if (layerData.RenderLayer == null)
                {
                    CreateAndAddRenderObject(layerData);
                }

                // Stage has changed
                if (layerData.RenderStage != component.RenderStage)
                {
                    VisibilityGroup.RenderObjects.Remove(layerData.RenderLayer);
                    layerData.RenderStage = component.RenderStage;
                    CreateAndAddRenderObject(layerData);
                }

                var layer = layerData.RenderLayer;
                layer.Enabled = component.Enabled;
                if (component.Enabled)
                {
                    // Update world matrix befor rendering
                    component.Layer?.SetEntityWorldMatrix(component.Entity.Transform.WorldMatrix);
                    layer.SingleCallPerFrame = component.SingleCallPerFrame;
                    layer.Layer = component.Layer;
                }
            }

            base.Draw(context);
        }

        private void CreateAndAddRenderObject(LayerData layerData)
        {
            layerData.RenderLayer = CreateRenderObject(layerData.RenderStage);
            VisibilityGroup.RenderObjects.Add(layerData.RenderLayer);
        }

        protected override void OnEntityComponentAdding(Entity entity, [NotNull] LayerComponent component, [NotNull] LayerData data)
        {
            base.OnEntityComponentAdding(entity, component, data);
        }

        protected override void OnEntityComponentRemoved(Entity entity, [NotNull] LayerComponent component, [NotNull] LayerData data)
        {
            if (data.RenderLayer != null)
                VisibilityGroup.RenderObjects.Remove(data.RenderLayer);

            base.OnEntityComponentRemoved(entity, component, data);
        }
    }
}
