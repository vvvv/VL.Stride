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
        }
    }
}
