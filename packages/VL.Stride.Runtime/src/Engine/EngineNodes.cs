using Stride.Engine;
using System.Collections.Generic;
using VL.Core;

namespace VL.Stride.Engine
{
    static class EngineNodes
    {
        public static IEnumerable<IVLNodeDescription> GetNodeDescriptions(IVLNodeDescriptionFactory factory)
        {
            yield return new CustomNodeDesc<SceneInstanceSystem>(factory,
                ctor: nodeContext =>
                {
                    var gameHandle = nodeContext.GetGameHandle();
                    var game = gameHandle.Resource;
                    var instance = new SceneInstanceSystem(game.Services);
                    return (instance, () => gameHandle.Dispose());
                },
                category: "Stride",
                copyOnWrite: false)
                .AddInput(nameof(SceneInstanceSystem.RootScene), x => x.RootScene, (x, v) => x.RootScene = v)
                .AddOutput(nameof(SceneInstanceSystem.SceneInstance), x => x.SceneInstance);

            yield return new CustomNodeDesc<SceneInstanceRenderer>(factory,
                ctor: nodeContext =>
                {
                    var gameHandle = nodeContext.GetGameHandle();
                    var game = gameHandle.Resource;
                    var instance = new SceneInstanceRenderer();
                    return (instance, () => gameHandle.Dispose());
                },
                category: "Stride",
                copyOnWrite: false)
                .AddInput(nameof(SceneInstanceRenderer.SceneInstance), x => x.SceneInstance, (x, v) => x.SceneInstance = v)
                .AddInput(nameof(SceneInstanceRenderer.GraphicsCompositor), x => x.GraphicsCompositor, (x, v) => x.GraphicsCompositor = v);

            yield return new CustomNodeDesc<LayerSystem>(factory,
                ctor: nodeContext =>
                {
                    var gameHandle = nodeContext.GetGameHandle();
                    var game = gameHandle.Resource;
                    var instance = new LayerSystem(game.Services);
                    return (instance, () => gameHandle.Dispose());
                },
                category: "Stride",
                copyOnWrite: false,
                fragmented: true)
                .AddInput(nameof(LayerSystem.Layer), x => x.Layer, (x, v) => x.Layer = v);

            var lightsCategory = "Stride.Lights";

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
        }
    }
}