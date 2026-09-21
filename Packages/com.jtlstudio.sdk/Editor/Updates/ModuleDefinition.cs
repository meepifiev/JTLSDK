using System.Collections.Generic;

namespace JTLStudio.SDK.Editor.Updates
{
    public class ModuleDefinition
    {
        public ModuleDefinition(string id, string name, string package, string repository, string path, string requires, IReadOnlyList<string> platforms)
        {
            Id = id;
            Name = name;
            Package = package;
            Repository = repository;
            Path = path;
            Requires = requires;
            Platforms = platforms;
        }

        public string Id { get; }
        public string Name { get; }
        public string Package { get; }
        public string Repository { get; }
        public string Path { get; }
        public string Requires { get; }
        public IReadOnlyList<string> Platforms { get; }
    }
}
