using System;
using JTLStudio.SDK.Bridge;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YandexGames
{
    [Serializable]
    public class YandexGamesLanguageProvider : BridgeProviderBase, ILanguageProvider
    {
        public string LanguageCode { get; private set; } = "";

        public void Initialize(Action<ProviderState> onInitialized)
        {
            InitializeWith("platform", "initialize", onInitialized, response => LanguageCode = response.GetString("language"));
        }
    }
}
