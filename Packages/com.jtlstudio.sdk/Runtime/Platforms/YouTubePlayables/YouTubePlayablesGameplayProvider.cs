using System;
using JTLStudio.SDK.Bridge;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YouTubePlayables
{
    [Serializable]
    public class YouTubePlayablesGameplayProvider : BridgeProviderBase, IGameplayProvider
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
        }

        public void ReportGameplayStop()
        {
        }
    }
}
