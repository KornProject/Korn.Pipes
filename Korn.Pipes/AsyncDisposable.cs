using System;
using System.Threading;

namespace Korn.Pipes
{
    public abstract class AsyncDisposable : IDisposable
    {
        public AsyncDisposable() : this(new CancellationTokenSource()) { }

        public AsyncDisposable(CancellationTokenSource cancellationTokenSource) => this.cancellationTokenSource = cancellationTokenSource;

        private protected CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private protected CancellationToken cancellationToken => cancellationTokenSource.Token;

        private protected abstract void OnDispose();

        public void Dispose()
        {
            if (cancellationToken.IsCancellationRequested)
                return;
            cancellationTokenSource.Cancel();

            OnDispose();
            cancellationTokenSource.Dispose();
        }            
    }
}