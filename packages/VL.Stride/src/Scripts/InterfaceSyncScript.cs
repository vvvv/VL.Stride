using System;
using System.Collections.Generic;
using System.Text;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace VL.Stride.Scripts
{
    public class InterfaceSyncScript : SyncScript
    {
        ISyncScript script;

        public InterfaceSyncScript(ISyncScript script)
        {
            this.script = script;
        }

        public override void Start()
        {
            script?.Start(this);
        }

        public override void Update()
        {
            script?.Update();
        }

        public override void Cancel()
        {
            script?.Cancel();
        }
    }
}
