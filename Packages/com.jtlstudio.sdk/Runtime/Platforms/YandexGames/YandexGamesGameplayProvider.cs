using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YandexGames
{
    [Serializable]
    public class YandexGamesGameplayProvider : IGameplayProvider
    {
        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Failed);
        }

        public void ReportGameReady()
        {
        }

        public void ReportGameplayStart()
        {
        }

        public void ReportGameplayStop()
        {
        }
    }
}
