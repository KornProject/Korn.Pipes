﻿using System.Collections.Concurrent;
using System.Threading.Tasks;
using System;

namespace Korn.Pipes
{
    public class InputPipe : Pipe
    {
        public InputPipe(PipeConfiguration configuration) : base(configuration)
        {
            PipeClient = new NamedPipeClient(configuration.GlobalizedName, cancellationTokenSource);

            Task.Run(HandlerBody);
        }

        public readonly NamedPipeClient PipeClient;
        BlockingCollection<byte[]> collection = new BlockingCollection<byte[]>();

        public bool HasPacketsInQueue => collection.Count > 0;

        async void HandlerBody()
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                if (PipeClient.IsConnected)
                {
                    var queue = collection.GetConsumingEnumerable();
                    foreach (var bytes in queue)
                    {
                        var lengthHeaderBytes = BitConverter.GetBytes(bytes.Length);

                        if (!PipeClient.Write(lengthHeaderBytes, 0, lengthHeaderBytes.Length) ||
                            !PipeClient.Write(bytes, 0, bytes.Length) ||
                            !PipeClient.Flush())
                            break;
                    }
                }
                else
                {
                    if (WasConnected && !IsConnected)
                        OnDisconnected();

                    await PipeClient.ConnectAsync();

                    OnConnected();
                }
            }
        }

        public void Send(byte[] bytes)
        {
            lock (this)
                collection.Add(bytes);
        }

        private protected override void OnDispose()
        {
            PipeClient.Dispose();
            collection.Dispose();
        }
    }
}