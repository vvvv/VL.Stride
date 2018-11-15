using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VL.Core;

namespace VL.Xenko.EffectLib
{
    abstract class EffectNodeBase : VLObject, IDisposable
    {
        readonly EffectNodeDescription description;
        int counter = 1;

        public EffectNodeBase(NodeContext nodeContext, EffectNodeDescription description) : base(nodeContext)
        {
            this.description = description;
        }

        public IVLNodeDescription NodeDescription => description;

        public void Dispose()
        {
            if (Interlocked.Decrement(ref counter) == 0)
            {
                Destroy();
            }
        }

        protected abstract void Destroy();
    }
}
