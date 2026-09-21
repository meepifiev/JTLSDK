using System;

namespace JTLStudio.SDK.Providers
{
    [Serializable]
    public class UnsupportedFlagsProvider : IFlagsProvider
    {
        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Unsupported);
        }

        public bool TryGetValue(string key, out string value)
        {
            value = "";
            return false;
        }
    }
}
