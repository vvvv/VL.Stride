using System;
using System.Collections.Generic;
using System.Text;
using Stride.Core.Diagnostics;

namespace VL.Stride.Core
{
    public static class LoggerExtensions
    {
        public static ILogger GetLoggerResult(string name = null) 
            => new LoggerResult(name);
    }
}
