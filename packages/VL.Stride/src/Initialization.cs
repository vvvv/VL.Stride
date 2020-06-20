using Stride.Graphics;
using VL.Core;
using VL.Core.CompilerServices;
using VL.Lib.Basics.Resources;

[assembly: AssemblyInitializer(typeof(VL.Stride.Core.Initialization))]

namespace VL.Stride.Core
{
    public sealed class Initialization : AssemblyInitializer<Initialization>
    {
        protected override void RegisterServices(IVLFactory factory)
        {
            Serialization.RegisterSerializers(factory);

            // Graphics device
            factory.RegisterService<NodeContext, IResourceProvider<GraphicsDevice>>(nodeContext =>
            {
                var gameProvider = nodeContext.GetGameProvider();
                return gameProvider.Bind(game => ResourceProvider.Return(game.GraphicsDevice));
            });

            // Graphics context
            factory.RegisterService<NodeContext, IResourceProvider<Resource<GraphicsContext>>>(nodeContext =>
            {
                var gameProvider = nodeContext.GetGameProvider();
                return gameProvider.Bind(game => ResourceProvider.Return(game.GraphicsContext.ToResource()));
            });
        }
    }
}