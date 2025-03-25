using System;
using System.Threading;

namespace Korn.Pipes
{
    public abstract class AsyncDisposable : IDisposable
    {
        private protected CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private protected CancellationToken cancellationToken => cancellationTokenSource.Token;

        private protected abstract void OnDispose();

        public void Dispose()
        {
            if (cancellationToken.IsCancellationRequested)
                return;
            cancellationTokenSource.Cancel();
            DeveloperTools.Debug("disposed cancellation token");

            OnDispose();
            cancellationTokenSource.Dispose();
        }            
    }
}