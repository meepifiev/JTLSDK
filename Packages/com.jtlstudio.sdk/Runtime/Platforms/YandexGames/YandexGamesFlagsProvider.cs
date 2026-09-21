using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YandexGames
{
    [Serializable]
    public class YandexGamesFlagsProvider : IFlagsProvider
    {
        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Failed);
        }

        public bool TryGetValue(string key, out string value)
        {
            value = "";
            return false;
        }
    }
}
