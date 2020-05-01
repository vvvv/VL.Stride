using System.Linq;
using System.Threading;
using VL.Core;
using VL.Core.CompilerServices;
using VL.Lib.Basics.Resources;
using VL.Xenko;
using VL.Stride.Assets;
using VL.Stride.Games;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;

[assembly: AssemblyInitializer(typeof(VL.Stride.Integration.Initialization))]

namespace VL.Stride.Integration
{
    public sealed class Initialization : AssemblyInitializer<Initialization>
    {
        protected override void RegisterServices(IVLFactory factory)
        {
            factory.RegisterService<NodeContext, IResourceProvider<Game>>(nodeContext =>
            {
                return ResourceProvider.Return(VLGame.GameInstance);
            });

            factory.RegisterService<NodeContext, IResourceProvider<GameWindow>>(nodeContext =>
            {
                return ResourceProvider.Return(VLGame.GameInstance.Window);
            });
        }
    }
}