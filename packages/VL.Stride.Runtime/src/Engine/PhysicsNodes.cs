using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;
using System;
using System.Collections.Generic;
using VL.Core;

namespace VL.Stride.Engine
{
    static class PhysicsNodes
    {
        public static IEnumerable<IVLNodeDescription> GetNodeDescriptions(IVLNodeDescriptionFactory factory)
        {

            var physicsCategory = "Stride.Physics";
            var physicsColliderShapesCategory = $"{physicsCategory}.ColliderShapes";

            yield return NewPhysicsComponentNode<StaticColliderComponent>(factory, physicsCategory)
                .WithEnabledPin();

            yield return NewPhysicsComponentNode<RigidbodyComponent>(factory, physicsCategory)
                .WithEnabledPin();

            yield return NewColliderShapeNode<SphereColliderShapeDesc>(factory, physicsColliderShapesCategory)
                .AddInput(nameof(SphereColliderShapeDesc.Radius), x => x.Radius, (x, v) => x.Radius = v, 0.5f)
                .AddInput(nameof(SphereColliderShapeDesc.LocalOffset), x => x.LocalOffset, (x, v) => x.LocalOffset = v, Vector3.Zero)
                .AddInput(nameof(SphereColliderShapeDesc.Is2D), x => x.Is2D, (x, v) => x.Is2D = v, false)
                ;

            yield return NewColliderShapeNode<BoxColliderShapeDesc>(factory, physicsColliderShapesCategory)
                .AddInput(nameof(BoxColliderShapeDesc.Size), x => x.Size, (x, v) => x.Size = v, Vector3.One)
                .AddInput(nameof(BoxColliderShapeDesc.LocalOffset), x => x.LocalOffset, (x, v) => x.LocalOffset = v, Vector3.Zero)
                .AddInput(nameof(BoxColliderShapeDesc.LocalRotation), x => x.LocalRotation, (x, v) => x.LocalRotation = v, Quaternion.Identity)
                .AddInput(nameof(BoxColliderShapeDesc.Is2D), x => x.Is2D, (x, v) => x.Is2D = v, false)
                ;

            yield return NewColliderShapeNode<StaticPlaneColliderShapeDesc>(factory, physicsColliderShapesCategory)
                .AddInput(nameof(StaticPlaneColliderShapeDesc.Normal), x => x.Normal, (x, v) => x.Normal = v, Vector3.UnitY)
                .AddInput(nameof(StaticPlaneColliderShapeDesc.Offset), x => x.Offset, (x, v) => x.Offset = v, 0f)
                ;
        }

        static CustomNodeDesc<TColliderShape> NewColliderShapeNode<TColliderShape>(IVLNodeDescriptionFactory factory, string category, string name = null)
            where TColliderShape : IInlineColliderShapeDesc, new()
        {
            return factory.NewNode<TColliderShape>(name: name, category: category, copyOnWrite: true, fragmented: false);
        }

        static CustomNodeDesc<TPhysicsComponent> NewPhysicsComponentNode<TPhysicsComponent>(IVLNodeDescriptionFactory factory, string category)
            where TPhysicsComponent : PhysicsComponent, new()
        {
            return factory.NewComponentNode<TPhysicsComponent>(category: category)
                .AddListInput(nameof(PhysicsComponent.ColliderShapes), x => x.ColliderShapes, ColliderShapeChanged)
                .AddInput(nameof(PhysicsComponent.Friction), x => x.Friction, (x, v) => x.Friction = 0.5f)
                .AddInput(nameof(PhysicsComponent.RollingFriction), x => x.RollingFriction, (x, v) => x.RollingFriction = v, 0f)
                .AddInput(nameof(PhysicsComponent.Restitution), x => x.Restitution, (x, v) => x.Restitution = v, 0.5f)
                ;
        }

        private static void ColliderShapeChanged<TPhysicsComponent>(TPhysicsComponent x) where TPhysicsComponent : PhysicsComponent, new()
        {
            if (x.ColliderShapes.Count > 0) // stride crashes when collider shape set to null
            {
                if (x.ColliderShape != null)
                {
                    // preserve scaling
                    var scaling = x.ColliderShape.Scaling;
                    x.ComposeShape();
                    x.ColliderShape.Scaling = scaling; 
                }
                else
                {
                    x.ComposeShape();
                }
            }
        }
    }
}