// Copyright (c) Xenko contributors (https://xenko.com) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading.Tasks;
using Xenko.Core;
using Xenko.Core.Assets;
using Xenko.Core.Assets.Analysis;

namespace VL.Xenko.Assets
{
    public interface IRuntimeDatabase
    {
        Task<ISyncLockable> ReserveSyncLock();
        Task<IDisposable> LockAsync();
        Task Build(AssetItem x, BuildDependencyType dependencyType);
        Task<IDisposable> MountInCurrentMicroThread();

        void ResetDependencyCompiler();
    }
}