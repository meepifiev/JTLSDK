using System;

namespace JTLStudio.SDK.Providers
{
    public interface IProvider
    {
        void Initialize(Action<ProviderState> onInitialized);
    }
}
