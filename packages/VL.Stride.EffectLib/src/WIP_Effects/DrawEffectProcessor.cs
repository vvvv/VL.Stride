using Stride.Core.Annotations;
using Stride.Engine;
using Stride.Rendering;

namespace VL.Stride.Rendering
{
    /// <summary>
    /// The draw effect processor installs for each <see cref="DrawEffectComponent"/> a <see cref="RenderDrawEffect"/> object in its visibility group.
    /// </summary>
    public class DrawEffectProcessor : EntityProcessor<DrawEffectComponent, RenderDrawEffect>, IEntityComponentRenderProcessor
    {
        public DrawEffectProcessor()
        {

        }

        public VisibilityGroup VisibilityGroup { get; set; }

        protected override RenderDrawEffect GenerateComponentData([NotNull] Entity entity, [NotNull] DrawEffectComponent component)
        {
            return new RenderDrawEffect();
        }

        public override void Draw(RenderContext context)
        {
            // Drawing for a processor usually means to add/remove components from the visibility group.
            // But since we're doing custom drawing here we have no idea whether or not the result will be visible or not so we just add/remove them
            // when they're created. See below.


            //// Go thru components of this entity
            //foreach (var entityKeyPair in ComponentDatas)
            //{
            //    var myEntityComponent = entityKeyPair.Key;
            //    var myRenderObject = entityKeyPair.Value;
            //    myRenderObject.Enabled = myEntityComponent.Enabled;
            //    if (myEntityComponent.Enabled)
            //    {
            //        myRenderObject.SingleCallPerFrame = myEntityComponent.SingleCallPerFrame;
            //        myRenderObject.WorldMatrix = myEntityComponent.Entity.Transform.WorldMatrix;
            //        myRenderObject.Layer = myEntityComponent.Layer;
            //    }
            //}

            base.Draw(context);

        }

        protected override void OnEntityComponentAdding(Entity entity, [NotNull] DrawEffectComponent component, [NotNull] RenderDrawEffect data)
        {
            VisibilityGroup.RenderObjects.Add(data);
            base.OnEntityComponentAdding(entity, component, data);
        }

        protected override void OnEntityComponentRemoved(Entity entity, [NotNull] DrawEffectComponent component, [NotNull] RenderDrawEffect data)
        {
            VisibilityGroup.RenderObjects.Remove(data);
            base.OnEntityComponentRemoved(entity, component, data);
        }
    }
}
