using System;
using JTLStudio.SDK.Bridge;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YandexGames
{
    [Serializable]
    [ProviderPlatforms(PlatformId.YandexGames)]
    public class YandexGamesGameplayProvider : BridgeProviderBase, IGameplayProvider
    {
        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(IsBridgeReady ? ProviderState.Ready : ProviderState.Failed);
        }

        public void ReportGameReady()
        {
            Call("gameplay", "ready", null, _ => { });
        }

        public void ReportGameplayStart()
        {
            Call("gameplay", "start", null, _ => { });
        }

        public void ReportGameplayStop()
        {
            Call("gameplay", "stop", null, _ => { });
        }
    }
}
