using System.Linq;
using System.Threading;
using VL.Core;
using VL.Core.CompilerServices;
using VL.Lib.Basics.Resources;
using VL.Xenko;
using VL.Xenko.Assets;
using VL.Xenko.Games;
using Xenko.Engine;
using Xenko.Games;
using Xenko.Graphics;

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