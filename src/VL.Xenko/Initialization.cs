using VL.Core;

namespace VL.Lib
{
    // Importer will look for static class VL.Lib.Initialization in each assembly
    static class Initialization
    {
        public static void RegisterServices(IVLFactory factory)
        {
            VL.Xenko.Core.Serialization.RegisterSerializers(factory);
        }
    }
}