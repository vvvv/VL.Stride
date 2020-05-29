using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Text;
using VL.Core;

namespace VL.Stride.Engine
{
    static class EngineNodes
    {
        public static IEnumerable<IVLNodeDescription> GetNodeDescriptions(IVLNodeDescriptionFactory factory)
        {
            var engineCategory = "Stride.Engine";

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

            // TODO: Parent/Child tree node manager in C#?
            //// Lights
            //yield return factory.ForComponent<LightComponent>(engineCategory)
            //    .AddInput(nameof(LightComponent.Type), x => x.Type, (x, v) => x.Type = v)
            //    .AddInput(nameof(LightComponent.Intensity), x => x.Intensity, (x, v) => x.Intensity = v, 1f)
            //    .WithEnabledPin();
        }

        static CustomNodeDesc<TComponent> ForComponent<TComponent>(this IVLNodeDescriptionFactory factory, string category)
            where TComponent : ActivableEntityComponent, new()
        {
            return factory.Create<TComponent>(category: category, copyOnWrite: false);
        }

        static IVLNodeDescription WithEnabledPin<TComponent>(this CustomNodeDesc<TComponent> node)
            where TComponent : ActivableEntityComponent
        {
            return node.AddInput("Enabled", x => x.Enabled, (x, v) => x.Enabled = v, true);
        }
    }
}
