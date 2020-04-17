using System;
using System.Collections.Generic;
using System.Text;
using Xenko.Core.Mathematics;
using Xenko.Engine;

namespace VL.Xenko.Scripts
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
