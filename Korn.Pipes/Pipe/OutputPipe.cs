using System.Threading.Tasks;
using System;

namespace Korn.Pipes
{
    public class OutputPipe : Pipe
    {
        public OutputPipe(PipeConfiguration configuration) : base(configuration)
        {
            PipeServer = new NamedPipeServer(configuration.GlobalizedName, cancellationTokenSource);

            Task.Run(HandlerBody);
        }

        public readonly NamedPipeServer PipeServer;

        public Action<byte[]> Received;

        byte[] lengthHeaderBytes = new byte[sizeof(int)];
        async void HandlerBody()
        {
            const int PacketLengthThreshold = short.MaxValue;

            NamedPipeServer.ReadResult readResult;
            while (!cancellationToken.IsCancellationRequested)
            {
                if (PipeServer.IsConnected)
                {
                    readResult = await PipeServer.ReadAsync(lengthHeaderBytes, 0, lengthHeaderBytes.Length);
                    if (!readResult.IsSuccess)
                        continue;

                    var lengthHeader = BitConverter.ToInt32(lengthHeaderBytes, 0);
                    if (lengthHeader == 0)
                        continue;

                    if (lengthHeader > PacketLengthThreshold)
                        throw new Exception($"Korn.Pipes.OutputPipe: packet length more than max threshold {PacketLengthThreshold}");

                    var packetBytes = new byte[lengthHeader];
                    var offset = 0;

                    do
                    {
                        readResult = await PipeServer.ReadAsync(packetBytes, offset, packetBytes.Length - offset);
                        if (!readResult.IsSuccess)
                            break;

                        offset += readResult.ReadBytes;
                    }
                    while (offset < lengthHeader);
                    
                    if (offset == lengthHeader)
                        if (Received != null)
                            Received.Invoke(packetBytes);
                }
                else
                {
                    if (WasConnected && !IsConnected)
                        OnDisconnected();

                    await PipeServer.WaitForConnectionAsync();
                    OnConnected();
                }
            }               
        }

        public void Disconnect() => PipeServer.Disconnect();

        private protected override void OnDispose() => PipeServer.Dispose();
    }
}