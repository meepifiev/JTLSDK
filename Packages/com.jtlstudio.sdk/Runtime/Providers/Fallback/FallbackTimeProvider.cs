using System;

namespace JTLStudio.SDK.Providers
{
    [Serializable]
    public class FallbackTimeProvider : ITimeProvider
    {
        public bool IsServerTime => false;
        public DateTimeOffset Now => DateTimeOffset.Now;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Ready);
        }
    }
}
