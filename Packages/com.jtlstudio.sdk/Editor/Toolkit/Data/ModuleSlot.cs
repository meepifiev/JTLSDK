using System;

namespace JTLStudio.SDK.Editor.Toolkit.Data
{
    public class ModuleSlot
    {
        public ModuleSlot(string nameKey, string propertyName, Type providerInterface)
        {
            NameKey = nameKey ?? throw new ArgumentNullException(nameof(nameKey));
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            ProviderInterface = providerInterface ?? throw new ArgumentNullException(nameof(providerInterface));
        }

        public string NameKey { get; }
        public string PropertyName { get; }
        public Type ProviderInterface { get; }
    }
}
