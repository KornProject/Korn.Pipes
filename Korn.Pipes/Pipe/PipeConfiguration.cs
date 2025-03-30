namespace Korn.Pipes
{
    public class PipeConfiguration
    {
        public PipeConfiguration(string name) => Name = name;

        public readonly string Name;
        public string GlobalizedName => NameGlobalizer.GlobalizeName(Name);
    }
}