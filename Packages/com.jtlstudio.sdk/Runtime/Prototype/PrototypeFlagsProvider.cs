#if UNITY_EDITOR
using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Prototype
{
    public class PrototypeFlagsProvider : IFlagsProvider
    {
        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Ready);
        }

        public bool TryGetValue(string key, out string value)
        {
            value = "";
            return false;
        }
    }
}
#endif
