using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;
using System;
using System.Collections.Generic;
using VL.Core;
using VL.Lib.Collections;

namespace VL.Stride.Engine
{
    static class PhysicsNodes
    {
        public static IEnumerable<IVLNodeDescription> GetNodeDescriptions(IVLNodeDescriptionFactory factory)
        {

            var physicsCategory = "Stride.Physics";
            var physicsColliderShapesCategory = $"{physicsCategory}.ColliderShapes";

            yield return NewPhysicsComponentNode<StaticColliderComponent>(factory, physicsCategory)
                .AddColliderParams()
                .AddSimulationParams()
                //.AddInput(nameof(StaticColliderComponent.AlwaysUpdateNaviMeshCache), x => x.AlwaysUpdateNaviMeshCache, (x, v) => x.AlwaysUpdateNaviMeshCache = v)
                .WithEnabledPin();

            yield return NewRigidbodyComponentNode(factory, physicsCategory, name: "DynamicColliderComponent")
                .AddRigidBodyParams()
                .AddColliderParams()
                .AddSimulationParams()
                .WithEnabledPin();

            yield return NewRigidbodyComponentNode(factory, physicsCategory, name: "KinematicColliderComponent", isKinematic: true)
                .AddRigidBodyParams()
                .AddColliderParams()
                .AddSimulationParams()
                .WithEnabledPin();

            yield return NewRigidbodyComponentNode(factory, physicsCategory, name: "TriggerColliderComponent", isTrigger: true)
                .AddRigidBodyParams()
                .AddColliderParams()
                .AddSimulationParams()
                .WithEnabledPin();

            yield return NewColliderShapeNode<SphereColliderShapeDesc>(factory, physicsColliderShapesCategory)
                .AddInput(nameof(SphereColliderShapeDesc.Radius), x => x.Radius, (x, v) => x.Radius = v, 0.5f)
                .AddInput(nameof(SphereColliderShapeDesc.LocalOffset), x => x.LocalOffset, (x, v) => x.LocalOffset = v, Vector3.Zero)
                .AddInput(nameof(SphereColliderShapeDesc.Is2D), x => x.Is2D, (x, v) => x.Is2D = v, false)
                ;

            yield return NewColliderShapeNode<CapsuleColliderShapeDesc>(factory, physicsColliderShapesCategory)
                .AddInput(nameof(CapsuleColliderShapeDesc.Radius), x => x.Radius, (x, v) => x.Radius = v, 0.5f)
                .AddInput(nameof(CapsuleColliderShapeDesc.Length), x => x.Length, (x, v) => x.Length = v, 0.5f)
                .AddInput(nameof(CapsuleColliderShapeDesc.Orientation), x => x.Orientation, (x, v) => x.Orientation = v, ShapeOrientation.UpY)
                .AddInput(nameof(CapsuleColliderShapeDesc.LocalOffset), x => x.LocalOffset, (x, v) => x.LocalOffset = v, Vector3.Zero)
                .AddInput(nameof(CapsuleColliderShapeDesc.LocalRotation), x => x.LocalRotation, (x, v) => x.LocalRotation = v, Quaternion.Identity)
                .AddInput(nameof(CapsuleColliderShapeDesc.Is2D), x => x.Is2D, (x, v) => x.Is2D = v, false)
                ;

            yield return NewColliderShapeNode<BoxColliderShapeDesc>(factory, physicsColliderShapesCategory, "CubeColliderShapeDesc")
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
            return factory.NewNode<TColliderShape>(name: name, category: category, copyOnWrite: true, hasStateOutput: true, fragmented: true)
                //.AddCachedOutput("Output", x => Spread.Create<IInlineColliderShapeDesc>(x))
                ;
        }

        // Physics component defaults that differ from strides
        static class Defaults
        {
            public const float Friction = 0.1f;
            public const float RollingFriction = 0.1f;
            public const float Restitution = 0.5f;
        }

        static CustomNodeDesc<TPhysicsComponent> NewPhysicsComponentNode<TPhysicsComponent>(IVLNodeDescriptionFactory factory, string category, string name = null, Action<TPhysicsComponent> init = null)
            where TPhysicsComponent : PhysicsComponent, new()
        {
            init ??= InitPhysicsComponent;
            return factory.NewComponentNode(name: name, category: category, init: init)
                .AddListInput(nameof(PhysicsComponent.ColliderShapes), x => x.ColliderShapes, ColliderShapeChanged)
                ;
        }

