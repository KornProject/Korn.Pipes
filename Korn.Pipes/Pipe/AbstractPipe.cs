namespace Korn.Pipes
{
    public abstract class AbstractPipe : AsyncDisposable
    {
        public AbstractPipe(PipeConfiguration configuration) => Configuration = configuration;

        public PipeConfiguration Configuration { get; set; }
    }
}