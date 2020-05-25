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
        }
    }
}