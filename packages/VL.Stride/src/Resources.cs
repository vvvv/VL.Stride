using System;
using System.Linq;
using VL.Core;
using VL.Lib.Basics.Resources;
using Xenko.Engine;
using Xenko.Games;

namespace VL.Xenko
{
    public static class Resources
    {
        public static uint GetResourceKey(this NodeContext nodeContext)
        {
            return nodeContext.Path.Stack.Last();
        }

        public static IResourceProvider<Game> GetGameProvider(this NodeContext nodeContext)
        {
            return nodeContext.Factory.CreateService<IResourceProvider<Game>>(nodeContext);
        }

        public static IResourceHandle<Game> GetGameHandle(this NodeContext nodeContext)
        {
            return nodeContext.GetGameProvider().GetHandle();
        }

        public static IResourceProvider<GameWindow> GetGameWindowProvider(this NodeContext nodeContext)
        {
            var key = nodeContext.GetResourceKey();
            return ResourceProvider.NewPooled(key, k =>
            {
                var gameProvider = nodeContext.GetGameProvider();
                return gameProvider
                    .Bind(game =>
                    {
                        game.Window.Visible = true;
                        return ResourceProvider.Return(game.Window, disposeAction: (window) =>
                        {
                            window.Visible = false;
                        });
                    });
            });
        }
    }
}
