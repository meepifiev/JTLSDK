using System;

namespace JTLStudio.SDK.Providers
{
    [Serializable]
    public class UnsupportedAnalyticsProvider : IAnalyticsProvider
    {
        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Unsupported);
        }

        public void Report(string eventName, string parametersJson)
        {
        }
    }
}
