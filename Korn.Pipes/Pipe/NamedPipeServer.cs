using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Korn.Pipes
{
    public class NamedPipeServer : AsyncDisposable
    {
        public NamedPipeServer(string name, CancellationTokenSource cancellationTokenSource) : base(cancellationTokenSource)
        {
            Stream = new NamedPipeServerStream(name, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.WriteThrough);
        }

        public readonly NamedPipeServerStream Stream;

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

        public async Task WaitForConnectionAsync()
        {
            try
            {
                var state = Stream.GetState();
                if (state == PipeState.Broken)
                    Stream.Disconnect();

                await Stream.WaitForConnectionAsync(cancellationToken);
            }
            catch (TaskCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (IOException) { }
        }

        public async Task<ReadResult> ReadAsync(byte[] buffer) => await ReadAsync(buffer, 0, buffer.Length);

        public async Task<ReadResult> ReadAsync(byte[] buffer, int offset, int count)
        {
            try
            {
                var read = await Stream.ReadAsync(buffer, offset, count, cancellationToken);
                return new ReadResult(IsConnected, read);
            }
            catch (TaskCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (IOException) { }

            return new ReadResult(false, 0);
        }

        public void Disconnect() => Stream.Disconnect();

        private protected override void OnDispose() => Stream.Dispose();

        public struct ReadResult
        {
            public ReadResult(bool isSuccess, int readBytes) => (IsSuccess, ReadBytes) = (isSuccess, readBytes);

            public bool IsSuccess;
            public int ReadBytes;
        }
    }
}
