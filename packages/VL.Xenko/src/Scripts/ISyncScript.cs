using System;
using System.Collections.Generic;
using System.Text;
using Xenko.Engine;

namespace VL.Xenko.Scripts
{
    public interface ISyncScript
    {
        void Start(InterfaceSyncScript syncScriptComponent);

        void Update();

        void Cancel();
    }
}
