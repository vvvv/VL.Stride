using Stride.Graphics;
using VL.Core;
using VL.Core.CompilerServices;
using VL.Lib.Basics.Resources;

[assembly: AssemblyInitializer(typeof(VL.Stride.GameStudio.Core.Initialization))]

namespace VL.Stride.GameStudio.Core
{
    public sealed class Initialization : AssemblyInitializer<Initialization>
    {
        protected override void RegisterServices(IVLFactory factory)
        {
            
        }
    }
}