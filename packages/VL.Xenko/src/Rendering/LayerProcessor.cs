using Xenko.Core.Annotations;
using Xenko.Engine;
using Xenko.Rendering;

namespace VL.Xenko.Rendering
{
    /// <summary>
    /// The layer processor installs for each <see cref="LayerComponent"/> a <see cref="RenderLayer"/> object in its visibility group.
    /// </summary>
    public class LayerProcessor : EntityProcessor<LayerComponent, RenderLayer>, IEntityComponentRenderProcessor
    {
        public LayerProcessor()
        {

        }

        public VisibilityGroup VisibilityGroup { get; set; }

        protected override RenderLayer GenerateComponentData([NotNull] Entity entity, [NotNull] LayerComponent component)
        {
            return new RenderLayer();
        }

        public override void Draw(RenderContext context)
        {
            // Drawing for a processor usually means to add/remove components from the visibility group.
            // But since we're talking about layers here we have no idea whether or not their rendering will be visible or not so we just add/remove them
            // when they're created. See below.


            // Go thru components of this entity
            foreach (var entityKeyPair in ComponentDatas)
            {
                var myEntityComponent = entityKeyPair.Key;
                var myRenderObject = entityKeyPair.Value;
                myRenderObject.Enabled = myEntityComponent.Enabled;
                if (myEntityComponent.Enabled)
                {
                    myRenderObject.SingleCallPerFrame = myEntityComponent.SingleCallPerFrame;
                    myRenderObject.WorldMatrix = myEntityComponent.Entity.Transform.WorldMatrix;
                    myRenderObject.Layer = myEntityComponent.Layer;
                }
            }

            base.Draw(context);

        }

        protected override void OnEntityComponentAdding(Entity entity, [NotNull] LayerComponent component, [NotNull] RenderLayer data)
        {
            VisibilityGroup.RenderObjects.Add(data);
            base.OnEntityComponentAdding(entity, component, data);
        }

        protected override void OnEntityComponentRemoved(Entity entity, [NotNull] LayerComponent component, [NotNull] RenderLayer data)
        {
            VisibilityGroup.RenderObjects.Remove(data);
            base.OnEntityComponentRemoved(entity, component, data);
        }
    }
}
