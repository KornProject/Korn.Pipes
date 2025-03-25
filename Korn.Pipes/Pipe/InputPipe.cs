using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.IO.Pipes;
using System;
using System.Threading;

namespace Korn.Pipes
{
    public class InputPipe : AbstractPipe
    {
        public InputPipe(PipeConfiguration configuration) : base(configuration)
        {
            Stream = new NamedPipeClientStream(".", configuration.GlobalizedName, PipeDirection.Out, PipeOptions.WriteThrough);
            DeveloperTools.Debug("initialized NamedPipeClientStream");

            Task.Run(HandlerBody);
        }

        public readonly NamedPipeClientStream Stream;
        BlockingCollection<byte[]> collection = new BlockingCollection<byte[]>();

        public bool HasPacketsInQueue => collection.Count > 0;

        async void HandlerBody()
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    if (Stream.IsConnected)
                    {
                        try
                        {
                            var queue = collection.GetConsumingEnumerable();
                            foreach (var bytes in queue)
                            {
                                DeveloperTools.Debug($"trying write packet");
                                var lengthHeaderBytes = BitConverter.GetBytes(bytes.Length);
                                Stream.Write(lengthHeaderBytes, 0, lengthHeaderBytes.Length);
                                Stream.Write(bytes, 0, bytes.Length);
                                Stream.Flush();
                            }
                        }
                        catch (OperationCanceledException) { }
                    }
                    else
                    {
                        DeveloperTools.Debug($"trying to connect");
                        await Stream.ConnectAsync(250, cancellationToken);
                        DeveloperTools.Debug($"connected");
                    }
                }
                catch (TaskCanceledException) { }
            }                
        }

        public void Send(byte[] bytes)
        {
            lock (this)
            {
                collection.Add(bytes);
            }
        }

        private protected override void OnDispose()
        {
            // waiting until packets drained or timeouted for ~200 ms
            for (var i = 0; i < 200 && Stream.IsConnected && collection.Count != 0; i++) 
                Thread.Sleep(1);

            Stream.Dispose();
            collection.Dispose();
        }
    }
}