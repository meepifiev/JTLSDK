using System;

namespace JTLStudio.SDK.Providers
{
    [Serializable]
    public class FallbackGameplayProvider : IGameplayProvider
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
