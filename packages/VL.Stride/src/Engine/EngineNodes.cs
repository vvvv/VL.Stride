using Stride.Engine;
using System.Collections.Generic;
using VL.Core;

namespace VL.Stride.Engine
{
    static class EngineNodes
    {
        public static IEnumerable<IVLNodeDescription> GetNodeDescriptions(IVLNodeDescriptionFactory factory)
        {
            var engineCategory = "Stride.Engine";
            var engineComponentsCategory = $"{engineCategory}.Components";

            yield return new CustomNodeDesc<SceneInstance>(factory,
                ctor: nodeContext =>
                {
                    var gameHandle = nodeContext.GetGameHandle();
                    var game = gameHandle.Resource;
                    var instance = new SceneInstance(game.Services);
                    return (instance, () => gameHandle.Dispose());
                },
                category: "Stride.Engine",
                copyOnWrite: false)
                .AddInput(nameof(SceneInstance.RootScene), x => x.RootScene, (x, v) => x.RootScene = v);

            // Light components
            yield return factory.CreateComponentNode<LightComponent>(engineComponentsCategory)
                .AddInput(nameof(LightComponent.Type), x => x.Type, (x, v) => x.Type = v)
                .AddInput(nameof(LightComponent.Intensity), x => x.Intensity, (x, v) => x.Intensity = v, 1f)
                .WithEnabledPin();

            yield return factory.CreateComponentNode<LightShaftComponent>(engineComponentsCategory)
                .AddInput(nameof(LightShaftComponent.DensityFactor), x => x.DensityFactor, (x, v) => x.DensityFactor = v, 0.002f)
                .AddInput(nameof(LightShaftComponent.SampleCount), x => x.SampleCount, (x, v) => x.SampleCount = v, 16)
                .AddInput(nameof(LightShaftComponent.SeparateBoundingVolumes), x => x.SeparateBoundingVolumes, (x, v) => x.SeparateBoundingVolumes = v, true)
                .WithEnabledPin();

            yield return factory.CreateComponentNode<LightShaftBoundingVolumeComponent>(engineComponentsCategory)
                .AddInput(nameof(LightShaftBoundingVolumeComponent.Model), x => x.Model, (x, v) => x.Model = v) // Ensure to check for change! Property throws event!
                .AddInput(nameof(LightShaftBoundingVolumeComponent.LightShaft), x => x.LightShaft, (x, v) => x.LightShaft = v) // Ensure to check for change! Property throws event!
                .WithEnabledPin();
        }
    }
}