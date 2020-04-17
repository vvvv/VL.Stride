using System;
using System.Collections.Generic;
using System.Text;

namespace VL.Xenko.Core
{
    public static class ObjectDebugger
    {
        static object lastObjectToDebug;

        public static void DebugObject(object objectToDebug)
        {
            lastObjectToDebug = objectToDebug;
        }
    }
}
