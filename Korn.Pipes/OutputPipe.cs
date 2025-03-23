using System.IO.Pipes;

namespace Korn.Pipes;
public class OutputPipe : AbstractPipe, IDisposable
{
    public OutputPipe(PipeConfiguration configuration) : base(configuration)
        => Stream = new NamedPipeServerStream(configuration.GlobalizedName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.WriteThrough);

    public readonly NamedPipeServerStream Stream;

    public void WaitConnection() => Stream.WaitForConnection();

    public byte[] Read(int length)
    {
        var buffer = new byte[length];
        var offset = 0;
        while ((offset += Stream.Read(buffer, offset, buffer.Length - offset)) < length);

        return buffer;
    }

    bool disposed;
    public void Dispose()
    {
        if (disposed)
            return;
        disposed = true;

        Stream.Disconnect();
        Stream.Dispose();
    }
}