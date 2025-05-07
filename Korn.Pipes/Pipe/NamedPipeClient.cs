using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Korn.Pipes
{
    public class NamedPipeClient : AsyncDisposable
    {
        public NamedPipeClient(string name, CancellationTokenSource cancellationTokenSource) : base(cancellationTokenSource)
        {
            Stream = new NamedPipeClientStream(".", name, PipeDirection.Out, PipeOptions.WriteThrough);
        }

        public readonly NamedPipeClientStream Stream;

        public bool IsConnected
        {
            get
            {
                try
                {
                    return Stream.IsConnected;
                }
                catch (ObjectDisposedException) { }
                return false;
            }
        }

        public bool Write(byte[] bytes) => Write(bytes, 0, bytes.Length);

        public bool Write(byte[] bytes, int offset, int length)
        {
            try
            {
                Stream.Write(bytes, 0, bytes.Length);
                return true;
            }
            catch (ObjectDisposedException) { }
            catch (IOException) { }
            return false;
        }

        public bool Flush()
        {
            try
            {
                Stream.Flush();
                return true;
            }
            catch (ObjectDisposedException) { }
            catch (IOException) { }
            return false;
        }

        public async Task ConnectAsync()
        {
            try
            {
                do await Stream.ConnectAsync(cancellationToken);
                while (!Stream.IsConnected);
            }
            catch (TaskCanceledException) { }
            catch (ObjectDisposedException) { }
        }

        private protected override void OnDispose() => Stream.Dispose();
    }
}