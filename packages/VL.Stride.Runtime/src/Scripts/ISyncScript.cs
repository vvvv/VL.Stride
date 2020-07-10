using System;
using System.Collections.Generic;
using System.Text;
using Stride.Engine;

namespace VL.Stride.Scripts
{
    public interface ISyncScript
    {
        void Start(InterfaceSyncScript syncScriptComponent);

        void Update();

        void Cancel();
    }
}
