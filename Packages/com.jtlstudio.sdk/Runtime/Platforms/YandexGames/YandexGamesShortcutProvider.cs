using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YandexGames
{
    [Serializable]
    public class YandexGamesShortcutProvider : IShortcutProvider
    {
        public bool CanRequest => false;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Failed);
        }

        public void Request(Action<bool> onResult)
        {
            onResult(false);
        }
    }
}
