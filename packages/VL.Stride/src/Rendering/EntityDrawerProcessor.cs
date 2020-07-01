using Stride.Core.Annotations;
using Stride.Engine;
using Stride.Rendering;

namespace VL.Stride.Rendering
{
    /// <summary>
    /// The layer processor installs for each <see cref="LayerComponent"/> a <see cref="RenderInScene"/> object in its visibility group.
    /// </summary>
    public class EntityDrawerProcessor : EntityProcessor<EntityDrawerComponent, EntityDrawerProcessor.DrawerData>, IEntityComponentRenderProcessor
    {
        public class DrawerData
        {
            public DrawerRenderStage RenderStage;
            public RenderDrawer RenderDrawer;
        }

        public VisibilityGroup VisibilityGroup { get; set; }

        protected override DrawerData GenerateComponentData([NotNull] Entity entity, [NotNull] EntityDrawerComponent component)
        {
            var data = new DrawerData() { RenderStage = component.RenderStage };
            return data;
        }

        protected override bool IsAssociatedDataValid([NotNull] Entity entity, [NotNull] EntityDrawerComponent component, [NotNull] DrawerData associatedData)
        {
            return associatedData.RenderStage == component.RenderStage;
        }

        public override void Draw(RenderContext context)
        {
            // Go thru components
            foreach (var entityKeyPair in ComponentDatas)
            {
                var component = entityKeyPair.Key;
                var drawerData = entityKeyPair.Value;

                // Component was just added
                if (drawerData.RenderDrawer == null)
                {
                    CreateAndAddRenderObject(drawerData);
                }

                // Stage has changed
                if (drawerData.RenderStage != component.RenderStage)
                {
                    VisibilityGroup.RenderObjects.Remove(drawerData.RenderDrawer);
                    drawerData.RenderStage = component.RenderStage;
                    CreateAndAddRenderObject(drawerData);
                }

                var renderDrawer = drawerData.RenderDrawer;
                renderDrawer.Enabled = component.Enabled;
                if (component.Enabled)
                {
                    renderDrawer.SingleCallPerFrame = component.SingleCallPerFrame;
                    renderDrawer.Renderer = component.Drawer?.Process(component.Entity);
                }
            }

            base.Draw(context);
        }

        private void CreateAndAddRenderObject(DrawerData drawerData)
        {
            drawerData.RenderDrawer = new RenderDrawer() { RenderStage = drawerData.RenderStage };
            VisibilityGroup.RenderObjects.Add(drawerData.RenderDrawer);
        }

        protected override void OnEntityComponentAdding(Entity entity, [NotNull] EntityDrawerComponent component, [NotNull] DrawerData data)
        {
            base.OnEntityComponentAdding(entity, component, data);
        }

        protected override void OnEntityComponentRemoved(Entity entity, [NotNull] EntityDrawerComponent component, [NotNull] DrawerData data)
        {
            if (data.RenderDrawer != null)
                VisibilityGroup.RenderObjects.Remove(data.RenderDrawer);

            base.OnEntityComponentRemoved(entity, component, data);
        }
    }
}
