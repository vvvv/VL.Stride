using VL.Core;
using VL.Lib.Basics.Resources;
using Xenko.Engine;

namespace VL.Xenko
{
    public static class Resources
    {
        public static IResourceProvider<Game> GetGameProvider(this NodeContext nodeContext)
        {
            return nodeContext.Factory.CreateService<IResourceProvider<Game>>(nodeContext);
        }

        public static IResourceHandle<Game> GetGameHandle(this NodeContext nodeContext)
        {
            return nodeContext.GetGameProvider().GetHandle();
        }
    }
}
