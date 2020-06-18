using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VL.Core;
using VL.Lib.Basics.Resources;
using VL.Stride.Shaders;
using Stride.Core.Mathematics;
using Stride.Games;

namespace VL.Stride.EffectLib
{
    abstract class EffectNodeBase : VLObject, IDisposable
    {
        readonly IResourceHandle<GameBase> gameHandle;
        protected readonly EffectNodeDescription description;
        protected int version;
        int counter = 1;

        public EffectNodeBase(NodeContext nodeContext, EffectNodeDescription description) : base(nodeContext)
        {
            gameHandle = nodeContext.GetGameHandle();
            this.description = description;
            this.version = description.Version;
        }

        public IVLNodeDescription NodeDescription => description;

        protected GameBase Game => gameHandle.Resource;

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
                gameHandle.Dispose();
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
