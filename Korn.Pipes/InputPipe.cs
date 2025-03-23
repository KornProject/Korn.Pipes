using System.IO.Pipes;

namespace Korn.Pipes;
public unsafe class InputPipe : AbstractPipe, IDisposable
{
    public InputPipe(PipeConfiguration configuration) : base(configuration) 
        => Stream = new NamedPipeClientStream(".", configuration.GlobalizedName, PipeDirection.Out, PipeOptions.WriteThrough);

    public readonly NamedPipeClientStream Stream;

    public bool TryConnect()
    {
        Stream.Connect(Configuration.ConnectTimespan);
        return Stream.IsConnected;
    }

    public void Write(byte[] bytes) => Write(new Span<byte>(bytes));
    public void Write(byte* pointer, int length) => Write(new Span<byte>(pointer, length));
    public void Write(Span<byte> span)
    {
        Stream.Write(span);
        Stream.Flush();
    }

    bool disposed;
    public void Dispose()
    {
        if (disposed)
            return;
        disposed = true;

        Stream.Dispose();
    }
}