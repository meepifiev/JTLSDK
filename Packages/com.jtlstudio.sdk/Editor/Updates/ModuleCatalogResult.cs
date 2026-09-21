using System.Collections.Generic;

namespace JTLStudio.SDK.Editor.Updates
{
    public class ModuleCatalogResult
    {
        public ModuleCatalogResult(bool success, string error, IReadOnlyList<ModuleDefinition> modules)
        {
            IsSuccess = success;
            Error = error;
            Modules = modules;
        }

        public bool IsSuccess { get; }
        public string Error { get; }
        public IReadOnlyList<ModuleDefinition> Modules { get; }
    }
}
