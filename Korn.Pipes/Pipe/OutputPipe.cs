using System.Threading.Tasks;
using System.IO.Pipes;
using System;

namespace Korn.Pipes
{
    public class OutputPipe : AbstractPipe
    {
        public OutputPipe(PipeConfiguration configuration) : base(configuration)
        {
            Stream = new NamedPipeServerStream(configuration.GlobalizedName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.WriteThrough);
            DeveloperTools.Debug("initialized NamedPipeServerStream");

            Task.Run(HandlerBody);
        }

        public readonly NamedPipeServerStream Stream;
        public Action<byte[]> Received;

        byte[] lengthHeaderBytes = new byte[sizeof(int)];
        async void HandlerBody()
        {
            const int PacketLengthThreshold = short.MaxValue;

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    if (Stream.IsConnected)
                    {
                        await Stream.ReadAsync(lengthHeaderBytes, 0, lengthHeaderBytes.Length, cancellationToken);
                        DeveloperTools.Debug($"read length header");
                        if (!Stream.IsConnected)
                            continue;

                        var lengthHeader = BitConverter.ToInt32(lengthHeaderBytes, 0);
                        DeveloperTools.Debug($"length header: {lengthHeader}");
                        if (lengthHeader == 0)
                            continue;

                        if (lengthHeader > PacketLengthThreshold)
                            throw new Exception($"Korn.Pipes.OutputPipe: packet length more than max threshold {PacketLengthThreshold}");

                        var packetBytes = new byte[lengthHeader];
                        var offset = 0;
                        while ((offset += await Stream.ReadAsync(packetBytes, offset, packetBytes.Length - offset, cancellationToken)) < lengthHeader);

                        DeveloperTools.Debug($"read packet body");
                        if (Received != null)
                            Received.Invoke(packetBytes);
                    }
                    else
                    {
                        var state = Stream.GetState();
                        DeveloperTools.Debug($"trying handle connection, state: {state}");
                        if (state == PipeState.Broken)
                            Stream.Disconnect();
                        await Stream.WaitForConnectionAsync(cancellationToken);
                        DeveloperTools.Debug($"handled connection, state: {Stream.GetState()}");
                    }
                }
                catch (TaskCanceledException) { }
            }               
        }

        private protected override void OnDispose() => Stream.Dispose();
    }
}