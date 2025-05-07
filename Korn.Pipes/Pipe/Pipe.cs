using System;
using System.Diagnostics.CodeAnalysis;

namespace Korn.Pipes
{
    public abstract class Pipe : AsyncDisposable
    {
        public Pipe(PipeConfiguration configuration) => Configuration = configuration;

        public PipeConfiguration Configuration { get; private set; }
        public bool WasConnected { get; protected set; }

        public Action Connected;
        public Action Disconnected;

        public bool IsConnected { get; protected set; }

        protected void OnConnected()
        {
            WasConnected = true;

            IsConnected = true;
            Connected?.Invoke();
        }

        protected void OnDisconnected()
        {
            IsConnected = false;
            Disconnected?.Invoke();
        }
    }
}