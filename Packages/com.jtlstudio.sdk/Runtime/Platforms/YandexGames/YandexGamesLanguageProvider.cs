using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YandexGames
{
    [Serializable]
    public class YandexGamesLanguageProvider : ILanguageProvider
    {
        public string LanguageCode => "";

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Failed);
        }
    }
}
