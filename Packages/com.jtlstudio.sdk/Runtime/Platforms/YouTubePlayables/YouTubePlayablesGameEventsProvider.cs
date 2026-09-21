using System;
using UnityEngine.Scripting.APIUpdating;
using JTLStudio.SDK.Bridge;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.YouTubePlayables
{
    [Serializable]
    [MovedFrom(false, sourceClassName: "YouTubePlayablesGameplayProvider")]
    [ProviderPlatforms(PlatformId.YouTubePlayables)]
    public class YouTubePlayablesGameEventsProvider : BridgeProviderBase, IGameEventsProvider
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
