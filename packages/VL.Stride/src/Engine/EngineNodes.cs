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
        }
    }
}