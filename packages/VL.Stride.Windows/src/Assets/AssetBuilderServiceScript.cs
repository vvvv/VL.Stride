using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using VL.Stride.Assets;
using Stride.Core.Assets;
using Stride.Engine;

namespace VL.Stride.Assets
{
    /// <summary>
    /// Custom vl script that sets MSBuild
    /// </summary>
    public class AssetBuilderServiceScript : AsyncScript
    {
        public RuntimeContentLoader ContentLoader;
        ConcurrentQueue<AssetItem> workQueue = new ConcurrentQueue<AssetItem>();

        public AssetBuilderServiceScript()
        {
            //set msbuild
            PackageSessionPublicHelper.FindAndSetMSBuildVersion();
        }

        public void PushWork(IEnumerable<AssetItem> items)
        {
            foreach(var item in items)
                workQueue.Enqueue(item);
        }

        public void PushWork(AssetItem item)
        {
            workQueue.Enqueue(item);
        }


        public override async Task Execute()
        {
            while (true)
            {
                await Script.NextFrame();
                if (!workQueue.IsEmpty)
                    ContentLoader?.BuildAndReloadAssets(DequeueItems());
            }
        }

        private IEnumerable<AssetItem> DequeueItems()
        {
            while (workQueue.TryDequeue(out var item))
                yield return item;
        }
    }
}