        static CustomNodeDesc<RigidbodyComponent> NewRigidbodyComponentNode(IVLNodeDescriptionFactory factory, string category, string name = null, bool isKinematic = false, bool isTrigger = false)

        {
            Action<RigidbodyComponent> init = x =>
            {
                InitPhysicsComponent(x);
                x.IsKinematic = isKinematic;
                x.IsTrigger = isTrigger;
            };
            return NewPhysicsComponentNode(factory, name: name, category: category, init: init)
                ;
        }

        static CustomNodeDesc<TPhysicsComponent> AddColliderParams<TPhysicsComponent>(this CustomNodeDesc<TPhysicsComponent> nodeDesc)
            where TPhysicsComponent : PhysicsComponent, new()
        {
            return nodeDesc
                .AddInput(nameof(PhysicsComponent.Restitution), x => x.Restitution, (x, v) => x.Restitution = v, Defaults.Restitution)
                .AddInput(nameof(PhysicsComponent.Friction), x => x.Friction, (x, v) => x.Friction = v, Defaults.Friction)
                .AddInput(nameof(PhysicsComponent.RollingFriction), x => x.RollingFriction, (x, v) => x.RollingFriction = v, Defaults.RollingFriction)
                .AddInput(nameof(PhysicsComponent.CcdMotionThreshold), x => x.CcdMotionThreshold, (x, v) => x.CcdMotionThreshold = v)
                .AddInput(nameof(PhysicsComponent.CcdSweptSphereRadius), x => x.CcdSweptSphereRadius, (x, v) => x.CcdSweptSphereRadius = v)
                ;
        }

        static CustomNodeDesc<RigidbodyComponent> AddRigidBodyParams(this CustomNodeDesc<RigidbodyComponent> nodeDesc)
        {
            return nodeDesc
                .AddInput(nameof(RigidbodyComponent.Mass), x => x.Mass, (x, v) => x.Mass = v, 1f)
                .AddInput(nameof(RigidbodyComponent.LinearDamping), x => x.LinearDamping, (x, v) => x.LinearDamping = v)
                .AddInput(nameof(RigidbodyComponent.AngularDamping), x => x.AngularDamping, (x, v) => x.AngularDamping = v)
                //.AddInput(nameof(RigidbodyComponent.OverrideGravity), x => x.OverrideGravity, (x, v) => x.OverrideGravity = v)
                //.AddInput(nameof(RigidbodyComponent.Gravity), x => x.Gravity, (x, v) => x.Gravity = v, Vector3.Zero)
                //.AddInput(nameof(RigidbodyComponent.IsKinematic), x => x.IsKinematic, (x, v) => x.IsKinematic = v)
                //.AddInput(nameof(RigidbodyComponent.IsTrigger), x => x.IsTrigger, (x, v) => x.IsTrigger = v)
                //.AddInput(nameof(RigidbodyComponent.NodeName), x => x.NodeName, (x, v) => x.NodeName = v)
                ;
        }

        static CustomNodeDesc<TPhysicsComponent> AddSimulationParams<TPhysicsComponent>(this CustomNodeDesc<TPhysicsComponent> nodeDesc)
            where TPhysicsComponent : PhysicsComponent, new()
        {
            return nodeDesc
                .AddInput(nameof(PhysicsComponent.CanSleep), x => x.CanSleep, (x, v) => x.CanSleep = v, true)
                .AddInput(nameof(PhysicsComponent.CollisionGroup), x => x.CollisionGroup, (x, v) => x.CollisionGroup = v, CollisionFilterGroups.DefaultFilter)
                .AddInput(nameof(PhysicsComponent.CanCollideWith), x => x.CanCollideWith, (x, v) => x.CanCollideWith = v, CollisionFilterGroupFlags.AllFilter)
                ;
        }

        static void InitPhysicsComponent<TPhysicsComponent>(TPhysicsComponent x) where TPhysicsComponent : PhysicsComponent, new()
        {
            x.Friction = Defaults.Friction;
            x.RollingFriction = Defaults.RollingFriction;
            x.Restitution = Defaults.Restitution;
        }

        static void ColliderShapeChanged<TPhysicsComponent>(TPhysicsComponent x) where TPhysicsComponent : PhysicsComponent, new()
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