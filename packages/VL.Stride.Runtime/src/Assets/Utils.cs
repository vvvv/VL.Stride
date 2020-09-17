using Stride.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace VL.Stride.Assets
{
    public abstract class AssetWrapperBase
    {
        protected bool Exists;
        protected bool Loading;
        protected string Name;

        public void SetLoading(bool loading) => Loading = loading;

        public void SetExists(bool exists) => Exists = exists;

        public void SetName(string name) => Name = name;

        public abstract void SetAssetObject(object asset);
    }

    public class AssetWrapper<T> : AssetWrapperBase
    {
        T Asset;

        public void SetAsset(T asset)
        {
            Asset = asset;
        }

        public void SetValues(T asset, bool exists)
        {
            Asset = asset;
            Exists = exists;
        }

        public void GetValues(out T asset, out bool exists, out bool loading)
        {
            asset = Asset;
            exists = Exists;
            loading = Loading;
        }

        public override void SetAssetObject(object asset)
        {
            Asset = (T)asset;

            if (Asset is ComponentBase componentBase && !string.IsNullOrWhiteSpace(Name))
                componentBase.Name = Name;
        }
    }
}
