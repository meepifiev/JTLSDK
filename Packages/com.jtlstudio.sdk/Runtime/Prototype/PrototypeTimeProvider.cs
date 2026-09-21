#if UNITY_EDITOR
using System;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Prototype
{
    public class PrototypeTimeProvider : ITimeProvider
    {
        public PrototypeTimeProvider(bool isServerTime)
        {
            IsServerTime = isServerTime;
        }

        public bool IsServerTime { get; }
        public DateTimeOffset Now => DateTimeOffset.Now;

        public void Initialize(Action<ProviderState> onInitialized)
        {
            onInitialized(ProviderState.Ready);
        }
    }
}
#endif
