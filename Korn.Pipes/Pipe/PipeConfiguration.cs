namespace Korn.Pipes
{
    public class PipeConfiguration
    {
        public PipeConfiguration(string name) => Name = name;

        public readonly string Name;
        public string GlobalizedName => NameGlobalizer.GlobalizeName(Name);

        public PipeConfiguration GetSubPipeConfiguration(string subname) => new PipeConfiguration($"{Name}-{subname}");
    }
}