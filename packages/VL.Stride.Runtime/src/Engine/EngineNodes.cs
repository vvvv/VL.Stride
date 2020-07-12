using Stride.Engine;
using Stride.Physics;
using System.Collections.Generic;
using VL.Core;

namespace VL.Stride.Engine
{
    static class EngineNodes
    {
        public static IEnumerable<IVLNodeDescription> GetNodeDescriptions(IVLNodeDescriptionFactory factory)
        {
            var lightsCategory = "Stride.Lights";

            yield return new CustomNodeDesc<SceneInstance>(factory,
                ctor: nodeContext =>
                {
                    var gameHandle = nodeContext.GetGameHandle();
                    var game = gameHandle.Resource;
                    var instance = new SceneInstance(game.Services);
                    return (instance, () => gameHandle.Dispose());
                },
                category: "Stride",
                copyOnWrite: false)
                .AddInput(nameof(SceneInstance.RootScene), x => x.RootScene, (x, v) => x.RootScene = v);

            // Light components
            yield return factory.NewComponentNode<LightComponent>(lightsCategory)
                .AddInput(nameof(LightComponent.Type), x => x.Type, (x, v) => x.Type = v)
                .AddInput(nameof(LightComponent.Intensity), x => x.Intensity, (x, v) => x.Intensity = v, 1f)
                .WithEnabledPin();

            yield return factory.NewComponentNode<LightShaftComponent>(lightsCategory)
                .AddInput(nameof(LightShaftComponent.DensityFactor), x => x.DensityFactor, (x, v) => x.DensityFactor = v, 0.002f)
                .AddInput(nameof(LightShaftComponent.SampleCount), x => x.SampleCount, (x, v) => x.SampleCount = v, 16)
                .AddInput(nameof(LightShaftComponent.SeparateBoundingVolumes), x => x.SeparateBoundingVolumes, (x, v) => x.SeparateBoundingVolumes = v, true)
                .WithEnabledPin();

            yield return factory.NewComponentNode<LightShaftBoundingVolumeComponent>(lightsCategory)
                .AddInput(nameof(LightShaftBoundingVolumeComponent.Model), x => x.Model, (x, v) => x.Model = v) // Ensure to check for change! Property throws event!
                .AddInput(nameof(LightShaftBoundingVolumeComponent.LightShaft), x => x.LightShaft, (x, v) => x.LightShaft = v) // Ensure to check for change! Property throws event!
                .WithEnabledPin();

            var physicsCategory = "Stride.Physics";

            yield return factory.NewComponentNode<StaticColliderComponent>(physicsCategory)
                .AddInput(nameof(StaticColliderComponent.ColliderShape), x => x.ColliderShape, (x, v) => x.ColliderShape = v)
                .AddInput(nameof(StaticColliderComponent.Friction), x => x.Friction, (x, v) => x.Friction = 0.5f)
                .AddInput(nameof(StaticColliderComponent.RollingFriction), x => x.RollingFriction, (x, v) => x.RollingFriction = v, 0f)
                .AddInput(nameof(StaticColliderComponent.Restitution), x => x.Restitution, (x, v) => x.Restitution = v, 0f)
                .WithEnabledPin();

            yield return factory.NewComponentNode<RigidbodyComponent>(physicsCategory)
                .AddInput(nameof(RigidbodyComponent.ColliderShape), x => x.ColliderShape, (x, v) => x.ColliderShape = v)
                .AddInput(nameof(RigidbodyComponent.Friction), x => x.Friction, (x, v) => x.Friction = 0.5f)
                .AddInput(nameof(RigidbodyComponent.RollingFriction), x => x.RollingFriction, (x, v) => x.RollingFriction = v, 0f)
                .AddInput(nameof(RigidbodyComponent.Restitution), x => x.Restitution, (x, v) => x.Restitution = v, 0f)
                .WithEnabledPin();

            yield return factory.NewNode<SphereColliderShape>(
                () => new SphereColliderShape(is2D: false, radiusParam: 1f),
                name: nameof(SphereColliderShape),
                physicsCategory);
                //.AddInput(nameof(SphereColliderShape.Radius), x => x.Radius, (x, v) => x.Radius = v, 1f);


        }
    }
}