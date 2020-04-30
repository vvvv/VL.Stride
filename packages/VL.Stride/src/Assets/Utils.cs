using System;
using System.Collections.Generic;
using System.Text;

namespace VL.Stride.Assets
{
    public class AssetWrapperCSharp<T>
    {
        T Asset;
        bool Exists;

        public void SetValues(T asset, bool exists)
        {
            Asset = asset;
            Exists = exists;
        }

        public void GetValues(out T asset, out bool exists)
        {
            asset = Asset;
            exists = Exists;
        }
    }
}
