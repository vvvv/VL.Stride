using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VL.Core;
using VL.Xenko.Shaders;
using Xenko.Core.Mathematics;

namespace VL.Xenko.EffectLib
{
    abstract class EffectNodeBase : VLObject, IDisposable
    {
        protected readonly EffectNodeDescription description;
        protected int version;
        int counter = 1;

        public EffectNodeBase(NodeContext nodeContext, EffectNodeDescription description) : base(nodeContext)
        {
            this.description = description;
            this.version = description.Version;
        }

        public IVLNodeDescription NodeDescription => description;

        protected ValueParameterPin<Matrix> worldPin;

        protected Matrix entityWorldMatrix = Matrix.Identity;
        public void SetEntityWorldMatrix(Matrix entityWorld)
            => entityWorldMatrix = entityWorld;

        protected Matrix ComputeWorldMatrix()
        {
            Matrix result;
            if (worldPin != null)
            {
                var world = worldPin.Value;
                Matrix.MultiplyTo(ref world, ref entityWorldMatrix, out result);
                worldPin.Value = result;
            }
            else
                result = entityWorldMatrix;
            return result;
        }


        public void Dispose()
        {
            if (Interlocked.Decrement(ref counter) == 0)
            {
                Destroy();
            }
        }

        protected abstract void Destroy();

        protected void ReportException(Exception e)
        {
            var re = new RuntimeException(e.InnermostException(), this);
            RuntimeGraph.ReportException(re);
        }
    }
}
