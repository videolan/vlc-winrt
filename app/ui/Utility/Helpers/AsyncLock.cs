/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/


using System;
using System.Threading;
using System.Threading.Tasks;

namespace VLC_WINRT.Utility.Helpers
{
    public sealed class AsyncLock
    {
        private readonly SemaphoreSlim _semphore = new SemaphoreSlim(1, 1);
        private readonly Task<IDisposable> _completedResult;

        public AsyncLock()
        {
            _completedResult = Task.FromResult<IDisposable>(new Releaser(this));
        }

        public Task<IDisposable> LockAsync()
        {
            var wait = _semphore.WaitAsync();
            return wait.IsCompleted ?
                        _completedResult :
                        wait.ContinueWith((_, state) => (IDisposable)state,
                            _completedResult.Result, CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        private sealed class Releaser : IDisposable
        {
            private readonly AsyncLock _toRelease;
            internal Releaser(AsyncLock toRelease) { _toRelease = toRelease; }
            public void Dispose() { _toRelease._semphore.Release(); }
        }
    }
}
