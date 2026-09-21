using System;
using UnityEngine.Scripting.APIUpdating;
using JTLStudio.SDK.Bridge;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YandexGames
{
    [Serializable]
    [MovedFrom(false, sourceClassName: "YandexGamesGameplayProvider")]
    [ProviderPlatforms(PlatformId.YandexGames)]
    public class YandexGamesGameEventsProvider : BridgeProviderBase, IGameEventsProvider
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
