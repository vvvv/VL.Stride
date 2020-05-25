using SharpDX.Direct3D11;
using VL.Core;
using VL.Core.CompilerServices;
using VL.Lib.Basics.Resources;
using VL.Xenko.Games;
using Xenko.Graphics;

[assembly: AssemblyInitializer(typeof(VL.Xenko.Core.Initialization))]

namespace VL.Xenko.Core
{
    public sealed class Initialization : AssemblyInitializer<Initialization>
    {
        protected override void RegisterServices(IVLFactory factory)
        {
            Serialization.RegisterSerializers(factory);

            // Register the native graphics device - needed by VL.MediaFoundation and VL.CEF
            factory.RegisterService<NodeContext, IResourceProvider<Device>>(nodeContext =>
            {
                return ResourceProvider.Return((Device)SharpDXInterop.GetNativeDevice(VLGame.GameInstance.GraphicsDevice));
            });
        }
    }
}