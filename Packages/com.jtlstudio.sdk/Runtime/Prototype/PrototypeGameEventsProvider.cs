#if UNITY_EDITOR
using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Prototype
{
    public class PrototypeGameEventsProvider : IGameEventsProvider
    {
        public bool GameReadyReported { get; private set; }
        public bool IsPlaying { get; private set; }

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Ready);
        }

        public void ReportGameReady()
        {
            GameReadyReported = true;
        }

        public void ReportGameplayStart()
        {
            IsPlaying = true;
        }

        public void ReportGameplayStop()
        {
            IsPlaying = false;
        }
    }
}
#endif
