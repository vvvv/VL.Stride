using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using VL.Xenko.Assets;
using Xenko.Core.Assets;
using Xenko.Engine;

namespace VL.Xenko.Assets
{
    public class AssetBuildScript : AsyncScript
    {
        public RuntimeContentLoader ContentLoader;
        ConcurrentQueue<AssetItem> workQueue = new ConcurrentQueue<AssetItem>();

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
                    await ContentLoader?.BuildAndReloadAssets(DequeueItems());
            }
        }

        private IEnumerable<AssetItem> DequeueItems()
        {
            while (workQueue.TryDequeue(out var item))
                yield return item;
        }
    }
}
