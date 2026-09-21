using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YandexGames
{
    [Serializable]
    public class YandexGamesTimeProvider : ITimeProvider
    {
        public bool IsServerTime => false;
        public DateTimeOffset Now => DateTimeOffset.Now;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Failed);
        }
    }
}
