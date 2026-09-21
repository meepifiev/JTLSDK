using System;
using UnityEngine.Scripting.APIUpdating;

namespace JTLStudio.SDK.Providers
{
    [Serializable]
    [MovedFrom(false, sourceClassName: "FallbackGameplayProvider")]
    public class FallbackGameEventsProvider : IGameEventsProvider
    {
        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Ready);
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
